namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

/// <summary>
/// The kind names carried by <see cref="CsharpType.Kind"/>. A kind change - struct to class,
/// enum to struct - is a different type, so the rules compare these as plain strings.
/// </summary>
internal static class CsharpTypeKinds
{
    internal const string Class = "class";
    internal const string Interface = "interface";
    internal const string Struct = "struct";
    internal const string Record = "record";
    internal const string Enum = "enum";
    internal const string Delegate = "delegate";
}
