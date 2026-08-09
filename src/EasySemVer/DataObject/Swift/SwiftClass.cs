using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[XmlType("Class")]
public class SwiftClass : SwiftType, ISwiftClass
{
    public override string Kind => SwiftTypeKinds.Class;
}
