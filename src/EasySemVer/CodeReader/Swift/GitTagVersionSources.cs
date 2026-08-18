using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - for a SwiftPM package a git tag is one version location among several; for a Go module
/// it is the only one there is. Scoped to the run rather than to the unit, because the tag list is a
/// property of the checkout and not of any package inside it.
/// <para>
/// Reading is unconditional. Writing follows <c>--tag</c> (TAG-01) and creates a local tag only.
/// </para>
/// </summary>
internal class GitTagVersionSources(
    IRunProcess runProcess,
    bool writesGitTag,
    string languageId,
    string unitKind) : IDiscoverVersionSources
{
    // Registered once per language rather than claiming several, because VersionSourceFactories.For
    // matches on language before unit kind: a factory claiming "swift" is never offered a Go unit,
    // however many kinds it lists.
    public string LanguageId => languageId;

    public IReadOnlyList<string> UnitKinds => [unitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        yield return new GitTagVersionSource(runProcess, scope.FolderRoot, writesGitTag);
    }
}
