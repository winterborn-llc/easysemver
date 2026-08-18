using System.Text;
using Winterborn.Tools.EasySemVer.Persistence;
using Winterborn.Tools.EasySemVer.Settings;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test.Persistence;

/// <summary>
/// TOK-01…TOK-06 - the version, stamped into free text the tool knows nothing else about.
/// <para>
/// Every case here names its own token rather than using the default, and the one case that does
/// assert the default builds the literal from pieces. This repository is the escape hatch's own
/// use case (CLI-13): its README and specs document the default token, so a source file spelling
/// it out would be rewritten by any run of the tool against this folder - including, silently, a
/// maintainer's local one.
/// </para>
/// </summary>
public class TestVersionTokens : IDisposable
{
    private const string TokenName = "stamp-here";

    private readonly string _folder =
        Directory.CreateTempSubdirectory("easysemver-tokens").FullName;

    public TestVersionTokens()
    {
        // FLD-06 - the token walk honours the same exclusions discovery does, and they now come
        // from the registered providers rather than from a global list.
        Exclusions.BeginRun();
    }

    public void Dispose()
    {
        Directory.Delete(this._folder, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Write(string relativePath, string content)
    {
        var path = Path.Combine(this._folder, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    private IReadOnlyList<string> Stamp(string tokenName = TokenName, bool isDryRun = false) =>
        VersionTokens.Stamp(this._folder, tokenName, new Version("2.4.0"), isDryRun);

    private static string Token(string tokenName) => VersionTokens.GetToken(tokenName);

    /// <summary>
    /// TOK-02 - the default is the name inside the braces, so an unconfigured run searches for
    /// the two-brace form of "vnext". Written in pieces for the reason the class comment gives.
    /// </summary>
    [Fact]
    public void TheDefaultTokenIsVnextInDoubleBraces()
    {
        Assert.Equal("vnext", MagicValues.DefaultVersionTokenName);
        Assert.Equal("{" + "{vnext}" + "}", Token(MagicValues.DefaultVersionTokenName));
    }

    /// <summary>TOK-01 - the token becomes the version this run produced.</summary>
    [Fact]
    public void TheTokenBecomesTheNewVersion()
    {
        var path = this.Write("CHANGELOG.md", $"## {Token(TokenName)}\n\n- Something happened.\n");

        var stamped = this.Stamp();

        Assert.Equal(["CHANGELOG.md"], stamped);
        Assert.Equal("## 2.4.0\n\n- Something happened.\n", File.ReadAllText(path));
    }

    /// <summary>
    /// TOK-03 - every occurrence, in every file, exactly as SYN-02 treats a version location.
    /// Anything less would leave a document half-stamped and no way to tell which half.
    /// </summary>
    [Fact]
    public void EveryOccurrenceInEveryFileIsReplaced()
    {
        var token = Token(TokenName);
        var notes = this.Write("docs/notes.md", $"{token} shipped. See {token}.");
        var chart = this.Write("deploy/chart.yaml", $"appVersion: \"{token}\"\n");

        var stamped = this.Stamp();

        Assert.Equal(["deploy/chart.yaml", "docs/notes.md"], stamped.Order(StringComparer.Ordinal));
        Assert.Equal("2.4.0 shipped. See 2.4.0.", File.ReadAllText(notes));
        Assert.Equal("appVersion: \"2.4.0\"\n", File.ReadAllText(chart));
    }

    /// <summary>TOK-04 - the paths reported are REP-10's shape: relative, forward slashes.</summary>
    [Fact]
    public void ReportedPathsAreFolderRootRelativeWithForwardSlashes()
    {
        this.Write("deep/nested/file.txt", Token(TokenName));

        Assert.Equal(["deep/nested/file.txt"], this.Stamp());
    }

    /// <summary>A file that does not carry the token is neither rewritten nor reported.</summary>
    [Fact]
    public void AFileWithoutTheTokenIsUntouched()
    {
        var path = this.Write("README.md", "Nothing to see here.");
        var written = File.GetLastWriteTimeUtc(path);

        Assert.Empty(this.Stamp());
        Assert.Equal("Nothing to see here.", File.ReadAllText(path));
        Assert.Equal(written, File.GetLastWriteTimeUtc(path));
    }

    /// <summary>
    /// TOK-05 - a token the run was not told to look for is left alone, which is the whole point
    /// of CLI-13: a project that legitimately writes the default literal renames the one it means.
    /// </summary>
    [Fact]
    public void OnlyTheNamedTokenIsReplaced()
    {
        var path = this.Write("mixed.txt", $"{Token("other")} {Token(TokenName)}");

        this.Stamp();

        Assert.Equal($"{Token("other")} 2.4.0", File.ReadAllText(path));
    }

    /// <summary>
    /// TOK-04 - the same walk and the same exclusions as discovery (FLD-04), so a token inside a
    /// dependency checkout is not this project's to stamp and does not cost a read either.
    /// </summary>
    [Fact]
    public void ExcludedDirectoriesAreNotSearched()
    {
        var vendored = this.Write("node_modules/pkg/readme.md", Token(TokenName));
        var dotted = this.Write(".build/checkouts/notes.md", Token(TokenName));

        Assert.Empty(this.Stamp());
        Assert.Equal(Token(TokenName), File.ReadAllText(vendored));
        Assert.Equal(Token(TokenName), File.ReadAllText(dotted));
    }

    /// <summary>
    /// The baseline is this tool's own file, written moments earlier by the same run (PER-06).
    /// Rewriting it here would be a run editing the history the next run reads back.
    /// </summary>
    [Fact]
    public void TheBaselineIsNeverRewritten()
    {
        var path = this.Write(MagicValues.SignatureFileName, $"<EasySemVer>{Token(TokenName)}</EasySemVer>");

        Assert.Empty(this.Stamp());
        Assert.Contains(Token(TokenName), File.ReadAllText(path));
    }

    /// <summary>
    /// TOK-07 - a binary file is skipped even where the bytes happen to spell the token. Reading
    /// it as text and writing it back would corrupt everything around the match.
    /// </summary>
    [Fact]
    public void BinaryFilesAreLeftAlone()
    {
        var path = Path.Combine(this._folder, "logo.png");
        var content = Encoding.UTF8.GetBytes("PNG\0\0" + Token(TokenName));
        File.WriteAllBytes(path, content);

        Assert.Empty(this.Stamp());
        Assert.Equal(content, File.ReadAllBytes(path));
    }

    /// <summary>
    /// TOK-07 - and text that is not UTF-8, for the same reason: decoding it leniently would
    /// replace every byte this reader could not make sense of with U+FFFD, in a file the tool was
    /// only ever passing through.
    /// </summary>
    [Fact]
    public void TextThatIsNotUtf8IsLeftAlone()
    {
        var path = Path.Combine(this._folder, "latin1.txt");
        var content = Encoding.Latin1.GetBytes($"café {Token(TokenName)}");
        File.WriteAllBytes(path, content);

        Assert.Empty(this.Stamp());
        Assert.Equal(content, File.ReadAllBytes(path));
    }

    /// <summary>
    /// TOK-07 - everything but the token survives the round trip, byte for byte, including a
    /// leading BOM. A file that acquired or lost one would show up as changed in a diff that
    /// should have carried one line.
    /// </summary>
    [Fact]
    public void AByteOrderMarkIsPreservedAndOneIsNeverAdded()
    {
        var withMark = Path.Combine(this._folder, "with-bom.txt");
        File.WriteAllText(withMark, Token(TokenName), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        var without = this.Write("without-bom.txt", Token(TokenName));

        this.Stamp();

        var expected = new byte[] { 0xEF, 0xBB, 0xBF }.Concat("2.4.0"u8.ToArray()).ToArray();
        Assert.Equal(expected, File.ReadAllBytes(withMark));
        Assert.Equal("2.4.0"u8.ToArray(), File.ReadAllBytes(without));
    }

    /// <summary>
    /// TOK-06 - a dry run names what it would take the token out of and writes nothing, because
    /// this is the one write in a run that consumes what it replaces.
    /// </summary>
    [Fact]
    public void ADryRunNamesTheFilesAndChangesNone()
    {
        var path = this.Write("CHANGELOG.md", Token(TokenName));

        Assert.Equal(["CHANGELOG.md"], this.Stamp(isDryRun: true));
        Assert.Equal(Token(TokenName), File.ReadAllText(path));
    }
}
