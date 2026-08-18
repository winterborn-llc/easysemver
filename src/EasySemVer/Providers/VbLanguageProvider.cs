using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.CodeReader.Vb;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Vb;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces;
using UnitAdded = Winterborn.Tools.EasySemVer.Evaluators.Vb.UnitAdded;
using UnitRemoved = Winterborn.Tools.EasySemVer.Evaluators.Vb.UnitRemoved;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Everything VB.NET contributes to a run (ML-02). One .vbproj is one unit, on the same terms as
/// UNI-02's .csproj.
/// <para>
/// **VB-01 — VB reuses C#'s signature model and rules.** VB and C# compile to one metadata format;
/// Roslyn produces the same <c>INamedTypeSymbol</c> graph from both, and they break compatibility
/// in the same ways. So this provider builds a <see cref="CsharpProject"/> and classifies with
/// <see cref="CompareSignatures"/> - all forty-one C# rules, unmodified. The only VB-specific code
/// in the tool is a parse front end (<see cref="VbUnitBuilder"/>) and this file.
/// </para>
/// <para>
/// The cost is named rather than hidden: VB signatures live in types spelled <c>Csharp*</c> and are
/// persisted in <c>&lt;CsharpProject&gt;</c> elements, which is exactly the "one language described
/// in another's vocabulary" that ML-01 forbids. It is allowed here, and only here, because the
/// vocabulary is really the CLR's rather than C#'s. A language that does not compile to that
/// metadata gets its own topology, no matter how similar it looks.
/// </para>
/// <para>
/// What is <em>not</em> shared is anything keyed by language: VB owns its own unit-existence rules
/// (ML-04), its own language id, its own unit kind, and its own version-source registration.
/// </para>
/// </summary>
internal class VbLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources) : ILanguageProvider
{
    internal const string VbLanguageId = "vb";

    internal const string VbprojUnitKind = "vbproj";

    /// <inheritdoc cref="CsharpLanguageProvider"/>
    private static readonly IEvaluateUnitExistence[] ExistenceRules =
    [
        new UnitRemoved(),
        new UnitAdded()
    ];

    private string _folderRoot = string.Empty;

    public string LanguageId => VbLanguageId;

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        this._folderRoot = folderRoot;
        var units = new List<IPackageableUnit>();
        foreach (var projectFilePath in FolderScanner.FindFiles(folderRoot, "*.vbproj"))
        {
            var relativePath = FolderScanner.GetRelativePath(folderRoot, projectFilePath);
            units.Add(new PackageableUnit
            {
                LanguageId = VbLanguageId,

                // DSC-05, as for C#. A Foo.vbproj and a Foo.csproj in one tree are distinct units
                // because identity is (language, unit id) - the shared name is not a collision.
                UnitId = Path.GetFileNameWithoutExtension(projectFilePath),
                DisplayName = Path.GetFileName(projectFilePath),
                RelativePath = relativePath,
                UnitKind = VbprojUnitKind,
                VersionSources = VersionSourceFactories.For(
                    versionSources,
                    VbLanguageId,
                    new VersionSourceScope(folderRoot, projectFilePath, VbprojUnitKind))
            });
        }

        return units;
    }

    /// <summary>
    /// UNI-04. A .vbproj is MSBuild, so the signals are the same ones
    /// <see cref="CsProjTestProject"/> already reads - an explicit <c>IsTestProject</c>, or a
    /// reference to Microsoft.NET.Test.Sdk, xunit, NUnit or MSTest. Declared here rather than
    /// inherited so that <c>TestLanguageSeam</c> can see VB answered the question (UNI-04).
    /// </summary>
    public bool IsTestCode(IPackageableUnit unit)
    {
        return CsProjTestProject.Read(this.GetProjectFilePath(unit));
    }

    public void Extract(IPackageableUnit unit)
    {
        unit.Signature = VbUnitBuilder.GetProjectSignature(this.GetProjectFilePath(unit));
    }

    public IReadOnlyList<ChangeFinding> Classify(IUnitsToCompare units)
    {
        var findings = new List<ChangeFinding>(
            UnitExistence.GetFindings(VbLanguageId, ExistenceRules, units));

        // NCL-03, and the payoff of VB-01: these are the C# rules, and they are correct for VB
        // because both sides of the comparison are the same metadata model.
        foreach (var pair in UnitPairing.GetUnitsInBoth(units.Older, units.Newer))
        {
            findings.AddRange(CompareSignatures.GetFindings(
                pair.Newer,
                pair.Older.Signature as CsharpProject,
                pair.Newer.Signature as CsharpProject));
        }

        return findings;
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

    public XElement WriteSignature(IPackageableUnit unit)
    {
        var project = unit.Signature as CsharpProject ?? new CsharpProject(unit.UnitId);
        project.SortForPersistence();
        return project.SerializeToElement();
    }

    public object ReadSignature(XElement element)
    {
        return element.DeserializeElement<CsharpProject>();
    }

    private string GetProjectFilePath(IPackageableUnit unit)
    {
        return Path.Combine(this._folderRoot, unit.RelativePath);
    }
}
