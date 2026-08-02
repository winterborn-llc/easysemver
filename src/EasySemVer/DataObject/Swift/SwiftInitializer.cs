using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("{Name}")]
[XmlType("Initializer")]
public class SwiftInitializer : SwiftDeclaration, ISwiftInitializer
{
    [XmlAttribute("failable")]
    public bool IsFailable { get; set; }

    [XmlAttribute("required")]
    public bool IsRequired { get; set; }

    [XmlAttribute("convenience")]
    public bool IsConvenience { get; set; }

    [XmlAttribute("async")]
    public bool IsAsync { get; set; }

    [XmlAttribute("throws")]
    public bool Throws { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<SwiftParameter> Parameters { get; set; } = [];

    IReadOnlyList<ISwiftParameter> ISwiftInitializer.Parameters => this.Parameters;
}
