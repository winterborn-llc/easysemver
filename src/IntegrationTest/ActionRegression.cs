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

    private string GithubStepSummary => Path.Combine(this._temp, "github-step-summary");

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

    /// <summary>
    /// The name a self-contained publish gives the executable, read from the project rather than
    /// restated, because the Action has to reach for that exact filename inside the archive.
    /// </summary>
    private static string AssemblyName =>
        Regex.Match(
            File.ReadAllText(Path.Combine(RepositoryRoot, "src", "EasySemVer", "EasySemVer.csproj")),
            @"<AssemblyName>(.+?)</AssemblyName>").Groups[1].Value;

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
        var toolPath = Path.Combine(AppContext.BaseDirectory, AssemblyName);

        var published = Path.Combine(staging, AssemblyName);
        File.WriteAllText(published, $"#!/bin/bash\nexec \"{toolPath}\" \"$@\"\n");
        Process.Start("chmod", ["+x", published])!.WaitForExit();

        var archive = $"easysemver-{rid}.tar.gz";
        Process.Start(new ProcessStartInfo("tar",
            ["-czf", Path.Combine(staging, archive), "-C", staging, AssemblyName]))!
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

        // Both destinations CLI-10 writes to. The runner provides them; the Action never names
        // them, because it no longer does the writing.
        ["GITHUB_OUTPUT"] = this.GithubOutput,
        ["GITHUB_STEP_SUMMARY"] = this.GithubStepSummary,
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

    /// <summary>
    /// ACT-11's environment: what the commit step reads, all of it from the run step's outputs
    /// rather than from the inputs, so it describes what happened.
    /// </summary>
    private Dictionary<string, string> CommitEnvironment(
        string folder,
        string commit,
        string tag,
        string branch = "main")
    {
        return new Dictionary<string, string>
        {
            ["EASYSEMVER_COMMIT"] = commit,
            ["EASYSEMVER_TAG"] = tag,

            // Empty, as it is in the one-step form: the report comes from the run step. The
            // two-step form is the same step with this set instead, which
            // TheTwoStepFormCommitsTheEarlierVerdict exercises.
            ["EASYSEMVER_REPORT"] = string.Empty,
            ["EASYSEMVER_RUN_REPORT"] = this.PublishedOutputs().GetValueOrDefault("report", string.Empty),
            ["GITHUB_REF_NAME"] = branch,

            // The step runs where the workspace is, which for a caller is the checkout.
            ["EASYSEMVER_WORKING_DIRECTORY"] = folder
        };
    }

    private (int ExitCode, string Output) Git(string workingDirectory, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    /// <summary>
    /// A folder that is a real git repository with a real remote, so the commit step's push is
    /// exercised rather than stubbed. A bare repository beside it stands in for GitHub, which is
    /// enough: `git push` cannot tell the difference, and the ref-level behaviour under test -
    /// atomicity, rejection on a diverged branch - is git's, not GitHub's.
    /// </summary>
    private string CreateRepository(string version = "2.3.4")
    {
        var folder = this.CreateFolder(version);
        var remote = Path.Combine(this._temp, Guid.NewGuid().ToString("N") + ".git");

        this.Git(this._temp, "init", "--bare", "--initial-branch=main", remote);
        this.Git(folder, "init", "--initial-branch=main");
        this.Git(folder, "config", "user.name", "Fixture");
        this.Git(folder, "config", "user.email", "fixture@example.invalid");
        this.Git(folder, "remote", "add", "origin", remote);
        this.Git(folder, "add", "-A");
        this.Git(folder, "commit", "-m", "Initial");
        this.Git(folder, "push", "-u", "origin", "main");

        // What actions/checkout leaves behind, and the reason the step pushes `HEAD:<branch>`.
        this.Git(folder, "checkout", "--detach", "HEAD");
        return folder;
    }

    /// <summary>Runs the commit step (ACT-11) against a folder a real run has just versioned.</summary>
    private (int ExitCode, string Output) RunCommitStep(
        string folder,
        string commit,
        string tag,
        string branch = "main")
    {
        var environment = this.CommitEnvironment(folder, commit, tag, branch);
        var script = "cd \"$EASYSEMVER_WORKING_DIRECTORY\"\n" + Script(2);
        return this.RunScript(script, environment);
    }

    private string RemoteOf(string folder) =>
        this.Git(folder, "remote", "get-url", "origin").Output.Trim();

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

    /// <summary>
    /// ACT-02 - the Action reaches into the archive for one exact filename, and that name is the
    /// project's <c>AssemblyName</c>. Renaming the assembly without editing the Action produces
    /// the nastiest failure this thing has: the download succeeds, the archive unpacks, and then
    /// there is nothing there to run. It has happened once already.
    /// </summary>
    [Fact]
    public void TheActionUnpacksTheNameTheProjectActuallyPublishes()
    {
        Assert.Contains($"/{AssemblyName}$exe", Script(0));
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

    /// <summary>
    /// ACT-10 - the steps the README tells other repositories to copy are the steps this repository
    /// actually releases with. Documentation that is not executed rots quietly, and the failure
    /// lands in someone else's workflow, on a copy-paste, in a repository we never see.
    /// <para>
    /// It also pins the `uses:` ref transitively: the README's refs must match ACT-02's default
    /// (asserted above), and the workflow must contain the README's text, so all three name one
    /// release or this fails.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("- name: Compute and apply the version")]
    [InlineData("- name: Commit and tag the release")]
    public void TheDocumentedReleaseStepsAreTheOnesThisRepositoryRuns(string opening)
    {
        var documented = StepStartingWith(
            File.ReadAllText(Path.Combine(RepositoryRoot, "README.md")), opening);

        Assert.False(string.IsNullOrWhiteSpace(documented), $"No step starting '{opening}' in README.md");

        var executed = Dedent(File.ReadAllText(
            Path.Combine(RepositoryRoot, ".github", "workflows", "dotnet.yml")));

        Assert.Contains(documented, executed);
    }

    /// <summary>
    /// One step from the README, taken by its opening line rather than by position, so reordering
    /// the file cannot quietly point this test at a different block.
    /// <para>
    /// A step ends at whichever comes first: the blank line before the next step, or the fence
    /// closing the example. Stopping only at the blank line reads the fence itself into the step -
    /// which is what the first run of this test did, and it failed for that reason rather than for
    /// any drift.
    /// </para>
    /// </summary>
    private static string StepStartingWith(string readme, string opening)
    {
        var start = readme.IndexOf(opening, StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        var end = readme.Length;
        foreach (var terminator in (string[])["\n\n", "\n```"])
        {
            var found = readme.IndexOf(terminator, start, StringComparison.Ordinal);
            if (found >= 0 && found < end)
            {
                end = found;
            }
        }

        return readme[start..end].TrimEnd().ReplaceLineEndings("\n");
    }

    /// <summary>
    /// The workflow indents its steps four spaces to sit under `steps:`; the README does not. That
    /// is the only difference permitted between them, so it is the only one normalised away.
    /// </summary>
    private static string Dedent(string workflow) =>
        string.Join('\n', workflow.ReplaceLineEndings("\n").Split('\n')
            .Select(line => line.StartsWith("    ", StringComparison.Ordinal) ? line[4..] : line));

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
    /// ACT-05 - the outputs `action.yml` promises and the outputs the tool publishes are one set.
    /// They are wired at opposite ends of the file and nothing else connects them: an output added
    /// to the yaml with no publisher behind it is silently empty on a runner, and a name published
    /// by the tool that the yaml does not declare never reaches the caller at all.
    /// </summary>
    [Fact]
    public void TheToolPublishesExactlyTheOutputsTheActionDeclares()
    {
        var result = this.InstallAndRun(this.CreateFolder(), "false");
        Assert.True(result.ExitCode == 0, result.Output);

        Assert.Equal(
            Section(Action, "outputs").Keys.Select(key => (string)key)
                .OrderBy(key => key, StringComparer.Ordinal),
            this.PublishedOutputs().Keys.OrderBy(key => key, StringComparer.Ordinal));
    }

    /// <summary>
    /// ACT-05 - the Action delegates the mapping rather than re-implementing it. It carried its own
    /// `jq` block until CLI-10, and so did every workflow that called the CLI directly; a copy that
    /// reappears here is a second thing to keep correct and will drift.
    /// </summary>
    [Fact]
    public void TheActionAsksTheToolForTheOutputsRatherThanParsingTheReport()
    {
        Assert.Contains("--github", Script(1));
        Assert.DoesNotContain("jq", Script(1));
        Assert.DoesNotContain("GITHUB_OUTPUT", Script(1));
    }

    /// <summary>
    /// CLI-10 - the job summary reaches the runner's file too, with the evidence behind the verdict
    /// (REP-09) and not merely the number.
    /// </summary>
    [Fact]
    public void TheRunPublishesAJobSummaryCarryingTheVerdictAndItsEvidence()
    {
        var result = this.InstallAndRun(this.CreateFolder(), "false");
        Assert.True(result.ExitCode == 0, result.Output);

        var summary = File.ReadAllText(this.GithubStepSummary);
        Assert.Contains("### EasySemVer: 2.3.4 → 2.4.0 (minor)", summary);

        // A first run has no baseline, so NCL-02 names the unit that is new.
        Assert.Contains("Widgets", summary);
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

    // ----------------------------------------------------------------------------------------
    // ACT-11 - commit and tag
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// ACT-11 - the whole point: `commit: true` and `tag: true` replace the dozen lines of git
    /// plumbing every consuming workflow used to carry. The bump is staged, committed, tagged and
    /// pushed, and what lands on the remote is the version the run reported.
    /// </summary>
    [Fact]
    public void CommitAndTagPushTheBumpAndTheTagTogether()
    {
        var folder = this.CreateRepository();

        var run = this.InstallAndRun(folder, "false");
        Assert.True(run.ExitCode == 0, run.Output);

        var result = this.RunCommitStep(folder, commit: "true", tag: "true");
        Assert.True(result.ExitCode == 0, result.Output);

        var remote = this.RemoteOf(folder);
        Assert.Contains("EasySemVer: 2.4.0", this.Git(remote, "log", "-1", "--pretty=%s").Output);
        Assert.Contains("v2.4.0", this.Git(remote, "tag", "--list").Output);

        // The tag names the commit that carries the bump, not the one the run started from.
        Assert.Equal(
            this.Git(remote, "rev-parse", "v2.4.0^{commit}").Output.Trim(),
            this.Git(remote, "rev-parse", "main").Output.Trim());
    }

    /// <summary>
    /// REP-10 is why this is a fix and not a convenience. The old hand-written
    /// `git add EasySemVer.xml src/*/*.csproj` misses a project one level deeper *silently*: green
    /// run, tag pushed, and the commit it points at has no bump in it.
    /// </summary>
    [Fact]
    public void EveryVersionedFileIsStagedIncludingOnesAGlobWouldMiss()
    {
        var folder = this.CreateRepository();
        var nested = Directory.CreateDirectory(Path.Combine(folder, "src", "deep", "Nested")).FullName;
        File.WriteAllText(Path.Combine(nested, "Nested.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>2.3.4</AssemblyVersion>
               </PropertyGroup>
            </Project>
            """);
        this.Git(folder, "add", "-A");
        this.Git(folder, "commit", "-m", "Nested project");
        this.Git(folder, "push", "origin", "HEAD:main");

        var run = this.InstallAndRun(folder, "false");
        Assert.True(run.ExitCode == 0, run.Output);

        var result = this.RunCommitStep(folder, commit: "true", tag: "false");
        Assert.True(result.ExitCode == 0, result.Output);

        var committed = this.Git(this.RemoteOf(folder), "show", "--name-only", "--pretty=", "main").Output;
        Assert.Contains("src/deep/Nested/Nested.csproj", committed);
        Assert.Contains("EasySemVer.xml", committed);
    }

    /// <summary>
    /// TST-05's integration step mutates the working copy on purpose, and a caller's job may build
    /// into the tree as well. Staging by what the run wrote rather than by `git add -u` is what
    /// keeps that debris out of a release commit.
    /// </summary>
    [Fact]
    public void UnrelatedWorkingCopyChangesAreNotSweptIntoTheCommit()
    {
        var folder = this.CreateRepository();

        var run = this.InstallAndRun(folder, "false");
        Assert.True(run.ExitCode == 0, run.Output);

        File.WriteAllText(Path.Combine(folder, "Widget.cs"), "namespace Widgets; public class Widget { int x; }");
        File.WriteAllText(Path.Combine(folder, "test-debris.txt"), "left behind by a test run");

        var result = this.RunCommitStep(folder, commit: "true", tag: "false");
        Assert.True(result.ExitCode == 0, result.Output);

        var committed = this.Git(this.RemoteOf(folder), "show", "--name-only", "--pretty=", "main").Output;
        Assert.DoesNotContain("test-debris.txt", committed);
        Assert.DoesNotContain("Widget.cs", committed);
        Assert.Contains("Widgets.csproj", committed);
    }

    /// <summary>
    /// ACT-11's two-step form, which is the one that matters for any pipeline that builds between
    /// versioning and releasing. The version has to be stamped before the build so the artifacts
    /// carry it, but the commit must not be pushed until the tests have passed - otherwise a
    /// failing test leaves a release commit and a tag with nothing behind them.
    /// <para>
    /// The second invocation is handed the first one's report and commits exactly that verdict,
    /// without downloading or running the tool again.
    /// </para>
    /// </summary>
    [Fact]
    public void TheTwoStepFormCommitsTheEarlierVerdict()
    {
        var folder = this.CreateRepository();

        var run = this.InstallAndRun(folder, "false");
        Assert.True(run.ExitCode == 0, run.Output);
        var report = this.PublishedOutputs()["report"];

        // What a build and its tests would do in between: the version is already on disk.
        Assert.Contains("2.4.0", File.ReadAllText(Path.Combine(folder, "Widgets.csproj")));

        var environment = this.CommitEnvironment(folder, commit: "true", tag: "true");
        environment["EASYSEMVER_REPORT"] = report;
        environment["EASYSEMVER_RUN_REPORT"] = string.Empty;

        var result = this.RunScript("cd \"$EASYSEMVER_WORKING_DIRECTORY\"\n" + Script(2), environment);
        Assert.True(result.ExitCode == 0, result.Output);

        var remote = this.RemoteOf(folder);
        Assert.Contains("EasySemVer: 2.4.0", this.Git(remote, "log", "-1", "--pretty=%s").Output);
        Assert.Contains("v2.4.0", this.Git(remote, "tag", "--list").Output);
    }

    /// <summary>
    /// The install and run steps are skipped when `report:` is set, which is what stops the
    /// two-step form versioning twice in one job - a double bump that would be invisible until
    /// someone noticed the minor number climbing two at a time.
    /// </summary>
    [Fact]
    public void SupplyingAReportSkipsVersioningAltogether()
    {
        foreach (var index in (int[])[0, 1])
        {
            var step = (Dictionary<object, object>)Steps[index];

            Assert.Equal("inputs.report == ''", step["if"]);
        }

        // ...and the commit step is not skipped, because it is what that invocation is for.
        Assert.False(((Dictionary<object, object>)Steps[2]).ContainsKey("if"));
    }

    /// <summary>
    /// `commit: true` with neither a `report:` nor a run behind it is named rather than left to
    /// fail inside `jq` with something about a null input.
    /// </summary>
    [Fact]
    public void CommittingWithNoReportAtAllIsNamed()
    {
        var folder = this.CreateRepository();
        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);

        var environment = this.CommitEnvironment(folder, commit: "true", tag: "false");
        environment["EASYSEMVER_RUN_REPORT"] = string.Empty;

        var result = this.RunScript("cd \"$EASYSEMVER_WORKING_DIRECTORY\"\n" + Script(2), environment);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("No report to commit", result.Output);
    }

    /// <summary>ACT-06 - opt-in. The default leaves the repository exactly as it found it.</summary>
    [Fact]
    public void TheDefaultCommitsNothing()
    {
        var folder = this.CreateRepository();

        var run = this.InstallAndRun(folder, "false");
        Assert.True(run.ExitCode == 0, run.Output);

        var result = this.RunCommitStep(folder, commit: "false", tag: "false");
        Assert.True(result.ExitCode == 0, result.Output);

        Assert.Equal("Initial", this.Git(this.RemoteOf(folder), "log", "-1", "--pretty=%s").Output.Trim());
        Assert.NotEmpty(this.Git(folder, "status", "--porcelain").Output);
    }

    /// <summary>
    /// ACT-11 - `tag: true` alone is rejected rather than quietly tagging whatever HEAD happens to
    /// be, which on a caller's checkout is the commit *before* the bump.
    /// </summary>
    [Fact]
    public void TaggingWithoutCommittingIsRejected()
    {
        var folder = this.CreateRepository();
        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);

        var result = this.RunCommitStep(folder, commit: "false", tag: "true");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("requires commit: true", result.Output);
        Assert.Empty(this.Git(this.RemoteOf(folder), "tag", "--list").Output.Trim());
    }

    /// <summary>
    /// ACT-11 - committing a dry run is a contradiction: CLI-07 wrote nothing. Failing says so;
    /// succeeding silently would leave someone hunting for a release that never happened.
    /// </summary>
    [Fact]
    public void CommittingADryRunIsRejected()
    {
        var folder = this.CreateRepository();
        Assert.True(this.InstallAndRun(folder, "true").ExitCode == 0);

        var result = this.RunCommitStep(folder, commit: "true", tag: "true");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("dry run writes nothing", result.Output);
        Assert.Equal("Initial", this.Git(this.RemoteOf(folder), "log", "-1", "--pretty=%s").Output.Trim());
    }

    /// <summary>ACT-04's rule again - anything but true/false is named, not treated as false.</summary>
    [Theory]
    [InlineData("yes", "false")]
    [InlineData("True", "false")]
    [InlineData("", "false")]
    [InlineData("true", "yes")]
    public void AnUnrecognisedCommitOrTagValueIsRejected(string commit, string tag)
    {
        var folder = this.CreateRepository();
        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);

        var result = this.RunCommitStep(folder, commit, tag);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("must be 'true' or 'false'", result.Output);
        Assert.Equal("Initial", this.Git(this.RemoteOf(folder), "log", "-1", "--pretty=%s").Output.Trim());
    }

    /// <summary>
    /// CI-03's guarantee, at the level that actually provides it. Someone pushing to the branch
    /// mid-run makes the branch push fail; `--atomic` is what stops the tag going anyway and
    /// pointing at a commit the remote never received.
    /// </summary>
    [Fact]
    public void ARejectedBranchPushTakesTheTagWithIt()
    {
        var folder = this.CreateRepository();
        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);

        // A second clone lands a commit on main while this run was working.
        var other = Path.Combine(this._temp, "other");
        this.Git(this._temp, "clone", this.RemoteOf(folder), other);
        this.Git(other, "config", "user.name", "Someone");
        this.Git(other, "config", "user.email", "someone@example.invalid");
        File.WriteAllText(Path.Combine(other, "Other.cs"), "namespace Widgets; public class Other { }");
        this.Git(other, "add", "-A");
        this.Git(other, "commit", "-m", "Landed first");
        Assert.Equal(0, this.Git(other, "push", "origin", "main").ExitCode);

        var result = this.RunCommitStep(folder, commit: "true", tag: "true");

        Assert.Equal(1, result.ExitCode);

        var remote = this.RemoteOf(folder);
        Assert.Empty(this.Git(remote, "tag", "--list").Output.Trim());
        Assert.Equal("Landed first", this.Git(remote, "log", "-1", "--pretty=%s").Output.Trim());
    }

    /// <summary>
    /// A workflow that configured its own identity - a signing bot - keeps it, and needs no input
    /// here to say so. The default is only a default.
    /// </summary>
    [Fact]
    public void AnIdentityTheCallerAlreadySetIsNotOverwritten()
    {
        var folder = this.CreateRepository();
        this.Git(folder, "config", "user.name", "Release Bot");
        this.Git(folder, "config", "user.email", "release@example.invalid");

        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);
        Assert.True(this.RunCommitStep(folder, commit: "true", tag: "false").ExitCode == 0);

        Assert.Equal(
            "Release Bot",
            this.Git(this.RemoteOf(folder), "log", "-1", "--pretty=%an").Output.Trim());
    }

    /// <summary>
    /// The branch comes from the workflow's own ref, not from a hardcoded `main`, which is what
    /// lets the same block be copied into a repository whose default branch is something else.
    /// </summary>
    [Fact]
    public void TheBranchPushedIsTheOneThatTriggeredTheWorkflow()
    {
        var folder = this.CreateRepository();
        this.Git(folder, "checkout", "-b", "release");
        this.Git(folder, "push", "-u", "origin", "release");
        this.Git(folder, "checkout", "--detach", "HEAD");

        Assert.True(this.InstallAndRun(folder, "false").ExitCode == 0);
        Assert.True(this.RunCommitStep(folder, "true", "false", branch: "release").ExitCode == 0);

        var remote = this.RemoteOf(folder);
        Assert.Contains("EasySemVer: 2.4.0", this.Git(remote, "log", "-1", "--pretty=%s", "release").Output);
        Assert.Equal("Initial", this.Git(remote, "log", "-1", "--pretty=%s", "main").Output.Trim());
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
        var apphost = Path.Combine(AppContext.BaseDirectory, AssemblyName);
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
