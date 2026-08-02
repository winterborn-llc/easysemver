using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[XmlType("Interface")]
public class CsharpInterface : CsharpType, ICsharpInterface
{
    public override string Kind => CsharpTypeKinds.Interface;
}
