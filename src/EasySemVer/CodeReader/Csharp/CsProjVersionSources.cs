using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Csharp;

/// <summary>MVR-03 - the version properties in the project file the unit already is.</summary>
internal class CsProjVersionSources : IDiscoverVersionSources
{
    public string LanguageId => CsharpLanguageProvider.CsharpLanguageId;

    public IReadOnlyList<string> UnitKinds => [CsharpLanguageProvider.CsprojUnitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        yield return new CsProjVersionSource(
            scope.UnitPath,
            FolderScanner.GetRelativePath(scope.FolderRoot, scope.UnitPath));
    }
}
