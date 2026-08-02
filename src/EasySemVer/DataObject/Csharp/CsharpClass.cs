using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[XmlType("Class")]
public class CsharpClass : CsharpType, ICsharpClass
{
    public override string Kind => CsharpTypeKinds.Class;
}
