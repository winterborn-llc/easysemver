using System.Text.Json.Serialization;

namespace Winterborn.Library.EasySemVer.Reporting;

/// <summary>
/// The shape of the <c>--json</c> report (REP-02). Deliberately small: discovered units,
/// individual findings and the written-file list were all weighed and left out, because they
/// belong in the log and no consumer for them exists. REP-04 makes adding a field
/// backwards-compatible, so this can grow into whatever a real consumer turns out to need -
/// which is what makes starting minimal safe rather than short-sighted.
/// <para>
/// Property order is pinned rather than left to reflection, because REP-07 requires two runs over
/// unchanged source to produce byte-identical output.
/// </para>
/// </summary>
public class JsonReportDocument
{
    /// <summary>
    /// REP-03. Versioned independently of the baseline's own format version: the two documents
    /// have different audiences and different failure modes.
    /// </summary>
    [JsonPropertyName("formatVersion")]
    [JsonPropertyOrder(0)]
    public int FormatVersion { get; init; } = 1;

    /// <summary>The only signal that a report describes a preview rather than something that happened.</summary>
    [JsonPropertyName("dryRun")]
    [JsonPropertyOrder(1)]
    public bool IsDryRun { get; init; }

    /// <summary>
    /// "major" | "minor" | "patch". REP-06: stated, never inferred - VER-05's overflow rollover
    /// makes deriving it from the two versions wrong in both directions.
    /// </summary>
    [JsonPropertyName("changeType")]
    [JsonPropertyOrder(2)]
    public string ChangeType { get; init; } = string.Empty;

    [JsonPropertyName("oldVersion")]
    [JsonPropertyOrder(3)]
    public JsonReportVersion OldVersion { get; init; } = new();

    [JsonPropertyName("newVersion")]
    [JsonPropertyOrder(4)]
    public JsonReportVersion NewVersion { get; init; } = new();
}
