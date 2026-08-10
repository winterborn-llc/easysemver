using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Providers;
using Winterborn.Tools.EasySemVer.Reporting;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Reporting;

/// <summary>
/// CLI-10 - what the tool publishes to <c>$GITHUB_OUTPUT</c> and <c>$GITHUB_STEP_SUMMARY</c>.
/// <para>
/// The output names are a published contract (ACT-05): renaming one does not fail a consumer's
/// workflow, it silently empties an expression two steps later. <c>ActionRegression</c> asserts
/// these same names against <c>action.yml</c>, so the two ends cannot drift apart.
/// </para>
/// </summary>
public class TestGitHubActionsReport : IDisposable
{
    private readonly string _temp = Directory.CreateTempSubdirectory("easysemver-github").FullName;

    private string OutputPath => Path.Combine(this._temp, "github-output");

    private string SummaryPath => Path.Combine(this._temp, "github-summary");

    public void Dispose()
    {
        Directory.Delete(this._temp, recursive: true);
        GC.SuppressFinalize(this);
    }

    private static JsonReportDocument Document(
        string oldVersion = "2.3.4",
        string newVersion = "3.0.0",
        bool isDryRun = false,
        params ChangeFinding[] findings)
    {
        return JsonChangeReport.Build(
            new ChangeReport(findings),
            new Version(oldVersion),
            new Version(newVersion),
            isDryRun);
    }

    private static ChangeFinding Finding(
        VersionType impact = VersionType.Major,
        string symbol = "Widgets.Gone") =>
        new()
        {
            LanguageId = CsharpLanguageProvider.CsharpLanguageId,
            UnitId = "Widgets",
            Rule = "TypeRemoved",
            Symbol = symbol,
            Description = "was removed",
            Impact = impact
        };

    /// <summary>Resolves only the variables the test set, exactly as an absent one behaves.</summary>
    private Func<string, string?> Environment(bool output = true, bool summary = true) => name => name switch
    {
        "GITHUB_OUTPUT" when output => this.OutputPath,
        "GITHUB_STEP_SUMMARY" when summary => this.SummaryPath,
        _ => null
    };

    private Dictionary<string, string> ReadOutputs() =>
        File.ReadAllLines(this.OutputPath)
            .Where(line => line.Contains('='))
            .ToDictionary(line => line[..line.IndexOf('=')], line => line[(line.IndexOf('=') + 1)..]);

    // ------------------------------------------------------------------------------------------
    // The outputs
    // ------------------------------------------------------------------------------------------

    /// <summary>ACT-05 - exactly these names, and no others.</summary>
    [Fact]
    public void TheOutputsAreExactlyTheAgreedNames()
    {
        GitHubActionsReport.Write(Document(), "/tmp/report.json", this.Environment());

        Assert.Equal(
            ["change-type", "dry-run", "major", "minor", "old-version", "patch", "report", "version"],
            this.ReadOutputs().Keys.OrderBy(key => key, StringComparer.Ordinal));
    }

    [Fact]
    public void TheVerdictReachesTheOutputs()
    {
        GitHubActionsReport.Write(
            Document(oldVersion: "2.3.4", newVersion: "3.0.0", findings: Finding()),
            "/tmp/report.json",
            this.Environment());

        var outputs = this.ReadOutputs();
        Assert.Equal("major", outputs["change-type"]);
        Assert.Equal("2.3.4", outputs["old-version"]);
        Assert.Equal("3.0.0", outputs["version"]);
        Assert.Equal("false", outputs["dry-run"]);
        Assert.Equal("/tmp/report.json", outputs["report"]);
    }

    /// <summary>REP-02's decomposition, which is the whole reason the three parts are published.</summary>
    [Fact]
    public void TheVersionIsPublishedInPartsAsWellAsWhole()
    {
        GitHubActionsReport.Write(Document(newVersion: "2.4.0"), string.Empty, this.Environment());

        var outputs = this.ReadOutputs();
        Assert.Equal("2", outputs["major"]);
        Assert.Equal("4", outputs["minor"]);
        Assert.Equal("0", outputs["patch"]);
    }

