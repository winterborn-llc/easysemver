using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Rust crates, marked by a Cargo.toml (LNG-01, version-sync).
/// <para>
/// A workspace member is a unit like any other: each has its own Cargo.toml and its own version, and
/// the [package] table is what MVR-04 probes for. A virtual workspace root, which has no [package],
/// yields no version source and is stamped nowhere.
/// </para>
/// </summary>
internal class RustLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string RustLanguageId = "rust";

    internal const string RustUnitKind = "cargo-package";

    /// <summary>
    /// FLD-06. `target` is Cargo's build directory, and it holds the full source of every
    /// dependency under `target/package`. It is also the most dangerous name to exclude globally -
    /// plenty of projects have a `target` that is theirs - so the Cargo.toml has to vouch for it.
    /// </summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
        [DirectoryExclusion.Beside("target", "Cargo.toml")];

    public override string LanguageId => RustLanguageId;

    protected override string UnitKind => RustUnitKind;

    protected override string ManifestFileName => "Cargo.toml";
}
