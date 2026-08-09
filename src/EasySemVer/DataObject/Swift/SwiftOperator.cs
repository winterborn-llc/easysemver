using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.DataObject.Swift;

[DebuggerDisplay("operator {Name}")]
[XmlType("Operator")]
public class SwiftOperator : SwiftDeclaration, ISwiftOperator
{
    [XmlAttribute("operatorKind")]
    public string OperatorKind { get; set; } = string.Empty;

    [XmlAttribute("precedenceGroup")]
    public string PrecedenceGroup { get; set; } = string.Empty;
}
