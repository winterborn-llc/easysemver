using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// The Xcode CURRENT_PROJECT_VERSION row: written, never read.
/// <para>
/// It is written because the version changes on every run, so the counter it feeds
/// (CFBundleVersion, via <c>$(CURRENT_PROJECT_VERSION)</c>) moves on every upload without anyone
/// maintaining it by hand.
/// </para>
/// <para>
/// It is never read because it is a build counter, not a version: the value is routinely a bare
/// integer like <c>87</c>, and VER-01 normalises that to 87.0.0. Feeding it into MVR-03's
/// highest-wins seed would hand the whole folder a version three orders of magnitude past the real
/// one, and highest-wins means there is no way back down. <see cref="Read"/> therefore always
/// returns null, which <c>ReadVersions</c> skips.
/// </para>
/// </summary>
internal partial class BuildCounterVersionSource(string pbxprojPath, string relativePath)
    : IVersionSource
{
    // Xcode writes the value bare or quoted depending on how it was edited; both are literals.
    [GeneratedRegex(@"(?<prefix>CURRENT_PROJECT_VERSION\s*=\s*""?)(?<counter>[0-9][0-9.]*)(?<suffix>""?\s*;)")]
    private static partial Regex BuildCounter();

    internal static bool HasLiteralCounter(string pbxprojText)
    {
        return BuildCounter().IsMatch(pbxprojText);
    }

    public string Kind => "xcode-build-counter";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    /// <summary>Always null - a build counter is not a version seed. See the type remarks.</summary>
    public Version? Read()
    {
        return null;
    }

    public void Write(Version version)
    {
        var text = File.ReadAllText(pbxprojPath);
        var updated = BuildCounter().Replace(
            text,
            match => $"{match.Groups["prefix"].Value}{version}{match.Groups["suffix"].Value}");
        File.WriteAllText(pbxprojPath, updated);
    }
}
