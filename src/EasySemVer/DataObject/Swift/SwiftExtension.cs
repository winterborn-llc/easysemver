using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftExtension"/>
[DebuggerDisplay("extension {ExtendedType}")]
[XmlType("Extension")]
public class SwiftExtension : ISwiftExtension
{
    [XmlAttribute("extendedType")]
    public string ExtendedType { get; set; } = string.Empty;

    [XmlAttribute("constraints")]
    public string Constraints { get; set; } = string.Empty;

    [XmlArray("AddedConformances")]
    [XmlArrayItem("Conformance")]
    public List<string> AddedConformances { get; set; } = [];

    [XmlArray("Functions")]
    [XmlArrayItem("Function")]
    public List<SwiftFunction> Functions { get; set; } = [];

    [XmlArray("Properties")]
    [XmlArrayItem("Property")]
    public List<SwiftProperty> Properties { get; set; } = [];

    [XmlArray("Subscripts")]
    [XmlArrayItem("Subscript")]
    public List<SwiftSubscript> Subscripts { get; set; } = [];

    [XmlIgnore]
    public string Key => this.Constraints.Length < 1
        ? this.ExtendedType
        : $"{this.ExtendedType} where {this.Constraints}";

    IReadOnlyList<string> ISwiftExtension.AddedConformances => this.AddedConformances;

    IReadOnlyList<ISwiftFunction> ISwiftExtension.Functions => this.Functions;

    IReadOnlyList<ISwiftProperty> ISwiftExtension.Properties => this.Properties;

    IReadOnlyList<ISwiftSubscript> ISwiftExtension.Subscripts => this.Subscripts;

    internal void SortForPersistence()
    {
        this.AddedConformances.Sort(StringComparer.Ordinal);
        SwiftSorting.ByName(this.Functions);
        SwiftSorting.ByName(this.Properties);
        SwiftSorting.ByName(this.Subscripts);
    }
}
