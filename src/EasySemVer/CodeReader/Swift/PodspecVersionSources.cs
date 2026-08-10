using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>MVR-03/MVR-04 - a literal version in a podspec beside the package manifest.</summary>
internal class PodspecVersionSources : IDiscoverVersionSources
{
    public string LanguageId => SwiftLanguageProvider.SwiftLanguageId;

    public IReadOnlyList<string> UnitKinds => [SwiftLanguageProvider.SwiftPackageTargetUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        foreach (var path in FolderScanner.FindFiles(scope.UnitPath, "*.podspec"))
        {
            if (!PodspecVersionSource.HasLiteralVersion(File.ReadAllText(path)))
            {
                continue;
            }

            yield return new PodspecVersionSource(
                path,
                FolderScanner.GetRelativePath(scope.FolderRoot, path));
        }
    }
}
