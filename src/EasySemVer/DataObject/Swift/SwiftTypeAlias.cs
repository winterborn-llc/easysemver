using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("typealias {Name} = {UnderlyingType}")]
[XmlType("TypeAlias")]
public class SwiftTypeAlias : SwiftDeclaration, ISwiftTypeAlias
{
    [XmlAttribute("underlyingType")]
    public string UnderlyingType { get; set; } = string.Empty;
}
