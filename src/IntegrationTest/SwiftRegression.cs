using Winterborn.Library.EasySemVer;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Winterborn.Library.EasySemVer.CodeReader.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Winterborn.Library.EasySemVer.Process;
using Winterborn.Library.EasySemVer.Providers;
using Xunit;

namespace IntegrationTest;

/// <summary>
/// TST-M6 and acceptance criterion 5, against a real toolchain. Traited so a machine without
/// Swift can run everything else: <c>dotnet test --filter Toolchain!=Swift</c>.
/// The fixture package has no external dependencies, so `swift build` never resolves and the
/// suite needs no network.
/// </summary>
[Trait("Toolchain", "Swift")]
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
        Assert.Contains("language=\"Csharp\"", baseline);
        Assert.Contains("language=\"Swift\"", baseline);
        Assert.Contains("unitKind=\"swiftpm-target\"", baseline);
        Assert.Contains("<SwiftModule name=\"Widgets\"", baseline);
        Assert.Contains("<SwiftModule name=\"WidgetsTests\"", baseline);

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

    /// <summary>TST-M6 - the graph a real toolchain emits reaches the model intact.</summary>
    [Fact]
    public void LiveSymbolGraphCarriesTheExpectedSymbols()
    {
        using var fixture = new SwiftPackageFixture();

        var provider = new SwiftLanguageProvider(new ProcessRunner());
        var units = provider.Discover(fixture.FolderRoot);

        // UNI-03: test targets are units too, and a plain `swift build` does not build them.
        Assert.Equal(
            ["SwiftPackage:Widgets", "SwiftPackage:WidgetsTests"],
            units.Select(u => u.UnitId).Order());

        foreach (var discovered in units)
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

    /// <summary>Acceptance criterion 7, for real: no toolchain means exit 1 and nothing written.</summary>
    [Fact]
    public void MissingToolchainFailsTheRun()
    {
        using var fixture = new SwiftPackageFixture();
        var before = File.ReadAllText(fixture.PodspecPath);
        var reportPath = Path.Combine(fixture.FolderRoot, "report.json");

        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", "/nonexistent-for-easysemver");
            Assert.Equal(1, Program.Main(fixture.FolderRoot, "--json", reportPath));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }

        Assert.False(File.Exists(Path.Combine(fixture.FolderRoot, "EasySemVer.xml")));
        Assert.Equal(before, File.ReadAllText(fixture.PodspecPath));

        // REP-08: the failure happened after classification, which is the case that could have
        // produced a plausible-looking but untrue report.
        Assert.False(File.Exists(reportPath));
    }
}
