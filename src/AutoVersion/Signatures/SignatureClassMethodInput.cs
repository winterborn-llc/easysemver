namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureClassMethodInput
{
    public string ParameterName { get; init; } = string.Empty;

    public string ParameterType { get; init; } = string.Empty;

    public bool IsRequired { get; init; } = true;
}