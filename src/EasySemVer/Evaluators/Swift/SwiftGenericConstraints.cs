using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>Constraint-set comparison for S11-S13, mirroring the C# helper's shape.</summary>
internal static class SwiftGenericConstraints
{
    internal static bool DidCountChange(
        IReadOnlyList<ISwiftGenericParameter> older,
        IReadOnlyList<ISwiftGenericParameter> newer)
    {
        return older.Count != newer.Count;
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

    internal static IEnumerable<(ISwiftGenericParameter Older, ISwiftGenericParameter Newer)> GetPaired(
        IReadOnlyList<ISwiftGenericParameter> older,
        IReadOnlyList<ISwiftGenericParameter> newer)
    {
        for (var i = 0; i < older.Count && i < newer.Count; i++)
        {
            yield return (older[i], newer[i]);
        }
    }

    private static string[] Split(string constraints)
    {
        return constraints.Length < 1
            ? []
            : constraints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
