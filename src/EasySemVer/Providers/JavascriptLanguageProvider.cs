using Winterborn.Tools.EasySemVer.DataObject;
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

    /// <summary>
    /// FLD-06. `node_modules` is the one name in the old global list that genuinely cannot mean
    /// anything else, so it stays unconditional - it is simply owned by the language a reader would
    /// look under, rather than by a shared list.
    /// </summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
        [DirectoryExclusion.Always("node_modules")];

    public override string LanguageId => JavascriptLanguageId;

    protected override string UnitKind => JavascriptUnitKind;

    protected override string ManifestFileName => "package.json";
}
