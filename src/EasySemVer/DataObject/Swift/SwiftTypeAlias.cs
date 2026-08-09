using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[DebuggerDisplay("typealias {Name} = {UnderlyingType}")]
[XmlType("TypeAlias")]
public class SwiftTypeAlias : SwiftDeclaration, ISwiftTypeAlias
{
    [XmlAttribute("underlyingType")]
    public string UnderlyingType { get; set; } = string.Empty;
}
