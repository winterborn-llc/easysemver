using System.Text;
using System.Xml.Serialization;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

/// <inheritdoc cref="ICsharpMethodOverride"/>
/// <remarks>
/// Deliberately not a collection type: it carries facets of its own, and XmlSerializer drops
/// every property on anything it decides is a collection.
/// </remarks>
[DebuggerDisplay("({DebugText})")]
[XmlType("Override")]
public class CsharpMethodOverride : ICsharpMethodOverride
{
    private string DebugText
    {
        get
        {
            var text = new StringBuilder();
            foreach (var input in this.Parameters)
            {
                if (text.Length > 0)
                {
                    text.Append(", ");
                }

                text.Append(input.ParameterType);
                text.Append(' ');
                text.Append(input.ParameterName);
            }

            return text.ToString();
        }
    }

    public CsharpMethodOverride()
    {
    }

    public CsharpMethodOverride(params CsharpMethodParameter[] inputs)
    {
        this.Parameters.AddRange(inputs);
    }

    [XmlAttribute("returns")]
    public string ReturnType { get; set; } = string.Empty;

    [XmlAttribute("static")]
    public bool IsStatic { get; set; }

    [XmlAttribute("virtual")]
    public bool IsVirtual { get; set; }

    [XmlAttribute("abstract")]
    public bool IsAbstract { get; set; }

    [XmlAttribute("override")]
    public bool IsOverride { get; set; }

    [XmlAttribute("sealed")]
    public bool IsSealed { get; set; }

    [XmlAttribute("hasDefaultImplementation")]
    public bool HasDefaultImplementation { get; set; }

    [XmlArray("GenericParameters")]
    [XmlArrayItem("GenericParameter")]
    public List<CsharpGenericParameter> GenericParameters { get; set; } = [];

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<CsharpMethodParameter> Parameters { get; set; } = [];

    IReadOnlyList<ICsharpMethodParameter> ICsharpMethodOverride.Parameters => this.Parameters;

    IReadOnlyList<ICsharpGenericParameter> ICsharpMethodOverride.GenericParameters =>
        this.GenericParameters;
}
