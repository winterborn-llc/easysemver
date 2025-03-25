namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class Method
{
    public string MethodName { get; init; } = string.Empty;
    
    public string MethodType { get; init; } = string.Empty;

    public MethodOverrides Overrides { get; init; } = [];
}