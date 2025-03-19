namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureClassMethod
{
    public string MethodName { get; init; } = string.Empty;
    
    public string MethodType { get; init; } = string.Empty;

    public List<SignatureClassMethodInput> Parameters { get; init; } = [];
}