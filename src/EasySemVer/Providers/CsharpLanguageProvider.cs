using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>Everything C# contributes to a run (ML-02). One .csproj is one unit (UNI-02).</summary>
internal class CsharpLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources) : ILanguageProvider
{
    internal const string CsharpLanguageId = "csharp";

    internal const string CsprojUnitKind = "csproj";

    /// <summary>
    /// C#'s unit-existence rules (§7). Owned here rather than by the core: every rule belongs to
    /// exactly one language, and these two agree with the other languages only because they chose
    /// to subclass rather than override.
    /// </summary>
    private static readonly IEvaluateUnitExistence[] ExistenceRules =
    [
        new UnitRemoved(),
        new UnitAdded()
    ];

    private string _folderRoot = string.Empty;

    public string LanguageId => CsharpLanguageId;

    /// <summary>
    /// FLD-06/FLD-07 - MSBuild's output, vouched for by the project file beside it. These were
    /// global until every language owned its own: `bin` is an ordinary source directory in plenty
    /// of repositories, and excluding it everywhere is the silent-swallow failure `Packages` was
    /// removed for.
    /// <para>
    /// A project using a centralised output path - .NET 8's artifacts layout, or a custom
    /// OutputPath - has a `bin` that is not beside a project file and is therefore walked. That
    /// costs a walk and finds no units, because build output contains no project files; and it
    /// cannot reach extraction, which only ever scans a project's own directory.
    /// </para>
    /// </summary>
    public IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
    [
        DirectoryExclusion.Beside("bin", "*.csproj"),
        DirectoryExclusion.Beside("obj", "*.csproj")
    ];

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        this._folderRoot = folderRoot;
        var units = new List<IPackageableUnit>();
        foreach (var projectFilePath in FolderScanner.FindFiles(folderRoot, "*.csproj"))
        {
            var relativePath = FolderScanner.GetRelativePath(folderRoot, projectFilePath);
            units.Add(new PackageableUnit
            {
                LanguageId = CsharpLanguageId,

                // DSC-05: identity is the filename without extension, so a rename reads as
                // remove + add and a move within the tree does not.
                UnitId = Path.GetFileNameWithoutExtension(projectFilePath),
                DisplayName = Path.GetFileName(projectFilePath),
                RelativePath = relativePath,
                UnitKind = CsprojUnitKind,
                VersionSources = VersionSourceFactories.For(
                    versionSources,
                    CsharpLanguageId,
                    new VersionSourceScope(folderRoot, projectFilePath, CsprojUnitKind))
            });
        }

        return units;
    }

    /// <summary>
    /// UNI-04 - read from the project file, which is cheap enough to open again here rather than
    /// be cached across discovery. The signals are MSBuild's own and live in
    /// <see cref="CsProjTestProject"/>.
    /// </summary>
    public bool IsTestCode(IPackageableUnit unit)
    {
        return CsProjTestProject.Read(this.GetProjectFilePath(unit));
    }

    public void Extract(IPackageableUnit unit)
    {
        unit.Signature = CsharpUnitBuilder.GetProjectSignature(this.GetProjectFilePath(unit));
    }

    public IReadOnlyList<ChangeFinding> Classify(IUnitsToCompare units)
    {
        var findings = new List<ChangeFinding>(
            UnitExistence.GetFindings(CsharpLanguageId, ExistenceRules, units));

        // NCL-03: units are paired before any signature rule runs, so a removed project is
        // reported once as a removal and never again as everything inside it disappearing.
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
        return UnitVersions.Read(unit);
    }

    public IReadOnlyList<string> WriteVersion(IPackageableUnit unit, Version version)
    {
        return UnitVersions.Write(unit, version);
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

    /// <summary>
    /// Units keep their paths root-relative so nothing machine-specific can reach the baseline
    /// (BAS-04); the absolute path is rebuilt only when the disk actually has to be touched.
    /// </summary>
    private string GetProjectFilePath(IPackageableUnit unit)
    {
        return Path.Combine(this._folderRoot, unit.RelativePath);
    }
}
