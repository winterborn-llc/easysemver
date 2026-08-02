namespace Winterborn.Library.EasySemVer.Settings;

/// <summary>The parsed command line (§4).</summary>
[DebuggerDisplay("{FolderRoot} dry-run={IsDryRun}")]
internal class RunOptions
{
    private const string DryRunFlag = "--dry-run";

    /// <summary>FLD-01 - the folder handed to the CLI is the root, full stop.</summary>
    internal string FolderRoot { get; private init; } = string.Empty;

    /// <summary>
    /// O-04. Classify and report without writing anything - no baseline, no version stamps. A dry
    /// run is not a release, so it does not conflict with OVR-03.
    /// </summary>
    internal bool IsDryRun { get; private init; }

    internal static RunOptions Parse(params string[] args)
    {
        var isDryRun = false;
        var directories = new List<string>();
        foreach (var arg in args)
        {
            if (string.Equals(arg, DryRunFlag, StringComparison.OrdinalIgnoreCase))
            {
                isDryRun = true;
                continue;
            }

            directories.Add(arg);
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
            IsDryRun = isDryRun
        };
    }
}
