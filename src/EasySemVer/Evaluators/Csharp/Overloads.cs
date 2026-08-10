using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// Matches an old overload to the new overload that is recognisably the same one - same generic
/// arity, and the same parameter count, names and types, in order - which is R02's matcher. The
/// modifier rules (R36-R39) all need that pairing before they can ask what changed about it.
/// </summary>
internal static class Overloads
{
    internal class OverloadPair(
        string symbol,
        ICsharpMethodOverride older,
        ICsharpMethodOverride newer)
    {
        /// <summary>
        /// What the pair is called, carried alongside it because a rule that fires on an overload
        /// has to be able to name it and the overload itself does not know its own type or method
        /// name (SIG-04).
        /// </summary>
        internal string Symbol { get; } = symbol;

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

                    yield return new OverloadPair(
                        $"{typePair.Newer.Name}.{methodName}({olderOverride.GetMethodSignature()})",
                        olderOverride,
                        newerOverride);
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
            if (!DoGenericAritiesMatch(olderOverride, newerOverride))
            {
                continue;
            }

            if (!DoParametersMatch(olderOverride, newerOverride))
            {
                continue;
            }

            return newerOverride;
        }

        return null;
    }

    /// <summary>
    /// Arity is part of what makes an overload that overload: `M&lt;T&gt;(Type type)` and
    /// `M(Type type)` are two different methods with identical parameter lists, and C# overloads on
    /// exactly that. Without this the non-generic one matched the generic one - the first parameter
    /// match wins - and every run reported a changed return type and a tightened constraint against
    /// a tree nobody had touched, so an unchanged repository bumped a major on every release.
    ///
    /// Arity only, never the names or the constraints: R39 and R40 exist to compare the constraints
    /// of a matched pair, and a matcher that required them to be equal would pair nothing for those
    /// rules to fire on.
    /// </summary>
    private static bool DoGenericAritiesMatch(
        ICsharpMethodOverride olderOverride,
        ICsharpMethodOverride newerOverride)
    {
        return olderOverride.GenericParameters.Count == newerOverride.GenericParameters.Count;
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
