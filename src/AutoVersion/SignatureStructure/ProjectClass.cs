namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class ProjectClass
{
    public string Name { get; init; } = string.Empty;

    public Dictionary<string,Method> Methods { get; init; } = [];

    public Dictionary<string,Property> Properties { get; init; } = [];
}