using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>
/// One C# classification rule (CLS-01, preserved per-language by ML-04). Swift has its own
/// equivalent over its own comparison context; there is deliberately no shared base.
/// </summary>
public interface IEvaluateCsharpSignatures
{
    /// <summary>
    /// The rule's identifier from specs/07 - "R02", "S18", "NCL-01". Published in the JSON report
    /// (REP-02), so it is a contract: an id is never reused and never silently changes. It is
    /// carried rather than derived from the class name precisely so that renaming the class
    /// cannot break a consumer.
    /// </summary>
    public string RuleId { get; }

    public VersionType EvaluationImpact { get; }

    /// <summary>The phrase completing "&lt;symbol&gt; ..." in the report, such as "was removed".</summary>
    public string ChangeDescription { get; }

    /// <summary>
    /// Every symbol this rule fires on, namespace-qualified (SIG-04); empty means it did not
    /// fire. A rule yields its symbols rather than a bool because a bool cannot be reported: the
    /// symbol is the whole point of the dry run's explanation (§20 O-04).
    /// </summary>
    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures);
}
