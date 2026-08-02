using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftType"/>
[DebuggerDisplay("{Kind} {Name}")]
public abstract class SwiftType : SwiftDeclaration, ISwiftType
{
    [XmlAttribute("final")]
    public bool IsFinal { get; set; }

    [XmlAttribute("frozen")]
    public bool IsFrozen { get; set; }

    [XmlAttribute("superclass")]
    public string Superclass { get; set; } = string.Empty;

    [XmlArray("Conformances")]
    [XmlArrayItem("Conformance")]
    public List<string> Conformances { get; set; } = [];

    [XmlArray("GenericParameters")]
    [XmlArrayItem("GenericParameter")]
    public List<SwiftGenericParameter> GenericParameters { get; set; } = [];

    [XmlArray("Initializers")]
    [XmlArrayItem("Initializer")]
    public List<SwiftInitializer> Initializers { get; set; } = [];

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
    public abstract string Kind { get; }

    IReadOnlyList<string> ISwiftType.Conformances => this.Conformances;

    IReadOnlyList<ISwiftGenericParameter> ISwiftType.GenericParameters => this.GenericParameters;

    IReadOnlyList<ISwiftInitializer> ISwiftType.Initializers => this.Initializers;

    IReadOnlyList<ISwiftFunction> ISwiftType.Functions => this.Functions;

    IReadOnlyList<ISwiftProperty> ISwiftType.Properties => this.Properties;

    IReadOnlyList<ISwiftSubscript> ISwiftType.Subscripts => this.Subscripts;

    internal override void SortForPersistence()
    {
        base.SortForPersistence();
        this.Conformances.Sort(StringComparer.Ordinal);
        this.GenericParameters.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        SwiftSorting.ByName(this.Initializers);
        SwiftSorting.ByName(this.Functions);
        SwiftSorting.ByName(this.Properties);
        SwiftSorting.ByName(this.Subscripts);
        foreach (var function in this.Functions)
        {
            function.SortForPersistence();
        }
    }
}
