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
    /// <para>
    /// Bumped to 4 when the language stopped being an enum member and became a provider-owned id
    /// (ML-02). A version-3 baseline spells it "Csharp" where a version-4 run writes "csharp", so
    /// every unit in it would fail to resolve a provider and read as removed - Major, for a rename
    /// nobody made.
    /// </para>
    /// <para>
    /// Deliberately <em>not</em> bumped when Swift signatures stopped coming from the toolchain's
    /// symbol graph and started coming from the source. That invalidates every Swift signature in
    /// every baseline, but it does not touch the file's structure and it does not touch C# at all
    /// - and bumping this would have made a repository with no Swift in it re-seed, and take a
    /// release it did not earn, for a change that could not have affected it. What carries that
    /// instead is the per-unit signature version (BAS-07), which drops the Swift units and leaves
    /// everything around them intact.
    /// </para>
    /// </summary>
    internal const string BaselineFormatVersion = "4";

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
