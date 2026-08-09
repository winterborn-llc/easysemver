using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluators;

/// <summary>
/// NCL-01 - a unit in the baseline is gone from this run. Replaces the C#-only R07 for every
/// language at once.
/// </summary>
public class UnitRemoved : IEvaluateUnitExistence
{
    public string RuleId => "NCL-01";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<IPackageableUnit> FindDifferences(IUnitsToCompare units)
    {
        foreach (var olderUnit in units.Older)
        {
            var newerUnit = UnitPairing.Find(units.Newer, olderUnit);
            if (newerUnit != null)
            {
                continue;
            }

            yield return olderUnit;
        }
    }
}
