using System.Text.RegularExpressions;
using Winterborn.Library.EasySemVer.Interfaces;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the generated Swift version file row: a <c>let version = "1.2.3"</c> style constant.
/// Read and written only where such a file already exists (MVR-04); EasySemVer never creates one.
/// </summary>
internal partial class SwiftVersionFileSource(string filePath, string relativePath) : IVersionSource
{
    [GeneratedRegex(@"(?<prefix>(?:let|var)\s+\w*[Vv]ersion\w*\s*(?::\s*String\s*)?=\s*)""(?<version>[0-9][0-9.]*)""")]
    private static partial Regex VersionConstant();

    internal static bool HasVersionConstant(string swiftText)
    {
        return VersionConstant().IsMatch(swiftText);
    }

    public string Kind => "swift-version-file";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        var match = VersionConstant().Match(File.ReadAllText(filePath));
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
        var text = File.ReadAllText(filePath);
        var updated = VersionConstant().Replace(text, match => $"{match.Groups["prefix"].Value}\"{version}\"");
        File.WriteAllText(filePath, updated);
    }
}
