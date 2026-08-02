namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IProjectClass
{
    public string Name { get; init; }
    public IMethodList Methods { get; init; }
    public IPropertyList Properties { get; init; }
}