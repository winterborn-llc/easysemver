using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("{Name} : {Constraints}")]
[XmlType("GenericParameter")]
public class SwiftGenericParameter : ISwiftGenericParameter
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("constraints")]
    public string Constraints { get; set; } = string.Empty;
}
