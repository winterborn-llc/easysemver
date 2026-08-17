using Winterborn.Tools.EasySemVer.Settings;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>
/// FLD-04's exclusion list as it applies to one run: which excluded names the caller asked to keep
/// (CLI-12), and which directories the walk actually skipped, so the run can say so out loud.
/// <para>
/// A skipped directory used to be invisible. That is the failure mode that made `Packages` a bug
/// rather than a preference: a first-party unit inside an excluded directory is not versioned, and
/// nothing in the log says why. Naming the skips turns that into something a reader can act on.
/// </para>
/// <para>
/// State is [ThreadStatic] rather than plain static because a run is single-threaded while xUnit
/// runs test classes in parallel, each on its own thread - one test's --do-not-exclude must not
/// reach another test's discovery.
/// </para>
/// </summary>
internal static class DirectoryExclusions
{
    [ThreadStatic]
    private static string[]? _doNotExclude;

    /// <summary>Full paths, so a directory skipped by several walks is still one skip.</summary>
    [ThreadStatic]
    private static HashSet<string>? _skipped;

    internal static void BeginRun(IReadOnlyList<string> doNotExclude)
    {
        _doNotExclude = [.. doNotExclude];
        _skipped = new HashSet<string>(StringComparer.Ordinal);
    }

    internal static bool IsExcluded(DirectoryInfo directory)
    {
        if (!MagicValues.IsExcludedDirectory(directory.Name, _doNotExclude ?? []))
        {
            return false;
        }

        // The walk runs once per search pattern, so the same directory is skipped several times a
        // run. Recording the path rather than counting the skips keeps the tally honest.
        _skipped ??= new HashSet<string>(StringComparer.Ordinal);
        _skipped.Add(directory.FullName);
        return true;
    }

    /// <summary>
    /// One line, naming each skipped directory name and how many distinct directories carried it.
    /// <para>
    /// It reports every skip rather than only the skips that hid a unit, because deciding the
    /// latter means walking into the directory - which is the cost the exclusion exists to avoid,
    /// and `node_modules` is exactly where it would hurt most.
    /// </para>
    /// </summary>
    internal static void LogSkipped()
    {
        if (_skipped == null || _skipped.Count < 1)
        {
            return;
        }

        var byName = _skipped
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => $"{group.Key} ({group.Count()})");

        Log.WriteLine(
            $"Skipped {_skipped.Count} directories: {string.Join(", ", byName)}. " +
            "Pass --do-not-exclude <name> for any that holds code you version.");
    }
}
