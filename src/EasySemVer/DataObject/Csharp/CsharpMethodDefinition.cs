using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Type} {Name}")]
internal class CsharpMethodDefinition : ICsharpMethodDefinition
{
    public string Name { get; init; } = string.Empty;
    
    public string Type { get; init; } = string.Empty;

    public ICsharpMethodOverride Inputs { get; init; } = new CsharpMethodOverride();
}