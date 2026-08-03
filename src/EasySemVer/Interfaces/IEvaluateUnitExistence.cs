using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces;

/// <summary>
/// A neutral rule: "a shippable module appeared or disappeared" means the same thing in every
/// language, so it lives here rather than being restated per provider (§7). Same rule-object
/// discipline as the per-language evaluators (CLS-01, ML-04).
/// </summary>
public interface IEvaluateUnitExistence
{
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
