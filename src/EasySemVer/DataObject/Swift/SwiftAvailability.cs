using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftAvailability"/>
[DebuggerDisplay("{Domain}")]
[XmlType("Available")]
public class SwiftAvailability : ISwiftAvailability
{
    [XmlAttribute("domain")]
    public string Domain { get; set; } = string.Empty;

    [XmlAttribute("introduced")]
    public string Introduced { get; set; } = string.Empty;

    [XmlAttribute("deprecated")]
    public string Deprecated { get; set; } = string.Empty;

    [XmlAttribute("obsoleted")]
    public string Obsoleted { get; set; } = string.Empty;

    [XmlAttribute("isDeprecated")]
    public bool IsDeprecated { get; set; }

    [XmlAttribute("unavailable")]
    public bool IsUnavailable { get; set; }

    [XmlAttribute("renamed")]
    public string RenamedTo { get; set; } = string.Empty;
}
