using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

/// <summary>
/// NCL-01 - a unit in the baseline is gone from this run. Replaces the C#-only R07 for every
/// language at once.
/// </summary>
public class UnitRemoved : IEvaluateUnitExistence
{
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
