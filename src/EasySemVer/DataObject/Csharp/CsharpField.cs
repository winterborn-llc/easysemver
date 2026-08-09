using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Type} {Name}")]
[XmlType("Field")]
public class CsharpField : ICsharpField
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("readonly")]
    public bool IsReadOnly { get; set; }

    [XmlAttribute("const")]
    public bool IsConstant { get; set; }
}
