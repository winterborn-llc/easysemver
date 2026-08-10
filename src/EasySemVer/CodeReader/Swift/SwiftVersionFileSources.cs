using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>MVR-03/MVR-04 - a version constant declared in Swift source.</summary>
internal class SwiftVersionFileSources : IDiscoverVersionSources
{
    public string LanguageId => SwiftLanguageProvider.SwiftLanguageId;

    public IReadOnlyList<string> UnitKinds => [SwiftLanguageProvider.SwiftPackageTargetUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        foreach (var path in FolderScanner.FindFiles(scope.UnitPath, "*Version.swift"))
        {
            if (!SwiftVersionFileSource.HasVersionConstant(File.ReadAllText(path)))
            {
                continue;
            }

            yield return new SwiftVersionFileSource(
                path,
                FolderScanner.GetRelativePath(scope.FolderRoot, path));
        }
    }
}
