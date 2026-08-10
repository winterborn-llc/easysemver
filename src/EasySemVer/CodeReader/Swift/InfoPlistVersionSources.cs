using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03/MVR-04 - CFBundleShortVersionString. The Info.plist belongs to the target, which lives
/// beside the .xcodeproj rather than inside it, so this looks at the bundle's parent.
/// </summary>
internal class InfoPlistVersionSources : IDiscoverVersionSources
{
    public string LanguageId => SwiftLanguageProvider.SwiftLanguageId;

    public IReadOnlyList<string> UnitKinds => [SwiftLanguageProvider.XcodeTargetUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        var parent = Path.GetDirectoryName(scope.UnitPath);
        if (parent == null)
        {
            yield break;
        }

        foreach (var path in FolderScanner.FindFiles(parent, "Info.plist"))
        {
            if (!InfoPlistVersionSource.HasShortVersionString(File.ReadAllText(path)))
            {
                continue;
            }

            yield return new InfoPlistVersionSource(
                path,
                FolderScanner.GetRelativePath(scope.FolderRoot, path));
        }
    }
}
