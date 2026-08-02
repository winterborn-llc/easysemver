using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[XmlType("Struct")]
public class SwiftStruct : SwiftType, ISwiftStruct
{
    public override string Kind => SwiftTypeKinds.Struct;
}
