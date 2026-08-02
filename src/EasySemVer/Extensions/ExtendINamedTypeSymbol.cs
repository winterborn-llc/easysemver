using Microsoft.CodeAnalysis;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendINamedTypeSymbol
{
    public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol)
    {
        const string divider = "::";
        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (!fullName.Contains(divider))
        {
            return fullName;
        }
        
        var genericMarkerIndex = fullName.IndexOf(divider, StringComparison.Ordinal) + divider.Length;
        // ReSharper disable once ReplaceSubstringWithRangeIndexer
        var realName = fullName.Substring(genericMarkerIndex);
        return realName;
    }

    public static bool IsPublic(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.DeclaredAccessibility == Accessibility.Public;
    }
}