    /// <summary>
    /// REP-06 - the verdict is stated, never inferred. A consumer comparing the two versions across
    /// VER-05's rollover reads this Patch as a Minor; the output says Patch because the run did.
    /// </summary>
    [Fact]
    public void TheChangeTypeSurvivesAnOverflowRollover()
    {
        GitHubActionsReport.Write(
            Document(oldVersion: "1.0.2147483647", newVersion: "1.1.0"),
            string.Empty,
            this.Environment());

        Assert.Equal("patch", this.ReadOutputs()["change-type"]);
    }

    /// <summary>
    /// An output pointing at a path that was never written is worse than an absent one: it breaks
    /// `if: steps.version.outputs.report` as a guard, which is the only use it has.
    /// </summary>
    [Fact]
    public void NoJsonReportMeansNoReportOutput()
    {
        GitHubActionsReport.Write(Document(), string.Empty, this.Environment());

        Assert.DoesNotContain("report", this.ReadOutputs().Keys);
    }

    /// <summary>
    /// CLI-10 - appended, never truncated. A job may run the tool more than once, and the file is
    /// shared with every other step in the job.
    /// </summary>
    [Fact]
    public void TwoRunsInOneJobBothPublish()
    {
        GitHubActionsReport.Write(Document(newVersion: "3.0.0"), string.Empty, this.Environment());
        GitHubActionsReport.Write(Document(newVersion: "4.0.0"), string.Empty, this.Environment());

        var lines = File.ReadAllLines(this.OutputPath);
        Assert.Contains("version=3.0.0", lines);
        Assert.Contains("version=4.0.0", lines);
    }

    // ------------------------------------------------------------------------------------------
    // The job summary
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void TheSummaryLeadsWithTheVersionTransitionAndTheVerdict()
    {
        GitHubActionsReport.Write(
            Document(oldVersion: "2.3.4", newVersion: "3.0.0", findings: Finding()),
            string.Empty,
            this.Environment());

        Assert.StartsWith(
            "### EasySemVer: 2.3.4 → 3.0.0 (major)",
            File.ReadAllText(this.SummaryPath));
    }

    /// <summary>REP-09 - the evidence, so a version nobody expected can be accounted for.</summary>
    [Fact]
    public void TheSummaryListsTheFindingsBehindTheVerdict()
    {
        GitHubActionsReport.Write(
            Document(findings: [Finding(symbol: "Widgets.Gone"), Finding(VersionType.Minor, "Widgets.Added")]),
            string.Empty,
            this.Environment());

        var summary = File.ReadAllText(this.SummaryPath);
        Assert.Contains("- **major** `csharp/TypeRemoved` `Widgets.Gone` was removed", summary);
        Assert.Contains("- **minor** `csharp/TypeRemoved` `Widgets.Added` was removed", summary);
    }

    /// <summary>
    /// CLS-04's fail-safe can raise the floor with no symbol to name, so the summary says what was
    /// found rather than restating the verdict as though the two were the same thing.
    /// </summary>
    [Fact]
    public void NoFindingsSaysSoRatherThanShowingAnEmptyList()
    {
        GitHubActionsReport.Write(Document(), string.Empty, this.Environment());

        Assert.Contains("No public API changes were detected.", File.ReadAllText(this.SummaryPath));
    }

    /// <summary>A summary that did not say so would read as a release that happened.</summary>
    [Fact]
    public void ADryRunIsMarkedInTheSummary()
    {
        GitHubActionsReport.Write(Document(isDryRun: true), string.Empty, this.Environment());

        Assert.Contains("dry run", File.ReadAllText(this.SummaryPath));
        Assert.Equal("true", this.ReadOutputs()["dry-run"]);
    }

    // ------------------------------------------------------------------------------------------
    // Off a runner
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// CLI-10 - a missing destination is a skip, never a failure. The versioning run has already
    /// succeeded by the time this is called; failing a release over a report would be the wrong
    /// trade, and REP-08 is the requirement that says which way round it goes.
    /// </summary>
    [Fact]
    public void AMissingDestinationIsSkippedRatherThanFailing()
    {
        GitHubActionsReport.Write(Document(), string.Empty, _ => null);

        GitHubActionsReport.Write(Document(), string.Empty, this.Environment(output: false));
        Assert.False(File.Exists(this.OutputPath));
        Assert.True(File.Exists(this.SummaryPath));
    }
}
