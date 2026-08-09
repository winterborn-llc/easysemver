using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("event {HandlerType} {Name}")]
[XmlType("Event")]
public class CsharpEvent : ICsharpEvent
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("handler")]
    public string HandlerType { get; set; } = string.Empty;

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }
}
