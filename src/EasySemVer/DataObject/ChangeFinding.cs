namespace Winterborn.Tools.EasySemVer.DataObject;

/// <summary>
/// One detected change: where it was found, what symbol it concerns, what happened to that
/// symbol, and what it costs (LOG-03, §20 O-04). Rules used to answer a bare yes/no, which is
/// why a run could report a version but never the reasoning behind it. A finding is that
/// reasoning in a form a formatter can render - as text now, as JSON later - without walking the
/// signatures a second time.
/// </summary>
[DebuggerDisplay("{Impact} {Symbol} {Description}")]
public class ChangeFinding
{
    /// <summary>The language of the unit the change was found in, for grouping the report.</summary>
    public Language Language { get; init; }

    /// <summary>Machine-stable and free of absolute paths, exactly as on the unit (ML-03).</summary>
    public string UnitId { get; init; } = string.Empty;

    /// <summary>The rule class that fired, so a surprising line can be traced to its rule.</summary>
    public string RuleName { get; init; } = string.Empty;

    /// <summary>
    /// The rule's identifier from the specs - "R02", "S18", "NCL-01". Published in the JSON
    /// report (REP-02) where <see cref="RuleName"/> is not, because a class name is an
    /// implementation detail a consumer should not be keyed to.
    /// </summary>
    public string RuleId { get; init; } = string.Empty;

    /// <summary>
    /// What the change is about, named the way the language names it: a namespace-qualified type
    /// or member for C# (SIG-04), a declaration path including argument labels for Swift
    /// (SWE-03). Unit-level findings name the unit itself.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>The rule's phrase, completing "&lt;symbol&gt; ..." - "was removed".</summary>
    public string Description { get; init; } = string.Empty;

    public VersionType Impact { get; init; }
}
