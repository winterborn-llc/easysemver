using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Everything Swift contributes to a run (ML-02). A unit is a target, not a product and not a
/// package (D-05, UNI-03).
/// <para>
/// Nothing here runs a process. Targets come from the text of Package.swift and project.pbxproj,
/// and signatures come from the Swift files themselves - so a versioning run needs neither a Swift
/// toolchain nor Xcode, and cannot fail because a dependency could not be resolved.
/// </para>
/// </summary>
internal class SwiftLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources) : ILanguageProvider
{
    internal const string SwiftLanguageId = "swift";

    internal const string SwiftPackageTargetUnitKind = "swiftpm-target";

    internal const string XcodeTargetUnitKind = "xcode-target";

    private const string PackageManifestFileName = "Package.swift";

    private string _folderRoot = string.Empty;

    /// <summary>
    /// The Swift files behind each unit, worked out while discovery was already reading the
    /// manifests and project files. Re-deriving them per unit would mean parsing the same
    /// project.pbxproj once per target it declares.
    /// </summary>
    private readonly Dictionary<string, IReadOnlyList<string>> _sourceFilesByUnitId = [];

    /// <summary>
    /// UNI-04, keyed by ML-03 unit id so that two packages with a target of the same name stay
    /// distinct.
    /// </summary>
    private readonly HashSet<string> _testUnitIds = [];

    /// <summary>
    /// Swift's unit-existence rules (§7), owned here because every rule belongs to exactly one
    /// language. They subclass the shared bases rather than overriding them, so a target
    /// disappearing means today what it means everywhere else - but a target is not a package, and
    /// this is where that would be said if it ever stops being true.
    /// </summary>
    private static readonly IEvaluateUnitExistence[] ExistenceRules =
    [
        new UnitRemoved(),
        new UnitAdded()
    ];

    public string LanguageId => SwiftLanguageId;

    /// <summary>
    /// BAS-07. Generation 1 was the toolchain's symbol graph. It described the same API in
    /// different words: the graph qualified every type it did not have to resolve, so a superclass
    /// was "ObjectiveC.NSObject" where the source says "NSObject", and an extension of
    /// "Swift.String" is written as an extension of "String". Diffing generation 1 against
    /// generation 2 would report every one of those as an API change - a Major release for a
    /// change in wording nobody made. Swift units re-seed once instead.
    /// </summary>
    public string SignatureVersion => "2";

    /// <summary>
    /// UNI-04 - answered from what discovery read. A target that was never discovered is not test
    /// code as far as this provider knows, which is the same answer it gave before the question
    /// existed.
    /// </summary>
    public bool IsTestCode(IPackageableUnit unit)
    {
        return this._testUnitIds.Contains(unit.UnitId);
    }

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        this._folderRoot = folderRoot;
        this._testUnitIds.Clear();
        this._sourceFilesByUnitId.Clear();

        var units = new List<IPackageableUnit>();
        this.DiscoverXcodeProjects(folderRoot, units);
        this.DiscoverSwiftPackages(folderRoot, units);
        return units;
    }

    /// <summary>SWD-01 - one unit per SwiftPM target, read from the manifest's text.</summary>
    private void DiscoverSwiftPackages(string folderRoot, List<IPackageableUnit> units)
    {
        foreach (var manifestPath in FolderScanner.FindFiles(folderRoot, PackageManifestFileName))
        {
            var packageDirectory = Path.GetDirectoryName(manifestPath)!;
            var packageRelativePath = FolderScanner.GetRelativePath(folderRoot, packageDirectory);
            var sources = VersionSourceFactories.For(
                versionSources,
                SwiftLanguageId,
                new VersionSourceScope(folderRoot, packageDirectory, SwiftPackageTargetUnitKind));

            foreach (var target in SwiftPackageManifest.Read(File.ReadAllText(manifestPath)))
            {
                // ML-03/SWD-03: package-relative directory plus target name, so the id is stable
                // across machines and unique when two packages share a target name.
                var unitId = $"{packageRelativePath}:{target.Name}";
                if (target.IsTest)
                {
                    this._testUnitIds.Add(unitId);
                }

                this._sourceFilesByUnitId[unitId] =
                    SwiftPackageSources.Find(packageDirectory, target);

                units.Add(new PackageableUnit
                {
                    LanguageId = SwiftLanguageId,
                    UnitId = unitId,
                    DisplayName = target.Name,
                    RelativePath = packageRelativePath,
                    UnitKind = SwiftPackageTargetUnitKind,
                    VersionSources = sources
                });
            }
        }
    }

    /// <summary>SWD-02/SWD-03 - one unit per Xcode target, identified by project path plus name.</summary>
    private void DiscoverXcodeProjects(string folderRoot, List<IPackageableUnit> units)
    {
        foreach (var projectPath in FolderScanner.FindDirectories(folderRoot, "*.xcodeproj"))
        {
            var projectRelativePath = FolderScanner.GetRelativePath(folderRoot, projectPath);
            var sources = VersionSourceFactories.For(
                versionSources,
                SwiftLanguageId,
                new VersionSourceScope(folderRoot, projectPath, XcodeTargetUnitKind));

            foreach (var target in XcodeProject.Read(projectPath))
            {
                var unitId = $"{projectRelativePath}:{target.Name}";
                if (target.IsTest)
                {
                    this._testUnitIds.Add(unitId);
                }

                this._sourceFilesByUnitId[unitId] = target.SourceFiles;

                units.Add(new PackageableUnit
                {
                    LanguageId = SwiftLanguageId,
                    UnitId = unitId,
                    DisplayName = target.Name,
                    RelativePath = projectRelativePath,
                    UnitKind = XcodeTargetUnitKind,
                    VersionSources = sources
                });
            }
        }
    }

    /// <summary>
    /// SWE-01. A target with no Swift in it at all - an Objective-C or C target, which both
    /// SwiftPM and Xcode allow - is recorded as an empty module rather than failing the run: it
    /// still carries versions, and it is still a unit whose disappearance is a real change (O-06).
    /// </summary>
    public void Extract(IPackageableUnit unit)
    {
        var files = this._sourceFilesByUnitId.GetValueOrDefault(unit.UnitId, []);
        if (files.Count < 1)
        {
            Log.WriteLine(
                $"Swift target '{unit.DisplayName}' in {unit.RelativePath} has no Swift source; "
                + "treating it as version-sync-only.");
            unit.Signature = new SwiftModule(unit.DisplayName);
            return;
        }

        var texts = new List<string>();
        foreach (var file in files)
        {
            texts.Add(File.ReadAllText(file));
        }

        unit.Signature = SwiftSourceReader.Read(unit.DisplayName, texts);
    }

    public IReadOnlyList<ChangeFinding> Classify(IUnitsToCompare units)
    {
        var findings = new List<ChangeFinding>(
            UnitExistence.GetFindings(SwiftLanguageId, ExistenceRules, units));

        // NCL-03: units are paired before any signature rule runs, so a removed target is
        // reported once as a removal and never again as everything inside it disappearing.
        foreach (var pair in UnitPairing.GetUnitsInBoth(units.Older, units.Newer))
        {
            findings.AddRange(CompareSwiftSignatures.GetFindings(
                pair.Newer,
                pair.Older.Signature as SwiftModule,
                pair.Newer.Signature as SwiftModule));
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
        var module = unit.Signature as SwiftModule ?? new SwiftModule(unit.DisplayName);
        module.SortForPersistence();
        return module.SerializeToElement();
    }

    public object ReadSignature(XElement element)
    {
        return element.DeserializeElement<SwiftModule>();
    }
}
