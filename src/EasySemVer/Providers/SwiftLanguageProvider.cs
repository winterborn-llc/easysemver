using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Everything Swift contributes to a run (ML-02). A unit is a target, not a product and not a
/// package (D-05, UNI-03).
/// </summary>
internal class SwiftLanguageProvider(IRunProcess runProcess) : ILanguageProvider
{
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

    public Language Language => Language.Swift;

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        this._folderRoot = folderRoot;
        var units = new List<IPackageableUnit>();
        this.DiscoverXcodeProjects(folderRoot, units);
        foreach (var manifestPath in FolderScanner.FindFiles(folderRoot, PackageManifestFileName))
        {
            var packageDirectory = Path.GetDirectoryName(manifestPath)!;
            var packageRelativePath = FolderScanner.GetRelativePath(folderRoot, packageDirectory);
            var versionSources = this.GetVersionSources(folderRoot, packageDirectory);

            foreach (var targetName in SwiftPackageManifest.GetTargetNames(runProcess, packageDirectory))
            {
                units.Add(new PackageableUnit
                {
                    Language = Language.Swift,

                    // ML-03/SWD-03: package-relative directory plus target name, so the id is
                    // stable across machines and unique when two packages share a target name.
                    UnitId = $"{packageRelativePath}:{targetName}",
                    DisplayName = targetName,
                    RelativePath = packageRelativePath,
                    UnitKind = SwiftPackageTargetUnitKind,
                    VersionSources = versionSources
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
            var versionSources = GetXcodeVersionSources(folderRoot, projectPath);

            foreach (var targetName in XcodeProject.GetTargetNames(runProcess, projectPath))
            {
                units.Add(new PackageableUnit
                {
                    Language = Language.Swift,
                    UnitId = $"{projectRelativePath}:{targetName}",
                    DisplayName = targetName,
                    RelativePath = projectRelativePath,
                    UnitKind = XcodeTargetUnitKind,
                    VersionSources = versionSources
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

    /// <summary>MVR-03/MVR-04 - MARKETING_VERSION and CFBundleShortVersionString, where they exist.</summary>
    private static IVersionSource[] GetXcodeVersionSources(string folderRoot, string projectPath)
    {
        var sources = new List<IVersionSource>();

        var pbxprojPath = Path.Combine(projectPath, XcodeProjectFileName);
        if (File.Exists(pbxprojPath)
            && MarketingVersionSource.HasLiteralVersion(File.ReadAllText(pbxprojPath)))
        {
            sources.Add(new MarketingVersionSource(
                pbxprojPath,
                FolderScanner.GetRelativePath(folderRoot, pbxprojPath)));
        }

        // The Info.plist belongs to the target, which lives beside the project, not inside it.
        var projectParent = Path.GetDirectoryName(projectPath);
        if (projectParent == null)
        {
            return sources.ToArray();
        }

        foreach (var plistPath in FolderScanner.FindFiles(projectParent, "Info.plist"))
        {
            if (!InfoPlistVersionSource.HasShortVersionString(File.ReadAllText(plistPath)))
            {
                continue;
            }

            sources.Add(new InfoPlistVersionSource(
                plistPath,
                FolderScanner.GetRelativePath(folderRoot, plistPath)));
        }

        return sources.ToArray();
    }

    public IReadOnlyList<ChangeFinding> Classify(IPackageableUnit? older, IPackageableUnit newer)
    {
        return CompareSwiftSignatures.GetFindings(
            newer,
            older?.Signature as SwiftModule,
            newer.Signature as SwiftModule);
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

    /// <summary>
    /// MVR-03/MVR-04 - a source exists only because the value it wraps already exists on disk.
    /// Every target of a package shares the package's version locations.
    /// </summary>
    private IVersionSource[] GetVersionSources(string folderRoot, string packageDirectory)
    {
        var sources = new List<IVersionSource> { new GitTagVersionSource(runProcess, folderRoot) };

        foreach (var podspecPath in FolderScanner.FindFiles(packageDirectory, "*.podspec"))
        {
            if (!PodspecVersionSource.HasLiteralVersion(File.ReadAllText(podspecPath)))
            {
                continue;
            }

            sources.Add(new PodspecVersionSource(
                podspecPath,
                FolderScanner.GetRelativePath(folderRoot, podspecPath)));
        }

        foreach (var swiftPath in FolderScanner.FindFiles(packageDirectory, "*Version.swift"))
        {
            if (!SwiftVersionFileSource.HasVersionConstant(File.ReadAllText(swiftPath)))
            {
                continue;
            }

            sources.Add(new SwiftVersionFileSource(
                swiftPath,
                FolderScanner.GetRelativePath(folderRoot, swiftPath)));
        }

        return sources.ToArray();
    }
}
