using System.Xml.Serialization;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

/// <summary>
/// The facets and members every C# type kind shares (CSX-01…CSX-03). Concrete-typed throughout,
/// with the interface view supplied explicitly, so the whole graph stays serializable (BAS-02).
/// </summary>
[DebuggerDisplay("{Kind} {Name}")]
public abstract class CsharpType : ICsharpType
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("declaringType")]
    public string DeclaringType { get; set; } = string.Empty;

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("abstract")]
    public bool IsAbstract { get; set; }

    [XmlAttribute("sealed")]
    public bool IsSealed { get; set; }

    [XmlAttribute("base")]
    public string BaseType { get; set; } = string.Empty;

    [XmlArray("Implements")]
    [XmlArrayItem("Interface")]
    public List<string> ImplementedInterfaces { get; set; } = [];

    [XmlArray("GenericParameters")]
    [XmlArrayItem("GenericParameter")]
    public List<CsharpGenericParameter> GenericParameters { get; set; } = [];

    [XmlArray("Methods")]
    [XmlArrayItem("Method")]
    public CsharpMethodList Methods { get; set; } = [];

    [XmlArray("Properties")]
    [XmlArrayItem("Property")]
    public CsharpPropertyList Properties { get; set; } = [];

    [XmlArray("Fields")]
    [XmlArrayItem("Field")]
    public List<CsharpField> Fields { get; set; } = [];

    [XmlArray("Events")]
    [XmlArrayItem("Event")]
    public List<CsharpEvent> Events { get; set; } = [];

    [XmlIgnore]
    public abstract string Kind { get; }

    IReadOnlyList<string> ICsharpType.ImplementedInterfaces => this.ImplementedInterfaces;

    IReadOnlyList<ICsharpGenericParameter> ICsharpType.GenericParameters => this.GenericParameters;

    ICsharpMethodList ICsharpType.Methods => this.Methods;

    ICsharpPropertyList ICsharpType.Properties => this.Properties;

    IReadOnlyList<ICsharpField> ICsharpType.Fields => this.Fields;

    IReadOnlyList<ICsharpEvent> ICsharpType.Events => this.Events;

    /// <summary>BAS-04 - everything inside a type is ordered by identity before it is written.</summary>
    internal virtual void SortForPersistence()
    {
        this.ImplementedInterfaces.Sort(StringComparer.Ordinal);
        this.GenericParameters.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        this.Methods.Sort((left, right) => string.CompareOrdinal(left.MethodName, right.MethodName));
        this.Properties.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        this.Fields.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        this.Events.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        foreach (var method in this.Methods)
        {
            method.SortForPersistence();
        }
    }
}
