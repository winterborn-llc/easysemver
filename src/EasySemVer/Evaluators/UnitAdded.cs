using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluators;

/// <summary>
/// NCL-02 - a unit exists this run that the baseline never saw. Replaces the C#-only R14, and is
/// what makes a first run Minor (BAS-05).
/// </summary>
public class UnitAdded : IEvaluateUnitExistence
{
    public string RuleId => "NCL-02";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<IPackageableUnit> FindDifferences(IUnitsToCompare units)
    {
        foreach (var newerUnit in units.Newer)
        {
            var olderUnit = UnitPairing.Find(units.Older, newerUnit);
            if (olderUnit != null)
            {
                continue;
            }

            yield return newerUnit;
        }
    }
}
