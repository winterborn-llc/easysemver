using Microsoft.CodeAnalysis;

namespace Winterborn.Tools.EasySemVer.Extensions;

internal static class ExtendINamedTypeSymbol
{
    /// <summary>
    /// Roslyn's fully-qualified format, minus the global namespace prefix.
    /// <para>
    /// This asks Roslyn to omit it rather than stripping it afterwards, because the prefix is
    /// spelled per language: C# writes <c>global::Widgets.Gadget</c> and Visual Basic writes
    /// <c>Global.Widgets.Gadget</c>. Cutting at the <c>::</c> handled C# and silently let VB's
    /// through, which put every VB type in the baseline under a <c>Global.</c> that no rule and no
    /// reader would ever match; cutting at a leading <c>Global.</c> instead would corrupt a C# type
    /// that genuinely lives in a namespace called <c>Global</c>. Omitted is the answer that needs no
    /// per-language knowledge at all.
    /// </para>
    /// </summary>
    private static readonly SymbolDisplayFormat FullyQualified =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
            SymbolDisplayGlobalNamespaceStyle.Omitted);

    public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(FullyQualified);
    }

    public static bool IsPublic(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.DeclaredAccessibility == Accessibility.Public;
    }
}