using System.Text;
using System.Xml;
using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;
using Winterborn.Tools.EasySemVer.Settings;

namespace Winterborn.Tools.EasySemVer.Persistence;

/// <summary>
/// Baseline v2 (§6): one file at the folder root whose content is a flat array of packageable
/// units, each carrying its own language's signature as an opaque payload element. Nothing in
/// here knows what any of those payloads contain - that is what lets a new language be added
/// without editing this file (acceptance criterion 8).
/// </summary>
internal static class BaselineFile
{
    private const string UnitElementName = "Unit";
    private const string LanguageAttributeName = "language";
    private const string UnitIdAttributeName = "unitId";
    private const string UnitKindAttributeName = "unitKind";
    private const string PathAttributeName = "path";
    private const string SignatureVersionAttributeName = "signatureVersion";

    /// <summary>
    /// BAS-07 - what a unit written before signature versions existed is read as. The first
    /// generation of every language's signatures is the one that was not stamped.
    /// </summary>
    private const string FirstSignatureVersion = "1";

    internal static string GetPath(string folderRoot)
    {
        return Path.Combine(folderRoot, MagicValues.SignatureFileName);
    }

    /// <summary>
    /// BAS-05 - a missing file is an empty baseline, and a file that exists but cannot be read
    /// fails the run.
    ///
    /// The two are not the same thing. Nothing on disk is a first run and the only honest verdict
    /// is that everything is new; a baseline that is present and unreadable is history the run was
    /// meant to compare against and could not, and continuing past it silently publishes a verdict
    /// with nothing behind it. That used to be a warning in a green run, which is exactly where a
    /// warning goes unread - and a release cannot be recalled once a package manager has it.
    ///
    /// Deleting the baseline is the way through, and the message says so: it costs one release
    /// classified against an empty history, which is the same cost the fallback imposed except
    /// that someone chooses it.
    /// </summary>
    internal static IReadOnlyList<IPackageableUnit> Read(
        string folderRoot,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var path = GetPath(folderRoot);
        if (!File.Exists(path))
        {
            Log.WriteLine($"No baseline at {MagicValues.SignatureFileName}; treating this as a first run");
            return [];
        }

        try
        {
            return ReadDocument(XDocument.Load(path), providers);
        }
        catch (Exception e)
        {
            throw new InvalidDataException(
                $"Unable to read the baseline at {path}, so this run has no history to classify "
                + "against. Fix the file, or delete it to start from an empty baseline - that "
                + "costs one release classified as if every unit were new.",
                e);
        }
    }

    /// <summary>
    /// BAS-06 - written to a temporary file in the same directory and moved into place, so a
    /// half-written baseline can never replace a good one.
    /// </summary>
    internal static void Write(
        string folderRoot,
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var document = BuildDocument(units, providers);
        var path = GetPath(folderRoot);
        var temporaryPath = path + ".tmp";

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "   ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            NewLineChars = "\n"
        };

        using (var writer = XmlWriter.Create(temporaryPath, settings))
        {
            document.Save(writer);
        }

        File.Move(temporaryPath, path, overwrite: true);

