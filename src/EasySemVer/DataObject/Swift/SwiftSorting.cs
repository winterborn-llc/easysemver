namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <summary>BAS-04 - symbol-graph ordering is not guaranteed by the toolchain, so we impose one.</summary>
internal static class SwiftSorting
{
    internal static void ByName<T>(List<T> declarations)
    where T : SwiftDeclaration
    {
        declarations.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
    }
}
