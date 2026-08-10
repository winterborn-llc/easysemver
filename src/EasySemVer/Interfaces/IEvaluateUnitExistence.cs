using Winterborn.Tools.EasySemVer.DataObject;

namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// A rule about a shippable module appearing or disappearing, rather than about what is inside
/// one. Same rule-object discipline as the signature evaluators (CLS-01, ML-04), and the same
/// ownership: it belongs to exactly one language, even though every language has one and most
/// will answer alike. The shape is shared here; the answers are not.
/// <para>
/// This interface names no language because it needs none - it reads units, which are neutral.
/// The rules that implement it live in their language's folder, and the base classes under
/// Evaluators/ carry the diffing they mostly agree on.
/// </para>
/// </summary>
public interface IEvaluateUnitExistence
{
    /// <summary>
    /// The rule's name from the spec tables - "UnitRemoved". Published in the JSON report
    /// (REP-02) as half of the (language, rule) key, so it is a contract: a name is never reused
    /// and never silently changes. Carried rather than derived from the class name precisely so
    /// that renaming the class cannot break a consumer.
    /// </summary>
    public string Rule { get; }

    public VersionType EvaluationImpact { get; }

    /// <summary>The phrase completing "&lt;unit&gt; ..." in the report, such as "was removed".</summary>
    public string ChangeDescription { get; }

    /// <summary>
    /// Every unit this rule fires on; empty means it did not fire. It yields the units rather
    /// than answering yes/no so the run can report what it found and not only that it found
    /// something (§20 O-04).
    /// </summary>
    public IEnumerable<IPackageableUnit> FindDifferences(IUnitsToCompare units);
}
