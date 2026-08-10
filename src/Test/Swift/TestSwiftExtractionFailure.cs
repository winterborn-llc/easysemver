using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Swift;

/// <summary>
/// TST-M7 - with Swift units present and the toolchain unavailable, the run fails, names the unit
/// and the command, and leaves every file on disk byte-identical. D-03 is explicit that there is
/// no skip-and-warn here: a partial baseline would silently under-report the next change.
/// </summary>
public class TestSwiftExtractionFailure
{
    private class StubProcessRunner(ProcessResult result) : IRunProcess
    {
        internal List<string> Commands { get; } = [];

        public ProcessResult Run(
            string executable,
            IReadOnlyList<string> arguments,
            string workingDirectory,
            TimeSpan timeout)
        {
            var commandLine = $"{executable} {string.Join(' ', arguments)}";
            this.Commands.Add(commandLine);
            return new ProcessResult
            {
                CommandLine = commandLine,
                ExitCode = result.ExitCode,
                StandardOutput = result.StandardOutput,
                StandardError = result.StandardError,
                WasExecutableFound = result.WasExecutableFound,
                DidTimeOut = result.DidTimeOut
            };
        }
    }

    private static ProcessResult CommandNotFound => new() { WasExecutableFound = false };

    private static ProcessResult NonZeroExit => new()
    {
        ExitCode = 70,
        StandardError = "error: no such module 'Missing'"
    };

    private static ProcessResult TimedOut => new() { DidTimeOut = true };

    public static TheoryData<ProcessResult> EveryFailureMode() =>
    [
        CommandNotFound,
        NonZeroExit,
        TimedOut
    ];

    [Theory]
    [MemberData(nameof(EveryFailureMode))]
    public void DiscoveryFailsWhenTheToolchainCannotRun(ProcessResult failure)
    {
        using var fixture = new SwiftPackageFixture();
        var runner = new StubProcessRunner(failure);

        var exception = Assert.ThrowsAny<Exception>(
            () => new SwiftLanguageProvider(runner, VersionSourceFactories.Create(runner)).Discover(fixture.FolderRoot));

        Assert.Contains("swift package dump-package", exception.Message);
        Assert.Contains("SwiftPackage", exception.Message);
    }

    [Fact]
    public void FailureMessageNamesTheUnitTheCommandAndTheToolsStderr()
    {
        using var fixture = new SwiftPackageFixture();
        var runner = new StubProcessRunner(NonZeroExit);

        var exception = Assert.ThrowsAny<Exception>(
            () => new SwiftLanguageProvider(runner, VersionSourceFactories.Create(runner)).Discover(fixture.FolderRoot));

        Assert.Contains("SwiftPackage", exception.Message);
        Assert.Contains("swift package dump-package", exception.Message);
        Assert.Contains("exited with code 70", exception.Message);
        Assert.Contains("no such module 'Missing'", exception.Message);
    }

    [Fact]
    public void NothingOnDiskIsTouchedWhenExtractionFails()
    {
        using var fixture = new SwiftPackageFixture();
        var before = Snapshot(fixture.FolderRoot);

        Assert.ThrowsAny<Exception>(
            () => new SwiftLanguageProvider(
                    new StubProcessRunner(CommandNotFound),
                    VersionSourceFactories.Create(new StubProcessRunner(CommandNotFound)))
                .Discover(fixture.FolderRoot));

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

        var exitCode = RunWithStubbedSwift(fixture.FolderRoot, CommandNotFound);

        Assert.Equal(1, exitCode);
        Assert.Equal(before, Snapshot(fixture.FolderRoot));
        Assert.False(File.Exists(Path.Combine(fixture.FolderRoot, "EasySemVer.xml")));
    }

    private static int RunWithStubbedSwift(string folderRoot, ProcessResult failure)
    {
        try
        {
            Winterborn.Tools.EasySemVer.Evaluation.VersioningRun.Execute(
                Winterborn.Tools.EasySemVer.Settings.RunOptions.Parse(folderRoot),
                LanguageProviders.Create(new StubProcessRunner(failure)));
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
