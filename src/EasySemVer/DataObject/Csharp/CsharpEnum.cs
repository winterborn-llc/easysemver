using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[XmlType("Enum")]
public class CsharpEnum : CsharpType, ICsharpEnum
{
    public override string Kind => CsharpTypeKinds.Enum;

    [XmlAttribute("underlyingType")]
    public string UnderlyingType { get; set; } = string.Empty;

    [XmlArray("Members")]
    [XmlArrayItem("Member")]
    public List<CsharpEnumMember> Members { get; set; } = [];

    IReadOnlyList<ICsharpEnumMember> ICsharpEnum.Members => this.Members;

    internal override void SortForPersistence()
    {
        base.SortForPersistence();
        this.Members.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
    }
}
