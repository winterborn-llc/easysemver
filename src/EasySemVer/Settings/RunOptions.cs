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

    private const string DoNotExcludeFlag = "--do-not-exclude";

    private const string VnextTokenNameFlag = "--vnext-token-name";

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

    /// <summary>
    /// CLI-12 - directory names FLD-04 would have excluded that this run keeps anyway. Repeatable,
    /// because a project that vendors one thing usually vendors one thing, not a list, and a
    /// comma-separated value would make a directory name containing a comma unnameable.
    /// </summary>
    internal IReadOnlyList<string> DoNotExclude { get; private init; } = [];

    /// <summary>
    /// CLI-13 - the name inside the braces of the token TOK-01 replaces, defaulting to
    /// <see cref="MagicValues.DefaultVersionTokenName"/> so that <c>20.0.3</c> is what a run
    /// searches for unless told otherwise.
    /// <para>
    /// It is a name rather than an on/off switch because the only reason to change it is that the
    /// default literal means something else in this repository - documentation about this tool,
    /// most obviously, which is why the tool's own release run sets it. Naming a token you never
    /// write turns the feature off as a side effect, and there is no second flag to keep in step
    /// with this one.
    /// </para>
    /// </summary>
    internal string VersionTokenName { get; private init; } = MagicValues.DefaultVersionTokenName;

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
        var doNotExclude = new List<string>();
        var versionTokenName = MagicValues.DefaultVersionTokenName;

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
                else if (string.Equals(flagAwaitingValue, MaxPatchFlag, StringComparison.Ordinal))
                {
                    maximumPatch = ParseSegmentMaximum(flagAwaitingValue, arg);
                }
                else if (string.Equals(flagAwaitingValue, VnextTokenNameFlag, StringComparison.Ordinal))
                {
                    versionTokenName = ParseTokenName(arg);
                }
                else
                {
                    doNotExclude.Add(ParseDirectoryName(arg));
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

            if (string.Equals(arg, DoNotExcludeFlag, StringComparison.OrdinalIgnoreCase))
            {
                flagAwaitingValue = DoNotExcludeFlag;
                continue;
            }

            if (string.Equals(arg, VnextTokenNameFlag, StringComparison.OrdinalIgnoreCase))
            {
                flagAwaitingValue = VnextTokenNameFlag;
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

        if (string.Equals(flagAwaitingValue, DoNotExcludeFlag, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{DoNotExcludeFlag} requires a directory name");
        }

        if (string.Equals(flagAwaitingValue, VnextTokenNameFlag, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{VnextTokenNameFlag} requires a token name");
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
            MaximumPatch = maximumPatch,
            DoNotExclude = doNotExclude,
            VersionTokenName = versionTokenName
        };
    }

    /// <summary>
    /// The name inside the braces, not the whole token: <c>--vnext-token-name release</c> searches
    /// for <c>{{release}}</c>. A value carrying a brace is a caller who has written the delimiters
    /// out as well, and would silently search for <c>{{{{release}}}}</c>; a value carrying
    /// whitespace is the <c>'255 '</c> failure CLI-11 already guards against, arriving through a
    /// templated workflow input and matching nothing for months.
    /// </summary>
    private static string ParseTokenName(string text)
    {
        if (text.Length < 1 ||
            text.Contains('{', StringComparison.Ordinal) ||
            text.Contains('}', StringComparison.Ordinal) ||
            text.Any(char.IsWhiteSpace))
        {
            throw new InvalidOperationException(
                $"{VnextTokenNameFlag} takes the name inside the braces, with no braces and no "
                + $"whitespace in it, and got '{text}'");
        }

        return text;
    }

    /// <summary>
    /// A bare directory name, not a path: the exclusion is matched against one path segment, so
    /// `--do-not-exclude Pods/Alamofire` would silently never match anything.
    /// </summary>
    private static string ParseDirectoryName(string text)
    {
        if (text.Length < 1 ||
            text.Contains('/', StringComparison.Ordinal) ||
            text.Contains('\\', StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{DoNotExcludeFlag} takes a single directory name, not a path, and got '{text}'");
        }

        return text;
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
