using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[XmlType("Class")]
public class SwiftClass : SwiftType, ISwiftClass
{
    public override string Kind => SwiftTypeKinds.Class;
}
