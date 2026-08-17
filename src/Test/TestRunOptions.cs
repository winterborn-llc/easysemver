using Winterborn.Tools.EasySemVer.Settings;

namespace Test;

/// <summary>The CLI contract of §4: FLD-01 (was G-06), CLI-02, CLI-03, and the O-04 dry run.</summary>
public class TestRunOptions
{
    [Fact]
    public void SingleArgumentIsTheFolderRoot()
    {
        var folder = Directory.CreateTempSubdirectory("easysemver-cli").FullName;
        try
        {
            var options = RunOptions.Parse(folder);

            Assert.Equal(new DirectoryInfo(folder).FullName, options.FolderRoot);
            Assert.NotEqual(Environment.CurrentDirectory, options.FolderRoot);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void NoArgumentsMeansTheCurrentDirectory()
    {
        Assert.Equal(
            new DirectoryInfo(Environment.CurrentDirectory).FullName,
            RunOptions.Parse().FolderRoot);
    }

    [Fact]
    public void TwoDirectoriesIsAnError()
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse(".", "."));
    }

    [Fact]
    public void MissingDirectoryIsAnError()
    {
        Assert.Throws<InvalidOperationException>(
            () => RunOptions.Parse(Path.Combine(Path.GetTempPath(), "easysemver-does-not-exist")));
    }

    [Fact]
    public void DryRunIsOffByDefault()
    {
        Assert.False(RunOptions.Parse().IsDryRun);
    }

    [Fact]
    public void JsonFlagTakesTheFollowingArgumentAsItsPath()
    {
        var options = RunOptions.Parse("--json", "out/report.json");

        Assert.Equal("out/report.json", options.JsonReportPath);
        Assert.Equal(new DirectoryInfo(Environment.CurrentDirectory).FullName, options.FolderRoot);
    }

    /// <summary>The path is not mistaken for the folder, whichever order the two arrive in.</summary>
    [Fact]
    public void JsonPathAndFolderCoexist()
    {
        var folder = Directory.CreateTempSubdirectory("easysemver-cli").FullName;
        try
        {
            var options = RunOptions.Parse(folder, "--json", "report.json");

            Assert.Equal(new DirectoryInfo(folder).FullName, options.FolderRoot);
            Assert.Equal("report.json", options.JsonReportPath);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void JsonFlagWithoutAPathIsAnError()
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse("--json"));
    }

    [Fact]
    public void NoJsonFlagMeansNoReport()
    {
        Assert.Equal(string.Empty, RunOptions.Parse().JsonReportPath);
    }

