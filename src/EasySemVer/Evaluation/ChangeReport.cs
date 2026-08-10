using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Extensions;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>
/// Everything a run found, in a deterministic order, and the change type it adds up to
/// (CLS-03/ML-05). The ordering is not cosmetic: for the same reason a baseline is sorted
/// (BAS-04), identical input has to produce identical output, so findings are sorted by unit and
/// then by symbol rather than left in whatever order the rules happen to be registered in.
/// This type is the single source both report formats read, so adding a machine-readable output
/// is a second formatter and not a second traversal.
/// </summary>
internal class ChangeReport
{
    internal IReadOnlyList<ChangeFinding> Findings { get; }

    internal VersionType ChangeType { get; }

    /// <param name="floor">
    /// The impact the run carries before any finding is counted. Only CLS-04's defensive path
    /// passes anything but Patch: when there is no comparable baseline at all there is no symbol
    /// to name, so the fail-safe cannot be expressed as a finding.
    /// </param>
    internal ChangeReport(IEnumerable<ChangeFinding> findings, VersionType floor = VersionType.Patch)
    {
        var sorted = new List<ChangeFinding>(findings);
        sorted.Sort(Compare);
        this.Findings = sorted;
        this.ChangeType = GetChangeType(sorted, floor);
    }

    internal int Count(VersionType impact)
    {
        var count = 0;
        foreach (var finding in this.Findings)
        {
            if (finding.Impact != impact)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    /// <summary>CLS-03 - Patch is the default, so a report with no findings is a Patch.</summary>
    private static VersionType GetChangeType(IReadOnlyList<ChangeFinding> findings, VersionType floor)
    {
        var changeType = floor;
        foreach (var finding in findings)
        {
            changeType = changeType.GetHigherImpact(finding.Impact);
        }

        return changeType;
    }

    /// <summary>
    /// Unit, then symbol, then rule, all ordinal - the same discipline the baseline sort uses, and
    /// ordinal for the same reason: a culture-sensitive comparison would order the report
    /// differently on different machines (BAS-04).
    /// </summary>
    private static int Compare(ChangeFinding left, ChangeFinding right)
    {
        var byUnit = string.CompareOrdinal(GetUnitSortKey(left), GetUnitSortKey(right));
        if (byUnit != 0)
        {
            return byUnit;
        }

        var bySymbol = string.CompareOrdinal(left.Symbol, right.Symbol);
        if (bySymbol != 0)
        {
            return bySymbol;
        }

        return string.CompareOrdinal(left.Rule, right.Rule);
    }

    /// <summary>Mirrors <see cref="PackageableUnit.GetSortKey"/>, which findings cannot reach.</summary>
    private static string GetUnitSortKey(ChangeFinding finding)
    {
        return $"{finding.LanguageId} {finding.UnitId}";
    }
}
