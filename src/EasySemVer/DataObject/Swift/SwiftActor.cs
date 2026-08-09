using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[XmlType("Actor")]
public class SwiftActor : SwiftType, ISwiftActor
{
    public override string Kind => SwiftTypeKinds.Actor;
}
