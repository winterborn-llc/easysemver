using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>CURRENT_PROJECT_VERSION in the project file inside the .xcodeproj.</summary>
internal class BuildCounterVersionSources : IDiscoverVersionSources
{
    public string LanguageId => SwiftLanguageProvider.SwiftLanguageId;

    public IReadOnlyList<string> UnitKinds => [SwiftLanguageProvider.XcodeTargetUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        var path = Path.Combine(scope.UnitPath, MarketingVersionSources.XcodeProjectFileName);

        // MVR-04: a source exists only because the value it wraps already existed on disk, so a
        // project that has never set a counter does not acquire one here.
        if (!File.Exists(path) || !BuildCounterVersionSource.HasLiteralCounter(File.ReadAllText(path)))
        {
            yield break;
        }

        yield return new BuildCounterVersionSource(
            path,
            FolderScanner.GetRelativePath(scope.FolderRoot, path));
    }
}
