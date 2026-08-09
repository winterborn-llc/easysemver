using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[XmlType("Protocol")]
public class SwiftProtocol : SwiftType, ISwiftProtocol
{
    public override string Kind => SwiftTypeKinds.Protocol;

    [XmlArray("AssociatedTypes")]
    [XmlArrayItem("AssociatedType")]
    public List<string> AssociatedTypes { get; set; } = [];

    IReadOnlyList<string> ISwiftProtocol.AssociatedTypes => this.AssociatedTypes;

    internal override void SortForPersistence()
    {
        base.SortForPersistence();
        this.AssociatedTypes.Sort(StringComparer.Ordinal);
    }
}
