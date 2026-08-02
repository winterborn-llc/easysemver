using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Name}")]
public class CsharpClass : ICsharpClass
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlArray("Methods")]
    [XmlArrayItem("Method")]
    public CsharpMethodList Methods { get; set; } = [];

    [XmlArray("Properties")]
    [XmlArrayItem("Property")]
    public CsharpPropertyList Properties { get; set; } = [];

    ICsharpMethodList ICsharpClass.Methods => this.Methods;

    ICsharpPropertyList ICsharpClass.Properties => this.Properties;

    internal void SortForPersistence()
    {
        this.Methods.Sort((left, right) => string.CompareOrdinal(left.MethodName, right.MethodName));
        this.Properties.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        foreach (var method in this.Methods)
        {
            method.SortForPersistence();
        }
    }
}
