using Winterborn.Library.EasySemVer.Settings;

namespace Winterborn.Library.EasySemVer.Evaluation;

/// <summary>
/// The one recursive walk of the folder root (FLD-03), honouring the exclusion list (FLD-04).
/// Results are sorted so that discovery order - and therefore everything downstream of it - does
/// not depend on the file system's enumeration order (BAS-04).
/// </summary>
internal static class FolderScanner
{
    internal static string[] FindFiles(string folderRoot, string searchPattern)
    {
        var found = new List<string>();
        Walk(new DirectoryInfo(folderRoot), searchPattern, found, isDirectorySearch: false);
        found.Sort(StringComparer.Ordinal);
        return found.ToArray();
    }

    /// <summary>Finds directory bundles such as <c>*.xcodeproj</c>, which are directories, not files.</summary>
    internal static string[] FindDirectories(string folderRoot, string searchPattern)
    {
        var found = new List<string>();
        Walk(new DirectoryInfo(folderRoot), searchPattern, found, isDirectorySearch: true);
        found.Sort(StringComparer.Ordinal);
        return found.ToArray();
    }

    /// <summary>Folder-root-relative, forward slashes, so nothing machine-specific reaches the baseline.</summary>
    internal static string GetRelativePath(string folderRoot, string fullPath)
    {
        var relative = Path.GetRelativePath(folderRoot, fullPath);
        return relative.Replace(Path.DirectorySeparatorChar, '/').Replace('\\', '/');
    }

    private static void Walk(
        DirectoryInfo directory,
        string searchPattern,
        List<string> found,
        bool isDirectorySearch)
    {
        if (!directory.Exists)
        {
            return;
        }

        foreach (var subDirectory in directory.GetDirectories())
        {
            if (isDirectorySearch && IsMatch(subDirectory.Name, searchPattern))
            {
                // A matched bundle is a leaf: nothing inside an .xcodeproj is a unit of its own.
                found.Add(subDirectory.FullName);
                continue;
            }

            if (MagicValues.IsExcludedDirectory(subDirectory.Name))
            {
                continue;
            }

            Walk(subDirectory, searchPattern, found, isDirectorySearch);
        }

        if (isDirectorySearch)
        {
            return;
        }

        foreach (var file in directory.GetFiles(searchPattern))
        {
            found.Add(file.FullName);
        }
    }

    private static bool IsMatch(string name, string searchPattern)
    {
        if (!searchPattern.StartsWith('*'))
        {
            return string.Equals(name, searchPattern, StringComparison.OrdinalIgnoreCase);
        }

        // ReSharper disable once ReplaceSubstringWithRangeIndexer
        var suffix = searchPattern.Substring(1);
        return name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }
}
