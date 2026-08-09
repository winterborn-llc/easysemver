using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>
/// The member lookups the Swift rules share. Every declaration is identified by its full Swift
/// name including argument labels (SWE-03), so a rename or a label change is a different member.
/// </summary>
internal static class SwiftMembers
{
    /// <summary>Every member of a type, as (kind-tag, name) pairs, for the existence rules.</summary>
    internal static IEnumerable<ISwiftDeclaration> GetAll(ISwiftType type)
    {
        foreach (var initializer in type.Initializers)
        {
            yield return initializer;
        }

        foreach (var function in type.Functions)
        {
            yield return function;
        }

        foreach (var property in type.Properties)
        {
            yield return property;
        }

        foreach (var subscriptDeclaration in type.Subscripts)
        {
            yield return subscriptDeclaration;
        }
    }

    internal static ISwiftDeclaration? Find(ISwiftType type, ISwiftDeclaration wanted)
    {
        foreach (var candidate in GetAll(type))
        {
            if (candidate.GetType() != wanted.GetType())
            {
                continue;
            }

            if (candidate.Name != wanted.Name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    internal static ISwiftFunction? FindFunction(ISwiftType type, string name)
    {
        foreach (var candidate in type.Functions)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    internal static ISwiftProperty? FindProperty(ISwiftType type, string name)
    {
        foreach (var candidate in type.Properties)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    internal static ISwiftEnumCase? FindCase(ISwiftEnum enumeration, string name)
    {
        foreach (var candidate in enumeration.Cases)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    /// <summary>Function and property pairs that exist on both sides of a paired type.</summary>
    internal static IEnumerable<(ISwiftFunction Older, ISwiftFunction Newer)> GetPairedFunctions(
        ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var olderFunction in typePair.Older.Functions)
            {
                var newerFunction = FindFunction(typePair.Newer, olderFunction.Name);
                if (newerFunction == null)
                {
                    continue;
                }

                yield return (olderFunction, newerFunction);
            }
        }
    }

    internal static IEnumerable<(ISwiftProperty Older, ISwiftProperty Newer)> GetPairedProperties(
        ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var olderProperty in typePair.Older.Properties)
            {
                var newerProperty = FindProperty(typePair.Newer, olderProperty.Name);
                if (newerProperty == null)
                {
                    continue;
                }

                yield return (olderProperty, newerProperty);
            }
        }
    }

    /// <summary>Every paired declaration of any kind, for the rules that apply to all of them.</summary>
    internal static IEnumerable<(ISwiftDeclaration Older, ISwiftDeclaration Newer)> GetPairedDeclarations(
        ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            yield return (typePair.Older, typePair.Newer);
            foreach (var olderMember in GetAll(typePair.Older))
            {
                var newerMember = Find(typePair.Newer, olderMember);
                if (newerMember == null)
                {
                    continue;
                }

                yield return (olderMember, newerMember);
            }
        }
    }
}
