using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[XmlType("Enum")]
public class SwiftEnum : SwiftType, ISwiftEnum
{
    public override string Kind => SwiftTypeKinds.Enum;

    [XmlAttribute("rawValueType")]
    public string RawValueType { get; set; } = string.Empty;

    [XmlArray("Cases")]
    [XmlArrayItem("Case")]
    public List<SwiftEnumCase> Cases { get; set; } = [];

    IReadOnlyList<ISwiftEnumCase> ISwiftEnum.Cases => this.Cases;

    internal override void SortForPersistence()
    {
        base.SortForPersistence();
        SwiftSorting.ByName(this.Cases);
    }
}
