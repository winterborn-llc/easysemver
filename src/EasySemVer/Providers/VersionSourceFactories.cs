using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.CodeReader.Manifests;
using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.CodeReader.Vb;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// The registration point for version conventions, and the only file a new one has to touch
/// outside its own class - the same bargain <see cref="LanguageProviders"/> makes for languages.
/// <para>
/// This exists because the alternative was a private GetVersionSources on every provider, each
/// with its own hand-rolled glob-probe-add loop; there were three of them for two languages, and
/// C# had no loop at all, so teaching it a second convention meant restructuring the provider
/// rather than adding a class.
/// </para>
/// </summary>
internal static class VersionSourceFactories
{
    internal static IReadOnlyList<IDiscoverVersionSources> Create(
        IRunProcess runProcess,
        bool writesGitTag = false)
    {
        return
        [
            new CsProjVersionSources(),
            new VbProjVersionSources(),
            new GitTagVersionSources(
                runProcess,
                writesGitTag,
                SwiftLanguageProvider.SwiftLanguageId,
                SwiftLanguageProvider.SwiftPackageTargetUnitKind),
            new PodspecVersionSources(),
            new SwiftVersionFileSources(),
            new MarketingVersionSources(),
            new BuildCounterVersionSources(),
            new InfoPlistVersionSources(),

            // The version-sync ecosystems (LNG-01). One line each, because the convention they
            // share - a literal version in the package's own manifest - differs only in the
            // pattern that finds it. Maven is the exception and is read as XML, because a pom
            // mentions <version> for its parent and every dependency too.
            new ManifestVersionSources(
                JavascriptLanguageProvider.JavascriptLanguageId,
                JavascriptLanguageProvider.JavascriptUnitKind,
                "package.json",
                ManifestPatterns.Json()),
            new ManifestVersionSources(
                RustLanguageProvider.RustLanguageId,
                RustLanguageProvider.RustUnitKind,
                "cargo",
                ManifestPatterns.Toml()),
            new ManifestVersionSources(
                PythonLanguageProvider.PythonLanguageId,
                PythonLanguageProvider.PythonUnitKind,
                "pyproject",
                ManifestPatterns.Toml()),
            new ManifestVersionSources(
                DartLanguageProvider.DartLanguageId,
                DartLanguageProvider.DartUnitKind,
                "pubspec",
                ManifestPatterns.Yaml()),
            new ManifestVersionSources(
                PhpLanguageProvider.PhpLanguageId,
                PhpLanguageProvider.PhpUnitKind,
                "composer.json",
                ManifestPatterns.Json()),
            new PomVersionSources(),
            new ManifestVersionSources(
                CppLanguageProvider.CppLanguageId,
                CppLanguageProvider.CppUnitKind,
                "cmake",
                ManifestPatterns.CMakeProject()),

            // Ruby and Perl keep the number beside the manifest rather than in it, so each
            // registers both: the literal where one exists, and the constant it points at.
            new ManifestVersionSources(
                RubyLanguageProvider.RubyLanguageId,
                RubyLanguageProvider.RubyUnitKind,
                "gemspec",
                ManifestPatterns.GemspecLiteral()),
            new NearbyVersionSources(
                RubyLanguageProvider.RubyLanguageId,
                RubyLanguageProvider.RubyUnitKind,
                "version.rb",
                "version.rb",
                ManifestPatterns.RubyConstant()),
            new ManifestVersionSources(
                PerlLanguageProvider.PerlLanguageId,
                PerlLanguageProvider.PerlUnitKind,
                "dist.ini",
                ManifestPatterns.Properties()),
            new NearbyVersionSources(
                PerlLanguageProvider.PerlLanguageId,
                PerlLanguageProvider.PerlUnitKind,
                "pm",
                "*.pm",
                ManifestPatterns.PerlVariable()),

            // Gradle keeps its version in the build script or in gradle.properties beside it, and
            // plenty of builds have both. Writing both is MVR-05 working: every module in a folder
            // root gets the same version anyway (ML-06), so a properties file claimed by a parent
            // and by its own module is written the same value twice.
            new ManifestVersionSources(
                GradleLanguageProvider.GradleLanguageId,
                GradleLanguageProvider.GradleUnitKind,
                "gradle",
                ManifestPatterns.GradleScript()),
            new NearbyVersionSources(
                GradleLanguageProvider.GradleLanguageId,
                GradleLanguageProvider.GradleUnitKind,
                "gradle.properties",
                "gradle.properties",
                ManifestPatterns.Properties()),

            // Go's only version location. A go.mod never names its own version - `go get` resolves
            // against the tag and nothing else - so unlike every other language here, this is not
            // one source among several (TAG-01).
            new GitTagVersionSources(
                runProcess,
                writesGitTag,
                GoLanguageProvider.GoLanguageId,
                GoLanguageProvider.GoUnitKind),

            // PHP and Python version by tag at least as often as by a literal - Composer's own docs
            // advise against declaring a version in composer.json, and a pyproject using
            // setuptools-scm has none either. For those packages the tag is not a second opinion,
            // it is the only one, so both read it and both write it under --tag (TAG-01).
            new GitTagVersionSources(
                runProcess,
                writesGitTag,
                PhpLanguageProvider.PhpLanguageId,
                PhpLanguageProvider.PhpUnitKind),
            new GitTagVersionSources(
                runProcess,
                writesGitTag,
                PythonLanguageProvider.PythonLanguageId,
                PythonLanguageProvider.PythonUnitKind)
        ];
    }

    /// <summary>
    /// Every source of every convention that applies to this scope, in registration order so that
    /// a unit's sources are ordered the same way on every machine (BAS-04).
    /// </summary>
    internal static IVersionSource[] For(
        IReadOnlyList<IDiscoverVersionSources> factories,
        string languageId,
        VersionSourceScope scope)
    {
        var sources = new List<IVersionSource>();
        foreach (var factory in factories)
        {
            if (!string.Equals(factory.LanguageId, languageId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!factory.UnitKinds.Contains(scope.UnitKind))
            {
                continue;
            }

            sources.AddRange(factory.Discover(scope));
        }

        return sources.ToArray();
    }
}
