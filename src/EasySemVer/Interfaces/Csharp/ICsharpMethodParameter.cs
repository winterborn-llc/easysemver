namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodParameter
{
    public string ParameterName { get; init; }
    public string ParameterType { get; init; }
    public bool IsRequired { get; init; }
}