using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// One Swift classification rule (ML-04). There is deliberately no base type shared with
/// IEvaluateCsharpSignatures: the two operate on different object models.
/// </summary>
public interface IEvaluateSwiftSignatures
{
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
