using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{DebugText}")]
[XmlType("Property")]
public class CsharpProperty : ICsharpProperty
{
    private string DebugText
    {
        get
        {
            var get = this.IsReadable ? " get;" : string.Empty;
            var set = this.IsWritable ? this.IsInitOnly ? " init;" : " set;" : string.Empty;
            return $"{this.Type} {this.Name} {{{get}{set} }}";
        }
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute("readable")]
    public bool IsReadable { get; set; }

    [XmlAttribute("writable")]
    public bool IsWritable { get; set; }

    [XmlAttribute("initOnly")]
    public bool IsInitOnly { get; set; }

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("required")]
    public bool IsRequired { get; set; }

    [XmlAttribute("hasDefaultImplementation")]
    public bool HasDefaultImplementation { get; set; }
}
