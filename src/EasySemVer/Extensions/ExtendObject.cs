using System.Xml;
using System.Xml.Serialization;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendObject
{
    internal static string Serialize(this object? item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        var serializer = new XmlSerializer(item.GetType());
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "   ", // three spaces
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        serializer.Serialize(xmlWriter, item);
        return stringWriter.ToString();
    }
}