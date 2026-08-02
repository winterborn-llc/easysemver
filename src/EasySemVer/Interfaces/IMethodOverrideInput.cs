namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IMethodOverrideInput
{
    public string ParameterName { get; init; }
    public string ParameterType { get; init; }
    public bool IsRequired { get; init; }
}