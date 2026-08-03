using System.Text.Json;
using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Reporting;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Test.Reporting;

/// <summary>REP-01…REP-08 - the machine-readable report's contract.</summary>
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
            ["formatVersion", "dryRun", "changeType", "oldVersion", "newVersion"],
            root.EnumerateObject().Select(p => p.Name));
    }

    /// <summary>REP-05 - the fields that were weighed and deliberately left out stay out.</summary>
    [Theory]
    [InlineData("units")]
    [InlineData("findings")]
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
