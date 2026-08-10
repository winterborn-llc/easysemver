using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>
/// NCL-03 - pair units by (Language, UnitId) before any language rule runs, so a removed unit is
/// never double-counted as "everything inside it was removed."
/// </summary>
internal static class UnitPairing
{
    internal static IPackageableUnit? Find(
        IReadOnlyList<IPackageableUnit> candidates,
        IPackageableUnit wanted)
    {
        foreach (var candidate in candidates)
        {
            if (candidate.LanguageId != wanted.LanguageId)
            {
                continue;
            }

            if (candidate.UnitId != wanted.UnitId)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    /// <summary>Every unit present on both sides, in the current run's order.</summary>
    internal static UnitPair[] GetUnitsInBoth(
        IReadOnlyList<IPackageableUnit> older,
        IReadOnlyList<IPackageableUnit> newer)
    {
        var pairs = new List<UnitPair>();
        foreach (var newerUnit in newer)
        {
            var olderUnit = Find(older, newerUnit);
            if (olderUnit == null)
            {
                continue;
            }

            pairs.Add(new UnitPair(olderUnit, newerUnit));
        }

        return pairs.ToArray();
    }
}
