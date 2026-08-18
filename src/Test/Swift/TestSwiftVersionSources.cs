using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Swift;

/// <summary>MVR-03/MVR-04 - the Swift rows of the version-source table.</summary>
public class TestSwiftVersionSources : IDisposable
{
    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-versions").FullName;

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Write(string fileName, string content)
    {
        var path = Path.Combine(this._folderRoot, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void PodspecVersionIsRead()
    {
        var path = this.Write("Widgets.podspec", "Pod::Spec.new do |s|\n  s.version = '2.3.4'\nend\n");

        Assert.Equal("2.3.4", new PodspecVersionSource(path, "Widgets.podspec").Read()!.ToString());
    }

    [Fact]
    public void PodspecVersionIsWrittenInPlace()
    {
        var path = this.Write("Widgets.podspec", "Pod::Spec.new do |s|\n  s.version = '2.3.4'\nend\n");

        new PodspecVersionSource(path, "Widgets.podspec").Write(new Version("9.9.9"));

        Assert.Contains("s.version = '9.9.9'", File.ReadAllText(path));
    }

    /// <summary>MVR-04 - a podspec that computes its version is read-skipped and write-skipped.</summary>
    [Fact]
    public void PodspecWithoutALiteralVersionIsNotAVersionSource()
    {
        Assert.False(PodspecVersionSource.HasLiteralVersion(
            "Pod::Spec.new do |s|\n  s.version = ENV['VERSION']\nend\n"));
    }

    [Fact]
    public void SwiftVersionFileIsReadAndWritten()
    {
        var path = this.Write(
            "WidgetsVersion.swift",
            "public enum WidgetsVersion {\n    public static let version = \"1.2.3\"\n}\n");
        var source = new SwiftVersionFileSource(path, "WidgetsVersion.swift");

        Assert.Equal("1.2.3", source.Read()!.ToString());

        source.Write(new Version("4.5.6"));
        Assert.Contains("let version = \"4.5.6\"", File.ReadAllText(path));
    }

    [Fact]
    public void SwiftFileWithoutAVersionConstantIsNotAVersionSource()
    {
        Assert.False(SwiftVersionFileSource.HasVersionConstant("public struct Widget { }"));
    }

    /// <summary>§20 O-02 - git tags are read for seeding and never written.</summary>
    [Fact]
    public void HighestGitTagWins()
    {
        var highest = GitTagVersionSource.GetHighestTag(
            ["v1.0.0", "1.9.3", "not-a-tag", "v2.0.0", "v1.5.0", "v2.0.0-rc1"]);

        Assert.Equal("2.0.0", highest!.ToString());
    }

    [Fact]
    public void NoSemanticTagsMeansNoSeed()
    {
        Assert.Null(GitTagVersionSource.GetHighestTag(["latest", "release-candidate"]));
    }

    /// <summary>
    /// TAG-01 - off by default. This used to assert "never writable"; §20 O-02 was confirmed on
    /// 2026-08-17 and writing is now opt-in, so what has to hold is that nothing writes a tag
    /// unless a caller asked for it in so many words.
    /// </summary>
    [Fact]
    public void GitTagSourceIsNotWritableByDefault()
    {
        IVersionSource source = new GitTagVersionSource(
            new NeverRunsProcess(),
            this._folderRoot,
            isWritable: false);

        Assert.False(source.IsWritable);
    }

    /// <summary>
    /// And that a run which did not ask for a tag cannot get one by accident: the source is handed
    /// a process runner that fails the test if it is called at all.
    /// </summary>
    [Fact]
    public void WritingWithoutTheFlagRunsNoGitCommand()
    {
        IVersionSource source = new GitTagVersionSource(
            new NeverRunsProcess(),
            this._folderRoot,
            isWritable: false);

        source.Write(new Winterborn.Tools.EasySemVer.DataObject.Version("1.2.3"));
    }

    [Fact]
    public void GitTagSourceIsWritableWhenOptedIn()
    {
        IVersionSource source = new GitTagVersionSource(
            new NeverRunsProcess(),
            this._folderRoot,
            isWritable: true);

        Assert.True(source.IsWritable);
    }

    private class NeverRunsProcess : IRunProcess
    {
        public ProcessResult Run(
            string executable,
            IReadOnlyList<string> arguments,
            string workingDirectory,
            TimeSpan timeout)
        {
            throw new InvalidOperationException("The version-source tests must not shell out");
        }
    }
}
