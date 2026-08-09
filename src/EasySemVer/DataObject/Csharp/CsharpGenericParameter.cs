using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Name} : {Constraints}")]
[XmlType("GenericParameter")]
public class CsharpGenericParameter : ICsharpGenericParameter
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("constraints")]
    public string Constraints { get; set; } = string.Empty;
}
