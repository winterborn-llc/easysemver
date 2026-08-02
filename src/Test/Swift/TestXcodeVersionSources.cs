using Winterborn.Library.EasySemVer.CodeReader.Swift;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

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

    /// <summary>MVR-06 / §20 O-01 - build counters are neither read nor written.</summary>
    [Fact]
    public void BuildCounterIsLeftAlone()
    {
        var path = this.Write("project.pbxproj", Pbxproj);

        new MarketingVersionSource(path, "App.xcodeproj/project.pbxproj").Write(new Version("2.0.0"));

        Assert.Contains("CURRENT_PROJECT_VERSION = 87;", File.ReadAllText(path));
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
        version.Increment(Winterborn.Library.EasySemVer.DataObject.VersionType.Patch);
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

    [Fact]
    public void TargetsAreReadFromTheXcodebuildListing()
    {
        const string listing = """
            {
              "project" : {
                "configurations" : [ "Debug", "Release" ],
                "name" : "App",
                "schemes" : [ "App" ],
                "targets" : [ "AppTests", "App" ]
              }
            }
            """;

        Assert.Equal(["App", "AppTests"], XcodeProject.ReadTargetNames(listing));
    }

    /// <summary>SWD-04 - system, binary, plugin and macro targets are never units.</summary>
    [Fact]
    public void NonSourceSwiftPackageTargetsAreNotUnits()
    {
        const string manifest = """
            {
              "targets" : [
                { "name" : "Widgets", "type" : "regular" },
                { "name" : "WidgetsTests", "type" : "test" },
                { "name" : "CLib", "type" : "system" },
                { "name" : "Prebuilt", "type" : "binary" },
                { "name" : "Gen", "type" : "plugin" }
              ]
            }
            """;

        Assert.Equal(["Widgets", "WidgetsTests"], SwiftPackageManifest.ReadTargetNames(manifest));
    }
}
