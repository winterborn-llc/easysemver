using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{DebugText}")]
public class CsharpProperty : ICsharpProperty
{
    private string DebugText
    {
        get
        {
            var get = this.IsReadable ? " get;" : string.Empty;
            var set = this.IsWritable ? " set;" : string.Empty;
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
}
