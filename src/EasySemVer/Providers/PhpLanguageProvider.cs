using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// PHP packages, marked by a composer.json (LNG-01, version-sync).
/// <para>
/// Most composer.json files deliberately carry no version: Packagist derives it from git tags, and
/// Composer's own docs advise against declaring one. Those packages are discovered and contribute
/// nothing, which is MVR-04 working - the tool never adds a version key a team chose not to have.
/// </para>
/// </summary>
internal class PhpLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string PhpLanguageId = "php";

    internal const string PhpUnitKind = "composer-package";

    public override string LanguageId => PhpLanguageId;

    protected override string UnitKind => PhpUnitKind;

    protected override string ManifestFileName => "composer.json";
}
