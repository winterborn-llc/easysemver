using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;
using Winterborn.Tools.EasySemVer.Settings;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test;

/// <summary>
/// TAG-01 (§20 O-02, confirmed 2026-08-17). Creating a tag is the only outward-facing act this tool
/// can take, so these assert what it does and - more importantly - what it will not do: no tag
/// without the flag, no push ever, and no failure when the tag is already there.
/// </summary>
public class TestGitTagWriting
{
    /// <summary>Records every command instead of running one, so nothing here touches a repository.</summary>
    private class RecordingProcess(string tagListOutput = "") : IRunProcess
    {
        internal List<string> Commands { get; } = [];

        public ProcessResult Run(
            string executable,
            IReadOnlyList<string> arguments,
            string workingDirectory,
            TimeSpan timeout)
        {
            var command = $"{executable} {string.Join(' ', arguments)}";
            this.Commands.Add(command);

            var isTagList = arguments.Count > 1 && arguments[1] == "--list";
            return new ProcessResult
            {
                ExitCode = 0,
                StandardOutput = isTagList ? tagListOutput : string.Empty,
                StandardError = string.Empty
            };
        }
    }

    [Fact]
    public void TheFlagIsOffUnlessAskedFor()
    {
        Assert.False(RunOptions.Parse(".").WritesGitTag);
        Assert.True(RunOptions.Parse(".", "--tag").WritesGitTag);
    }

    [Fact]
    public void OptedInItCreatesALocalTagNamedForTheVersion()
    {
        var process = new RecordingProcess();
        var source = new GitTagVersionSource(process, ".", isWritable: true);

        source.Write(new Version("4.5.6"));

        Assert.Contains("git tag v4.5.6", process.Commands);
    }

    /// <summary>
    /// The promise that makes TAG-01 acceptable at all. A local tag is deletable by whoever ran the
    /// command; a pushed one is not, and nothing here may push.
    /// </summary>
    [Fact]
    public void ItNeverPushes()
    {
        var process = new RecordingProcess();
        var source = new GitTagVersionSource(process, ".", isWritable: true);

        source.Write(new Version("4.5.6"));

        Assert.DoesNotContain(process.Commands, c => c.Contains("push", StringComparison.Ordinal));
    }

    /// <summary>
    /// A re-run recomputes the same version from the same source, so the tag it wants already
    /// exists. `git tag` fails on that, and failing the run over a tag that already says the right
    /// thing would make a repeat invocation an error.
    /// </summary>
    [Fact]
    public void AnExistingTagIsLeftAloneRatherThanFailing()
    {
        var process = new RecordingProcess(tagListOutput: "v4.5.6\n");
        var source = new GitTagVersionSource(process, ".", isWritable: true);

        source.Write(new Version("4.5.6"));

        Assert.DoesNotContain("git tag v4.5.6", process.Commands);
    }

    [Fact]
    public void WithoutTheFlagNothingIsRun()
    {
        var process = new RecordingProcess();
        var source = new GitTagVersionSource(process, ".", isWritable: false);

        source.Write(new Version("4.5.6"));

        Assert.Empty(process.Commands);
    }

    /// <summary>A Go module's only version location is the tag, so it must actually reach one.</summary>
    [Fact]
    public void AGoModuleGetsAWritableTagSourceOnlyWhenOptedIn()
    {
        var root = Directory.CreateTempSubdirectory("easysemver-go").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "svc"));
            File.WriteAllText(Path.Combine(root, "svc", "go.mod"), "module example.com/svc\n\ngo 1.22\n");

            var withoutFlag = LanguageProviders.Find(
                LanguageProviders.Create(new RecordingProcess()), "go")!;
            var optedIn = LanguageProviders.Find(
                LanguageProviders.Create(new RecordingProcess(), writesGitTag: true), "go")!;

            var plainUnit = withoutFlag.Discover(root).Single();
            var taggedUnit = optedIn.Discover(root).Single();

            Assert.Equal("svc", plainUnit.UnitId);
            Assert.All(plainUnit.VersionSources, s => Assert.False(s.IsWritable));
            Assert.Contains(taggedUnit.VersionSources, s => s.IsWritable);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
