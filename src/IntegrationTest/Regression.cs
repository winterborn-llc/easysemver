using Winterborn.Library.EasySemVer;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Xunit;

namespace IntegrationTest;

/// <summary>
/// TST-M8 - the end-to-end proof that the baseline can actually be written (was G-01), and that
/// each classification reaches the version on disk. Every case runs against a generated temporary
/// tree: EasySemVer is never pointed at this repository, so running the suite leaves the working
/// copy untouched. <see cref="SwiftRegression"/> covers the same ground for a multi-language tree
/// but is traited off without a Swift toolchain, which makes this the C#-only safety net.
/// </summary>
public class Regression
{
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

    /// <summary>
    /// OVR-03 - every run is a release, so a second run over unchanged source still moves, by
    /// exactly one Patch. This is what the old repository-scanning regression was checking.
    /// </summary>
    [Fact]
    public void SecondRunOverUnchangedSourceIsAPatch()
    {
        var folderRoot = CreateWidgetTree(out var projectPath, out _);
        try
        {
            Assert.Equal(0, Program.Main(folderRoot));
            Assert.Equal("1.3.0", new CsProjFile(projectPath).Version.ToString());

            Assert.Equal(0, Program.Main(folderRoot));
            Assert.Equal("1.3.1", new CsProjFile(projectPath).Version.ToString());
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>CLS - withdrawing a public member is the breaking case that has to reach Major.</summary>
    [Fact]
    public void RemovingAPublicMethodIsMajor()
    {
        var folderRoot = CreateWidgetTree(out var projectPath, out var sourcePath);
        try
        {
            Assert.Equal(0, Program.Main(folderRoot));
            Assert.Equal("1.3.0", new CsProjFile(projectPath).Version.ToString());

            File.WriteAllText(sourcePath, """
                namespace Widgets;

                public class Widget
                {
                    public string Name { get; set; } = "";

                    public int Count(string input) => input.Length;
                }
                """);

            Assert.Equal(0, Program.Main(folderRoot));
            Assert.Equal("2.0.0", new CsProjFile(projectPath).Version.ToString());
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>
    /// O-04 - a dry run classifies and reports without writing, so it is not a release. This is the
    /// mode a pull-request check runs in, where mutating the tree would be wrong.
    /// </summary>
    [Fact]
    public void DryRunWritesNothing()
    {
        var folderRoot = CreateWidgetTree(out var projectPath, out _);
        try
        {
            Assert.Equal(0, Program.Main(folderRoot, "--dry-run"));

            Assert.False(File.Exists(Path.Combine(folderRoot, "EasySemVer.xml")));
            Assert.Equal("1.2.3", new CsProjFile(projectPath).Version.ToString());
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>
    /// §20 O-04 - a dry run explains itself. Every detected change is listed under the unit it was
    /// found in, carrying its impact and the symbol it concerns, and the report ends with the
    /// change type and the version transition. Without this a reviewer reading a pull-request
    /// check sees a verdict with no evidence behind it.
    /// </summary>
    [Fact]
    public void DryRunListsEveryChangeWithItsImpact()
    {
        var folderRoot = CreateWidgetTree(out var projectPath, out var sourcePath);
        try
        {
            // The first run is the release that leaves a baseline at 1.3.0 to compare against.
            Assert.Equal(0, Program.Main(folderRoot));
            Assert.Equal("1.3.0", new CsProjFile(projectPath).Version.ToString());

            File.WriteAllText(sourcePath, """
                namespace Widgets;

                public class Widget
                {
                    public string Name { get; set; } = "";

                    public int Count(string input) => input.Length;

                    public string Describe(string prefix) => prefix;
                }
                """);

            var report = CaptureOutput(() => Assert.Equal(0, Program.Main(folderRoot, "--dry-run")));

            Assert.Contains("Csharp Widgets", report);
            Assert.Contains("Major  Widgets.Widget.Weigh was removed", report);
            Assert.Contains("Minor  Widgets.Widget.Describe was added", report);
            Assert.Contains("Change Type: Major (1 major, 1 minor, 0 patch)", report);
            Assert.Contains("Version: 1.3.0 -> 2.0.0", report);

            // Explaining itself is still not releasing: nothing on disk moved.
            Assert.Equal("1.3.0", new CsProjFile(projectPath).Version.ToString());
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>
    /// LOG-01 - everything the tool says goes through <c>Log</c> to stdout, so a test can read the
    /// report the same way a build log does.
    /// </summary>
    private static string CaptureOutput(Action run)
    {
        var original = Console.Out;
        var captured = new StringWriter();
        try
        {
            Console.SetOut(captured);
            run();
        }
        finally
        {
            Console.SetOut(original);
        }

        return captured.ToString();
    }

    /// <summary>
    /// One .csproj carrying all three version properties and one source file with two public
    /// methods, in a temporary directory. Generated rather than checked in so that no fixture
    /// .csproj ever sits under src/ and gets swept into this repository's own build.
    /// </summary>
    private static string CreateWidgetTree(out string projectPath, out string sourcePath)
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-csharp").FullName;

        projectPath = Path.Combine(folderRoot, "Widgets.csproj");
        File.WriteAllText(projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>1.2.3</AssemblyVersion>
                  <PackageVersion>1.2.3</PackageVersion>
                  <FileVersion>1.2.3</FileVersion>
               </PropertyGroup>
            </Project>
            """);

        sourcePath = Path.Combine(folderRoot, "Widget.cs");
        File.WriteAllText(sourcePath, """
            namespace Widgets;

            public class Widget
            {
                public string Name { get; set; } = "";

                public int Weigh() => 0;

                public int Count(string input) => input.Length;
            }
            """);

        return folderRoot;
    }
}