        // What went in, not what was offered: UNI-04 units are versioned but hold no signature, so
        // counting them here would describe a file that does not exist.
        var written = document.Root?.Elements(UnitElementName).Count() ?? 0;
        Log.WriteLine($"Wrote baseline {MagicValues.SignatureFileName} ({written} units)");
    }

    /// <summary>Exposed for the round-trip test (TST-M4), which must not touch the disk.</summary>
    internal static XDocument BuildDocument(
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var root = new XElement(
            MagicValues.BaselineRootElementName,
            new XAttribute(
                MagicValues.BaselineFormatVersionAttributeName,
                MagicValues.BaselineFormatVersion));

        foreach (var unit in SortUnits(units))
        {
            // UNI-04. The baseline is signature history, and a unit with no surface has none. It
            // was never extracted, so writing it would persist an empty graph that the next run
            // would read back as "everything in it was removed".
            if (!unit.HasPublicApiSurface)
            {
                continue;
            }

            var provider = LanguageProviders.Find(providers, unit.LanguageId)
                           ?? throw new InvalidOperationException(
                               $"No provider is registered for {unit.LanguageId}");

            root.Add(new XElement(
                UnitElementName,
                new XAttribute(LanguageAttributeName, unit.LanguageId),
                new XAttribute(UnitIdAttributeName, unit.UnitId),
                new XAttribute(UnitKindAttributeName, unit.UnitKind),
                new XAttribute(PathAttributeName, unit.RelativePath),
                new XAttribute(SignatureVersionAttributeName, provider.SignatureVersion),
                provider.WriteSignature(unit)));
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    internal static IReadOnlyList<IPackageableUnit> ReadDocument(
        XDocument document,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var root = document.Root;
        if (root == null || root.Name != MagicValues.BaselineRootElementName)
        {
            throw new InvalidDataException(
                $"The baseline's root element is not <{MagicValues.BaselineRootElementName}>");
        }

        // BAS-03: an unknown or absent format version is unreadable, not something to guess at.
        var formatVersion = root.Attribute(MagicValues.BaselineFormatVersionAttributeName)?.Value;
        if (formatVersion != MagicValues.BaselineFormatVersion)
        {
            throw new InvalidDataException(
                $"Baseline format version '{formatVersion}' is not '{MagicValues.BaselineFormatVersion}'");
        }

        var units = new List<IPackageableUnit>();
        foreach (var unitElement in root.Elements(UnitElementName))
        {
            var unit = ReadUnit(unitElement, providers);
            if (unit == null)
            {
                continue;
            }

            units.Add(unit);
        }

        return units;
    }

    private static IPackageableUnit? ReadUnit(
        XElement unitElement,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var languageId = unitElement.Attribute(LanguageAttributeName)?.Value ?? string.Empty;

        // BAS-05's spirit: a baseline written by a build that had a language this one does not is
        // an ordinary input, not a failure. The unit is skipped, so it is neither compared nor
        // rewritten, and the language it belongs to keeps its own units to itself.
        var provider = LanguageProviders.Find(providers, languageId);
        if (provider == null)
        {
            Log.WriteLine($"Ignoring a baseline unit for unregistered language '{languageId}'");
            return null;
        }

        // BAS-07: a signature written by a generation this provider no longer speaks is not
        // history it can compare against, and reading it as if it were would report the change in
        // wording as a change in API. Dropping the unit re-seeds that unit and nothing else - the
        // languages that did not change keep every bit of their history.
        var signatureVersion = unitElement.Attribute(SignatureVersionAttributeName)?.Value
                               ?? FirstSignatureVersion;
        if (signatureVersion != provider.SignatureVersion)
        {
            Log.WriteLine(
                $"Ignoring the baseline signature for {languageId} unit "
                + $"'{unitElement.Attribute(UnitIdAttributeName)?.Value}': it was written as "
                + $"signature version {signatureVersion} and this run reads "
                + $"{provider.SignatureVersion}. It will be classified as if it were new.");
            return null;
        }

        var payload = unitElement.Elements().FirstOrDefault();
        return new PackageableUnit
        {
            LanguageId = languageId,
            UnitId = unitElement.Attribute(UnitIdAttributeName)?.Value ?? string.Empty,
            DisplayName = unitElement.Attribute(UnitIdAttributeName)?.Value ?? string.Empty,
            UnitKind = unitElement.Attribute(UnitKindAttributeName)?.Value ?? string.Empty,
            RelativePath = unitElement.Attribute(PathAttributeName)?.Value ?? string.Empty,
            Signature = payload == null ? null : provider.ReadSignature(payload)
        };
    }

    /// <summary>BAS-04 - units are written sorted by (Language, UnitId).</summary>
    private static IPackageableUnit[] SortUnits(IReadOnlyList<IPackageableUnit> units)
    {
        var sorted = units.ToArray();
        Array.Sort(sorted, (left, right) => string.CompareOrdinal(
            PackageableUnit.GetSortKey(left),
            PackageableUnit.GetSortKey(right)));
        return sorted;
    }
}
