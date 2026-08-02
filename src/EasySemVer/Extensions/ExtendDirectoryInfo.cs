namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendDirectoryInfo
{
    public static DirectoryInfo? GetSubDirectory(this DirectoryInfo dir, string subFolderName)
    {
        var subs = dir.GetDirectories();
        foreach (var sub in subs)
        {
            if (sub.Name != subFolderName)
            {
                continue;
            }

            return sub;
        }

        return null;
    }
}