namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethod
{
    public string MethodName { get; init; }
    
    public string MethodType { get; init; }
    
    public ICsharpMethodOverrides Overrides { get; }
}