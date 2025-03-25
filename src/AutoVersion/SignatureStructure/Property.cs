namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class Property
{
    public string Name { get; init; } = string.Empty;
    
    public string Type { get; init; } = string.Empty;

    public bool IsReadable { get; init; }
    
    public bool IsWritable { get; init; }
}