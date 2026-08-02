using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

/// <summary>
/// The persisted C# signature. Every member is a concrete type with a public setter and a
/// parameterless constructor, and the interface view is supplied by explicit implementation - that
/// shape is what makes the baseline serializable at all (BAS-02, was G-01).
/// </summary>
[DebuggerDisplay("{Name} ({Classes.Count})")]
[XmlRoot("CsharpProject")]
public class CsharpProject : ICsharpProject
{
    public CsharpProject()
    {
    }

    public CsharpProject(string name)
    {
        this.Name = name;
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlArray("Classes")]
    [XmlArrayItem("Class")]
    public List<CsharpClass> Classes { get; set; } = [];

    IReadOnlyList<ICsharpClass> ICsharpProject.Classes => this.Classes;

    /// <summary>BAS-04 - everything in the file is ordered by identity before it is written.</summary>
    internal void SortForPersistence()
    {
        this.Classes.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        foreach (var projectClass in this.Classes)
        {
            projectClass.SortForPersistence();
        }
    }
}
