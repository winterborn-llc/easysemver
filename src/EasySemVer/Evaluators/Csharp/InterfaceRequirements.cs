using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// The half of R20/R21 they share. Split out rather than duplicated so the two directions cannot
/// drift apart.
/// </summary>
internal static class InterfaceRequirements
{
    internal static bool WasRequirementAdded(
        ICsharpSignaturesToCompare signatures,
        bool withDefaultImplementation)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Interface)
            {
                continue;
            }

            if (WasMethodAdded(typePair, withDefaultImplementation))
            {
                return true;
            }

            if (WasPropertyAdded(typePair, withDefaultImplementation))
            {
                return true;
            }
        }

        return false;
    }

    private static bool WasMethodAdded(ICsharpClassHistory typePair, bool withDefaultImplementation)
    {
        foreach (var name in typePair.Newer.Methods.Keys)
        {
            if (typePair.Older.Methods.Contains(name))
            {
                continue;
            }

            foreach (var added in typePair.Newer.Methods[name].Overrides)
            {
                if (added.HasDefaultImplementation != withDefaultImplementation)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    private static bool WasPropertyAdded(ICsharpClassHistory typePair, bool withDefaultImplementation)
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

            return true;
        }

        return false;
    }
}
