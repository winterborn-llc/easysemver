using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluators;

/// <summary>
/// Turns a language's unit-existence rules into findings. Each provider owns its rules; none of
/// them should own a second copy of this loop, because a finding that reported its unit slightly
/// differently in one language than another would be a difference nobody chose.
/// </summary>
public static class UnitExistence
{
    public static IEnumerable<ChangeFinding> GetFindings(
        string languageId,
        IEnumerable<IEvaluateUnitExistence> rules,
        IUnitsToCompare units)
    {
        foreach (var rule in rules)
        {
            foreach (var unit in rule.FindDifferences(units))
            {
                yield return new ChangeFinding
                {
                    LanguageId = languageId,
                    UnitId = unit.UnitId,
                    Rule = rule.Rule,

                    // A unit-level finding names where the unit lives rather than repeating the
                    // unit id the report has already grouped it under. BAS-04 keeps it relative.
                    Symbol = unit.RelativePath,
                    Description = rule.ChangeDescription,
                    Impact = rule.EvaluationImpact
                };
            }
        }
    }
}
