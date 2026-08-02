using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{MethodType} {MethodName}")]
public class CsharpMethod : ICsharpMethod
{
    [XmlAttribute("name")]
    public string MethodName { get; set; } = string.Empty;

    [XmlAttribute("returns")]
    public string MethodType { get; set; } = string.Empty;

    [XmlArray("Overrides")]
    [XmlArrayItem("Override")]
    public CsharpMethodOverrides Overrides { get; set; } = [];

    ICsharpMethodOverrides ICsharpMethod.Overrides => this.Overrides;

    internal void SortForPersistence()
    {
        this.Overrides.Sort((left, right) =>
            string.CompareOrdinal(left.GetMethodSignature(), right.GetMethodSignature()));
    }
}
