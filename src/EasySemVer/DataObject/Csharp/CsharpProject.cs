using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

/// <summary>
/// The persisted C# signature. Every member is a concrete type with a public setter and a
/// parameterless constructor, and the interface view is supplied by explicit implementation -
/// that shape is what makes the baseline serializable at all (BAS-02, was G-01).
/// Types are held in one list per kind so that the serializer never has to be told about
/// polymorphism, and so a kind change reads as remove + add.
/// </summary>
[DebuggerDisplay("{Name} ({Classes.Count} classes)")]
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

    [XmlArray("Interfaces")]
    [XmlArrayItem("Interface")]
    public List<CsharpInterface> Interfaces { get; set; } = [];

    [XmlArray("Structs")]
    [XmlArrayItem("Struct")]
    public List<CsharpStruct> Structs { get; set; } = [];

    [XmlArray("Records")]
    [XmlArrayItem("Record")]
    public List<CsharpRecord> Records { get; set; } = [];

    [XmlArray("Enums")]
    [XmlArrayItem("Enum")]
    public List<CsharpEnum> Enums { get; set; } = [];

    [XmlArray("Delegates")]
    [XmlArrayItem("Delegate")]
    public List<CsharpDelegate> Delegates { get; set; } = [];

    IReadOnlyList<ICsharpClass> ICsharpProject.Classes => this.Classes;

    IReadOnlyList<ICsharpInterface> ICsharpProject.Interfaces => this.Interfaces;

    IReadOnlyList<ICsharpStruct> ICsharpProject.Structs => this.Structs;

    IReadOnlyList<ICsharpRecord> ICsharpProject.Records => this.Records;

    IReadOnlyList<ICsharpEnum> ICsharpProject.Enums => this.Enums;

    IReadOnlyList<ICsharpDelegate> ICsharpProject.Delegates => this.Delegates;

    IReadOnlyList<ICsharpType> ICsharpProject.Types => this.GetAllTypes();

    internal List<CsharpType> GetAllTypes()
    {
        var types = new List<CsharpType>();
        types.AddRange(this.Classes);
        types.AddRange(this.Interfaces);
        types.AddRange(this.Structs);
        types.AddRange(this.Records);
        types.AddRange(this.Enums);
        types.AddRange(this.Delegates);
        return types;
    }

    internal void Add(CsharpType type)
    {
        switch (type)
        {
            case CsharpClass value:
                this.Classes.Add(value);
                return;
            case CsharpInterface value:
                this.Interfaces.Add(value);
                return;
            case CsharpStruct value:
                this.Structs.Add(value);
                return;
            case CsharpRecord value:
                this.Records.Add(value);
                return;
            case CsharpEnum value:
                this.Enums.Add(value);
                return;
            case CsharpDelegate value:
                this.Delegates.Add(value);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type.Kind, "Unknown C# type kind");
        }
    }

    /// <summary>BAS-04 - everything in the file is ordered by identity before it is written.</summary>
    internal void SortForPersistence()
    {
        SortByName(this.Classes);
        SortByName(this.Interfaces);
        SortByName(this.Structs);
        SortByName(this.Records);
        SortByName(this.Enums);
        SortByName(this.Delegates);
        foreach (var type in this.GetAllTypes())
        {
            type.SortForPersistence();
        }
    }

    private static void SortByName<T>(List<T> types)
    where T : CsharpType
    {
        types.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
    }
}
