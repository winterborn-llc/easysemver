using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{MethodType} {MethodName}")]
internal class CsharpMethod : ICsharpMethod
{
    public string MethodName { get; init; } = string.Empty;
    
    public string MethodType { get; init; } = string.Empty;

    public ICsharpMethodOverrides Overrides { get; init; } = new CsharpMethodOverrides();
}