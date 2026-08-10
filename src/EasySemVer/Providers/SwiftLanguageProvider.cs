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
/// </summary>
internal class SwiftLanguageProvider(
    IRunProcess runProcess,
    IReadOnlyList<IDiscoverVersionSources> versionSources) : ILanguageProvider
{
    internal const string SwiftLanguageId = "swift";

    internal const string SwiftPackageTargetUnitKind = "swiftpm-target";

    internal const string XcodeTargetUnitKind = "xcode-target";

    private const string PackageManifestFileName = "Package.swift";

    private const string XcodeProjectFileName = "project.pbxproj";

    private string _folderRoot = string.Empty;

    /// <summary>
    /// One build per package produces every one of its targets' graphs, so they are extracted
    /// together and cached rather than rebuilt per unit.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, SwiftModule>> _modulesByPackage = [];

    /// <summary>
    /// UNI-04. Collected while discovery is already reading the manifests and project files, and
    /// keyed by ML-03 unit id so that two packages with a target of the same name stay distinct.
    /// Re-deriving it per unit would mean another `swift package dump-package` each time, which is
    /// a compile.
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
        var units = new List<IPackageableUnit>();
        this.DiscoverXcodeProjects(folderRoot, units);
        foreach (var manifestPath in FolderScanner.FindFiles(folderRoot, PackageManifestFileName))
        {
            var packageDirectory = Path.GetDirectoryName(manifestPath)!;
            var packageRelativePath = FolderScanner.GetRelativePath(folderRoot, packageDirectory);
            var sources = VersionSourceFactories.For(
                versionSources,
                SwiftLanguageId,
                new VersionSourceScope(folderRoot, packageDirectory, SwiftPackageTargetUnitKind));

            // One dump, both answers: which targets are units, and which of those are tests.
            var manifestJson = SwiftPackageManifest.Dump(runProcess, packageDirectory);
            foreach (var testTarget in SwiftPackageManifest.ReadTestTargetNames(manifestJson))
            {
                this._testUnitIds.Add($"{packageRelativePath}:{testTarget}");
            }

            foreach (var targetName in SwiftPackageManifest.ReadTargetNames(manifestJson))
            {
                units.Add(new PackageableUnit
                {
                    LanguageId = SwiftLanguageId,

                    // ML-03/SWD-03: package-relative directory plus target name, so the id is
                    // stable across machines and unique when two packages share a target name.
                    UnitId = $"{packageRelativePath}:{targetName}",
                    DisplayName = targetName,
                    RelativePath = packageRelativePath,
                    UnitKind = SwiftPackageTargetUnitKind,
                    VersionSources = sources
                });
            }
        }

        return units;
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

            // UNI-04. From the project file, because the xcodebuild listing below carries names
            // and nothing else - see XcodeTestTarget for why that is not worth a process per target.
            foreach (var testTarget in XcodeTestTarget.Read(
                         Path.Combine(projectPath, XcodeProjectFileName)))
            {
                this._testUnitIds.Add($"{projectRelativePath}:{testTarget}");
            }

            foreach (var targetName in XcodeProject.GetTargetNames(runProcess, projectPath))
            {
                units.Add(new PackageableUnit
                {
                    LanguageId = SwiftLanguageId,
                    UnitId = $"{projectRelativePath}:{targetName}",
                    DisplayName = targetName,
                    RelativePath = projectRelativePath,
                    UnitKind = XcodeTargetUnitKind,
                    VersionSources = sources
                });
            }
        }
    }

    public void Extract(IPackageableUnit unit)
    {
        if (unit.UnitKind == XcodeTargetUnitKind)
        {
            this.ExtractXcodeTarget(unit);
            return;
        }

        var packageDirectory = Path.Combine(this._folderRoot, unit.RelativePath);
        if (!this._modulesByPackage.TryGetValue(packageDirectory, out var modules))
        {
            modules = new SwiftSymbolGraphExtractor(runProcess)
                .ExtractPackage(packageDirectory, $"{unit.RelativePath} ({PackageManifestFileName})");
            this._modulesByPackage[packageDirectory] = modules;
        }

        if (!modules.TryGetValue(unit.DisplayName, out var module))
        {
            // SWE-05: a discovered target with no graph is a failure, not something to skip. A
            // test-only target that compiles to nothing public still emits a graph.
            throw new InvalidOperationException(
                $"Swift extraction produced no symbol graph for target '{unit.DisplayName}' "
                + $"in {unit.RelativePath}. Modules found: "
                + $"{(modules.Count < 1 ? "none" : string.Join(", ", modules.Keys.Order()))}.");
        }

        unit.Signature = module;
    }

    /// <summary>
    /// §20 O-06 - a discovered Xcode target that turns out to be pure Objective-C has no Swift
    /// symbol graph at all. Rather than failing the run per SWE-05, it is recorded as an empty
    /// module and logged loudly: it still carries versions, and it is still a unit whose
    /// disappearance is a real change.
    /// </summary>
    private void ExtractXcodeTarget(IPackageableUnit unit)
    {
        var projectPath = Path.Combine(this._folderRoot, unit.RelativePath);
        var modules = new XcodeSymbolGraphExtractor(runProcess)
            .ExtractTarget(projectPath, unit.DisplayName, unit.UnitId);

        if (modules.TryGetValue(unit.DisplayName, out var module))
        {
            unit.Signature = module;
            return;
        }

        Log.WriteLine(
            $"Xcode target '{unit.DisplayName}' in {unit.RelativePath} produced no Swift symbol "
            + "graph; treating it as version-sync-only. If it does contain Swift, the build "
            + "settings are not emitting symbol graphs.");
        unit.Signature = new SwiftModule(unit.DisplayName);
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
