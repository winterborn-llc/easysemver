namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureProjectClass
{
    public string ClassName { get; init; } = string.Empty;

    public List<SignatureProjectClassMethod> Methods { get; init; } = [];

    public List<SignatureProjectClassProperty> Properties { get; init; } = [];
}