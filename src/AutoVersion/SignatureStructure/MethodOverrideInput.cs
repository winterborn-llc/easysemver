namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class MethodOverrideInput
{
    public string ParameterName { get; init; } = string.Empty;

    public string ParameterType { get; init; } = string.Empty;

    public bool IsRequired { get; init; } = true;
}