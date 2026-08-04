using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using Xunit;

namespace IntegrationTest;

/// <summary>
/// ACT-01…ACT-09 - the GitHub Action, tested as far as a GitHub Action can be tested off a runner.
/// <para>
/// The `run:` scripts are extracted from <c>action.yml</c> itself rather than copied into this
/// file, so a test can never pass against a script the Action no longer ships. Only the release
/// download is stubbed; the tool the scripts then invoke is the real one, and the exit codes,
/// the report and the published outputs are all real.
/// </para>
/// <para>
/// ACT-09 is the honest boundary: everything the runner owns - the real download, `gh`'s
/// authentication, <c>$GITHUB_OUTPUT</c> becoming step outputs, and every platform but this
/// host's - is not covered here and cannot be.
/// </para>
/// </summary>
[Trait("Toolchain", "Bash")]
public class ActionRegression : IDisposable
{
    private readonly string _temp = Directory.CreateTempSubdirectory("easysemver-action").FullName;

    /// <summary>Where the action's two steps put the binary and the report.</summary>
    private string RunnerTemp => Path.Combine(this._temp, "runner-temp");

    private string GithubPath => Path.Combine(this._temp, "github-path");

    private string GithubOutput => Path.Combine(this._temp, "github-output");

    public void Dispose()
    {
        Directory.Delete(this._temp, recursive: true);
        GC.SuppressFinalize(this);
    }

