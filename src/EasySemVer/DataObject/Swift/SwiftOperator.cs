using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.DataObject.Swift;

[DebuggerDisplay("operator {Name}")]
[XmlType("Operator")]
public class SwiftOperator : SwiftDeclaration, ISwiftOperator
{
    [XmlAttribute("operatorKind")]
    public string OperatorKind { get; set; } = string.Empty;

    [XmlAttribute("precedenceGroup")]
    public string PrecedenceGroup { get; set; } = string.Empty;
}
