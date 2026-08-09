using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[XmlType("Struct")]
public class CsharpStruct : CsharpType, ICsharpStruct
{
    public override string Kind => CsharpTypeKinds.Struct;
}
