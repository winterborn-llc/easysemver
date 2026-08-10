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
    /// <summary>
    /// The language of the unit the change was found in. Half of the finding's published key: a
    /// rule is identified by <see cref="LanguageId"/> and <see cref="Rule"/> together, so two
    /// languages may each carry a rule called <c>UnitRemoved</c> without collision.
    /// </summary>
    public string LanguageId { get; init; } = string.Empty;

    /// <summary>Machine-stable and free of absolute paths, exactly as on the unit (ML-03).</summary>
    public string UnitId { get; init; } = string.Empty;

    /// <summary>
    /// The rule that fired, named rather than numbered, and unique within
    /// <see cref="LanguageId"/> rather than globally. Published in the JSON report (REP-02), so it
    /// is a contract: a name is never reused and never silently changes.
    /// <para>
    /// Every rule carries this as a literal instead of deriving it from its class name, so that
    /// renaming the class cannot move the published key and changing the key is a visible edit.
    /// A base class must therefore declare it <c>abstract</c> and never default it.
    /// </para>
    /// </summary>
    public string Rule { get; init; } = string.Empty;

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
