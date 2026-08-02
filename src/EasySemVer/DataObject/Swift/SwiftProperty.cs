using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("{Name}: {Type}")]
[XmlType("Property")]
public class SwiftProperty : SwiftDeclaration, ISwiftProperty
{
    [XmlAttribute("type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute("settable")]
    public bool IsSettable { get; set; }

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("mutating")]
    public bool IsMutating { get; set; }

    [XmlAttribute("async")]
    public bool IsAsync { get; set; }

    [XmlAttribute("throws")]
    public bool Throws { get; set; }

    [XmlAttribute("hasDefaultImplementation")]
    public bool HasDefaultImplementation { get; set; }
}
