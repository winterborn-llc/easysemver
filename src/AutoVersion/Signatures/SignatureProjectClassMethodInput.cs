namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureProjectClassMethodInput
{
    public string ParameterName { get; init; } = string.Empty;

    public string ParameterType { get; init; } = string.Empty;

    public bool IsRequired { get; init; } = true;
}