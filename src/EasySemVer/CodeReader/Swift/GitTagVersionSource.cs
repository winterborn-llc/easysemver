using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the git-tag row. Reading the highest v?MAJOR.MINOR.PATCH tag as a seed is always safe.
/// <para>
/// Writing one is opt-in through <c>--tag</c> (TAG-01, §20 O-02 confirmed 2026-08-17), because it is
/// the only outward-facing act this tool can take. Even then it creates a **local** tag and never
/// pushes: a local tag is deletable by the person who ran the command, where a pushed one is not.
/// Publishing it is left to whatever already publishes releases - in this repository, the Action's
/// own tag step, which runs after the tests rather than before them.
/// </para>
/// </summary>
internal partial class GitTagVersionSource(
    IRunProcess runProcess,
    string folderRoot,
    bool isWritable) : IVersionSource
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    [GeneratedRegex(@"^v?(?<version>[0-9]+\.[0-9]+\.[0-9]+)$")]
    private static partial Regex SemanticTag();

    /// <summary>The tag list is the same for every unit in the tree, so it is read once per run.</summary>
    private static readonly Dictionary<string, Version?> HighestTagByRoot = [];

    public string Kind => "git-tag";

    public string Location => ".git";

    public bool IsWritable => isWritable;

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
        if (!isWritable)
        {
            return;
        }

        var name = $"v{version}";

        // Idempotent on purpose. A tag that already exists is the ordinary case when a run is
        // repeated - the same version recomputed from the same source - and `git tag` fails on it.
        // Failing the run there would turn a re-run into an error for a tag that already says what
        // this one wanted to say.
        var existing = runProcess.Run("git", ["tag", "--list", name], folderRoot, Timeout);
        if (existing.IsSuccess && existing.StandardOutput.Trim().Length > 0)
        {
            Log.WriteLine($"Tag {name} already exists; leaving it alone");
            return;
        }

        var result = runProcess.Run("git", ["tag", name], folderRoot, Timeout);
        if (!result.IsSuccess)
        {
            // A folder that is not a checkout, or a repository with no commit to tag, is an
            // ordinary input for every other source here and stays one for this.
            Log.WriteLine($"Could not create tag {name}: {result.FailureDescription}");
            return;
        }

        // Said out loud because it is the one thing this run did that is not a file edit, and
        // because the tag is local: whoever reads this log is the one who has to push it.
        Log.WriteLine($"Created local tag {name} (not pushed)");
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
