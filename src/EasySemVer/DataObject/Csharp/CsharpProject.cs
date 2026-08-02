using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Name} ({Classes.Count})")]
internal class CsharpProject(string name = "") : ICsharpProject
{
    public string Name { get; init; } = name;
    
    public List<ICsharpClass> Classes { get; set; } = [];
}