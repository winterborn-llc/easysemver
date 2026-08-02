using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

/// <inheritdoc cref="ICsharpMethodDefinition"/>
[DebuggerDisplay("{Type} {Name}")]
internal class CsharpMethodDefinition : ICsharpMethodDefinition
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public CsharpMethodOverride Inputs { get; init; } = [];

    ICsharpMethodOverride ICsharpMethodDefinition.Inputs => this.Inputs;
}
