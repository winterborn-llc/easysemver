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
    /// FLD-04's list, now empty: every entry moved to the language that recognises it, carrying the
    /// sibling marker that proves it (FLD-06/FLD-07). What remains global is the leading-dot rule,
    /// which is a convention rather than an ecosystem - `.git`, `.build`, `.packages`, `.swiftpm`.
    /// <para>
    /// Kept as an empty array rather than deleted so that the one place a name could be excluded
    /// for everyone still exists and still has to be argued for. The bar is now explicit: a name
    /// belongs here only if it cannot mean anything else in any language, and every name that was
    /// here failed that bar. `bin` and `build` are somebody's source directory somewhere; `Pods` is
    /// a module name; only `node_modules` came close, and it is better owned by JavaScript, which
    /// is where a reader would look for it.
    /// </para>
    /// </summary>
    internal static readonly string[] ExcludedDirectoryNames = [];

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
