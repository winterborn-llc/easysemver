using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Swift;

/// <summary>
/// TST-M7 - what SWE-05 still fails on now that no toolchain is involved. A manifest that declares
/// a target whose source is nowhere to be found is a broken package: the run fails, names the
/// target, says where it looked, and leaves every file on disk byte-identical. D-03 is explicit
/// that there is no skip-and-warn here - recording an empty surface for a target that has one
/// would silently under-report the next change.
/// <para>
/// A target with a source directory and no Swift in it is the other case entirely, and is an
/// ordinary Objective-C or C target rather than a failure (O-06).
/// </para>
/// </summary>
public class TestSwiftExtractionFailure
{
    private static SwiftLanguageProvider CreateProvider()
    {
        return new SwiftLanguageProvider(VersionSourceFactories.Create(new ProcessRunner()));
    }

    private static void DeclareTarget(SwiftPackageFixture fixture, string name)
    {
        var manifestPath = Path.Combine(fixture.PackageDirectory, "Package.swift");
        File.WriteAllText(
            manifestPath,
            File.ReadAllText(manifestPath).Replace(
                ".target(name: \"Widgets\"),",
                $".target(name: \"Widgets\"),\n        .target(name: \"{name}\"),"));
    }

    [Fact]
    public void DiscoveryFailsWhenADeclaredTargetHasNoSource()
    {
        using var fixture = new SwiftPackageFixture();
        DeclareTarget(fixture, "Ghost");

        var exception = Assert.Throws<SwiftSourceException>(
            () => CreateProvider().Discover(fixture.FolderRoot));

        Assert.Contains("Ghost", exception.Message);
        Assert.Contains("Sources/Ghost", exception.Message);
    }

    [Fact]
    public void NothingOnDiskIsTouchedWhenExtractionFails()
    {
        using var fixture = new SwiftPackageFixture();
        DeclareTarget(fixture, "Ghost");
        var before = Snapshot(fixture.FolderRoot);

        Assert.ThrowsAny<Exception>(() => CreateProvider().Discover(fixture.FolderRoot));

        Assert.Equal(before, Snapshot(fixture.FolderRoot));
    }

    /// <summary>
    /// The whole run, not just the provider: no baseline is written and no version is stamped
    /// (BAS-06, SWE-05).
    /// </summary>
    [Fact]
    public void TheRunExitsOneAndWritesNothing()
    {
        using var fixture = new SwiftPackageFixture();
        DeclareTarget(fixture, "Ghost");
        File.WriteAllText(
            Path.Combine(fixture.FolderRoot, "Widget.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>1.0.0</AssemblyVersion>
               </PropertyGroup>
            </Project>
            """);
        var before = Snapshot(fixture.FolderRoot);

        var exitCode = Run(fixture.FolderRoot);

        Assert.Equal(1, exitCode);
        Assert.Equal(before, Snapshot(fixture.FolderRoot));
        Assert.False(File.Exists(Path.Combine(fixture.FolderRoot, "EasySemVer.xml")));
    }

    /// <summary>
    /// O-06 - a target that exists but holds no Swift is a unit with no API surface, not a broken
    /// package. It keeps its versions and its disappearance is still a real change.
    /// </summary>
    [Fact]
    public void ATargetWithNoSwiftInItIsAnEmptyModuleRatherThanAFailure()
    {
        using var fixture = new SwiftPackageFixture();
        DeclareTarget(fixture, "CLib");
        Directory.CreateDirectory(Path.Combine(fixture.PackageDirectory, "Sources", "CLib"));
        File.WriteAllText(
            Path.Combine(fixture.PackageDirectory, "Sources", "CLib", "clib.c"),
            "int answer(void) { return 42; }");

        var provider = CreateProvider();
        var unit = provider.Discover(fixture.FolderRoot).First(u => u.DisplayName == "CLib");
        provider.Extract(unit);

        var module = Assert.IsType<SwiftModule>(unit.Signature);
        Assert.Empty(((ISwiftModule)module).Types);
    }

    private static int Run(string folderRoot)
    {
        try
        {
            Winterborn.Tools.EasySemVer.Evaluation.VersioningRun.Execute(
                Winterborn.Tools.EasySemVer.Settings.RunOptions.Parse(folderRoot),
                LanguageProviders.Create(new ProcessRunner()));
            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }

    private static Dictionary<string, string> Snapshot(string folderRoot)
    {
        var contents = new Dictionary<string, string>();
        foreach (var path in Directory.GetFiles(folderRoot, "*", SearchOption.AllDirectories))
        {
            contents[Path.GetRelativePath(folderRoot, path)] = File.ReadAllText(path);
        }

        return contents;
    }
}
