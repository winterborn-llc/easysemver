using System.Diagnostics;

namespace Yamamari.Library.AutoVersion.SignatureStructure;

[DebuggerDisplay("{Name} ({Classes.Count})")]
public class Project(string name = "")
{
    public string Name { get; init; } = name;
    
    public List<ProjectClass> Classes { get; set; } = [];
}