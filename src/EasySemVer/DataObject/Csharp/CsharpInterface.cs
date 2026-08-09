using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[XmlType("Interface")]
public class CsharpInterface : CsharpType, ICsharpInterface
{
    public override string Kind => CsharpTypeKinds.Interface;
}
