using System.Text;
using Winterborn.Tools.EasySemVer.Evaluation;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Persistence;

/// <summary>
/// TOK-01 - the new version, stamped into ordinary text. Everything else this tool writes is a
/// version location it knows the shape of: an element in a .csproj, an assignment in a
/// project.pbxproj, a constant in a .swift file. This is the surface for the places it cannot
/// know about - a changelog heading, a Helm chart, a docs page, an installer script - where the
/// consuming project marks the spot itself with <c>{{vnext}}</c> and the run replaces it.
/// <para>
/// It is a **consuming** replacement: the token is gone after the run that stamps it, because
/// what is left behind has to be the version, not a placeholder. That is the right behaviour for
/// the release-notes case it exists for and it is the property to understand before adopting it -
/// a file that needs the token every release has to be re-marked every release, which for a
/// changelog is what writing the next entry does anyway.
/// </para>
/// <para>
/// Only the folder root is searched, through the same walk and the same exclusions as discovery
/// (FLD-04), so <c>node_modules</c> and <c>bin</c> cost nothing here either.
/// </para>
/// </summary>
internal static class VersionTokens
{
    /// <summary>
    /// How much of a file is inspected before it is called binary. Every binary format this will
    /// meet carries a NUL well inside this; no text file carries one at all.
    /// </summary>
    private const int BytesSniffedForBinaryContent = 8000;

    /// <summary>
    /// The literal searched for, from the name inside the braces. The braces are fixed and only
    /// the name is configurable (CLI-13): what a caller needs is a way to say "not that word",
    /// and letting the delimiters move too would buy nothing and give a run two ways to be wrong.
    /// </summary>
    internal static string GetToken(string tokenName)
    {
        return "{{" + tokenName + "}}";
    }

    /// <summary>
    /// Replaces every occurrence of the token under the folder root and returns the
    /// folder-root-relative path of each file it changed, for REP-10.
    /// <para>
    /// On a dry run nothing is written and the files are named as "would" lines instead. The
    /// scan still happens, because "what would this run do to my tree" is exactly the question
    /// the mode exists to answer, and a consuming replacement is the one write here worth
    /// previewing.
    /// </para>
    /// </summary>
    internal static IReadOnlyList<string> Stamp(
        string folderRoot,
        string tokenName,
        Version version,
        bool isDryRun)
    {
        var token = GetToken(tokenName);
        var replacement = version.ToString();
        var baselinePath = BaselineFile.GetPath(folderRoot);
        var stamped = new List<string>();

        foreach (var path in FolderScanner.FindFiles(folderRoot, "*"))
        {
            // The baseline is this tool's own file and PER-06 wrote it moments ago. Nothing puts
            // the token in it, and a rewrite here would be this run editing the history the next
            // run reads back.
            if (string.Equals(path, baselinePath, StringComparison.Ordinal))
            {
                continue;
            }

            var text = ReadText(path, folderRoot, out var encoding);
            if (text == null || !text.Contains(token, StringComparison.Ordinal))
            {
                continue;
            }

            var relativePath = FolderScanner.GetRelativePath(folderRoot, path);
            stamped.Add(relativePath);

            if (isDryRun)
            {
                Log.WriteLine($"Would replace {token} with {replacement} in {relativePath}");
                continue;
            }

            File.WriteAllText(path, text.Replace(token, replacement, StringComparison.Ordinal), encoding);
            Log.WriteLine($"Replaced {token} with {replacement} in {relativePath}");
        }

        return stamped;
    }

    /// <summary>
    /// The file's text and the encoding to write it back as, or null for anything this must not
    /// rewrite: something binary, something that is not UTF-8, something it could not read.
    /// <para>
    /// The unreadable case is logged and skipped rather than thrown, because the walk reaches
    /// every file in the folder and a dangling symlink or a locked file is not a reason to fail a
    /// release. The binary and non-UTF-8 cases are silent by contrast: a repository with a few
    /// hundred images in it would otherwise produce a few hundred lines saying nothing, and unlike
    /// an unreadable file there is no decision behind them for a reader to make.
    /// </para>
    /// </summary>
    private static string? ReadText(string path, string folderRoot, out Encoding encoding)
    {
        // Only ever used for a file this returns text for; the null paths below leave it unread.
        encoding = Encoding.UTF8;

        try
        {
            if (IsBinary(path))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(path);

            // SYN-04's byte-faithfulness, for files this tool did not author: a BOM'd file keeps
            // its BOM and a file without one does not acquire one. Everything but the token has
            // to survive the round trip, because unlike a .csproj this is somebody's prose.
            var hasByteOrderMark =
                bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: hasByteOrderMark);

            // Strictly, so that a Latin-1 file carrying the token is skipped rather than rewritten
            // with its accented characters replaced by U+FFFD. Silent corruption of a file the
            // tool was only passing through is the one outcome here that cannot be undone.
            return new UTF8Encoding(false, throwOnInvalidBytes: true)
                .GetString(hasByteOrderMark ? bytes.AsSpan(3) : bytes);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Log.WriteLine(
                $"Skipping {FolderScanner.GetRelativePath(folderRoot, path)} while looking for "
                + $"the version token: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sniffed from its own small read rather than from the whole file, so that a repository of
    /// large assets is not loaded into memory to be told what it obviously is.
    /// </summary>
    private static bool IsBinary(string path)
    {
        using var stream = File.OpenRead(path);
        var head = new byte[BytesSniffedForBinaryContent];
        var read = stream.ReadAtLeast(head, head.Length, throwOnEndOfStream: false);
        return Array.IndexOf(head, (byte)0, 0, read) >= 0;
    }
}
