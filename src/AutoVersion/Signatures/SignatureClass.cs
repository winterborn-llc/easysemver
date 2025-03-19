namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureClass
{
    public string ClassName { get; init; } = string.Empty;

    public List<SignatureClassMethod> Methods { get; init; } = [];

    public List<SignatureClassProperty> Properties { get; init; } = [];
}