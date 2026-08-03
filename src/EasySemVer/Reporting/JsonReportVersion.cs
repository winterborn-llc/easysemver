using System.Text.Json.Serialization;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Reporting;

/// <summary>
/// One version, in the one shape both of them share (REP-02). <see cref="Version"/> is the
/// canonical string and the full truth; the three numbers are its first three segments, which
/// matters because a version may legitimately carry more (VER-07).
/// <para>
/// The decomposition is here because consumers routinely want the parts - a Docker tag set of
/// <c>3</c>, <c>3.0</c> and <c>3.0.0</c> comes from one run - and parsing a version string is
/// exactly the kind of thing a report should save them.
/// </para>
/// </summary>
public class JsonReportVersion
{
    [JsonPropertyName("version")]
    [JsonPropertyOrder(0)]
    public string Version { get; init; } = string.Empty;

    [JsonPropertyName("major")]
    [JsonPropertyOrder(1)]
    public int Major { get; init; }

    [JsonPropertyName("minor")]
    [JsonPropertyOrder(2)]
    public int Minor { get; init; }

    [JsonPropertyName("patch")]
    [JsonPropertyOrder(3)]
    public int Patch { get; init; }

    internal static JsonReportVersion From(Version version)
    {
        return new JsonReportVersion
        {
            Version = version.ToString(),
            Major = version.Major.GetValueOrDefault(),
            Minor = version.Minor.GetValueOrDefault(),
            Patch = version.Patch.GetValueOrDefault()
        };
    }
}
