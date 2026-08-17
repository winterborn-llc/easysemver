using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Swift;

/// <summary>MVR-03/MVR-04 - the Xcode rows of the version-source table (§18 P5).</summary>
public class TestXcodeVersionSources : IDisposable
{
    private const string Pbxproj = """
        // !$*UTF8*$!
        {
           buildSettings = {
              MARKETING_VERSION = 1.4.2;
              CURRENT_PROJECT_VERSION = 87;
           };
           buildSettings = {
              MARKETING_VERSION = 1.4.2;
              CURRENT_PROJECT_VERSION = 87;
           };
        }
        """;

    private const string InfoPlist = """
        <?xml version="1.0" encoding="UTF-8"?>
        <plist version="1.0">
        <dict>
           <key>CFBundleShortVersionString</key>
           <string>1.4.2</string>
           <key>CFBundleVersion</key>
           <string>87</string>
        </dict>
        </plist>
        """;

    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-xcode").FullName;

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Write(string fileName, string content)
    {
        var path = Path.Combine(this._folderRoot, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void MarketingVersionIsRead()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        Assert.Equal("1.4.2", new MarketingVersionSource(path, "App.xcodeproj/project.pbxproj").Read()!.ToString());
    }

    /// <summary>SYN-02 - every occurrence is updated, so conditional configurations converge.</summary>
    [Fact]
    public void EveryMarketingVersionOccurrenceIsWritten()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        new MarketingVersionSource(path, "App.xcodeproj/project.pbxproj").Write(new Version("2.0.0"));

        var written = File.ReadAllText(path);
        Assert.Equal(2, written.Split("MARKETING_VERSION = 2.0.0;").Length - 1);
    }

    /// <summary>The two rows are separate sources; neither writer may bleed into the other's row.</summary>
    [Fact]
    public void MarketingVersionWriteLeavesTheCounterAlone()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        new MarketingVersionSource(path, "App.xcodeproj/project.pbxproj").Write(new Version("2.0.0"));

