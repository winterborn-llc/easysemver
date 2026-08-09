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

    [Fact]
    public void DryRunFlagIsNotMistakenForTheFolder()
    {
        var options = RunOptions.Parse("--dry-run");

        Assert.True(options.IsDryRun);
        Assert.Equal(new DirectoryInfo(Environment.CurrentDirectory).FullName, options.FolderRoot);
    }
}
