using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{MethodType} {MethodName}")]
[XmlType("Method")]
public class CsharpMethod : ICsharpMethod
{
    [XmlAttribute("name")]
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// The return type of the first overload encountered. Kept for R03's original shape; the
    /// per-overload return type on <see cref="CsharpMethodOverride"/> is what actually closes
    /// G-14.
    /// </summary>
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
        foreach (var methodOverride in this.Overrides)
        {
            methodOverride.GenericParameters.Sort(
                (left, right) => string.CompareOrdinal(left.Name, right.Name));
        }
    }
}
