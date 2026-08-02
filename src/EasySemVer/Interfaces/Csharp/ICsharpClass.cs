namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpClass
{
    public string Name { get; init; }
    public ICsharpMethodList Methods { get; init; }
    public ICsharpPropertyList Properties { get; init; }
}