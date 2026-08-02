using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// Constraint-set comparison for R39/R40. Constraints are stored sorted and comma-joined, so
/// comparing them as sets is a split and a scan.
/// </summary>
internal static class GenericConstraints
{
    internal static string[] Split(string constraints)
    {
        return constraints.Length < 1
            ? []
            : constraints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>True when <paramref name="candidate"/> requires something the other side did not.</summary>
    internal static bool HasExtraConstraint(string candidate, string comparedWith)
    {
        var comparedConstraints = Split(comparedWith);
        foreach (var constraint in Split(candidate))
        {
            if (comparedConstraints.Contains(constraint))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    internal static IEnumerable<(ICsharpGenericParameter Older, ICsharpGenericParameter Newer)> GetPaired(
        IReadOnlyList<ICsharpGenericParameter> older,
        IReadOnlyList<ICsharpGenericParameter> newer)
    {
        for (var i = 0; i < older.Count && i < newer.Count; i++)
        {
            yield return (older[i], newer[i]);
        }
    }
}
