using Winterborn.Library.EasySemVer;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Xunit;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace IntegrationTest;

/// <summary>
/// TST-M8 - the end-to-end proof that the baseline can actually be written (was G-01). Two runs
/// over an unchanged tree must bump Patch by exactly one.
/// </summary>
public class Regression
{
    /// <summary>
    /// This test will fail the first time it is run after a major or minor change.
    /// </summary>
    [Fact]
    public void TestProgramInvocation()
    {
        var folderRoot = GetRepositoryRoot();

        // Set the baseline and ensure the file exists.
        Assert.Equal(0, Program.Main(folderRoot));

        // Get the current version from disk
        // Run the increment process
        // Get the updated version from disk, which should be a patch since nothing changed
        var previous = new Version(GetTestFile(folderRoot).Version);
        Assert.Equal(0, Program.Main(folderRoot));

        var current = new Version(GetTestFile(folderRoot).Version);
        Assert.Equal(previous.Patch + 1, current.Patch);
    }

    /// <summary>
    /// FLD-01 - the folder argument is honoured now (was G-06). Pointing the tool at a folder with
    /// no .sln in it or above it is an ordinary, working invocation (FLD-02, acceptance 4).
    /// </summary>
    [Fact]
    public void FolderWithNoSolutionFileIsVersioned()
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-nosln").FullName;
        try
        {
            var projectPath = Path.Combine(folderRoot, "Widget.csproj");
            File.WriteAllText(projectPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                   <PropertyGroup>
                      <AssemblyVersion>3.4.5</AssemblyVersion>
                   </PropertyGroup>
                </Project>
                """);
            File.WriteAllText(Path.Combine(folderRoot, "Widget.cs"), "namespace Widgets; public class Widget { }");

            Assert.Equal(0, Program.Main(folderRoot));

            Assert.True(File.Exists(Path.Combine(folderRoot, "EasySemVer.xml")));

            // First run sees a brand-new unit, so NCL-02 makes it Minor.
            Assert.Equal("3.5.0", new CsProjFile(projectPath).Version.ToString());
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>BAS-04 - two runs over unchanged source produce byte-identical baselines.</summary>
    [Fact]
    public void BaselineIsDeterministicAndCarriesNoAbsolutePaths()
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-determinism").FullName;
        try
        {
            var projectPath = Path.Combine(folderRoot, "Gadget.csproj");
            File.WriteAllText(projectPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                   <PropertyGroup>
                      <AssemblyVersion>1.0.0</AssemblyVersion>
                   </PropertyGroup>
                </Project>
                """);
            File.WriteAllText(
                Path.Combine(folderRoot, "Gadget.cs"),
                "namespace Gadgets; public class Gadget { public string Name { get; set; } = \"\"; }");

            Assert.Equal(0, Program.Main(folderRoot));
            var first = File.ReadAllText(Path.Combine(folderRoot, "EasySemVer.xml"));

            Assert.Equal(0, Program.Main(folderRoot));
            var second = File.ReadAllText(Path.Combine(folderRoot, "EasySemVer.xml"));

            Assert.Equal(first, second);
            Assert.DoesNotContain(folderRoot, first);
            Assert.DoesNotContain(Path.GetTempPath(), first);
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    private static CsProjFile GetTestFile(string folderRoot)
    {
        var path = Path.Combine(folderRoot, "src", "Test", "Test.csproj");
        Assert.True(File.Exists(path), $"Unable to load the test project file at {path}");
        return new CsProjFile(path);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Unable to locate the repository root from the test working directory");
        return string.Empty;
    }
}
