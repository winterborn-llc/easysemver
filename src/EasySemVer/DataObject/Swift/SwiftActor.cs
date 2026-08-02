using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[XmlType("Actor")]
public class SwiftActor : SwiftType, ISwiftActor
{
    public override string Kind => SwiftTypeKinds.Actor;
}
