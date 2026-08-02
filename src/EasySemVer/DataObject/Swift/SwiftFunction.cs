using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftFunction"/>
[DebuggerDisplay("{Name}")]
[XmlType("Function")]
public class SwiftFunction : SwiftDeclaration, ISwiftFunction
{
    [XmlAttribute("returns")]
    public string ReturnType { get; set; } = string.Empty;

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("mutating")]
    public bool IsMutating { get; set; }

    [XmlAttribute("async")]
    public bool IsAsync { get; set; }

    [XmlAttribute("throws")]
    public bool Throws { get; set; }

    [XmlAttribute("final")]
    public bool IsFinal { get; set; }

    [XmlAttribute("hasDefaultImplementation")]
    public bool HasDefaultImplementation { get; set; }

    [XmlAttribute("extensionConstraints")]
    public string ExtensionConstraints { get; set; } = string.Empty;

    [XmlArray("GenericParameters")]
    [XmlArrayItem("GenericParameter")]
    public List<SwiftGenericParameter> GenericParameters { get; set; } = [];

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<SwiftParameter> Parameters { get; set; } = [];

    IReadOnlyList<ISwiftParameter> ISwiftFunction.Parameters => this.Parameters;

    IReadOnlyList<ISwiftGenericParameter> ISwiftFunction.GenericParameters => this.GenericParameters;

    internal override void SortForPersistence()
    {
        base.SortForPersistence();
        this.GenericParameters.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
    }
}
