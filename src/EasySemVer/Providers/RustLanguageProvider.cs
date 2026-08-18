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

    public override string LanguageId => RustLanguageId;

    protected override string UnitKind => RustUnitKind;

    protected override string ManifestFileName => "Cargo.toml";
}
