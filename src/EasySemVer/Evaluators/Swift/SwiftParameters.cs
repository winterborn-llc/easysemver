using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

internal static class SwiftParameters
{
    /// <summary>
    /// Label, type and order, which is what a call site depends on. Default values, inout and
    /// ownership are compared separately by S31-S33 so they can carry their own direction.
    /// </summary>
    internal static bool AreTheSame(
        IReadOnlyList<ISwiftParameter> older,
        IReadOnlyList<ISwiftParameter> newer)
    {
        if (older.Count != newer.Count)
        {
            return false;
        }

        for (var i = 0; i < older.Count; i++)
        {
            if (older[i].Label != newer[i].Label)
            {
                return false;
            }

            if (older[i].Type != newer[i].Type)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Parameter pairs of a function whose shape is otherwise unchanged.</summary>
    internal static IEnumerable<(ISwiftParameter Older, ISwiftParameter Newer)> GetPaired(
        IReadOnlyList<ISwiftParameter> older,
        IReadOnlyList<ISwiftParameter> newer)
    {
        for (var i = 0; i < older.Count && i < newer.Count; i++)
        {
            yield return (older[i], newer[i]);
        }
    }
}
