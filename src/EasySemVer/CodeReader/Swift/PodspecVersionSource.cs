using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the .podspec row. Only a literal assignment is read or written: a podspec that
/// computes its version is left entirely alone (MVR-04).
/// </summary>
internal partial class PodspecVersionSource(string podspecPath, string relativePath) : IVersionSource
{
    [GeneratedRegex(@"(?<prefix>\.version\s*=\s*)(?<quote>['""])(?<version>[0-9][0-9.]*)\k<quote>")]
    private static partial Regex VersionAssignment();

    internal static bool HasLiteralVersion(string podspecText)
    {
        return VersionAssignment().IsMatch(podspecText);
    }

    public string Kind => "podspec";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        var match = VersionAssignment().Match(File.ReadAllText(podspecPath));
        if (!match.Success)
        {
            return null;
        }

        var text = match.Groups["version"].Value;
        if (Version.TryParse(text, out var version))
        {
            return version;
        }

        Log.WriteLine($"Skipping unparseable version '{text}' in {this.Location}");
        return null;
    }

    public void Write(Version version)
    {
        var text = File.ReadAllText(podspecPath);
        var updated = VersionAssignment().Replace(
            text,
            match => $"{match.Groups["prefix"].Value}{match.Groups["quote"].Value}{version}{match.Groups["quote"].Value}");
        File.WriteAllText(podspecPath, updated);
    }
}
