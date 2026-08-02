namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendFileInfo
{
    public static string GetFileText(this FileInfo fileInfo)
    {
        return File.ReadAllText(fileInfo.FullName);
    }
}