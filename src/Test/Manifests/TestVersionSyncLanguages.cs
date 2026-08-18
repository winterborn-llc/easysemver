using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Manifests;

/// <summary>
/// The version-sync tier (LNG-01), across every language in it.
/// <para>
/// The assertions that matter most here are the negative ones. These providers **write to files
/// people did not ask us to touch** - a package.json, a Cargo.toml, a pom.xml - and the failure
/// that would do real damage is not failing to stamp a version, it is stamping this repository's
/// version over a dependency's. Every language therefore gets a manifest containing a second,
/// deeper version that must survive untouched.
/// </para>
/// </summary>
public class TestVersionSyncLanguages : IDisposable
{
    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-version-sync").FullName;

    private readonly IReadOnlyList<ILanguageProvider> _providers =
        LanguageProviders.Create(new ProcessRunner());

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private ILanguageProvider Provider(string languageId) =>
        LanguageProviders.Find(this._providers, languageId)!;

    private string Write(string relativePath, string contents)
    {
        var path = Path.Combine(this._folderRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return path;
    }

    // ---- fixtures, each with a second version that must never be rewritten ----

    private const string PackageJson = """
        {
          "name": "widgets",
          "version": "1.2.3",
          "dependencies": {
            "left-pad": "1.0.0"
          },
          "engines": {
            "version": "18.0.0"
          }
        }
        """;

    private const string CargoToml = """
        [package]
        name = "widgets"
        version = "1.2.3"

        [dependencies]
        serde = { version = "9.9.9" }
        """;

    private const string PyprojectToml = """
        [project]
        name = "widgets"
        version = "1.2.3"

        [tool.poetry.dependencies]
        requests = { version = "9.9.9" }
        """;

    private const string PubspecYaml = """
        name: widgets
        version: 1.2.3

        dependencies:
          http:
            version: 9.9.9
        """;

    private const string ComposerJson = """
        {
          "name": "acme/widgets",
          "version": "1.2.3",
          "require": {
            "monolog/monolog": "9.9.9"
          }
        }
        """;

    private const string PomXml = """
        <project>
          <groupId>acme</groupId>
          <artifactId>widgets</artifactId>
          <version>1.2.3</version>
          <parent>
            <version>9.9.9</version>
          </parent>
          <dependencies>
            <dependency>
              <version>9.9.9</version>
            </dependency>
          </dependencies>
        </project>
        """;

    private const string CMakeLists = """
        cmake_minimum_required(VERSION 3.20)
        project(widgets VERSION 1.2.3 LANGUAGES CXX)

        find_package(Boost 9.9.9 REQUIRED)
        set(VENDORED_VERSION 9.9.9)
        """;

    private const string Gemspec = """
        Gem::Specification.new do |spec|
          spec.name = "widgets"
          spec.version = "1.2.3"
          spec.add_dependency "rails", "9.9.9"
        end
        """;

    private const string DistIni = """
        name = Widgets
        version = 1.2.3

        [Prereqs]
        Moose = 9.9.9
        """;

    public static TheoryData<string, string, string> Languages() => new()
    {
        { "javascript", "package.json", PackageJson },
        { "rust", "Cargo.toml", CargoToml },
        { "python", "pyproject.toml", PyprojectToml },
        { "dart", "pubspec.yaml", PubspecYaml },
        { "php", "composer.json", ComposerJson },
        { "java", "pom.xml", PomXml },
        { "cpp", "CMakeLists.txt", CMakeLists },
        { "ruby", "widgets.gemspec", Gemspec },
        { "perl", "dist.ini", DistIni }
    };

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheManifestIsDiscoveredAsExactlyOneUnit(string languageId, string manifest, string contents)
    {
        this.Write($"packages/widgets/{manifest}", contents);

        var units = this.Provider(languageId).Discover(this._folderRoot);

        var unit = Assert.Single(units);
        Assert.Equal(languageId, unit.LanguageId);
        Assert.Equal("packages/widgets", unit.UnitId);
    }

    /// <summary>LNG-01 - the whole tier is declared surfaceless at discovery, not at extraction.</summary>
    [Theory]
    [MemberData(nameof(Languages))]
    public void TheUnitCarriesNoApiSurface(string languageId, string manifest, string contents)
    {
        this.Write($"packages/widgets/{manifest}", contents);

        var unit = this.Provider(languageId).Discover(this._folderRoot).Single();

        Assert.False(unit.HasPublicApiSurface);
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheVersionIsRead(string languageId, string manifest, string contents)
    {
        this.Write($"packages/widgets/{manifest}", contents);

        var provider = this.Provider(languageId);
        var unit = provider.Discover(this._folderRoot).Single();

        Assert.Equal("1.2.3", Assert.Single(provider.ReadVersions(unit)).ToString());
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheVersionIsWritten(string languageId, string manifest, string contents)
    {
        var path = this.Write($"packages/widgets/{manifest}", contents);

        var provider = this.Provider(languageId);
        var unit = provider.Discover(this._folderRoot).Single();
        provider.WriteVersion(unit, new Version("4.5.6"));

        Assert.Contains("4.5.6", File.ReadAllText(path));
        Assert.DoesNotContain("1.2.3", File.ReadAllText(path));
    }

    /// <summary>
    /// The one that matters. Every fixture carries a 9.9.9 belonging to a dependency or a parent,
    /// and stamping over it would pin somebody else's package to this repository's version - a
    /// silent, committed, published mistake in a file the team reads every day.
    /// </summary>
    [Theory]
    [MemberData(nameof(Languages))]
    public void ADependencysVersionIsNeverRewritten(string languageId, string manifest, string contents)
    {
        var path = this.Write($"packages/widgets/{manifest}", contents);

        var provider = this.Provider(languageId);
        var unit = provider.Discover(this._folderRoot).Single();
        provider.WriteVersion(unit, new Version("4.5.6"));

        var updated = File.ReadAllText(path);
        Assert.Equal(
            contents.Split("9.9.9").Length - 1,
            updated.Split("9.9.9").Length - 1);
    }

    /// <summary>MVR-04 - a manifest with no literal version of its own is never given one.</summary>
    [Theory]
    [InlineData("javascript", "package.json", "{\n  \"name\": \"widgets\"\n}")]
    [InlineData("rust", "Cargo.toml", "[package]\nname = \"widgets\"\n")]
    [InlineData("python", "pyproject.toml", "[project]\nname = \"widgets\"\ndynamic = [\"version\"]\n")]
    [InlineData("dart", "pubspec.yaml", "name: widgets\n")]
    [InlineData("php", "composer.json", "{\n  \"name\": \"acme/widgets\"\n}")]
    public void AManifestWithNoVersionIsNotGivenOne(string languageId, string manifest, string contents)
    {
        var path = this.Write($"packages/widgets/{manifest}", contents);

        var provider = this.Provider(languageId);
        var unit = provider.Discover(this._folderRoot).Single();

        Assert.Empty(provider.ReadVersions(unit));
        provider.WriteVersion(unit, new Version("4.5.6"));
        Assert.Equal(contents, File.ReadAllText(path));
    }

    /// <summary>
    /// A Maven module that inherits its version from a parent has no version element of its own,
    /// and the parent's is not this module's to change.
    /// </summary>
    [Fact]
    public void AMavenModuleInheritingItsVersionIsLeftAlone()
    {
        const string childPom = """
            <project>
              <artifactId>widgets-core</artifactId>
              <parent>
                <version>9.9.9</version>
              </parent>
            </project>
            """;
        var path = this.Write("core/pom.xml", childPom);

        var provider = this.Provider("java");
        var unit = provider.Discover(this._folderRoot).Single();

        Assert.Empty(provider.ReadVersions(unit));
        provider.WriteVersion(unit, new Version("4.5.6"));
        Assert.Equal(childPom, File.ReadAllText(path));
    }

    /// <summary>
    /// TypeScript is not a separate unit: a TypeScript package is an npm package, with the same
    /// manifest and the same version key.
    /// </summary>
    [Fact]
    public void ATypeScriptPackageIsAJavascriptUnit()
    {
        this.Write("ui/package.json", PackageJson);
        this.Write("ui/tsconfig.json", "{}");

        var unit = this.Provider("javascript").Discover(this._folderRoot).Single();

        Assert.Equal("ui", unit.UnitId);
    }

    /// <summary>
    /// ML-03 - a manifest at the folder root is "." rather than an empty id, which would sort
    /// oddly and read as missing in the log.
    /// </summary>
    [Fact]
    public void AManifestAtTheRootGetsADottedUnitId()
    {
        this.Write("package.json", PackageJson);

        Assert.Equal(".", this.Provider("javascript").Discover(this._folderRoot).Single().UnitId);
    }

    /// <summary>
    /// The Ruby convention that actually predominates: the gemspec points at a constant, which is
    /// not a literal and so is untouchable (MVR-04), and the number lives in version.rb.
    /// </summary>
    [Fact]
    public void ARubyVersionConstantIsReadAndWritten()
    {
        this.Write(
            "widgets/widgets.gemspec",
            """
            Gem::Specification.new do |spec|
              spec.name = "widgets"
              spec.version = Widgets::VERSION
            end
            """);
        var versionFile = this.Write(
            "widgets/lib/widgets/version.rb",
            """
            module Widgets
              VERSION = "1.2.3"
            end
            """);

        var provider = this.Provider("ruby");
        var unit = provider.Discover(this._folderRoot).Single();

        Assert.Equal("1.2.3", Assert.Single(provider.ReadVersions(unit)).ToString());

        provider.WriteVersion(unit, new Version("4.5.6"));
        Assert.Contains("4.5.6", File.ReadAllText(versionFile));
    }

    /// <summary>
    /// MVR-05 - every .pm carrying a literal $VERSION is written, because keeping them in step by
    /// hand is the chore this removes.
    /// </summary>
    [Fact]
    public void EveryPerlModuleVersionIsWritten()
    {
        this.Write("Widgets/Makefile.PL", "use ExtUtils::MakeMaker;\n");
        var main = this.Write("Widgets/lib/Widgets.pm", "package Widgets;\nour $VERSION = '1.2.3';\n1;\n");
        var part = this.Write("Widgets/lib/Widgets/Part.pm", "package Widgets::Part;\nour $VERSION = '1.2.3';\n1;\n");

        var provider = this.Provider("perl");
        var unit = provider.Discover(this._folderRoot).Single();
        provider.WriteVersion(unit, new Version("4.5.6"));

        Assert.Contains("4.5.6", File.ReadAllText(main));
        Assert.Contains("4.5.6", File.ReadAllText(part));
    }

    /// <summary>
    /// LNG-05 - a distribution carrying two of Perl's three manifests is one package. Discovering
    /// it twice would version it twice and read as two units appearing on the first upgrade.
    /// </summary>
    [Fact]
    public void APerlDistributionWithSeveralManifestsIsOneUnit()
    {
        this.Write("Widgets/Makefile.PL", "use ExtUtils::MakeMaker;\n");
        this.Write("Widgets/Build.PL", "use Module::Build;\n");
        this.Write("Widgets/dist.ini", "name = Widgets\nversion = 1.2.3\n");

        Assert.Single(this.Provider("perl").Discover(this._folderRoot));
    }

    /// <summary>Each provider claims only its own manifest, or one repository becomes many units twice over.</summary>
    [Theory]
    [MemberData(nameof(Languages))]
    public void NoOtherLanguageClaimsThisManifest(string languageId, string manifest, string contents)
    {
        this.Write($"packages/widgets/{manifest}", contents);

        foreach (var provider in this._providers)
        {
            if (provider.LanguageId == languageId)
            {
                continue;
            }

            Assert.Empty(provider.Discover(this._folderRoot));
        }
    }
}
