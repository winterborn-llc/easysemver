using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Vb;

/// <summary>
/// VB-01 at the provider level: discovery, UNI-04, and the version sources. The extraction half is
/// <see cref="TestVbExtraction"/>.
/// </summary>
public class TestVbProvider : IDisposable
{
    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-vb-provider").FullName;

    /// <summary>
    /// One instance for the whole test, deliberately. A provider learns the folder root during
    /// <see cref="ILanguageProvider.Discover"/> and both <c>IsTestCode</c> and <c>Extract</c> need
    /// it afterwards, so a fresh instance per call would answer against an empty root - which is
    /// how the run itself uses it, and how these tests have to.
    /// </summary>
    private readonly ILanguageProvider Provider =
        LanguageProviders.Find(LanguageProviders.Create(new ProcessRunner()), "vb")!;

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string WriteProject(string name, string contents)
    {
        var directory = Path.Combine(this._folderRoot, name);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{name}.vbproj");
        File.WriteAllText(path, contents);
        return path;
    }

    [Fact]
    public void VbIsRegisteredAndReachableByItsId()
    {
        Assert.NotNull(this.Provider);
        Assert.Equal("vb", this.Provider.LanguageId);
    }

    [Fact]
    public void EveryVbprojIsAUnit()
    {
        this.WriteProject("Widgets", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        this.WriteProject("Gadgets", "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        var units = this.Provider.Discover(this._folderRoot);

        Assert.Equal(["Gadgets", "Widgets"], units.Select(u => u.UnitId).OrderBy(id => id));
        Assert.All(units, unit => Assert.Equal("vbproj", unit.UnitKind));
        Assert.All(units, unit => Assert.Equal("vb", unit.LanguageId));
    }

    /// <summary>A .csproj is C#'s unit and must not be swept up by VB's walk, or it is versioned twice.</summary>
    [Fact]
    public void ACsprojIsNotAVbUnit()
    {
        var directory = Path.Combine(this._folderRoot, "Widgets");
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, "Widgets.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        Assert.Empty(this.Provider.Discover(this._folderRoot));
    }

    /// <summary>MVR-03 - a .vbproj is MSBuild, so the version properties are the .csproj ones.</summary>
    [Fact]
    public void VersionIsReadFromTheProjectFile()
    {
        this.WriteProject(
            "Widgets",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <AssemblyVersion>4.2.1</AssemblyVersion>
              </PropertyGroup>
            </Project>
            """);

        var unit = this.Provider.Discover(this._folderRoot).Single();

        Assert.Equal("4.2.1", this.Provider.ReadVersions(unit).Single().ToString());
    }

    [Fact]
    public void VersionIsWrittenBackToTheProjectFile()
    {
        var path = this.WriteProject(
            "Widgets",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <AssemblyVersion>4.2.1</AssemblyVersion>
              </PropertyGroup>
            </Project>
            """);

        var unit = this.Provider.Discover(this._folderRoot).Single();
        this.Provider.WriteVersion(unit, new Winterborn.Tools.EasySemVer.DataObject.Version("5.0.0"));

        Assert.Contains("5.0.0", File.ReadAllText(path));
    }

    /// <summary>MVR-04 - a project with no version property is not given one.</summary>
    [Fact]
    public void AProjectWithNoVersionPropertyIsNotGivenOne()
    {
        var path = this.WriteProject("Widgets", "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        var unit = this.Provider.Discover(this._folderRoot).Single();
        this.Provider.WriteVersion(unit, new Winterborn.Tools.EasySemVer.DataObject.Version("5.0.0"));

        Assert.DoesNotContain("5.0.0", File.ReadAllText(path));
    }

    /// <summary>UNI-04, read from the same MSBuild signals C# uses (G-23).</summary>
    [Fact]
    public void ATestProjectIsRecognisedByItsMsBuildSignals()
    {
        this.WriteProject(
            "Widgets.Tests",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit" Version="2.9.2" />
              </ItemGroup>
            </Project>
            """);

        var unit = this.Provider.Discover(this._folderRoot).Single();

        Assert.True(this.Provider.IsTestCode(unit));
    }

    /// <summary>UNI-04's escape hatch, and the name-is-never-consulted rule.</summary>
    [Fact]
    public void AProjectNamedTestsIsNotTestCodeWithoutTheSignals()
    {
        this.WriteProject("Widgets.Tests", "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        var unit = this.Provider.Discover(this._folderRoot).Single();

        Assert.False(this.Provider.IsTestCode(unit));
    }
}
