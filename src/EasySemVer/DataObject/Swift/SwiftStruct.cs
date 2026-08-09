using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[XmlType("Struct")]
public class SwiftStruct : SwiftType, ISwiftStruct
{
    public override string Kind => SwiftTypeKinds.Struct;
}
