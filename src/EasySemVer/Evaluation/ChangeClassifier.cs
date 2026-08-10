using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>
/// ML-05 - the run's change type is the highest impact across every language provider's findings,
/// defaulting to Patch. A Swift-only change therefore moves the C# projects' versions too, because
/// there is one version per folder (ML-06).
/// <para>
/// This runs no rules of its own. It applies UNI-04, splits the units by language, and hands each
/// provider its own slice: every rule belongs to exactly one language, so there is nothing left
/// here for a rule to be neutral about. Nothing is logged either - classification produces
/// findings, and formatting them is the reporter's job, so a machine-readable format is a second
/// formatter and not a second run.
/// </para>
/// </summary>
internal static class ChangeClassifier
{
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

        // UNI-04. Units that carry a version but no contract are not compared at all - neither
        // against each other, nor for having appeared or vanished.
        var surfaceless = Where(newer, unit => !unit.HasPublicApiSurface);
        var comparableNewer = Where(newer, unit => unit.HasPublicApiSurface);

        // The older side comes out of a baseline, which does not record UNI-04 and, if it predates
        // it, still lists those units in full. Dropping them by the identity the pairing already
        // uses is what stops the first run after an upgrade reading them as removals and
        // classifying Major - the exact misclassification UNI-04 exists to prevent. After one write
        // the baseline no longer carries them and this matches nothing.
        var comparableOlder = Where(older, unit => UnitPairing.Find(surfaceless, unit) == null);

        var findings = new List<ChangeFinding>();
        foreach (var provider in providers)
        {
            // A provider is handed its own language and no other, so it never has to filter. A
            // baseline unit whose language is no longer registered is simply never classified -
            // the same silence BaselineFile already chose when it could not resolve a provider.
            var units = new UnitsToCompare(
                Where(comparableOlder, unit => unit.LanguageId == provider.LanguageId),
                Where(comparableNewer, unit => unit.LanguageId == provider.LanguageId));

            if (units.Older.Count < 1 && units.Newer.Count < 1)
            {
                continue;
            }

            findings.AddRange(provider.Classify(units));
        }

        return new ChangeReport(findings);
    }

    private static IReadOnlyList<IPackageableUnit> Where(
        IReadOnlyList<IPackageableUnit> units,
        Func<IPackageableUnit, bool> predicate)
    {
        var kept = new List<IPackageableUnit>();
        foreach (var unit in units)
        {
            if (!predicate(unit))
            {
                continue;
            }

            kept.Add(unit);
        }

        return kept;
    }
}
