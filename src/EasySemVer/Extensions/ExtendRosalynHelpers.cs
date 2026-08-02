using Microsoft.CodeAnalysis;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendRosalynHelpers
{
    // Walks all namespaces and types from the global namespace
    public static IEnumerable<INamespaceOrTypeSymbol> GetNamespaceTypes(this INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                foreach (var nested in GetNamespaceTypes(childNs))
                    yield return nested;
            }
            else
            {
                yield return member;
            }
        }
    }
}