using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>TST-M3 - fixture folder trees proving FLD-02, FLD-04 and FLD-05.</summary>
public class TestFolderDiscovery : IDisposable
{
    private readonly string _folderRoot = Directory.CreateTempSubdirectory("easysemver-discovery").FullName;

    public TestFolderDiscovery()
    {
        // FLD-06 - exclusions come from the registered providers now, so a test that skips this is
        // testing a walk no run performs.
        Exclusions.BeginRun();
    }

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private void WriteFile(string relativePath, string content = "")
    {
        var fullPath = Path.Combine(this._folderRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    /// <summary>FLD-02 - a folder with no .sln anywhere is a valid, ordinary input.</summary>
    [Fact]
    public void ProjectIsFoundWithNoSolutionFilePresent()
    {
        this.WriteFile("app/Widgets.csproj");

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

        var unit = Assert.Single(units);
        Assert.Equal("Widgets", unit.UnitId);
        Assert.Equal("app/Widgets.csproj", unit.RelativePath);
    }

    /// <summary>
    /// FLD-04/FLD-06 - build output and package caches never contribute units. The `app/bin` and
    /// `app/obj` cases sit beside `app/Widgets.csproj`, which is what vouches for them now that
    /// exclusions are owned by the language rather than global (FLD-07).
    /// </summary>
    [Theory]
    [InlineData("app/bin/Debug/Ghost.csproj")]
    [InlineData("app/obj/Ghost.csproj")]
    [InlineData("app/bin/Ghost.csproj")]
    [InlineData(".packages/some.package/Ghost.csproj")]
    [InlineData(".build/checkouts/dependency/Ghost.csproj")]
    [InlineData("node_modules/thing/Ghost.csproj")]
    [InlineData("DerivedData/Ghost.csproj")]
    public void ExcludedDirectoriesAreSkipped(string ghostPath)
    {
        this.WriteFile("app/Widgets.csproj");
        this.WriteFile(ghostPath);

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

        var unit = Assert.Single(units);
        Assert.Equal("Widgets", unit.UnitId);
    }

    /// <summary>
    /// FLD-07, stated as a test because it is the behaviour that changed. A `bin` or `obj` with no
    /// project file beside it is not provably build output - it is as likely to be somebody's source
    /// - so it is walked. Anything found there appears as a new unit, which is loud and reviewable,
    /// where the old global rule hid first-party code silently.
    /// </summary>
    [Theory]
    [InlineData("bin/Debug/Stray.csproj")]
    [InlineData("obj/Stray.csproj")]
    public void BuildOutputNamesWithNothingVouchingForThemAreWalked(string strayPath)
    {
        this.WriteFile("app/Widgets.csproj");
        this.WriteFile(strayPath);

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner()))
            .Discover(this._folderRoot);

        Assert.Equal(["Stray", "Widgets"], units.Select(u => u.UnitId).OrderBy(id => id));
    }

    /// <summary>
    /// Pods is Swift's, and needs the Podfile that creates it. Without one, `Pods` is a plausible
    /// module name and is kept.
    /// </summary>
    [Fact]
    public void PodsIsSkippedOnlyBesideAPodfile()
    {
        this.WriteFile("app/Widgets.csproj");
        this.WriteFile("Pods/Ghost.csproj");

        var provider = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner()));
        Assert.Equal(2, provider.Discover(this._folderRoot).Count);

        this.WriteFile("Podfile");
        Assert.Single(provider.Discover(this._folderRoot));
    }

    /// <summary>
    /// FLD-04 - `Packages` is not excluded. SwiftPM cloned dependencies there in the Swift 3 era
    /// and has used `.build/checkouts/` since; today it is where a modular app keeps its own local
    /// packages, so excluding it silently dropped first-party units.
    /// </summary>
    [Fact]
    public void PackagesHoldsFirstPartyUnitsAndIsDiscovered()
    {
        this.WriteFile("app/Widgets.csproj");
        this.WriteFile("Packages/Feature/Feature.csproj");

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

        Assert.Equal(["Feature", "Widgets"], units.Select(unit => unit.UnitId).Order());
    }

    /// <summary>CLI-12 - a caller can keep any excluded name, including a dotted one.</summary>
    [Theory]
    [InlineData("Pods", "Pods/Vendored.csproj")]
    [InlineData(".build", ".build/Vendored.csproj")]
    public void ANameTheCallerKeepsIsNotExcluded(string keep, string ghostPath)
    {
        this.WriteFile("app/Widgets.csproj");
        this.WriteFile(ghostPath);

        try
        {
            Exclusions.BeginRun(keep);

            var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

            Assert.Equal(["Vendored", "Widgets"], units.Select(unit => unit.UnitId).Order());
        }
        finally
        {
            // The state is thread-scoped and xUnit reuses threads across tests in a collection.
            Exclusions.BeginRun();
        }
    }

    /// <summary>FLD-05 - a root with nothing recognisable in it is not an error.</summary>
    [Fact]
    public void FolderWithNoUnitsYieldsNothing()
    {
        this.WriteFile("notes/README.md", "nothing to version here");

        Assert.Empty(new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot));
    }

    /// <summary>Discovery order does not depend on the file system's enumeration order (BAS-04).</summary>
    [Fact]
    public void DiscoveryIsOrdered()
    {
        this.WriteFile("z/Zebra.csproj");
        this.WriteFile("a/Aardvark.csproj");
        this.WriteFile("m/Mongoose.csproj");

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

        Assert.Equal(["Aardvark", "Mongoose", "Zebra"], units.Select(u => u.UnitId).ToArray());
    }

    [Fact]
    public void RelativePathsUseForwardSlashes()
    {
        this.WriteFile("src/nested/deep/Widgets.csproj");

        var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

        Assert.Equal("src/nested/deep/Widgets.csproj", Assert.Single(units).RelativePath);
    }

    [Fact]
    public void ScannerFindsDirectoryBundlesAsLeaves()
    {
        this.WriteFile("App.xcodeproj/project.pbxproj");
        this.WriteFile("App.xcodeproj/nested/Inner.xcodeproj/project.pbxproj");

        var found = FolderScanner.FindDirectories(this._folderRoot, "*.xcodeproj");

        Assert.Single(found);
        Assert.EndsWith("App.xcodeproj", found[0]);
    }
}
