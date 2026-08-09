using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[XmlType("Delegate")]
public class CsharpDelegate : CsharpType, ICsharpDelegate
{
    public override string Kind => CsharpTypeKinds.Delegate;

    [XmlAttribute("returns")]
    public string ReturnType { get; set; } = string.Empty;

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<CsharpMethodParameter> Parameters { get; set; } = [];

    IReadOnlyList<ICsharpMethodParameter> ICsharpDelegate.Parameters => this.Parameters;
}
