using System.Text.Json.Serialization;
using Winterborn.Tools.EasySemVer.DataObject;

namespace Winterborn.Tools.EasySemVer.Reporting;

/// <summary>
/// One piece of the evidence behind the verdict (REP-09). The array these sit in is what makes a
/// run auditable in a machine's hands rather than only a reader's - <see cref="Language"/> and
/// <see cref="Rule"/> together let a consumer point at the rule that cost it a Major without
/// matching on prose.
/// <para>
/// The two are one key, which is why the rule name carries no language of its own: a Swift rule
/// reported under <c>swift</c> would only be saying so twice. It also means two languages may
/// each have a <c>UnitRemoved</c>, and a consumer can gate on one without gating on the other.
/// </para>
/// </summary>
public class JsonReportFinding
{
    /// <summary>
    /// The rule's name from the spec tables - "MethodReturnType", "UnitRemoved". Unique within
    /// <see cref="Language"/>, not globally.
    /// </summary>
    [JsonPropertyName("rule")]
    [JsonPropertyOrder(0)]
    public string Rule { get; init; } = string.Empty;

    /// <summary>"major" | "minor" | "patch", lower case like every enum-valued field (REP-02).</summary>
    [JsonPropertyName("impact")]
    [JsonPropertyOrder(1)]
    public string Impact { get; init; } = string.Empty;

    /// <summary>The other half of the key: "csharp", "swift".</summary>
    [JsonPropertyName("language")]
    [JsonPropertyOrder(2)]
    public string Language { get; init; } = string.Empty;

    /// <summary>Machine-stable and free of absolute paths (ML-03), which is what keeps REP-07 true.</summary>
    [JsonPropertyName("unitId")]
    [JsonPropertyOrder(3)]
    public string UnitId { get; init; } = string.Empty;

    [JsonPropertyName("symbol")]
    [JsonPropertyOrder(4)]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Completes "&lt;symbol&gt; ..." - "was removed". Prose for a human reading the document;
    /// nothing should match on it, which is what <see cref="Rule"/> is for.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonPropertyOrder(5)]
    public string Description { get; init; } = string.Empty;

    internal static JsonReportFinding From(ChangeFinding finding)
    {
        return new JsonReportFinding
        {
            Rule = finding.Rule,
            Impact = finding.Impact.ToString().ToLowerInvariant(),
            Language = finding.LanguageId,
            UnitId = finding.UnitId,
            Symbol = finding.Symbol,
            Description = finding.Description
        };
    }
}
