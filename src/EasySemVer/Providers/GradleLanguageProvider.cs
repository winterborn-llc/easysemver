using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Gradle projects, marked by a build.gradle or build.gradle.kts (LNG-01, version-sync). This is
/// how Java, Kotlin and Groovy are versioned when they are not built with Maven.
/// <para>
/// **The id is "gradle" rather than a language**, and that is the honest answer rather than a
/// convenient one: a Gradle module does not say whether it is Java, Kotlin or Groovy, frequently
/// contains more than one of them, and inferring it from <c>src/main/kotlin</c> would be a guess
/// that fails silently on exactly the mixed modules that are hardest to notice. The unit really is
/// a Gradle project, so that is what it is called.
/// </para>
/// <para>
/// The id is also cheap to change later, which is why shipping it did not need to wait on the
/// question being settled: a version-sync unit is absent from the baseline (UNI-04), so no
/// repository stores this string and renaming it costs no re-seed. Only a report consumer matching
/// on it would notice, and this language produces no findings to match.
/// </para>
/// <para>
/// The version is read from a literal <c>version = "1.2.3"</c> at the top level of the build script,
/// and from <c>gradle.properties</c> where the convention puts it instead. A build script that
/// computes its version is left alone (MVR-04) - it is a Groovy or Kotlin program, and SWD-01's
/// reasoning about Package.swift applies to it in full.
/// </para>
/// </summary>
internal class GradleLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string GradleLanguageId = "gradle";

    internal const string GradleUnitKind = "gradle-project";

    public override string LanguageId => GradleLanguageId;

    protected override string UnitKind => GradleUnitKind;

    protected override string ManifestFileName => "build.gradle";

    protected override IReadOnlyList<string> ManifestFileNames => ["build.gradle", "build.gradle.kts"];
}
