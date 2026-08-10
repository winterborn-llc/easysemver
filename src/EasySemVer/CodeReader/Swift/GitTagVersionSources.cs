using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - a SwiftPM package's released version is a git tag. Scoped to the run rather than to
/// the unit, and read-only: EasySemVer seeds from a tag but never writes one.
/// </summary>
internal class GitTagVersionSources(IRunProcess runProcess) : IDiscoverVersionSources
{
    public string LanguageId => SwiftLanguageProvider.SwiftLanguageId;

    public IReadOnlyList<string> UnitKinds => [SwiftLanguageProvider.SwiftPackageTargetUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        yield return new GitTagVersionSource(runProcess, scope.FolderRoot);
    }
}
