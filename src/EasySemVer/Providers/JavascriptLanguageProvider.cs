using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// JavaScript and TypeScript, whose packages are both marked by a package.json (LNG-01, version-sync).
/// <para>
/// A TypeScript package is an npm package: it is the same manifest, the same `version` key and the
/// same publish. They are one language here because they are one *unit* here - and if a reader is
/// ever written it will read .d.ts, which is what both of them ship.
/// </para>
/// </summary>
internal class JavascriptLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string JavascriptLanguageId = "javascript";

    internal const string JavascriptUnitKind = "npm-package";

    public override string LanguageId => JavascriptLanguageId;

    protected override string UnitKind => JavascriptUnitKind;

    protected override string ManifestFileName => "package.json";
}
