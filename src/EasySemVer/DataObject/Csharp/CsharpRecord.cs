using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[XmlType("Record")]
public class CsharpRecord : CsharpType, ICsharpRecord
{
    public override string Kind => CsharpTypeKinds.Record;

    [XmlAttribute("valueType")]
    public bool IsValueType { get; set; }

    [XmlArray("PositionalParameters")]
    [XmlArrayItem("Parameter")]
    public List<CsharpMethodParameter> PositionalParameters { get; set; } = [];

    IReadOnlyList<ICsharpMethodParameter> ICsharpRecord.PositionalParameters =>
        this.PositionalParameters;
}
