namespace Winterborn.Tools.EasySemVer.Settings;

internal static class MagicValues
{
    internal const string SignatureFileName = "EasySemVer.xml";

    internal const string BaselineRootElementName = "EasySemVer";

    internal const string BaselineFormatVersionAttributeName = "formatVersion";

    /// <summary>
    /// BAS-03. A baseline carrying any other value is treated as unreadable (PER-04). Bumped to 3
    /// when signature extraction stopped recording metadata types (SIG-03): a version-2 baseline
    /// holds framework symbols that a version-3 run will never produce, so diffing the two would
    /// report their disappearance as a Major change. Rejecting it costs one Minor bump instead.
    /// </summary>
    internal const string BaselineFormatVersion = "3";

    internal static readonly string[] VersionPropertyNames =
        ["AssemblyVersion", "PackageVersion", "FileVersion"];

    /// <summary>
    /// FLD-04. With a folder root instead of a solution root, an unexcluded .build/checkouts or
    /// .packages would pull dependency source into the signature and make every dependency bump a
    /// Major change. Directories beginning with "." are excluded separately, which covers .git,
    /// .build, .packages, and .swiftpm.
    /// </summary>
    internal static readonly string[] ExcludedDirectoryNames =
    [
        "bin",
        "obj",
        "build",
        "DerivedData",
        "Pods",
        "Carthage",
        "node_modules",
        "Packages"
    ];

    internal static bool IsExcludedDirectory(string directoryName)
    {
        if (directoryName.StartsWith('.'))
        {
            return true;
        }

        foreach (var excluded in ExcludedDirectoryNames)
        {
            if (string.Equals(directoryName, excluded, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
