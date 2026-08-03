using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Providers;

namespace Winterborn.Library.EasySemVer.Evaluation;

/// <summary>
/// ML-05 - the run's change type is the highest impact across the neutral unit-existence rules
/// and every language provider's findings, defaulting to Patch. A Swift-only change therefore
/// moves the C# projects' versions too, because there is one version per folder (ML-06).
/// Nothing is logged here: classification produces findings, and formatting them is the
/// reporter's job, so a machine-readable format is a second formatter and not a second run.
/// </summary>
internal static class ChangeClassifier
{
    private static readonly IEvaluateUnitExistence[] ExistenceRules =
    [
        new UnitRemoved(),
        new UnitAdded()
    ];

    internal static ChangeReport Classify(
        IReadOnlyList<IPackageableUnit>? older,
        IReadOnlyList<IPackageableUnit>? newer,
        IReadOnlyList<ILanguageProvider> providers)
    {
        // NCL-04 / CLS-04: fail safe towards additive. There is no unit to name, so this is the
        // one impact the report carries without a finding behind it.
        if (older is null || newer is null)
        {
            return new ChangeReport([], VersionType.Minor);
        }

        var findings = new List<ChangeFinding>();
        var units = new UnitsToCompare(older, newer);
        foreach (var rule in ExistenceRules)
        {
            foreach (var unit in rule.FindDifferences(units))
            {
                findings.Add(new ChangeFinding
                {
                    Language = unit.Language,
                    UnitId = unit.UnitId,
                    RuleName = rule.GetType().Name,
                    RuleId = rule.RuleId,

                    // A unit-level finding names where the unit lives rather than repeating the
                    // unit id the report has already grouped it under. BAS-04 keeps it relative.
                    Symbol = unit.RelativePath,
                    Description = rule.ChangeDescription,
                    Impact = rule.EvaluationImpact
                });
            }
        }

        foreach (var pair in UnitPairing.GetUnitsInBoth(older, newer))
        {
            var provider = LanguageProviders.Find(providers, pair.Newer.Language);
            if (provider == null)
            {
                continue;
            }

            findings.AddRange(provider.Classify(pair.Older, pair.Newer));
        }

        return new ChangeReport(findings);
    }
}
