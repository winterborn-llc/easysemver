using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// Matches an old overload to the new overload that is recognisably the same one - same
/// parameter count, names and types, in order - which is R02's matcher. The modifier rules
/// (R36-R39) all need that pairing before they can ask what changed about it.
/// </summary>
internal static class Overloads
{
    internal class OverloadPair(ICsharpMethodOverride older, ICsharpMethodOverride newer)
    {
        internal ICsharpMethodOverride Older { get; } = older;

        internal ICsharpMethodOverride Newer { get; } = newer;
    }

    internal static IEnumerable<OverloadPair> GetMatchedOverloads(
        ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var methodName in typePair.Older.Methods.Keys)
            {
                if (!typePair.Newer.Methods.Contains(methodName))
                {
                    continue;
                }

                var olderMethod = typePair.Older.Methods[methodName];
                var newerMethod = typePair.Newer.Methods[methodName];
                foreach (var olderOverride in olderMethod.Overrides)
                {
                    var newerOverride = FindMatch(olderOverride, newerMethod);
                    if (newerOverride == null)
                    {
                        continue;
                    }

                    yield return new OverloadPair(olderOverride, newerOverride);
                }
            }
        }
    }

    internal static ICsharpMethodOverride? FindMatch(
        ICsharpMethodOverride olderOverride,
        ICsharpMethod newerMethod)
    {
        foreach (var newerOverride in newerMethod.Overrides)
        {
            if (!DoParametersMatch(olderOverride, newerOverride))
            {
                continue;
            }

            return newerOverride;
        }

        return null;
    }

    private static bool DoParametersMatch(
        ICsharpMethodOverride olderOverride,
        ICsharpMethodOverride newerOverride)
    {
        if (olderOverride.Parameters.Count != newerOverride.Parameters.Count)
        {
            return false;
        }

        for (var i = 0; i < olderOverride.Parameters.Count; i++)
        {
            if (olderOverride.Parameters[i].ParameterName != newerOverride.Parameters[i].ParameterName)
            {
                return false;
            }

            if (olderOverride.Parameters[i].ParameterType != newerOverride.Parameters[i].ParameterType)
            {
                return false;
            }
        }

        return true;
    }
}
