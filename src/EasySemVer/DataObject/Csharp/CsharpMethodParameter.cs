using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{DebugText}")]
[XmlType("Parameter")]
public class CsharpMethodParameter : ICsharpMethodParameter
{
    private string DebugText
    {
        get
        {
            var prefix = this.IsRequired ? "[" : string.Empty;
            var suffix = this.IsRequired ? "]" : string.Empty;
            return $"{prefix}{this.ParameterType} {this.ParameterName}{suffix}";
        }
    }

    [XmlAttribute("name")]
    public string ParameterName { get; set; } = string.Empty;

    [XmlAttribute("type")]
    public string ParameterType { get; set; } = string.Empty;

    [XmlAttribute("required")]
    public bool IsRequired { get; set; } = true;
}
