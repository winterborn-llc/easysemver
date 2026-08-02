using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[XmlType("Struct")]
public class CsharpStruct : CsharpType, ICsharpStruct
{
    public override string Kind => CsharpTypeKinds.Struct;
}
