using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// The half of R20/R21 they share. Split out rather than duplicated so the two directions cannot
/// drift apart.
/// </summary>
internal static class InterfaceRequirements
{
    internal static IEnumerable<string> GetAddedRequirements(
        ICsharpSignaturesToCompare signatures,
        bool withDefaultImplementation)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Interface)
            {
                continue;
            }

            foreach (var method in GetAddedMethods(typePair, withDefaultImplementation))
            {
                yield return method;
            }

            foreach (var property in GetAddedProperties(typePair, withDefaultImplementation))
            {
                yield return property;
            }
        }
    }

    private static IEnumerable<string> GetAddedMethods(
        ICsharpClassHistory typePair,
        bool withDefaultImplementation)
    {
        foreach (var name in typePair.Newer.Methods.Keys)
        {
            if (typePair.Older.Methods.Contains(name))
            {
                continue;
            }

            // One added method is one finding however many overloads it arrived with, so the
            // first matching overload settles it.
            foreach (var added in typePair.Newer.Methods[name].Overrides)
            {
                if (added.HasDefaultImplementation != withDefaultImplementation)
                {
                    continue;
                }

                yield return $"{typePair.Newer.Name}.{name}";
                break;
            }
        }
    }

    private static IEnumerable<string> GetAddedProperties(
        ICsharpClassHistory typePair,
        bool withDefaultImplementation)
    {
        foreach (var name in typePair.Newer.Properties.Keys)
        {
            if (typePair.Older.Properties.Contains(name))
            {
                continue;
            }

            if (typePair.Newer.Properties[name].HasDefaultImplementation != withDefaultImplementation)
            {
                continue;
            }

            yield return $"{typePair.Newer.Name}.{name}";
        }
    }
}
