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

    public bool AreDifferencesPresent(IUnitsToCompare units);
}
