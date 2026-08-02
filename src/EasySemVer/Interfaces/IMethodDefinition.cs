namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IMethodDefinition
{
    public string Name { get; init; }
    
    public string Type { get; init; }
    
    public IMethodOverride Inputs { get; }
}