using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendObject
{
    /// <summary>
    /// Renders a signature object as the element that goes inside its baseline entry. The xsi/xsd
    /// namespace declarations XmlSerializer emits by default are suppressed: they carry no
    /// information here and would only add noise to a file meant to be read in code review.
    /// </summary>
    internal static XElement SerializeToElement(this object item)
    {
        var serializer = new XmlSerializer(item.GetType());
        var settings = new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = true
        };

        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);

        using var stringWriter = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            serializer.Serialize(xmlWriter, item, namespaces);
        }

        return XElement.Parse(stringWriter.ToString(), LoadOptions.None);
    }

    /// <summary>The inverse of <see cref="SerializeToElement"/>.</summary>
    internal static T DeserializeElement<T>(this XElement element)
    where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = element.CreateReader();
        return serializer.Deserialize(reader) as T
               ?? throw new InvalidCastException(
                   $"Unable to create an instance of '{typeof(T).Name}' from <{element.Name}>");
    }
}
