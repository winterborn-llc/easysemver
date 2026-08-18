using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// The version-sync tier (LNG-01): a language whose packages are discovered, seeded and stamped,
/// but whose public API is never read and which therefore never votes on the change type.
/// <para>
/// It exists because a hand-rolled reader that under-reports public surface silently turns a
/// breaking change into a Patch, on a run nobody is watching (LNG-02). Version-sync is visibly
/// incomplete - it says so per unit in the log - where a bad reader is invisibly wrong, and this
/// tool's whole job is to be trusted with a number nobody checks.
/// </para>
/// <para>
/// A subclass declares four things: its language id, its unit kind, the manifest that marks a
/// package, and its own unit-existence rules. Everything below that is mechanical and shared,
/// including the read/write loops that were already identical in all three Full-tier providers.
/// </para>
/// </summary>
internal abstract class ManifestLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources) : ILanguageProvider
{
    public abstract string LanguageId { get; }

    /// <summary>The <see cref="IPackageableUnit.UnitKind"/> this provider's units carry.</summary>
    protected abstract string UnitKind { get; }

    /// <summary>
    /// The file whose presence marks a package - package.json, Cargo.toml, pubspec.yaml. May be a
    /// pattern (<c>*.gemspec</c>). One unit per <em>directory</em> holding one, not per file.
    /// </summary>
    protected abstract string ManifestFileName { get; }

    /// <summary>
    /// Every name that marks a package of this language, for the ecosystems that have more than
    /// one. Perl is the reason: a distribution is marked by a Makefile.PL, a Build.PL or a dist.ini
    /// depending on which decade and which toolchain built it, and a distribution carrying two of
    /// them is one package, not two.
    /// </summary>
    protected virtual IReadOnlyList<string> ManifestFileNames => [this.ManifestFileName];

    /// <summary>
    /// Empty, and deliberately not abstract. A version-sync language's units are dropped by
    /// ChangeClassifier before any provider is asked, so a rule registered here could never fire -
    /// requiring each language to write two unreachable classes would be ceremony that reads like
    /// coverage. A language graduating to the Full tier overrides this with its own pair, per
    /// ML-04, at the same moment it starts extracting.
    /// </summary>
    protected virtual IReadOnlyList<IEvaluateUnitExistence> ExistenceRules => [];

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        var units = new List<IPackageableUnit>();

        // One unit per directory, not per manifest file. A Perl distribution with both a
        // Makefile.PL and a dist.ini is one package; discovering it twice would version it twice
        // and read as two units appearing the first time anyone upgraded.
        var claimed = new HashSet<string>(StringComparer.Ordinal);

        foreach (var manifestPath in this.FindManifests(folderRoot))
        {
            var directory = Path.GetDirectoryName(manifestPath)!;
            if (!claimed.Add(directory))
            {
                continue;
            }

            var relativePath = FolderScanner.GetRelativePath(folderRoot, manifestPath);

            units.Add(new PackageableUnit
            {
                LanguageId = this.LanguageId,

                // ML-03: the manifest's folder-root-relative directory, so the id is stable across
                // machines and two packages of the same name in different folders stay distinct.
                // The root itself is "." rather than an empty string, which would sort oddly and
                // read as missing in the log.
                UnitId = NormaliseUnitId(FolderScanner.GetRelativePath(folderRoot, directory)),
                DisplayName = Path.GetFileName(directory),
                RelativePath = relativePath,
                UnitKind = this.UnitKind,

                // LNG-01/LNG-04. Declared here, once, rather than by calling this language's
                // production code test code to get the same effect.
                HasPublicApiSurface = false,
                VersionSources = VersionSourceFactories.For(
                    versionSources,
                    this.LanguageId,
                    new VersionSourceScope(folderRoot, manifestPath, this.UnitKind))
            });
        }

        return units;
    }

    /// <summary>
    /// Every manifest under the root, in declared name order then path order, so that a directory
    /// holding several is always claimed by the same one on every machine (BAS-04).
    /// </summary>
    private IEnumerable<string> FindManifests(string folderRoot)
    {
        foreach (var name in this.ManifestFileNames)
        {
            foreach (var path in FolderScanner.FindFiles(folderRoot, name))
            {
                yield return path;
            }
        }
    }

    private static string NormaliseUnitId(string relativeDirectory)
    {
        return relativeDirectory is "" or "." ? "." : relativeDirectory;
    }

    /// <summary>
    /// UNI-04. Declared rather than inherited so <c>TestLanguageSeam</c> can see the question was
    /// answered, and false because a version-sync unit already has no surface for test code to be
    /// excluded from - <see cref="Discover"/> said so.
    /// </summary>
    public virtual bool IsTestCode(IPackageableUnit unit)
    {
        return false;
    }

    /// <summary>
    /// Never called: <see cref="VersioningRun"/> skips extraction for a unit with no API surface.
    /// It throws rather than returning quietly so that a subclass which one day sets
    /// <c>HasPublicApiSurface</c> true - the first step of moving to the Full tier - finds out here
    /// instead of writing empty signatures into everyone's baseline.
    /// </summary>
    public void Extract(IPackageableUnit unit)
    {
        throw new NotSupportedException(
            $"{this.LanguageId} is a version-sync language (LNG-01) and has no signature to extract. "
            + $"Unit '{unit.UnitId}' should have been skipped by UNI-04.");
    }

    /// <summary>
    /// Always empty, and not because it is unimplemented. A version-sync language's units never
    /// reach here at all - ChangeClassifier drops units with no API surface before any provider is
    /// asked, so even the existence rules never see them (UNI-04). The rules are still owned and
    /// registered, because moving to the Full tier means flipping one flag and they have to be
    /// waiting when it happens.
    /// </summary>
    public IReadOnlyList<ChangeFinding> Classify(IUnitsToCompare units)
    {
        return [.. UnitExistence.GetFindings(this.LanguageId, this.ExistenceRules, units)];
    }

    public IReadOnlyList<Version> ReadVersions(IPackageableUnit unit)
    {
        var versions = new List<Version>();
        foreach (var source in unit.VersionSources)
        {
            var version = source.Read();
            if (version == null)
            {
                continue;
            }

            versions.Add(version);
        }

        return versions;
    }

    public IReadOnlyList<string> WriteVersion(IPackageableUnit unit, Version version)
    {
        var written = new List<string>();
        foreach (var source in unit.VersionSources)
        {
            if (!source.IsWritable)
            {
                continue;
            }

            source.Write(version);
            Log.WriteLine($"Wrote {version} to {source.Location}");
            written.Add(source.Location);
        }

        return written;
    }

    /// <summary>
    /// BAS-06 - a version-sync unit is absent from the baseline, because an entry with no signature
    /// reads back on the next run as "everything in it was removed" (UNI-04). Persistence already
    /// skips these, so neither of these is reached.
    /// </summary>
    public XElement WriteSignature(IPackageableUnit unit)
    {
        throw new NotSupportedException(
            $"{this.LanguageId} is a version-sync language (LNG-01) and writes no baseline entry.");
    }

    public object ReadSignature(XElement element)
    {
        throw new NotSupportedException(
            $"{this.LanguageId} is a version-sync language (LNG-01) and has no baseline entry to read.");
    }
}
