using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftModule"/>
/// <remarks>
/// Types are held in one list per kind for the same reason the C# model does it: XmlSerializer
/// never has to be told about polymorphism, and a kind change reads as remove + add (S03).
/// </remarks>
[DebuggerDisplay("{Name}")]
[XmlRoot("SwiftModule")]
public class SwiftModule : ISwiftModule
{
    public SwiftModule()
    {
    }

    public SwiftModule(string name)
    {
        this.Name = name;
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlArray("Classes")]
    [XmlArrayItem("Class")]
    public List<SwiftClass> Classes { get; set; } = [];

    [XmlArray("Structs")]
    [XmlArrayItem("Struct")]
    public List<SwiftStruct> Structs { get; set; } = [];

    [XmlArray("Actors")]
    [XmlArrayItem("Actor")]
    public List<SwiftActor> Actors { get; set; } = [];

    [XmlArray("Enums")]
    [XmlArrayItem("Enum")]
    public List<SwiftEnum> Enums { get; set; } = [];

    [XmlArray("Protocols")]
    [XmlArrayItem("Protocol")]
    public List<SwiftProtocol> Protocols { get; set; } = [];

    [XmlArray("Extensions")]
    [XmlArrayItem("Extension")]
    public List<SwiftExtension> Extensions { get; set; } = [];

    [XmlArray("GlobalFunctions")]
    [XmlArrayItem("Function")]
    public List<SwiftFunction> GlobalFunctions { get; set; } = [];

    [XmlArray("GlobalVariables")]
    [XmlArrayItem("Property")]
    public List<SwiftProperty> GlobalVariables { get; set; } = [];

    [XmlArray("TypeAliases")]
    [XmlArrayItem("TypeAlias")]
    public List<SwiftTypeAlias> TypeAliases { get; set; } = [];

    [XmlArray("Operators")]
    [XmlArrayItem("Operator")]
    public List<SwiftOperator> Operators { get; set; } = [];

    IReadOnlyList<ISwiftType> ISwiftModule.Types => this.GetAllTypes();

    IReadOnlyList<ISwiftExtension> ISwiftModule.Extensions => this.Extensions;

    IReadOnlyList<ISwiftFunction> ISwiftModule.GlobalFunctions => this.GlobalFunctions;

    IReadOnlyList<ISwiftProperty> ISwiftModule.GlobalVariables => this.GlobalVariables;

    IReadOnlyList<ISwiftTypeAlias> ISwiftModule.TypeAliases => this.TypeAliases;

    IReadOnlyList<ISwiftOperator> ISwiftModule.Operators => this.Operators;

    internal List<SwiftType> GetAllTypes()
    {
        var types = new List<SwiftType>();
        types.AddRange(this.Classes);
        types.AddRange(this.Structs);
        types.AddRange(this.Actors);
        types.AddRange(this.Enums);
        types.AddRange(this.Protocols);
        return types;
    }

    internal void Add(SwiftType type)
    {
        switch (type)
        {
            case SwiftClass value:
                this.Classes.Add(value);
                return;
            case SwiftStruct value:
                this.Structs.Add(value);
                return;
            case SwiftActor value:
                this.Actors.Add(value);
                return;
            case SwiftEnum value:
                this.Enums.Add(value);
                return;
            case SwiftProtocol value:
                this.Protocols.Add(value);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type.Kind, "Unknown Swift type kind");
        }
    }

    internal void SortForPersistence()
    {
        SwiftSorting.ByName(this.Classes);
        SwiftSorting.ByName(this.Structs);
        SwiftSorting.ByName(this.Actors);
        SwiftSorting.ByName(this.Enums);
        SwiftSorting.ByName(this.Protocols);
        SwiftSorting.ByName(this.GlobalFunctions);
        SwiftSorting.ByName(this.GlobalVariables);
        SwiftSorting.ByName(this.TypeAliases);
        SwiftSorting.ByName(this.Operators);
        this.Extensions.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));
        foreach (var type in this.GetAllTypes())
        {
            type.SortForPersistence();
        }

        foreach (var extension in this.Extensions)
        {
            extension.SortForPersistence();
        }
    }
}
