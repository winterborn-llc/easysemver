using Winterborn.Tools.EasySemVer.Evaluation;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Where a SwiftPM target's source lives. SwiftPM finds it by convention unless the manifest says
/// otherwise, and the conventions are a fixed list of directory names - so the same answer is
/// reachable by looking, which is what this does.
/// </summary>
internal static class SwiftPackageSources
{
    /// <summary>SwiftPM's predefined source directories, in the order it searches them.</summary>
    private static readonly string[] SourceDirectories = ["Sources", "Source", "src", "srcs"];

    private static readonly string[] TestDirectories =
        ["Tests", "Sources", "Source", "src", "srcs"];

    internal static IReadOnlyList<string> Find(string packageDirectory, SwiftPackageTarget target)
    {
        var targetDirectory = GetTargetDirectory(packageDirectory, target);
        if (targetDirectory.Length < 1)
        {
            // SWE-05: a declared target whose source is nowhere to be found is a broken package,
            // not an empty one, and carrying on would record an empty surface for something that
            // has one.
            throw new SwiftSourceException(
                target.Name,
                "the manifest declares it but no source directory for it exists. Looked for "
                + string.Join(", ", GetCandidates(target))
                + ". Add a \"path:\" to the manifest if it lives somewhere unconventional.");
        }

        var files = target.Sources.Count > 0
            ? FindListed(targetDirectory, target.Sources)
            : FolderScanner.FindFiles(targetDirectory, "*.swift");

        return Exclude(targetDirectory, files, target.Excluded);
    }

    /// <summary>The places that were looked, so a failure says what to do about itself.</summary>
    private static IEnumerable<string> GetCandidates(SwiftPackageTarget target)
    {
        if (target.Path.Length > 0)
        {
            return [target.Path];
        }

        var directories = target.IsTest ? TestDirectories : SourceDirectories;
        return directories.Select(directory => $"{directory}/{target.Name}");
    }

    private static string GetTargetDirectory(string packageDirectory, SwiftPackageTarget target)
    {
        if (target.Path.Length > 0)
        {
            var declared = Path.Combine(packageDirectory, target.Path);
            return Directory.Exists(declared) ? declared : string.Empty;
        }

        foreach (var directory in target.IsTest ? TestDirectories : SourceDirectories)
        {
            var candidate = Path.Combine(packageDirectory, directory, target.Name);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        // The flat layout an older package may use: sources sitting directly in "Sources" with no
        // directory per target. Files directly in it, deliberately - a "Sources" holding only
        // per-target directories is the ordinary layout, and matching it here would hand every
        // target that has lost its own directory the whole package's source.
        foreach (var directory in target.IsTest ? TestDirectories : SourceDirectories)
        {
            var candidate = Path.Combine(packageDirectory, directory);
            if (Directory.Exists(candidate)
                && Directory.GetFiles(candidate, "*.swift").Length > 0)
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    /// <summary>A "sources:" entry names either one file or a directory to take whole.</summary>
    private static string[] FindListed(string targetDirectory, IReadOnlyList<string> sources)
    {
        var files = new List<string>();
        foreach (var source in sources)
        {
            var path = Path.Combine(targetDirectory, source);
            if (Directory.Exists(path))
            {
                files.AddRange(FolderScanner.FindFiles(path, "*.swift"));
                continue;
            }

            if (File.Exists(path) && path.EndsWith(".swift", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(path);
            }
        }

        files.Sort(StringComparer.Ordinal);
        return files.ToArray();
    }

    private static IReadOnlyList<string> Exclude(
        string targetDirectory,
        IReadOnlyList<string> files,
        IReadOnlyList<string> excluded)
    {
        if (excluded.Count < 1)
        {
            return files;
        }

        var kept = new List<string>();
        foreach (var file in files)
        {
            var relative = FolderScanner.GetRelativePath(targetDirectory, file);
            if (IsExcluded(relative, excluded))
            {
                continue;
            }

            kept.Add(file);
        }

        return kept;
    }

    private static bool IsExcluded(string relativePath, IReadOnlyList<string> excluded)
    {
        foreach (var entry in excluded)
        {
            var normalised = entry.Replace('\\', '/').TrimEnd('/');
            if (relativePath == normalised
                || relativePath.StartsWith(normalised + "/", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
