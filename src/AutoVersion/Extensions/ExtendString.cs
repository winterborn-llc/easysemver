using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Yamamari.AutoVersion.Extensions;

internal static class ExtendString
{
    public static bool IsNullOrWhitespace([NotNullWhen(false)] this string? text)
    {
        return string.IsNullOrWhiteSpace(text);
    }
    
    public static string? GetXmlNodeValue(this string xml, string nodeName)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xml);
        return GetXmlNodeValue(xmlDoc, nodeName);
    }

    private static string? GetXmlNodeValue(XmlNode node, string nodeName)
    {
        if (node.Name == nodeName)
        {
            return node.Value;
        }
        
        foreach (XmlNode sub in node.ChildNodes)
        {
            var output = GetXmlNodeValue(sub, nodeName);
            if (output.IsNullOrWhitespace())
            {
                continue;
            }

            return output;
        }

        return null;
    }
}