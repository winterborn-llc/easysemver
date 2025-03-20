namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureProjectClassMethod
{
    public string MethodName { get; init; } = string.Empty;
    
    public string MethodType { get; init; } = string.Empty;

    public List<SignatureProjectClassMethodInput> Parameters { get; init; } = [];
}