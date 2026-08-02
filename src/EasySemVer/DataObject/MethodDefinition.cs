using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{Type} {Name}")]
internal class MethodDefinition : IMethodDefinition
{
    public string Name { get; init; } = string.Empty;
    
    public string Type { get; init; } = string.Empty;

    public IMethodOverride Inputs { get; init; } = new MethodOverride();
}