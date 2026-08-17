namespace Winterborn.Tools.EasySemVer.Settings;

/// <summary>The parsed command line (§4).</summary>
[DebuggerDisplay("{FolderRoot} dry-run={IsDryRun} json={JsonReportPath} github={WritesGitHubActionsReport}")]
internal class RunOptions
{
    private const string DryRunFlag = "--dry-run";

    private const string JsonFlag = "--json";

    private const string GitHubFlag = "--github";

    private const string NoGitHubFlag = "--no-github";

    private const string MaxMinorFlag = "--max-minor";

    private const string MaxPatchFlag = "--max-patch";

    /// <summary>FLD-01 - the folder handed to the CLI is the root, full stop.</summary>
    internal string FolderRoot { get; private init; } = string.Empty;

    /// <summary>
    /// O-04. Classify and report without writing anything - no baseline, no version stamps. A dry
    /// run is not a release, so it does not conflict with OVR-03.
    /// </summary>
    internal bool IsDryRun { get; private init; }

    /// <summary>
    /// REP-01 - where to write the machine-readable report, or empty for none. It is always a
    /// file: the report never goes to stdout, so LOG-01 keeps stdout unconditionally and nothing
    /// has to disentangle a report from a log.
    /// </summary>
    internal string JsonReportPath { get; private init; } = string.Empty;

    /// <summary>
    /// CLI-10 - whether to publish the verdict to <c>$GITHUB_OUTPUT</c> and <c>$GITHUB_STEP_SUMMARY</c>.
    /// Defaults to on when <c>GITHUB_ACTIONS</c> is <c>true</c>, which is what lets a workflow step be
    /// the bare command with nothing to remember.
    /// </summary>
    internal bool WritesGitHubActionsReport { get; private init; }

    /// <summary>
    /// The highest value the minor and patch segments may reach before carrying into the segment
    /// above, or null - the default - for no ceiling at all.
    /// <para>
    /// There is deliberately no default value. A ceiling makes the version representable in a
    /// target that cannot hold an arbitrary integer per segment, at the cost of the number's
    /// meaning: a carried patch reads as a Minor bump on a run where nothing was added. Which
    /// ceiling applies is a property of what the version is being written into and differs per
    /// ecosystem - 255 for the lower two segments of a Mach-O <c>DYLIB_CURRENT_VERSION</c>, 65535
    /// for .NET assembly versions and Win32 FILEVERSION fields - so guessing one on the caller's
    /// behalf would silently distort versions in every folder that did not need it.
    /// </para>
    /// </summary>
    internal int? MaximumMinor { get; private init; }

    /// <inheritdoc cref="MaximumMinor"/>
    internal int? MaximumPatch { get; private init; }

    internal static RunOptions Parse(params string[] args)
    {
        return Parse(Environment.GetEnvironmentVariable, args);
    }

    /// <summary>
    /// The environment is a parameter so that CLI-10's auto-detection is testable without a test
    /// mutating the process's own environment and colliding with whatever runs beside it.
    /// </summary>
    internal static RunOptions Parse(Func<string, string?> environment, string[] args)
    {
        var isDryRun = false;
        var jsonReportPath = string.Empty;
        var directories = new List<string>();
        int? maximumMinor = null;
        int? maximumPatch = null;

        // The flag whose value the next argument is, or null when the next argument is a flag or
        // the directory. One field rather than one bool per option, now that three flags take one.
        string? flagAwaitingValue = null;

        // CLI-10: detected, then overridden by an explicit flag either way.
        var writesGitHubActionsReport = string.Equals(
            environment("GITHUB_ACTIONS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (flagAwaitingValue != null)
            {
                if (string.Equals(flagAwaitingValue, JsonFlag, StringComparison.Ordinal))
                {
                    jsonReportPath = arg;
                }
                else if (string.Equals(flagAwaitingValue, MaxMinorFlag, StringComparison.Ordinal))
                {
                    maximumMinor = ParseSegmentMaximum(flagAwaitingValue, arg);
                }
                else
                {
                    maximumPatch = ParseSegmentMaximum(flagAwaitingValue, arg);
                }

                flagAwaitingValue = null;
                continue;
            }

            if (string.Equals(arg, DryRunFlag, StringComparison.OrdinalIgnoreCase))
            {
                isDryRun = true;
                continue;
            }

            if (string.Equals(arg, JsonFlag, StringComparison.OrdinalIgnoreCase))
            {
                flagAwaitingValue = JsonFlag;
                continue;
            }

            if (string.Equals(arg, MaxMinorFlag, StringComparison.OrdinalIgnoreCase))
            {
                flagAwaitingValue = MaxMinorFlag;
                continue;
            }

            if (string.Equals(arg, MaxPatchFlag, StringComparison.OrdinalIgnoreCase))
            {
                flagAwaitingValue = MaxPatchFlag;
                continue;
            }

            if (string.Equals(arg, GitHubFlag, StringComparison.OrdinalIgnoreCase))
            {
                writesGitHubActionsReport = true;
                continue;
            }

            if (string.Equals(arg, NoGitHubFlag, StringComparison.OrdinalIgnoreCase))
            {
                writesGitHubActionsReport = false;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"EasySemVer does not recognise the option {arg}");
            }

            directories.Add(arg);
        }

        if (string.Equals(flagAwaitingValue, JsonFlag, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{JsonFlag} requires a file path");
        }

        if (flagAwaitingValue != null)
        {
            throw new InvalidOperationException($"{flagAwaitingValue} requires a whole number");
        }

        // CLI-02: at most one directory.
        if (directories.Count > 1)
        {
            throw new InvalidOperationException(
                "EasySemVer requires a single parameter that specifies the directory in which to execute");
        }

        // CLI-03: no directory means the current working directory.
        var folderRoot = directories.Count < 1 ? Environment.CurrentDirectory : directories[0];
        if (!Directory.Exists(folderRoot))
        {
            throw new InvalidOperationException($"Directory {folderRoot} does not exist");
        }

        return new RunOptions
        {
            FolderRoot = new DirectoryInfo(folderRoot).FullName,
            IsDryRun = isDryRun,
            JsonReportPath = jsonReportPath,
            WritesGitHubActionsReport = writesGitHubActionsReport,
            MaximumMinor = maximumMinor,
            MaximumPatch = maximumPatch
        };
    }

    /// <summary>
    /// A ceiling of zero is legal and means the segment may never be anything but zero, so every
    /// increment of it carries. A negative ceiling is not, since no segment could ever satisfy it.
    /// </summary>
    private static int ParseSegmentMaximum(string flag, string text)
    {
        if (!int.TryParse(text, out var maximum) || maximum < 0)
        {
            throw new InvalidOperationException($"{flag} requires a whole number of zero or more");
        }

        return maximum;
    }
}
