namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IMethod
{
    public string MethodName { get; init; }
    
    public string MethodType { get; init; }
    
    public IMethodOverrides Overrides { get; }
}