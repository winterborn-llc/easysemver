namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodDefinition
{
    public string Name { get; init; }
    
    public string Type { get; init; }
    
    public ICsharpMethodOverride Inputs { get; }
}