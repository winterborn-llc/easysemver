using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Go modules, marked by a go.mod (LNG-01, version-sync).
/// <para>
/// Go is the one ecosystem here with **nothing writable in its manifest**. A go.mod names the module
/// and its dependencies and never its own version, because a Go module's version *is* its git tag -
/// `go get` resolves against the tag and nothing else. So Go was left unlisted until tag writing was
/// decided, on the grounds that a provider which discovers modules and changes nothing promises more
/// than it delivers.
/// </para>
/// <para>
/// With TAG-01 confirmed it has exactly one version location, the repository's tags, and it is
/// opt-in: `--tag` creates a local `v&lt;version&gt;` and never pushes. Without that flag a Go module
/// is still discovered and still seeds the run from its highest existing tag - it simply has nowhere
/// to write, which MVR-04 already treats as an ordinary outcome.
/// </para>
/// </summary>
internal class GoLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string GoLanguageId = "go";

    internal const string GoUnitKind = "go-module";

    /// <summary>
    /// FLD-06. `vendor` is Go's committed dependency tree - real source, first-party-looking, and
    /// emphatically not this repository's API. It is also an ordinary directory name outside Go,
    /// which is why the go.mod beside it has to vouch for it.
    /// </summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
        [DirectoryExclusion.Beside("vendor", "go.mod")];

    public override string LanguageId => GoLanguageId;

    protected override string UnitKind => GoUnitKind;

    protected override string ManifestFileName => "go.mod";
}
