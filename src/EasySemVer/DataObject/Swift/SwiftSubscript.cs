using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[DebuggerDisplay("{Name}")]
[XmlType("Subscript")]
public class SwiftSubscript : SwiftDeclaration, ISwiftSubscript
{
    [XmlAttribute("returns")]
    public string ReturnType { get; set; } = string.Empty;

    [XmlAttribute("settable")]
    public bool IsSettable { get; set; }

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<SwiftParameter> Parameters { get; set; } = [];

    IReadOnlyList<ISwiftParameter> ISwiftSubscript.Parameters => this.Parameters;
}
