using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{Name} ({Classes.Count})")]
internal class Project(string name = "") : IProject
{
    public string Name { get; init; } = name;
    
    public List<IProjectClass> Classes { get; set; } = [];
}