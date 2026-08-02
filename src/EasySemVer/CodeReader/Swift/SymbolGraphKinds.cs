namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>The <c>kind.identifier</c> values the symbol graph uses.</summary>
internal static class SymbolGraphKinds
{
    internal const string Class = "swift.class";
    internal const string Struct = "swift.struct";
    internal const string Enum = "swift.enum";
    internal const string Protocol = "swift.protocol";
    internal const string EnumCase = "swift.enum.case";
    internal const string Initializer = "swift.init";
    internal const string Method = "swift.method";
    internal const string TypeMethod = "swift.type.method";
    internal const string Function = "swift.func";
    internal const string Operator = "swift.func.op";
    internal const string Property = "swift.property";
    internal const string TypeProperty = "swift.type.property";
    internal const string Variable = "swift.var";
    internal const string Subscript = "swift.subscript";
    internal const string TypeSubscript = "swift.type.subscript";
    internal const string TypeAlias = "swift.typealias";
    internal const string AssociatedType = "swift.associatedtype";
    internal const string Extension = "swift.extension";
}
