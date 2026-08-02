using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("{Label} {InternalName}: {Type}")]
[XmlType("Parameter")]
public class SwiftParameter : ISwiftParameter
{
    [XmlAttribute("label")]
    public string Label { get; set; } = string.Empty;

    [XmlAttribute("internalName")]
    public string InternalName { get; set; } = string.Empty;

    [XmlAttribute("type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute("hasDefault")]
    public bool HasDefault { get; set; }

    [XmlAttribute("inout")]
    public bool IsInout { get; set; }

    [XmlAttribute("variadic")]
    public bool IsVariadic { get; set; }

    [XmlAttribute("ownership")]
    public string Ownership { get; set; } = string.Empty;
}
