using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("case {Name}")]
[XmlType("Case")]
public class SwiftEnumCase : SwiftDeclaration, ISwiftEnumCase
{
    [XmlAttribute("rawValue")]
    public string RawValue { get; set; } = string.Empty;

    [XmlArray("AssociatedValues")]
    [XmlArrayItem("Parameter")]
    public List<SwiftParameter> AssociatedValues { get; set; } = [];

    IReadOnlyList<ISwiftParameter> ISwiftEnumCase.AssociatedValues => this.AssociatedValues;
}
