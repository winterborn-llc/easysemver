namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethod
{
    public string MethodName { get; }

    /// <summary>Return type of the first overload encountered; R03's subject.</summary>
    public string MethodType { get; }

    public ICsharpMethodOverrides Overrides { get; }
}
