using Winterborn.Tools.EasySemVer.DataObject;

namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

/// <summary>
/// One Swift classification rule (ML-04). There is deliberately no base type shared with
/// IEvaluateCsharpSignatures: the two operate on different object models.
/// </summary>
public interface IEvaluateSwiftSignatures
{
    /// <summary>
    /// The rule's name from the spec table - "EnumCaseAdded". Published in the JSON report (REP-02) as half
    /// of the (language, rule) key, so it is a contract: a name is never reused and never silently
    /// changes. Carried rather than derived from the class name precisely so that renaming the
    /// class cannot break a consumer.
    /// </summary>
    public string Rule { get; }

    public VersionType EvaluationImpact { get; }

    /// <summary>The phrase completing "&lt;symbol&gt; ..." in the report, such as "was removed".</summary>
    public string ChangeDescription { get; }

    /// <summary>
    /// Every declaration this rule fires on, named as Swift names it including argument labels
    /// (SWE-03); empty means it did not fire. Yielding symbols rather than a bool is what lets a
    /// dry run explain itself (§20 O-04).
    /// </summary>
    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures);
}