        Assert.Contains("CURRENT_PROJECT_VERSION = 87;", File.ReadAllText(path));
    }

    /// <summary>SYN-02 - every occurrence, so conditional configurations converge.</summary>
    [Fact]
    public void EveryBuildCounterOccurrenceIsWritten()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        new BuildCounterVersionSource(path, "App.xcodeproj/project.pbxproj").Write(new Version("2.0.0"));

        var written = File.ReadAllText(path);
        Assert.Equal(2, written.Split("CURRENT_PROJECT_VERSION = 2.0.0;").Length - 1);
    }

    /// <summary>The counter is written but never seeds: a bare 87 must not become version 87.0.0.</summary>
    [Fact]
    public void BuildCounterIsNeverAVersionSeed()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        Assert.Null(new BuildCounterVersionSource(path, "App.xcodeproj/project.pbxproj").Read());
    }

    [Fact]
    public void BuildCounterWriteLeavesTheMarketingVersionAlone()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        new BuildCounterVersionSource(path, "App.xcodeproj/project.pbxproj").Write(new Version("2.0.0"));

        Assert.Equal(2, File.ReadAllText(path).Split("MARKETING_VERSION = 1.4.2;").Length - 1);
    }

    /// <summary>Xcode writes the value bare or quoted depending on how it was edited.</summary>
    [Fact]
    public void QuotedBuildCounterIsAlsoALiteral()
    {
        var path = this.Write("project.pbxproj", "{ CURRENT_PROJECT_VERSION = \"87\"; }");

        new BuildCounterVersionSource(path, "project.pbxproj").Write(new Version("2.0.0"));

        Assert.Contains("CURRENT_PROJECT_VERSION = \"2.0.0\";", File.ReadAllText(path));
    }

    /// <summary>MVR-04 - an interpolated counter is write-skipped, exactly as the version row is.</summary>
    [Fact]
    public void ProjectWithoutALiteralCounterIsNotAVersionSource()
    {
        Assert.False(BuildCounterVersionSource.HasLiteralCounter(
            "{ buildSettings = { CURRENT_PROJECT_VERSION = \"$(BUILD_NUMBER)\"; }; }"));
    }

    /// <summary>Xcode writes the value bare or quoted depending on how it was edited.</summary>
    [Fact]
    public void QuotedMarketingVersionIsAlsoALiteral()
    {
        var path = this.Write("project.pbxproj", "{ MARKETING_VERSION = \"1.4.2\"; }");
        var source = new MarketingVersionSource(path, "project.pbxproj");

        Assert.Equal("1.4.2", source.Read()!.ToString());

        source.Write(new Version("2.0.0"));
        Assert.Contains("MARKETING_VERSION = \"2.0.0\";", File.ReadAllText(path));
    }

    [Fact]
    public void ProjectWithoutALiteralVersionIsNotAVersionSource()
    {
        Assert.False(MarketingVersionSource.HasLiteralVersion(
            "{ buildSettings = { MARKETING_VERSION = \"$(APP_VERSION)\"; }; }"));
    }

    /// <summary>MVR-02 - a two-segment MARKETING_VERSION is routine input now, not an edge case.</summary>
    [Fact]
    public void TwoSegmentMarketingVersionIsUsable()
    {
        var path = this.Write("project.pbxproj", "{ MARKETING_VERSION = 1.2; }");

        var version = new MarketingVersionSource(path, "project.pbxproj").Read();

        Assert.Equal("1.2.0", version!.ToString());
        version.Increment(Winterborn.Tools.EasySemVer.DataObject.VersionType.Patch);
        Assert.Equal("1.2.1", version.ToString());
    }

    [Fact]
    public void InfoPlistShortVersionIsReadAndWritten()
    {
        var path = this.Write("Info.plist", InfoPlist);
        var source = new InfoPlistVersionSource(path, "App/Info.plist");

        Assert.Equal("1.4.2", source.Read()!.ToString());

        source.Write(new Version("3.1.4"));
        var written = File.ReadAllText(path);
        Assert.Contains("<string>3.1.4</string>", written);

        // MVR-06: CFBundleVersion is a build counter and stays where it is.
        Assert.Contains("<string>87</string>", written);
    }

    /// <summary>MVR-04 - a plist that interpolates a build setting is read- and write-skipped.</summary>
    [Fact]
    public void InterpolatedShortVersionIsLeftAlone()
    {
        var path = this.Write("Info.plist", InfoPlist.Replace("1.4.2", "$(MARKETING_VERSION)"));
        var source = new InfoPlistVersionSource(path, "App/Info.plist");

        Assert.Null(source.Read());

        source.Write(new Version("3.1.4"));
        Assert.Contains("$(MARKETING_VERSION)", File.ReadAllText(path));
    }

    /// <summary>
    /// SWD-02 - the target list comes from the project file. It used to come from
    /// `xcodebuild -list -json`, which resolves the project's package dependencies before it will
    /// print a set of names that are written in the file all along.
    /// </summary>
    [Fact]
    public void TargetsAreReadFromTheProjectFile()
    {
        var targets = XcodeProject.Read(PbxprojObjects.Read(ProjectFile), "/project");

        Assert.Equal(["App", "AppUITests", "My App Tests"], targets.Select(t => t.Name));
    }

    /// <summary>
    /// UNI-04 - unit-test and UI-test bundles both name themselves in their product type, and a
    /// name is never matched against: a target called `AppTests` that ships is not test code.
    /// </summary>
    [Fact]
    public void XcodeTestTargetsAreIdentifiedByProductType()
    {
        var targets = XcodeProject.Read(PbxprojObjects.Read(ProjectFile), "/project");

        Assert.Equal(
            ["AppUITests", "My App Tests"],
            targets.Where(t => t.IsTest).Select(t => t.Name));
    }

    /// <summary>
    /// A project with nothing in it, and one this cannot make sense of at all, both yield no
    /// targets rather than throwing. A project that declares no unit is not a failed run.
    /// </summary>
    [Theory]
    [InlineData("{ objects = { }; }")]
    [InlineData("{ }")]
    public void AProjectWithNoTargetsYieldsNone(string pbxproj)
    {
        Assert.Empty(XcodeProject.Read(PbxprojObjects.Read(pbxproj), "/project"));
    }

    /// <summary>
    /// SWE-01 for Xcode - a target's sources are the files its Sources build phase lists, resolved
    /// through the group hierarchy they hang from. This is what used to require a full
    /// `xcodebuild build` per target.
    /// </summary>
    [Fact]
    public void TargetSourcesAreResolvedThroughTheGroupHierarchy()
    {
        using var fixture = new XcodeProjectFixture();

        var target = Assert.Single(XcodeProject.Read(fixture.ProjectPath));

        Assert.Equal("App", target.Name);
        Assert.Equal(
            [Path.Combine(fixture.FolderRoot, "App", "Widget.swift")],
            target.SourceFiles);
    }

    /// <summary>SWD-04 - system, binary, plugin and macro targets are never units.</summary>
    [Fact]
    public void NonSourceSwiftPackageTargetsAreNotUnits()
    {
        const string manifest = """
            // swift-tools-version:5.9
            import PackageDescription

            let package = Package(
                name: "Widgets",
                targets: [
                    .target(name: "Widgets"),
                    .testTarget(name: "WidgetsTests", dependencies: ["Widgets"]),
                    .systemLibrary(name: "CLib"),
                    .binaryTarget(name: "Prebuilt", path: "Prebuilt.xcframework"),
                    .plugin(name: "Gen", capability: .buildTool())
                ]
            )
            """;

        Assert.Equal(
            ["Widgets", "WidgetsTests"],
            SwiftPackageManifest.Read(manifest).Select(t => t.Name));
    }

    /// <summary>
    /// UNI-04 - a test target is still a unit, so it keeps its versions; what it loses is a vote on
    /// the folder's API. The manifest states the kind, so no name matching is involved: `Scenarios`
    /// below is a test target and `WidgetsTests` is not.
    /// </summary>
    [Fact]
    public void SwiftPackageTestTargetsAreIdentifiedByKindNotByName()
    {
        const string manifest = """
            // swift-tools-version:5.9
            import PackageDescription

            let package = Package(
                name: "Widgets",
                targets: [
                    .target(name: "Widgets"),
                    .target(name: "WidgetsTests"),
                    .testTarget(name: "Scenarios"),
                    .binaryTarget(name: "Prebuilt", path: "Prebuilt.xcframework")
                ]
            )
            """;

        var targets = SwiftPackageManifest.Read(manifest);

        Assert.Equal(["Scenarios"], targets.Where(t => t.IsTest).Select(t => t.Name));
        Assert.Equal(["Scenarios", "Widgets", "WidgetsTests"], targets.Select(t => t.Name));
    }

    /// <summary>
    /// A manifest reads as text, so a comment that mentions a target does not declare one and a
    /// name that is not a literal cannot be read. The second is the real limit of doing it this
    /// way, and it is reported rather than guessed at.
    /// </summary>
    [Fact]
    public void OnlyTargetsActuallyDeclaredWithALiteralNameAreRead()
    {
        const string manifest = """
            // swift-tools-version:5.9
            import PackageDescription

            // .target(name: "Commented"),
            let generated = "Computed"
            let package = Package(
                name: "Widgets",
                targets: [
                    .target(name: "Widgets"),
                    .target(name: generated)
                ]
            )
            """;

        Assert.Equal(["Widgets"], SwiftPackageManifest.Read(manifest).Select(t => t.Name));
    }

    /// <summary>The "path:" argument overrides the convention, and "exclude:" trims what is left.</summary>
    [Fact]
    public void TheManifestsPathAndExcludeArgumentsAreHonoured()
    {
        const string manifest = """
            let package = Package(
                name: "Widgets",
                targets: [
                    .target(name: "Widgets", exclude: ["Legacy"], path: "Custom/Place")
                ]
            )
            """;

        var target = Assert.Single(SwiftPackageManifest.Read(manifest));

        Assert.Equal("Custom/Place", target.Path);
        Assert.Equal(["Legacy"], target.Excluded);
    }

    private const string ProjectFile = """
        // !$*UTF8*$!
        {
           objects = {
              AAA1 /* App */ = {
                 isa = PBXNativeTarget;
                 buildPhases = (
                    BBB1 /* Sources */,
                 );
                 name = App;
                 productType = "com.apple.product-type.application";
              };
              AAA2 /* My App Tests */ = {
                 isa = PBXNativeTarget;
                 name = "My App Tests";
                 productType = "com.apple.product-type.bundle.unit-test";
              };
              AAA3 /* AppUITests */ = {
                 isa = PBXNativeTarget;
                 name = AppUITests;
                 productType = "com.apple.product-type.bundle.ui-testing";
              };
              CCC1 /* Debug */ = {
                 isa = XCBuildConfiguration;
                 name = Debug;
              };
           };
        }
        """;
}
