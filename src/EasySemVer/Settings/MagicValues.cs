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
    /// TOK-02. The name inside the braces that <c>--vnext-token-name</c> replaces, so the literal
    /// a run searches for is <c>{{vnext}}</c> by default. "vnext" rather than "version" because
    /// the word has to be one that does not appear in ordinary prose by accident, and because it
    /// says which version it means: the one this run is about to produce, not the current one.
    /// </summary>
    internal const string DefaultVersionTokenName = "vnext";

    /// <summary>
    /// FLD-04. With a folder root instead of a solution root, an unexcluded .build/checkouts or
    /// .packages would pull dependency source into the signature and make every dependency bump a
    /// Major change. Directories beginning with "." are excluded separately, which covers .git,
    /// .build, .packages, and .swiftpm.
    /// <para>
    /// `Packages` is deliberately absent. It was here as "Xcode's local-package checkout dir",
    /// which conflates two things: SwiftPM cloned dependencies into `Packages/` in the Swift 3 era
    /// and has used `.build/checkouts/` since, while `Packages/` today is where a modular Xcode app
    /// keeps its *own* local packages. Excluding it defended a dead convention and silently swallowed
    /// first-party units - and a silent false negative is far worse here than the visible false
    /// positive of discovering a vendored copy, which shows up as new units in the baseline.
    /// </para>
    /// </summary>
    internal static readonly string[] ExcludedDirectoryNames =
    [
        "bin",
        "obj",
        "build",
        "DerivedData",
        "Pods",
        "Carthage",
        "node_modules"
    ];

    /// <summary>
    /// CLI-12: a name the caller listed is never excluded, whichever rule would have excluded it -
    /// including the leading-dot rule, so a project that really does keep source under a dotted
    /// directory can say so.
    /// </summary>
    internal static bool IsExcludedDirectory(string directoryName, IReadOnlyList<string> doNotExclude)
    {
        foreach (var kept in doNotExclude)
        {
            if (string.Equals(directoryName, kept, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

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
