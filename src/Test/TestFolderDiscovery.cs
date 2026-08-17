using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>TST-M3 - fixture folder trees proving FLD-02, FLD-04 and FLD-05.</summary>
public class TestFolderDiscovery : IDisposable
{
    private readonly string _folderRoot = Directory.CreateTempSubdirectory("easysemver-discovery").FullName;

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

    /// <summary>FLD-04 - build output and package caches never contribute units.</summary>
    [Theory]
    [InlineData("bin/Debug/Ghost.csproj")]
    [InlineData("obj/Ghost.csproj")]
    [InlineData("app/bin/Ghost.csproj")]
    [InlineData(".packages/some.package/Ghost.csproj")]
    [InlineData(".build/checkouts/dependency/Ghost.csproj")]
    [InlineData("node_modules/thing/Ghost.csproj")]
    [InlineData("Pods/Ghost.csproj")]
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
            DirectoryExclusions.BeginRun([keep]);

            var units = new CsharpLanguageProvider(VersionSourceFactories.Create(new ProcessRunner())).Discover(this._folderRoot);

            Assert.Equal(["Vendored", "Widgets"], units.Select(unit => unit.UnitId).Order());
        }
        finally
        {
            // The state is thread-scoped and xUnit reuses threads across tests in a collection.
            DirectoryExclusions.BeginRun([]);
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
