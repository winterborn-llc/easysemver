using System.Text.RegularExpressions;
using Winterborn.Library.EasySemVer.Interfaces;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the Xcode MARKETING_VERSION row, read from and written to project.pbxproj.
/// <para>
/// The spec's table says to read this through <c>xcodebuild -showBuildSettings -json</c>. This
/// reads the literal in project.pbxproj instead, deliberately: the literal is the only thing that
/// can be written back (MVR-04), and skipping the extra xcodebuild invocation per project matters
/// given what §20 O-03 already says about Xcode build cost. The consequence is that a project
/// that sets its version from an xcconfig is read-skipped as well as write-skipped, which is the
/// same treatment a podspec without a literal version gets.
/// </para>
/// </summary>
internal partial class MarketingVersionSource(string pbxprojPath, string relativePath) : IVersionSource
{
    [GeneratedRegex(@"(?<prefix>MARKETING_VERSION\s*=\s*)(?<version>[0-9][0-9.]*)(?<suffix>\s*;)")]
    private static partial Regex MarketingVersion();

    internal static bool HasLiteralVersion(string pbxprojText)
    {
        return MarketingVersion().IsMatch(pbxprojText);
    }

    public string Kind => "xcode-marketing-version";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        // A project with several configurations can set it more than once; highest wins, exactly
        // as it does across a solution's csproj files (VER-06).
        Version? highest = null;
        foreach (Match match in MarketingVersion().Matches(File.ReadAllText(pbxprojPath)))
        {
            var text = match.Groups["version"].Value;
            if (!Version.TryParse(text, out var version))
            {
                Log.WriteLine($"Skipping unparseable MARKETING_VERSION '{text}' in {this.Location}");
                continue;
            }

            if (highest != null && version <= highest)
            {
                continue;
            }

            highest = version;
        }

        return highest;
    }

    public void Write(Version version)
    {
        var text = File.ReadAllText(pbxprojPath);
        var updated = MarketingVersion().Replace(
            text,
            match => $"{match.Groups["prefix"].Value}{version}{match.Groups["suffix"].Value}");
        File.WriteAllText(pbxprojPath, updated);
    }
}
