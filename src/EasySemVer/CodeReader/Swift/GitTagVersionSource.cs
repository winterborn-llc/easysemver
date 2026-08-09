using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the git-tag row, read-only. Reading the highest v?MAJOR.MINOR.PATCH tag as a seed is
/// safe; writing a tag is an outward-facing, effectively irreversible act, so this source never
/// writes one (§20 O-02).
/// </summary>
internal partial class GitTagVersionSource(IRunProcess runProcess, string folderRoot) : IVersionSource
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    [GeneratedRegex(@"^v?(?<version>[0-9]+\.[0-9]+\.[0-9]+)$")]
    private static partial Regex SemanticTag();

    /// <summary>The tag list is the same for every unit in the tree, so it is read once per run.</summary>
    private static readonly Dictionary<string, Version?> HighestTagByRoot = [];

    public string Kind => "git-tag";

    public string Location => ".git";

    public bool IsWritable => false;

    public Version? Read()
    {
        lock (HighestTagByRoot)
        {
            if (HighestTagByRoot.TryGetValue(folderRoot, out var cached))
            {
                return cached;
            }

            var highest = this.ReadHighestTag();
            HighestTagByRoot[folderRoot] = highest;
            return highest;
        }
    }

    public void Write(Version version)
    {
        // Deliberately empty: see O-02.
    }

    internal static Version? GetHighestTag(IEnumerable<string> tagLines)
    {
        Version? highest = null;
        foreach (var line in tagLines)
        {
            var match = SemanticTag().Match(line.Trim());
            if (!match.Success)
            {
                continue;
            }

            if (!Version.TryParse(match.Groups["version"].Value, out var version))
            {
                continue;
            }

            if (highest != null && version <= highest)
            {
                continue;
            }

            highest = version;
        }

        return highest;
    }

    private Version? ReadHighestTag()
    {
        var result = runProcess.Run("git", ["tag", "--list"], folderRoot, Timeout);
        if (!result.IsSuccess)
        {
            // A folder that is not a git checkout is an ordinary input, not a failure.
            Log.WriteLine($"Skipping git tags: {result.FailureDescription}");
            return null;
        }

        return GetHighestTag(result.StandardOutput.Split('\n'));
    }
}
