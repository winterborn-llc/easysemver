using Winterborn.Tools.EasySemVer;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;
using Xunit;

namespace IntegrationTest;

/// <summary>
/// §18 P5 over a real .xcodeproj: target discovery, extraction, and `MARKETING_VERSION`
/// write-back. All three read project.pbxproj and the source it points at, so none of it needs
/// Xcode installed and the suite is no longer traited away from machines without it.
/// </summary>
public class XcodeRegression
{
    [Fact]
    public void XcodeTargetIsDiscoveredExtractedAndVersioned()
    {
        using var fixture = new XcodeProjectFixture();

        Assert.Equal(0, Program.Main(fixture.FolderRoot));

        var baseline = File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml"));
        Assert.Contains("unitKind=\"xcode-target\"", baseline);
        Assert.Contains("unitId=\"App.xcodeproj:App\"", baseline);

        // The surface really was extracted - this is not the empty-module O-06 fallback.
        Assert.Contains("name=\"Widget\"", baseline);
        Assert.Contains("name=\"Widget.describe()\"", baseline);

        // MARKETING_VERSION seeds 3.2.1; a first run is Minor.
        var pbxproj = File.ReadAllText(fixture.ProjectFilePath);
        Assert.Contains("MARKETING_VERSION = 3.3.0;", pbxproj);

        // MVR-06 / §20 O-01: the build counter is not a version and is left alone.
        Assert.Contains("CURRENT_PROJECT_VERSION = 42;", pbxproj);
    }

    [Fact]
    public void SecondRunOverAnUnchangedXcodeTreeIsAPatch()
    {
        using var fixture = new XcodeProjectFixture();

        Assert.Equal(0, Program.Main(fixture.FolderRoot));
        var firstBaseline = File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml"));

        Assert.Equal(0, Program.Main(fixture.FolderRoot));

        Assert.Contains("MARKETING_VERSION = 3.3.1;", File.ReadAllText(fixture.ProjectFilePath));
        Assert.Equal(
            firstBaseline,
            File.ReadAllText(Path.Combine(fixture.FolderRoot, "EasySemVer.xml")));
    }

    [Fact]
    public void ExtractedModuleCarriesTheTargetsSwiftSurface()
    {
        using var fixture = new XcodeProjectFixture();

        var provider = new SwiftLanguageProvider(VersionSourceFactories.Create(new ProcessRunner()));
        var unit = Assert.Single(provider.Discover(fixture.FolderRoot));
        Assert.Equal("xcode-target", unit.UnitKind);

        provider.Extract(unit);

        var module = Assert.IsAssignableFrom<ISwiftModule>(unit.Signature);
        var widget = Assert.Single(module.Types);
        Assert.Equal("struct", widget.Kind);
        Assert.Contains(widget.Functions, f => f.Name == "Widget.describe()");
        Assert.Contains(widget.Initializers, i => i.Name == "Widget.init(name:)");
        Assert.Contains(widget.Properties, p => p.Name == "Widget.name" && p.IsSettable);
    }
}
