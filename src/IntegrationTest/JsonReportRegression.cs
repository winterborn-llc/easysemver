using System.Text.Json;
using Winterborn.Library.EasySemVer;
using Xunit;

namespace IntegrationTest;

/// <summary>REP-01…REP-08 through a whole run, which is the only way REP-08 is observable.</summary>
public class JsonReportRegression : IDisposable
{
    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-json").FullName;

    private string ReportPath => Path.Combine(this._folderRoot, "out", "report.json");

    public JsonReportRegression()
    {
        File.WriteAllText(Path.Combine(this._folderRoot, "App.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <AssemblyVersion>2.3.4</AssemblyVersion>
               </PropertyGroup>
            </Project>
            """);
        this.WriteSource("public class Thing { public void One() { } }");
    }

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private void WriteSource(string body)
    {
        File.WriteAllText(Path.Combine(this._folderRoot, "Thing.cs"), $"namespace App; {body}");
    }

    private JsonElement ReadReport()
    {
        Assert.True(File.Exists(this.ReportPath), $"No report at {this.ReportPath}");
        return JsonDocument.Parse(File.ReadAllText(this.ReportPath)).RootElement.Clone();
    }

    /// <summary>REP-01 - the path is created if its directory does not exist yet.</summary>
    [Fact]
    public void ARealRunWritesTheReportAndTheVersionsAgree()
    {
        Assert.Equal(0, Program.Main(this._folderRoot, "--json", this.ReportPath));

        var report = this.ReadReport();
        Assert.False(report.GetProperty("dryRun").GetBoolean());

        // A first run has no baseline, so every unit is new: Minor (NCL-02).
        Assert.Equal("minor", report.GetProperty("changeType").GetString());
        Assert.Equal("2.3.4", report.GetProperty("oldVersion").GetProperty("version").GetString());
        Assert.Equal("2.4.0", report.GetProperty("newVersion").GetProperty("version").GetString());

        // And the version it reports is the one that actually reached the disk.
        Assert.Contains("2.4.0", File.ReadAllText(Path.Combine(this._folderRoot, "App.csproj")));
    }

    [Fact]
    public void ADryRunReportsTheVerdictAndChangesNothingElse()
    {
        Assert.Equal(0, Program.Main(this._folderRoot));
        this.WriteSource("public class Thing { }");
        var before = File.ReadAllText(Path.Combine(this._folderRoot, "App.csproj"));

        Assert.Equal(0, Program.Main(this._folderRoot, "--dry-run", "--json", this.ReportPath));

        var report = this.ReadReport();
        Assert.True(report.GetProperty("dryRun").GetBoolean());
        Assert.Equal("major", report.GetProperty("changeType").GetString());
        Assert.Equal(before, File.ReadAllText(Path.Combine(this._folderRoot, "App.csproj")));
    }

    /// <summary>REP-07 - two runs over unchanged source produce byte-identical reports.</summary>
    [Fact]
    public void TwoDryRunsOverUnchangedSourceProduceIdenticalReports()
    {
        Assert.Equal(0, Program.Main(this._folderRoot));

        Assert.Equal(0, Program.Main(this._folderRoot, "--dry-run", "--json", this.ReportPath));
        var first = File.ReadAllText(this.ReportPath);

        Assert.Equal(0, Program.Main(this._folderRoot, "--dry-run", "--json", this.ReportPath));

        Assert.Equal(first, File.ReadAllText(this.ReportPath));
    }

    [Fact]
    public void NoFlagMeansNoReport()
    {
        Assert.Equal(0, Program.Main(this._folderRoot));

        Assert.False(Directory.Exists(Path.Combine(this._folderRoot, "out")));
    }

    /// <summary>REP-08 - a run that fails before it gets there leaves no report behind.</summary>
    [Fact]
    public void AFailedRunWritesNoReport()
    {
        var missing = Path.Combine(this._folderRoot, "does-not-exist");

        Assert.Equal(1, Program.Main(missing, "--json", this.ReportPath));

        Assert.False(File.Exists(this.ReportPath));
    }
}
