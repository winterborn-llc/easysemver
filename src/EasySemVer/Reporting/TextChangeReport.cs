using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;

namespace Winterborn.Library.EasySemVer.Reporting;

/// <summary>
/// Renders a <see cref="ChangeReport"/> for a human, through <see cref="Log"/> so it nests inside
/// the run like everything else (LOG-01, LOG-02). It is a formatter and nothing else: it reads
/// findings that already exist and never re-examines a signature, which is what lets a
/// machine-readable format be a sibling of this class rather than a second pass over the run.
/// </summary>
internal static class TextChangeReport
{
    /// <param name="isDetailed">
    /// §20 O-04 - a dry run exists to be read, so it lists every finding grouped by unit. A real
    /// run's job is to write versions, so it keeps the one-line-per-firing-rule summary it has
    /// always had; the detail is a flag away when someone wants it.
    /// </param>
    internal static void Write(ChangeReport report, bool isDetailed)
    {
        if (isDetailed)
        {
            WriteFindings(report);
        }
        else
        {
            WriteFiringRules(report);
        }

        Log.WriteLine($"Change Type: {report.ChangeType}{GetCounts(report)}");
    }

    /// <summary>Every finding, grouped under the unit it was found in.</summary>
    private static void WriteFindings(ChangeReport report)
    {
        if (report.Findings.Count < 1)
        {
            Log.WriteLine("No API changes detected");
            return;
        }

        // The findings are already ordered by unit (BAS-04 determinism), so grouping is a scan.
        var currentUnit = string.Empty;
        var areInUnit = false;
        foreach (var finding in report.Findings)
        {
            var unit = $"{finding.Language} {finding.UnitId}";
            if (unit != currentUnit)
            {
                if (areInUnit)
                {
                    Log.Outdent();
                }

                Log.WriteLine(unit);
                Log.Indent();
                currentUnit = unit;
                areInUnit = true;
            }

            Log.WriteLine($"{finding.Impact}  {finding.Symbol} {finding.Description}");
        }

        Log.Outdent();
    }

    /// <summary>
    /// LOG-03's minimum: each firing rule with its unit and impact, once per unit however many
    /// symbols it fired on.
    /// </summary>
    private static void WriteFiringRules(ChangeReport report)
    {
        var alreadyWritten = new HashSet<string>(StringComparer.Ordinal);
        foreach (var finding in report.Findings)
        {
            if (!alreadyWritten.Add($"{finding.Language} {finding.UnitId} {finding.RuleName}"))
            {
                continue;
            }

            Log.WriteLine($"{finding.RuleId} {finding.RuleName}: {finding.Impact} in {finding.UnitId}");
        }
    }

    /// <summary>
    /// The tally behind the verdict, so a Major among fifty Minors is visible without counting
    /// lines. Suppressed when there is nothing to count, which keeps CLS-04's baseline-less run
    /// from claiming it counted something.
    /// </summary>
    private static string GetCounts(ChangeReport report)
    {
        if (report.Findings.Count < 1)
        {
            return string.Empty;
        }

        return $" ({report.Count(VersionType.Major)} major, "
               + $"{report.Count(VersionType.Minor)} minor, "
               + $"{report.Count(VersionType.Patch)} patch)";
    }
}
