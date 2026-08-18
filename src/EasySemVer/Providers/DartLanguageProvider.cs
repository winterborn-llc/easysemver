using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Dart and Flutter packages, marked by a pubspec.yaml (LNG-01, version-sync).
/// <para>
/// Dart versions may carry a +build suffix, which is part of the value pub reads and is replaced with
/// it rather than preserved separately.
/// </para>
/// </summary>
internal class DartLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string DartLanguageId = "dart";

    internal const string DartUnitKind = "pub-package";

    public override string LanguageId => DartLanguageId;

    protected override string UnitKind => DartUnitKind;

    protected override string ManifestFileName => "pubspec.yaml";
}
