using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{MethodType} {MethodName}")]
internal class Method : IMethod
{
    public string MethodName { get; init; } = string.Empty;
    
    public string MethodType { get; init; } = string.Empty;

    public IMethodOverrides Overrides { get; init; } = new MethodOverrides();
}