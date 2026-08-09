namespace Winterborn.Tools.EasySemVer.Settings;

/// <summary>The parsed command line (§4).</summary>
[DebuggerDisplay("{FolderRoot} dry-run={IsDryRun} json={JsonReportPath}")]
internal class RunOptions
{
    private const string DryRunFlag = "--dry-run";

    private const string JsonFlag = "--json";

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

    internal static RunOptions Parse(params string[] args)
    {
        var isDryRun = false;
        var jsonReportPath = string.Empty;
        var directories = new List<string>();
        var isReadingJsonPath = false;
        foreach (var arg in args)
        {
            if (isReadingJsonPath)
            {
                jsonReportPath = arg;
                isReadingJsonPath = false;
                continue;
            }

            if (string.Equals(arg, DryRunFlag, StringComparison.OrdinalIgnoreCase))
            {
                isDryRun = true;
                continue;
            }

            if (string.Equals(arg, JsonFlag, StringComparison.OrdinalIgnoreCase))
            {
                isReadingJsonPath = true;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"EasySemVer does not recognise the option {arg}");
            }

            directories.Add(arg);
        }

        if (isReadingJsonPath)
        {
            throw new InvalidOperationException($"{JsonFlag} requires a file path");
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
            JsonReportPath = jsonReportPath
        };
    }
}
