using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[XmlType("Class")]
public class CsharpClass : CsharpType, ICsharpClass
{
    public override string Kind => CsharpTypeKinds.Class;
}