    [Fact]
    public void UnknownOptionIsRejected()
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse("--dryrun"));
    }

    /// <summary>The ceilings are opt-in: absent flags mean no ceiling, not a platform default.</summary>
    [Fact]
    public void SegmentCeilingsAreAbsentByDefault()
    {
        var options = RunOptions.Parse();

        Assert.Null(options.MaximumMinor);
        Assert.Null(options.MaximumPatch);
    }

    [Fact]
    public void SegmentCeilingsAreRead()
    {
        var options = RunOptions.Parse("--max-minor", "65535", "--max-patch", "255");

        Assert.Equal(65535, options.MaximumMinor);
        Assert.Equal(255, options.MaximumPatch);
    }

    /// <summary>Either ceiling stands alone - a target may constrain one segment and not the other.</summary>
    [Fact]
    public void OneCeilingCanBeSetWithoutTheOther()
    {
        var options = RunOptions.Parse("--max-patch", "255");

        Assert.Null(options.MaximumMinor);
        Assert.Equal(255, options.MaximumPatch);
    }

    [Theory]
    [InlineData("--max-patch", "255.0")]
    [InlineData("--max-patch", "-1")]
    [InlineData("--max-minor", "lots")]
    public void ACeilingThatIsNotAWholeNumberIsAnError(string flag, string value)
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse(flag, value));
    }

    [Fact]
    public void CeilingFlagWithoutAValueIsAnError()
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse("--max-patch"));
    }

    /// <summary>CLI-12 - repeatable, so several excluded names can be kept in one run.</summary>
    [Fact]
    public void KeptDirectoryNamesAccumulate()
    {
        var options = RunOptions.Parse("--do-not-exclude", "Pods", "--do-not-exclude", "build");

        Assert.Equal(["Pods", "build"], options.DoNotExclude);
    }

    [Fact]
    public void NothingIsKeptByDefault()
    {
        Assert.Empty(RunOptions.Parse().DoNotExclude);
    }

    /// <summary>The exclusion matches one path segment, so a path could never match it.</summary>
    [Theory]
    [InlineData("Pods/Alamofire")]
    [InlineData("a\\b")]
    public void KeepingAPathRatherThanANameIsAnError(string value)
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse("--do-not-exclude", value));
    }

    [Fact]
    public void KeepFlagWithoutAValueIsAnError()
    {
        Assert.Throws<InvalidOperationException>(() => RunOptions.Parse("--do-not-exclude"));
    }

    /// <summary>A ceiling's value must not be mistaken for the folder argument.</summary>
    [Fact]
    public void CeilingValueIsNotMistakenForTheFolder()
    {
        var options = RunOptions.Parse("--max-patch", "255");

        Assert.Equal(new DirectoryInfo(Environment.CurrentDirectory).FullName, options.FolderRoot);
    }

    [Fact]
    public void DryRunFlagIsNotMistakenForTheFolder()
    {
        var options = RunOptions.Parse("--dry-run");

        Assert.True(options.IsDryRun);
        Assert.Equal(new DirectoryInfo(Environment.CurrentDirectory).FullName, options.FolderRoot);
    }

    // ------------------------------------------------------------------------------------------
    // CLI-10 - the GitHub Actions surface
    // ------------------------------------------------------------------------------------------

    private static RunOptions Parse(string? githubActions, params string[] args) =>
        RunOptions.Parse(name => name == "GITHUB_ACTIONS" ? githubActions : null, args);

    /// <summary>
    /// CLI-10 - detected, not asked for. A flag you must remember is a flag you will forget, and
    /// the failure is silent: an empty `steps.version.outputs.version` two steps later.
    /// </summary>
    [Fact]
    public void RunningUnderGitHubActionsIsDetected()
    {
        Assert.True(Parse("true").WritesGitHubActionsReport);
    }

    [Fact]
    public void AnythingButTrueIsNotGitHubActions()
    {
        Assert.False(Parse(null).WritesGitHubActionsReport);
        Assert.False(Parse("false").WritesGitHubActionsReport);
        Assert.False(Parse(string.Empty).WritesGitHubActionsReport);
    }

    /// <summary>
    /// GitHub sets it lower case, but the variable is not ours and reading it strictly would make
    /// the surface vanish silently rather than fail - the one failure mode CLI-10 exists to avoid.
    /// </summary>
    [Fact]
    public void TheDetectionIsCaseInsensitive()
    {
        Assert.True(Parse("TRUE").WritesGitHubActionsReport);
    }

    /// <summary>CLI-10 - an explicit flag wins over the detection, in both directions.</summary>
    [Fact]
    public void TheFlagsOverrideTheDetectionBothWays()
    {
        Assert.True(Parse(null, "--github").WritesGitHubActionsReport);
        Assert.False(Parse("true", "--no-github").WritesGitHubActionsReport);
    }

    /// <summary>The last flag wins, so a wrapper can override what it was handed.</summary>
    [Fact]
    public void TheLastGitHubFlagWins()
    {
        Assert.False(Parse(null, "--github", "--no-github").WritesGitHubActionsReport);
        Assert.True(Parse(null, "--no-github", "--github").WritesGitHubActionsReport);
    }

    /// <summary>CLI-02 again - neither flag is mistaken for the folder argument.</summary>
    [Fact]
    public void TheGitHubFlagsAreNotMistakenForTheFolder()
    {
        var options = Parse(null, "--github", "--json", "report.json");

        Assert.Equal(new DirectoryInfo(Environment.CurrentDirectory).FullName, options.FolderRoot);
        Assert.Equal("report.json", options.JsonReportPath);
    }
}
