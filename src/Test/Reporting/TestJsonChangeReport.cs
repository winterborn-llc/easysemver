using System.Text.Json;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Reporting;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Reporting;

/// <summary>REP-01…REP-09 - the machine-readable report's contract.</summary>
public class TestJsonChangeReport
{
    private static ChangeReport Report(params VersionType[] impacts)
    {
        var findings = impacts.Select((impact, index) => new ChangeFinding
        {
            Language = Language.Csharp,
            UnitId = "Widgets",
            RuleId = "R18",
            RuleName = "TypeRemoved",
            Symbol = $"Widgets.Gone{index}",
            Description = "was removed",
            Impact = impact
        });

        return new ChangeReport(findings);
    }

    private static JsonElement Render(
        ChangeReport report,
        string oldVersion = "2.3.4",
        string newVersion = "3.0.0",
        bool isDryRun = false)
    {
        var json = JsonChangeReport.Render(
            JsonChangeReport.Build(report, new Version(oldVersion), new Version(newVersion), isDryRun));
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    [Fact]
    public void TheDocumentIsExactlyTheAgreedShape()
    {
        var root = Render(Report(VersionType.Major));

        Assert.Equal(
            ["formatVersion", "dryRun", "changeType", "oldVersion", "newVersion", "findings"],
            root.EnumerateObject().Select(p => p.Name));
    }

    /// <summary>
    /// REP-05 - the fields that were weighed and deliberately left out stay out. Findings were
    /// among them until REP-09 gave them a consumer; discovered units and the written-file list
    /// were not, and are still absent on the original grounds.
    /// </summary>
    [Theory]
    [InlineData("units")]
    [InlineData("written")]
    [InlineData("folderRoot")]
    public void TheOmittedFieldsAreAbsent(string name)
    {
        Assert.False(Render(Report(VersionType.Major)).TryGetProperty(name, out _));
    }

    [Fact]
    public void FormatVersionIsOne()
    {
        Assert.Equal(1, Render(Report()).GetProperty("formatVersion").GetInt32());
    }

    /// <summary>REP-02 - enum-valued fields are lower case, because this is a wire format.</summary>
    [Theory]
    [InlineData(VersionType.Major, "major")]
    [InlineData(VersionType.Minor, "minor")]
    [InlineData(VersionType.Patch, "patch")]
    public void ChangeTypeIsLowerCase(VersionType impact, string expected)
    {
        Assert.Equal(expected, Render(Report(impact)).GetProperty("changeType").GetString());
    }

    [Fact]
    public void NoFindingsMeansPatch()
    {
        Assert.Equal("patch", Render(Report()).GetProperty("changeType").GetString());
    }

    [Fact]
    public void BothVersionsShareOneShape()
    {
        var root = Render(Report(), oldVersion: "2.3.4", newVersion: "3.0.0");

        foreach (var name in (string[])["oldVersion", "newVersion"])
        {
            Assert.Equal(
                ["version", "major", "minor", "patch"],
                root.GetProperty(name).EnumerateObject().Select(p => p.Name));
        }

        var older = root.GetProperty("oldVersion");
        Assert.Equal("2.3.4", older.GetProperty("version").GetString());
        Assert.Equal(2, older.GetProperty("major").GetInt32());
        Assert.Equal(3, older.GetProperty("minor").GetInt32());
        Assert.Equal(4, older.GetProperty("patch").GetInt32());
    }

    /// <summary>
    /// VER-07 - a version may carry more than three segments. The string is the full truth; the
    /// three numbers are its first three.
    /// </summary>
    [Fact]
    public void AFourSegmentVersionKeepsItsStringIntact()
    {
        var version = Render(Report(), newVersion: "1.2.3.4").GetProperty("newVersion");

        Assert.Equal("1.2.3.4", version.GetProperty("version").GetString());
        Assert.Equal(1, version.GetProperty("major").GetInt32());
        Assert.Equal(3, version.GetProperty("patch").GetInt32());
    }

    [Fact]
    public void DryRunIsReported()
    {
        Assert.True(Render(Report(), isDryRun: true).GetProperty("dryRun").GetBoolean());
        Assert.False(Render(Report(), isDryRun: false).GetProperty("dryRun").GetBoolean());
    }

    /// <summary>
    /// REP-06 - the verdict is stated, not inferred. VER-05's rollover makes deriving it from the
    /// two versions wrong in both directions, so these are the cases that would mislead a
    /// consumer who tried.
    /// </summary>
    [Fact]
    public void TheVerdictSurvivesAnOverflowRolloverThatWouldMisleadAnInference()
    {
        var patch = Render(Report(), oldVersion: "1.0.2147483647", newVersion: "1.1.0");
        Assert.Equal("patch", patch.GetProperty("changeType").GetString());

        var minor = Render(Report(VersionType.Minor), oldVersion: "1.2147483647.123", newVersion: "2.0.0");
        Assert.Equal("minor", minor.GetProperty("changeType").GetString());
    }

    /// <summary>REP-07 - identical input produces byte-identical output.</summary>
    [Fact]
    public void RenderingIsDeterministic()
    {
        var first = JsonChangeReport.Render(
            JsonChangeReport.Build(Report(VersionType.Major, VersionType.Minor),
                new Version("2.3.4"), new Version("3.0.0"), isDryRun: false));
        var second = JsonChangeReport.Render(
            JsonChangeReport.Build(Report(VersionType.Major, VersionType.Minor),
                new Version("2.3.4"), new Version("3.0.0"), isDryRun: false));

        Assert.Equal(first, second);
    }

    // --------------------------------------------------------------------------------------
    // REP-09 - findings
    // --------------------------------------------------------------------------------------

    private static JsonElement OnlyFinding(ChangeReport report) =>
        Render(report).GetProperty("findings").EnumerateArray().Single();

    [Fact]
    public void AFindingIsExactlyTheAgreedShape()
    {
        Assert.Equal(
            ["ruleId", "impact", "language", "unitId", "symbol", "description"],
            OnlyFinding(Report(VersionType.Major)).EnumerateObject().Select(p => p.Name));
    }

    /// <summary>
    /// REP-09 - the rule id is the point of the array: it is what lets a consumer name the rule
    /// that cost it a Major without matching on prose.
    /// </summary>
    [Fact]
    public void AFindingCarriesItsRuleIdAndTheRestOfItsEvidence()
    {
        var finding = OnlyFinding(Report(VersionType.Major));

        Assert.Equal("R18", finding.GetProperty("ruleId").GetString());
        Assert.Equal("major", finding.GetProperty("impact").GetString());
        Assert.Equal("Widgets", finding.GetProperty("unitId").GetString());
        Assert.Equal("Widgets.Gone0", finding.GetProperty("symbol").GetString());
        Assert.Equal("was removed", finding.GetProperty("description").GetString());
    }

    /// <summary>
    /// REP-09 - the rule's class name is an implementation detail. A consumer keyed to it would
    /// break on a rename that changed no behaviour, which is what the id exists to prevent.
    /// </summary>
    [Fact]
    public void TheRuleClassNameIsNotPublished()
    {
        Assert.False(OnlyFinding(Report(VersionType.Major)).TryGetProperty("ruleName", out _));

        Assert.DoesNotContain("TypeRemoved", JsonChangeReport.Render(
            JsonChangeReport.Build(Report(VersionType.Major),
                new Version("2.3.4"), new Version("3.0.0"), isDryRun: false)));
    }

    /// <summary>REP-02 - impact and language are lower case, like every enum-valued field.</summary>
    [Theory]
    [InlineData(Language.Csharp, "csharp")]
    [InlineData(Language.Swift, "swift")]
    public void ImpactAndLanguageAreLowerCase(Language language, string expected)
    {
        var report = new ChangeReport([
            new ChangeFinding { Language = language, Impact = VersionType.Minor, UnitId = "U" }
        ]);

        var finding = OnlyFinding(report);
        Assert.Equal(expected, finding.GetProperty("language").GetString());
        Assert.Equal("minor", finding.GetProperty("impact").GetString());
    }

    /// <summary>
    /// REP-09 - present and empty rather than absent, so that "no changes" and "an older writer"
    /// are never the same observation.
    /// </summary>
    [Fact]
    public void FindingsArePresentAndEmptyWhenNothingWasFound()
    {
        var findings = Render(Report()).GetProperty("findings");

        Assert.Equal(JsonValueKind.Array, findings.ValueKind);
        Assert.Empty(findings.EnumerateArray());
    }

    /// <summary>
    /// REP-09 and REP-06 - CLS-04's fail-safe raises the floor when there is no comparable
    /// baseline, and there is no symbol to name. A consumer inferring the verdict from an empty
    /// array would get this exactly wrong, which is why the verdict is stated.
    /// </summary>
    [Fact]
    public void AnEmptyArrayCanAccompanyAVerdictAbovePatch()
    {
        var root = Render(new ChangeReport([], VersionType.Major));

        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("major", root.GetProperty("changeType").GetString());
    }

    /// <summary>
    /// REP-07 - the array is in ChangeReport's sorted order, so the document is stable however
    /// the rules happened to fire. Handed to the report backwards, it comes out sorted.
    /// </summary>
    [Fact]
    public void FindingsKeepTheirDeterministicOrder()
    {
        var report = new ChangeReport([
            new ChangeFinding { UnitId = "Widgets", Symbol = "Zeta", Impact = VersionType.Minor },
            new ChangeFinding { UnitId = "Widgets", Symbol = "Alpha", Impact = VersionType.Minor }
        ]);

        Assert.Equal(
            ["Alpha", "Zeta"],
            Render(report).GetProperty("findings").EnumerateArray()
                .Select(finding => finding.GetProperty("symbol").GetString()));
    }

    /// <summary>REP-07 - nothing machine-specific, so two machines agree.</summary>
    [Fact]
    public void NothingMachineSpecificReachesTheDocument()
    {
        var json = JsonChangeReport.Render(
            JsonChangeReport.Build(Report(VersionType.Major),
                new Version("2.3.4"), new Version("3.0.0"), isDryRun: false));

        Assert.DoesNotContain(Path.GetTempPath(), json);
        Assert.DoesNotContain(Environment.CurrentDirectory, json);
        Assert.DoesNotContain(Environment.MachineName, json);
        Assert.DoesNotContain(DateTime.Now.Year.ToString(), json);
    }
}
