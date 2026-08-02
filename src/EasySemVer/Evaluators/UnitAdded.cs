using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

/// <summary>
/// NCL-02 - a unit exists this run that the baseline never saw. Replaces the C#-only R14, and is
/// what makes a first run Minor (BAS-05).
/// </summary>
public class UnitAdded : IEvaluateUnitExistence
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(IUnitsToCompare units)
    {
        foreach (var newerUnit in units.Newer)
        {
            var olderUnit = UnitPairing.Find(units.Older, newerUnit);
            if (olderUnit != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
