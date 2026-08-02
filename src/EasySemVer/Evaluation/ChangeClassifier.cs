using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Providers;

namespace Winterborn.Library.EasySemVer.Evaluation;

/// <summary>
/// ML-05 - the run's change type is the highest impact across the neutral unit-existence rules
/// and every language provider's verdict, defaulting to Patch. A Swift-only change therefore
/// moves the C# projects' versions too, because there is one version per folder (ML-06).
/// </summary>
internal static class ChangeClassifier
{
    private static readonly IEvaluateUnitExistence[] ExistenceRules =
    [
        new UnitRemoved(),
        new UnitAdded()
    ];

    internal static VersionType Classify(
        IReadOnlyList<IPackageableUnit>? older,
        IReadOnlyList<IPackageableUnit>? newer,
        IReadOnlyList<ILanguageProvider> providers)
    {
        // NCL-04 / CLS-04: fail safe towards additive.
        if (older is null || newer is null)
        {
            return VersionType.Minor;
        }

        var changeType = VersionType.Patch;
        var units = new UnitsToCompare(older, newer);
        foreach (var rule in ExistenceRules)
        {
            if (!rule.AreDifferencesPresent(units))
            {
                continue;
            }

            Log.WriteLine($"{rule.GetType().Name}: {rule.EvaluationImpact}");
            changeType = changeType.GetHigherImpact(rule.EvaluationImpact);
        }

        foreach (var pair in UnitPairing.GetUnitsInBoth(older, newer))
        {
            var provider = LanguageProviders.Find(providers, pair.Newer.Language);
            if (provider == null)
            {
                continue;
            }

            changeType = changeType.GetHigherImpact(provider.Classify(pair.Older, pair.Newer));
        }

        return changeType;
    }
}
