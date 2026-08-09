using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// Ordered parameter-list comparison, shared by the delegate and record rules (R26, R27).
/// </summary>
internal static class ParameterLists
{
    internal static bool AreTheSame(
        IReadOnlyList<ICsharpMethodParameter> older,
        IReadOnlyList<ICsharpMethodParameter> newer)
    {
        if (older.Count != newer.Count)
        {
            return false;
        }

        for (var i = 0; i < older.Count; i++)
        {
            if (older[i].ParameterName != newer[i].ParameterName)
            {
                return false;
            }

            if (older[i].ParameterType != newer[i].ParameterType)
            {
                return false;
            }
        }

        return true;
    }
}