    // ----------------------------------------------------------------------------------------
    // Reading action.yml
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// ACT-01 - the file is at the repository root, which is what makes
    /// <c>uses: winterborn-llc/easysemver@ref</c> resolve without a subdirectory.
    /// </summary>
    private static string RepositoryRoot
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
                 directory != null;
                 directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "action.yml")))
                {
                    return directory.FullName;
                }
            }

            throw new InvalidOperationException("No action.yml found above " + AppContext.BaseDirectory);
        }
    }

    private static Dictionary<object, object> Action =>
        (Dictionary<object, object>)new DeserializerBuilder().Build().Deserialize<object>(
            File.ReadAllText(Path.Combine(RepositoryRoot, "action.yml")))!;

    private static Dictionary<object, object> Section(Dictionary<object, object> node, string key) =>
        (Dictionary<object, object>)node[key];

    /// <summary>One named input's scalar setting, e.g. the <c>default</c> of <c>folder</c>.</summary>
    private static string Setting(string input, string key) =>
        (string)Section(Section(Action, "inputs"), input)[key];

    private static List<object> Steps =>
        (List<object>)Section(Action, "runs")["steps"];

    /// <summary>The <c>run:</c> body of one step, as it will reach bash on a runner.</summary>
    private static string Script(int index) =>
        (string)((Dictionary<object, object>)Steps[index])["run"];

    // ----------------------------------------------------------------------------------------
    // Running an extracted step
    // ----------------------------------------------------------------------------------------

    private (int ExitCode, string Output) RunScript(string script, Dictionary<string, string> environment)
    {
        var path = Path.Combine(this._temp, "step.sh");
        File.WriteAllText(path, script);

        var start = new ProcessStartInfo("/bin/bash", path)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = this._temp
        };

        foreach (var (key, value) in environment)
        {
            start.Environment[key] = value;
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    /// <summary>
    /// What <c>runner.os</c> and <c>runner.arch</c> would report for the machine running this
    /// test - the one platform the harness can exercise (ACT-09).
    /// </summary>
    private static (string Os, string Arch) ThisRunner => (
        OperatingSystem.IsMacOS() ? "macOS" : OperatingSystem.IsWindows() ? "Windows" : "Linux",
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "ARM64" : "X64");

    /// <summary>
    /// Stands in for the GitHub Release. The archive holds a file under the name a self-contained
    /// publish produces, so the Action's rename to <c>easysemver</c> is exercised for real; the
    /// file execs the tool built alongside this test, so what runs afterwards is the real thing.
    /// </summary>
    private void StubTheRelease(string rid)
    {
        var staging = Directory.CreateDirectory(Path.Combine(this._temp, "staging")).FullName;
        var toolPath = Path.Combine(AppContext.BaseDirectory, "Winterborn.Library.EasySemVer");

        var published = Path.Combine(staging, "Winterborn.Library.EasySemVer");
        File.WriteAllText(published, $"#!/bin/bash\nexec \"{toolPath}\" \"$@\"\n");
        Process.Start("chmod", ["+x", published])!.WaitForExit();

        var archive = $"easysemver-{rid}.tar.gz";
        Process.Start(new ProcessStartInfo("tar",
            ["-czf", Path.Combine(staging, archive), "-C", staging, "Winterborn.Library.EasySemVer"]))!
            .WaitForExit();

        // `gh release download --dir X` is the only thing the install step needs from the network.
        var bin = Directory.CreateDirectory(Path.Combine(this._temp, "stub-bin")).FullName;
        var gh = Path.Combine(bin, "gh");
        File.WriteAllText(gh, $"""
            #!/bin/bash
            for argument in "$@"; do
               [ "$previous" = "--dir" ] && cp "{Path.Combine(staging, archive)}" "$argument"
               previous="$argument"
            done
            """);
        Process.Start("chmod", ["+x", gh])!.WaitForExit();
    }

    private Dictionary<string, string> InstallEnvironment(string os, string arch) => new()
    {
        ["RUNNER_OS"] = os,
        ["RUNNER_ARCH"] = arch,
        ["RUNNER_TEMP"] = this.RunnerTemp,
        ["GITHUB_PATH"] = this.GithubPath,
        ["EASYSEMVER_VERSION"] = "v0.0.0-test",
        ["GH_TOKEN"] = "stub",
        ["PATH"] = Path.Combine(this._temp, "stub-bin") + ":" + Environment.GetEnvironmentVariable("PATH")
    };

    private Dictionary<string, string> RunEnvironment(string folder, string dryRun) => new()
    {
        ["RUNNER_OS"] = ThisRunner.Os,
        ["RUNNER_TEMP"] = this.RunnerTemp,
        ["GITHUB_OUTPUT"] = this.GithubOutput,
        ["EASYSEMVER_FOLDER"] = folder,
        ["EASYSEMVER_DRY_RUN"] = dryRun,

        // What appending to $GITHUB_PATH does for every later step.
        ["PATH"] = Path.Combine(this.RunnerTemp, "easysemver") + ":" +
                   Environment.GetEnvironmentVariable("PATH")
    };

    /// <summary>The <c>key=value</c> lines the run step published to <c>$GITHUB_OUTPUT</c>.</summary>
    private Dictionary<string, string> PublishedOutputs()
    {
        if (!File.Exists(this.GithubOutput))
        {
            return [];
        }

        return File.ReadAllLines(this.GithubOutput)
            .Where(line => line.Contains('='))
            .ToDictionary(line => line[..line.IndexOf('=')], line => line[(line.IndexOf('=') + 1)..]);
    }

    private string CreateFolder(string version = "2.3.4")
    {
        var folder = Directory.CreateDirectory(Path.Combine(this._temp, Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(folder, "Widgets.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>{version}</AssemblyVersion>
               </PropertyGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(folder, "Widget.cs"), "namespace Widgets; public class Widget { }");
        return folder;
    }

    /// <summary>Installs, then runs, returning what the run step published.</summary>
    private (int ExitCode, string Output) InstallAndRun(string folder, string dryRun)
    {
        this.StubTheRelease(RidFor(ThisRunner.Os, ThisRunner.Arch)!);

        var install = this.RunScript(Script(0), this.InstallEnvironment(ThisRunner.Os, ThisRunner.Arch));
        Assert.True(install.ExitCode == 0, install.Output);

        return this.RunScript(Script(1), this.RunEnvironment(folder, dryRun));
    }

    // ----------------------------------------------------------------------------------------
    // ACT-01, ACT-04, ACT-05 - the file's own wiring
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// ACT-05 - an output whose value points at no step, or at a step id that was renamed, is
    /// silently empty on a runner rather than an error, which is exactly why this is asserted.
    /// </summary>
    [Fact]
    public void EveryOutputIsWiredToTheRunStep()
    {
        var outputs = Section(Action, "outputs");

        Assert.Equal(
            ["change-type", "dry-run", "major", "minor", "old-version", "patch", "report", "version"],
            outputs.Keys.Select(key => (string)key).OrderBy(key => key, StringComparer.Ordinal));

        foreach (var (name, definition) in outputs)
        {
            var value = (string)((Dictionary<object, object>)definition)["value"];

            Assert.Equal($"${{{{ steps.run.outputs.{name} }}}}", value);
        }
    }

    /// <summary>ACT-01 - composite, and every step names the shell it runs under.</summary>
    [Fact]
    public void TheActionIsCompositeAndEveryStepNamesItsShell()
    {
        Assert.Equal("composite", Section(Action, "runs")["using"]);

        foreach (var step in Steps.Cast<Dictionary<object, object>>())
        {
            Assert.True(step.ContainsKey("shell"), $"Step '{step.GetValueOrDefault("name")}' names no shell");
        }
    }

    /// <summary>ACT-04 - the two inputs a caller is most likely to rely on, and their defaults.</summary>
    [Fact]
    public void FolderAndDryRunCarryTheDocumentedDefaults()
    {
        Assert.Equal(".", Setting("folder", "default"));
        Assert.Equal("false", Setting("dry-run", "default"));
    }

    /// <summary>
    /// ACT-02 - the version input is a fixed tag. A default of <c>latest</c> would make every
    /// consumer's build change under them the next time this repository cut a release.
    /// </summary>
    [Fact]
    public void TheVersionInputIsPinnedToATagAndDoesNotTrackLatest()
    {
        var version = Setting("version", "default");

        Assert.StartsWith("v", version);
        Assert.DoesNotContain("latest", version);
    }

    /// <summary>
    /// ACT-02 - the pinned default and every <c>uses:</c> in the README name the same release.
    /// They have to move together when a tag is cut, and a README example naming a tag with no
    /// release behind it is a copy-paste that fails in someone else's workflow, not in ours.
    /// </summary>
    [Fact]
    public void TheReadmeExamplesNameThePinnedRelease()
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryRoot, "README.md"));

        var referenced = Regex.Matches(readme, @"winterborn-llc/easysemver@(\S+)")
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .ToList();

        Assert.NotEmpty(referenced);
        Assert.All(referenced, reference => Assert.Equal(Setting("version", "default"), reference));
    }

    // ----------------------------------------------------------------------------------------
    // ACT-03 - the platform table, against the release job that produces the assets
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Asks the shipped platform table what asset it would download, by running the table itself.
    /// Returns null when the Action rejects the platform.
    /// </summary>
    private string? AssetFor(string os, string arch)
    {
        var table = Script(0);
        var start = table.IndexOf("case \"$RUNNER_OS", StringComparison.Ordinal);
        var end = table.IndexOf("\nesac", StringComparison.Ordinal);
        var script = table[start..end] + "\nesac\necho \"easysemver-$rid.$archive\"";

        var result = this.RunScript(script, new Dictionary<string, string>
        {
            ["RUNNER_OS"] = os,
            ["RUNNER_ARCH"] = arch
        });

        return result.ExitCode == 0 ? result.Output.Trim() : null;
    }

    private string? RidFor(string os, string arch) =>
        this.AssetFor(os, arch)?.Replace("easysemver-", "").Replace(".tar.gz", "").Replace(".zip", "");

    private static readonly (string Os, string Arch)[] EveryRunnerPlatform =
    [
        ("Linux", "X64"), ("Linux", "ARM64"), ("macOS", "X64"), ("macOS", "ARM64"),
        ("Windows", "X64"), ("Windows", "ARM64")
    ];

    /// <summary>
    /// ACT-03 - the Action supports exactly the runtimes the release job publishes, read from
    /// that job rather than restated here. Adding a runtime to the matrix without teaching the
    /// Action about it, or dropping one the Action still asks for, fails this test; the failure
    /// mode it prevents is a 404 mid-workflow in someone else's repository.
    /// </summary>
    [Fact]
    public void ThePlatformTableCoversExactlyTheRuntimesTheReleaseJobPublishes()
    {
        var workflow = File.ReadAllText(
            Path.Combine(RepositoryRoot, ".github", "workflows", "dotnet.yml"));

        var line = workflow.Split('\n').Single(text => text.Contains("runtime: ["));
        var published = line[(line.IndexOf('[') + 1)..line.IndexOf(']')]
            .Split(',').Select(runtime => runtime.Trim()).OrderBy(runtime => runtime, StringComparer.Ordinal);

        var supported = EveryRunnerPlatform
            .Select(platform => this.RidFor(platform.Os, platform.Arch))
            .Where(rid => rid != null)
            .Distinct()
            .OrderBy(rid => rid, StringComparer.Ordinal);

        Assert.Equal(published, supported!);
    }

    /// <summary>
    /// ACT-03 - and the asset names match what the release job's archive step actually writes,
    /// including which platform gets a zip rather than a tarball.
    /// </summary>
    [Fact]
    public void WindowsAsksForAZipAndEveryoneElseForATarball()
    {
        Assert.Equal("easysemver-win-x64.zip", this.AssetFor("Windows", "X64"));
        Assert.Equal("easysemver-linux-arm64.tar.gz", this.AssetFor("Linux", "ARM64"));
        Assert.Equal("easysemver-osx-arm64.tar.gz", this.AssetFor("macOS", "ARM64"));
    }

    /// <summary>
    /// ACT-03 - an unsupported runner is named. Falling back to x64 would surface as an
    /// exec-format error several steps later, somewhere that does not mention the architecture.
    /// </summary>
    [Fact]
    public void AnUnsupportedPlatformFailsAndNamesItself()
    {
        var result = this.RunScript(Script(0), this.InstallEnvironment("Windows", "ARM64"));

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Windows/ARM64", result.Output);
    }

    // ----------------------------------------------------------------------------------------
    // ACT-04, ACT-05, ACT-07 - the scripts, run
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// ACT-02, ACT-05 - install then run, end to end: the archive is unpacked, the binary is
    /// renamed to the name the tool is known by, and the verdict published to $GITHUB_OUTPUT
    /// matches the version that actually reached the .csproj.
    /// </summary>
    [Fact]
    public void TheActionInstallsTheBinaryAndPublishesTheVerdict()
    {
        var folder = this.CreateFolder();

        var result = this.InstallAndRun(folder, "false");
        Assert.True(result.ExitCode == 0, result.Output);

        var outputs = this.PublishedOutputs();

        // A first run has no baseline, so every unit is new: Minor (NCL-02).
        Assert.Equal("minor", outputs["change-type"]);
        Assert.Equal("2.3.4", outputs["old-version"]);
        Assert.Equal("2.4.0", outputs["version"]);
        Assert.Equal("false", outputs["dry-run"]);

        // REP-02's decomposition, which is the whole reason major/minor/patch are published.
        Assert.Equal("2", outputs["major"]);
        Assert.Equal("4", outputs["minor"]);
        Assert.Equal("0", outputs["patch"]);

        Assert.True(File.Exists(outputs["report"]));
        Assert.Contains("2.4.0", File.ReadAllText(Path.Combine(folder, "Widgets.csproj")));
    }

    /// <summary>
    /// ACT-05 - `dry-run` is read from the report, not echoed from the input, and CLI-07 means
    /// the folder is untouched.
    /// </summary>
    [Fact]
    public void ADryRunPublishesTheVerdictAndWritesNothing()
    {
        var folder = this.CreateFolder();
        var before = File.ReadAllText(Path.Combine(folder, "Widgets.csproj"));

        var result = this.InstallAndRun(folder, "true");
        Assert.True(result.ExitCode == 0, result.Output);

        var outputs = this.PublishedOutputs();
        Assert.Equal("true", outputs["dry-run"]);
        Assert.Equal("2.4.0", outputs["version"]);

        Assert.Equal(before, File.ReadAllText(Path.Combine(folder, "Widgets.csproj")));
        Assert.False(File.Exists(Path.Combine(folder, "EasySemVer.xml")));
    }

    /// <summary>
    /// ACT-04 - anything but true/false is rejected. Treating `yes` as false would stamp versions
    /// and rewrite the baseline on a workflow whose author asked for the opposite.
    /// </summary>
    [Theory]
    [InlineData("yes")]
    [InlineData("True")]
    [InlineData("")]
    public void AnUnrecognisedDryRunValueIsRejectedRatherThanTreatedAsFalse(string value)
    {
        var folder = this.CreateFolder();

        var result = this.InstallAndRun(folder, value);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("dry-run must be", result.Output);
        Assert.Empty(this.PublishedOutputs());
        Assert.False(File.Exists(Path.Combine(folder, "EasySemVer.xml")));
    }

    /// <summary>
    /// ACT-07 - the tool's exit 1 (CLI-06) fails the step, and because REP-08 leaves no report
    /// there is nothing for a consumer to read a half-truth from.
    /// </summary>
    [Fact]
    public void AFailedRunFailsTheStepAndPublishesNoOutputs()
    {
        var result = this.InstallAndRun(Path.Combine(this._temp, "does-not-exist"), "false");

        Assert.Equal(1, result.ExitCode);
        Assert.Empty(this.PublishedOutputs());
    }

    /// <summary>
    /// ACT-08 - a folder value is named, not run. The value below is a command substitution and a
    /// chained command; if either were interpreted, the marker file would exist.
    /// </summary>
    [Fact]
    public void AHostileFolderValueIsNotExecuted()
    {
        var marker = Path.Combine(this._temp, "executed");

        var result = this.InstallAndRun($"$(touch {marker}); touch {marker}", "false");

        Assert.Equal(1, result.ExitCode);
        Assert.False(File.Exists(marker), "The folder input reached the shell as code");
    }

    /// <summary>
    /// The Action renames the published binary to <c>easysemver</c>. That is only safe because a
    /// .NET apphost embeds the path to its managed assembly rather than deriving it from its own
    /// filename - if that ever stopped being true, every run of the Action would break on the
    /// first invocation, so it is pinned here rather than assumed.
    /// </summary>
    [Fact]
    public void RenamingThePublishedBinaryDoesNotBreakIt()
    {
        var apphost = Path.Combine(AppContext.BaseDirectory, "Winterborn.Library.EasySemVer");
        Assert.True(File.Exists(apphost), $"No apphost at {apphost}");

        // Beside the original, so the assembly and its dependencies resolve exactly as they do
        // inside an unpacked release archive.
        var renamed = Path.Combine(AppContext.BaseDirectory, "easysemver-rename-test");
        File.Copy(apphost, renamed, overwrite: true);
        try
        {
            Process.Start("chmod", ["+x", renamed])!.WaitForExit();

            var start = new ProcessStartInfo(renamed, [this.CreateFolder(), "--dry-run"])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(process.ExitCode == 0, output);
        }
        finally
        {
            File.Delete(renamed);
        }
    }
}
