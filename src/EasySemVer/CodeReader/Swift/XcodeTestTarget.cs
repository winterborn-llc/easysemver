namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// UNI-04 - which of an .xcodeproj's targets are test bundles, read from <c>project.pbxproj</c>.
/// <para>
/// SWD-02 asks <c>xcodebuild</c> for the target list rather than parsing the project, and that is
/// still where the units come from. <c>xcodebuild -list -json</c> reports names and nothing else,
/// though, so the product type has to come from somewhere: either one
/// <c>-showBuildSettings</c> per target - a process each, against a tool that already dominates
/// the wall clock - or the project file, which is read directly for MARKETING_VERSION already
/// (MVR-04). The cheap one wins, and being wrong here costs a test target keeping a vote it
/// should not have, not a failed run.
/// </para>
/// </summary>
internal static class XcodeTestTarget
{
    private const string NativeTargetMarker = "isa = PBXNativeTarget;";

    private const string NamePrefix = "name = ";

    private const string ProductTypePrefix = "productType = ";

    /// <summary>
    /// Unit tests and UI tests both. A UI test bundle's symbols are no more a contract than a unit
    /// test bundle's, and it is the same <c>.xctest</c> either way.
    /// </summary>
    private static readonly string[] TestProductTypes =
    [
        "com.apple.product-type.bundle.unit-test",
        "com.apple.product-type.bundle.ui-testing"
    ];

    /// <summary>
    /// A missing or unreadable project file yields no test targets, which is what the behaviour
    /// was before UNI-04. Discovery has already succeeded by this point via xcodebuild, so failing
    /// the run over the same project being unparseable here would be a new failure mode bought for
    /// nothing.
    /// </summary>
    internal static IReadOnlyList<string> Read(string pbxprojPath)
    {
        if (!File.Exists(pbxprojPath))
        {
            return [];
        }

        try
        {
            return ReadTestTargetNames(File.ReadAllText(pbxprojPath));
        }
        catch (IOException e)
        {
            Log.WriteLine($"Could not read {pbxprojPath} to find its test targets: {e.Message}");
            return [];
        }
    }

    /// <summary>
    /// A pbxproj is an OpenStep property list, which nothing in .NET parses. It does not need to
    /// be: every native target is one flat block of <c>key = value;</c> lines opened by an
    /// <c>isa</c> and closed by a <c>};</c>, and the two keys wanted are both in it. Anything that
    /// is not a native target block is skipped without being understood, so a project carrying
    /// constructs this knows nothing about reads correctly rather than confusingly.
    /// </summary>
    internal static IReadOnlyList<string> ReadTestTargetNames(string pbxproj)
    {
        var names = new List<string>();
        var insideTarget = false;
        var name = string.Empty;
        var productType = string.Empty;

        foreach (var raw in pbxproj.Split('\n'))
        {
            var line = raw.Trim();

            if (line == NativeTargetMarker)
            {
                insideTarget = true;
                name = string.Empty;
                productType = string.Empty;
                continue;
            }

            if (!insideTarget)
            {
                continue;
            }

            if (line.StartsWith(NamePrefix, StringComparison.Ordinal))
            {
                name = Value(line, NamePrefix);
                continue;
            }

            if (line.StartsWith(ProductTypePrefix, StringComparison.Ordinal))
            {
                productType = Value(line, ProductTypePrefix);
                continue;
            }

            if (line != "};")
            {
                continue;
            }

            insideTarget = false;
            if (name.Length > 0 && TestProductTypes.Contains(productType))
            {
                names.Add(name);
            }
        }

        names.Sort(StringComparer.Ordinal);
        return names;
    }

    /// <summary>
    /// Values are quoted only when they have to be - <c>name = App;</c> beside
    /// <c>name = "My App Tests";</c> - so both forms are unwrapped to the same thing.
    /// </summary>
    private static string Value(string line, string prefix)
    {
        var value = line[prefix.Length..].TrimEnd().TrimEnd(';').Trim();
        if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
        {
            value = value[1..^1];
        }

        return value;
    }
}
