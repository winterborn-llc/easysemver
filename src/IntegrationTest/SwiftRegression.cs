using Winterborn.Tools.EasySemVer;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;
using Xunit;

namespace IntegrationTest;

/// <summary>
/// TST-M6 and acceptance criterion 5, over a real package tree. There is no toolchain trait on
/// this suite any more and no toolchain to install: discovery reads Package.swift as text and
/// extraction reads the .swift files, so it runs on any machine that can run the tests at all.
/// </summary>
public class SwiftRegression
{
    [Fact]
    public void SwiftPackageIsDiscoveredExtractedAndVersioned()
    {
        using var fixture = new SwiftPackageFixture();

        // Acceptance 5: a folder holding a .csproj *and* a SwiftPM package.
        var projectPath = Path.Combine(fixture.FolderRoot, "Widget.csproj");
        File.WriteAllText(projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>1.0.0</AssemblyVersion>
               </PropertyGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(fixture.FolderRoot, "Widget.cs"),
            "namespace Widgets; public class Widget { }");

        Assert.Equal(0, Program.Main(fixture.FolderRoot));

        var baseline = File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml"));
        Assert.Contains("language=\"csharp\"", baseline);
        Assert.Contains("language=\"swift\"", baseline);
        Assert.Contains("unitKind=\"swiftpm-target\"", baseline);
        Assert.Contains("<SwiftModule name=\"Widgets\"", baseline);

        // UNI-04: the fixture's `.testTarget` is discovered and versioned like any other target,
        // and its symbols are not in the baseline.
        // Before this it was, and renaming an XCTest method moved the whole folder's version.
        Assert.DoesNotContain("<SwiftModule name=\"WidgetsTests\"", baseline);
        Assert.DoesNotContain("unitId=\"Widgets:WidgetsTests\"", baseline);

        // MVR-05: the one new version reaches both ecosystems' version locations. The podspec
        // seeds 2.3.4, which is higher than the csproj's 1.0.0, and a first run is Minor.
        Assert.Equal("2.4.0", new CsProjFile(projectPath).Version.ToString());
        Assert.Contains(
            "s.version = '2.4.0'",
            File.ReadAllText(Path.Combine(fixture.PackageDirectory, "Widgets.podspec")));
        Assert.Contains(
            "let version = \"2.4.0\"",
            File.ReadAllText(Path.Combine(
                fixture.PackageDirectory, "Sources", "Widgets", "WidgetsVersion.swift")));

        // BAS-04: no absolute paths, no timestamps, no toolchain versions.
        Assert.DoesNotContain(fixture.FolderRoot, baseline);
    }

    /// <summary>Acceptance criterion 3, extended to a multi-language tree.</summary>
    [Fact]
    public void SecondRunOverAnUnchangedSwiftTreeIsAPatch()
    {
        using var fixture = new SwiftPackageFixture();

        Assert.Equal(0, Program.Main(fixture.FolderRoot));
        var first = File.ReadAllText(fixture.PodspecPath);
        var firstBaseline = File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml"));

        Assert.Equal(0, Program.Main(fixture.FolderRoot));
        var second = File.ReadAllText(fixture.PodspecPath);

        Assert.Contains("s.version = '2.4.0'", first);
        Assert.Contains("s.version = '2.4.1'", second);

        // BAS-04: unchanged source produces a byte-identical baseline.
        Assert.Equal(
            firstBaseline,
            File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml")));
    }

    /// <summary>TST-M6 - a real package tree on disk reaches the model intact.</summary>
    [Fact]
    public void ExtractedModuleCarriesThePackagesSwiftSurface()
    {
        using var fixture = new SwiftPackageFixture();

        var provider = new SwiftLanguageProvider(VersionSourceFactories.Create(new ProcessRunner()));
        var units = provider.Discover(fixture.FolderRoot);

        // UNI-03: test targets are units too, and a plain `swift build` does not build them.
        Assert.Equal(
            ["SwiftPackage:Widgets", "SwiftPackage:WidgetsTests"],
            units.Select(u => u.UnitId).Order());

        // UNI-04, from what the manifest declares: the `.testTarget` is a unit that carries
        // versions, and is the one the provider names as test code.
        Assert.Equal(
            ["SwiftPackage:WidgetsTests"],
            units.Where(provider.IsTestCode).Select(u => u.UnitId).Order());

        // UNI-04, applied the way the run applies it: a test target is versioned but never
        // extracted, so nothing it declares reaches classification, the baseline or the report.
        foreach (var discovered in units.Where(u => !provider.IsTestCode(u)))
        {
            provider.Extract(discovered);
        }

        var unit = units.First(u => u.UnitId == "SwiftPackage:Widgets");

        var module = Assert.IsAssignableFrom<ISwiftModule>(unit.Signature);
        Assert.Equal("Widgets", module.Name);
        Assert.Contains(module.Types, t => t.Name == "Point" && t.Kind == "struct");
        Assert.Contains(module.Types, t => t.Name == "Movable" && t.Kind == "protocol");
        Assert.Contains(module.Types, t => t.Name == "Colour" && t.Kind == "enum");

        var gadget = module.Types.First(t => t.Name == "Gadget");
        Assert.Equal("class", gadget.Kind);
        Assert.Equal("open", gadget.AccessLevel);
        Assert.Contains(gadget.Functions, f => f.Name == "Gadget.move(to:)" && f.Throws);
    }

    /// <summary>
    /// The inverse of the test that used to be here. With no toolchain on the PATH at all, the run
    /// used to exit 1 having written nothing, because discovery could not start without
    /// <c>swift package dump-package</c>. It now completes: the manifest and the source are both
    /// files, and neither needs a process to read.
    /// <para>
    /// An empty PATH also takes git with it, which is the point of asserting on the podspec - the
    /// git-tag version source drops out with a log line and the run seeds from the other sources
    /// rather than failing (MVR-03).
    /// </para>
    /// </summary>
    [Fact]
    public void TheRunNeedsNoToolchainOnThePath()
    {
        using var fixture = new SwiftPackageFixture();
        var reportPath = Path.Combine(fixture.FolderRoot, "report.json");

        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", "/nonexistent-for-easysemver");
            Assert.Equal(0, Program.Main(fixture.FolderRoot, "--json", reportPath));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }

        var baseline = File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml"));
        Assert.Contains("<SwiftModule name=\"Widgets\"", baseline);
        Assert.Contains("s.version = '2.4.0'", File.ReadAllText(fixture.PodspecPath));
        Assert.True(File.Exists(reportPath));
    }
}
