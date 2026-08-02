using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <inheritdoc cref="ISwiftDeclaration"/>
[DebuggerDisplay("{Name}")]
public abstract class SwiftDeclaration : ISwiftDeclaration
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("access")]
    public string AccessLevel { get; set; } = SwiftAccessLevels.Public;

    [XmlAttribute("objc")]
    public string ObjCExposure { get; set; } = string.Empty;

    [XmlArray("Availability")]
    [XmlArrayItem("Available")]
    public List<SwiftAvailability> Availability { get; set; } = [];

    IReadOnlyList<ISwiftAvailability> ISwiftDeclaration.Availability => this.Availability;

    internal virtual void SortForPersistence()
    {
        this.Availability.Sort((left, right) => string.CompareOrdinal(left.Domain, right.Domain));
    }
}
