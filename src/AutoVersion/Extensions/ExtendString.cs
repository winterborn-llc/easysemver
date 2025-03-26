using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;

namespace Yamamari.Library.AutoVersion.Extensions;

internal static class ExtendString
{
    internal static T Deserialize<T>(this string json)
    {
        var stringReader = new StringReader(json);
        var reader = new JsonTextReader(stringReader);
        var serializer = new JsonSerializer();
        try
        {
            var item = DeserializeReader<T>(serializer, reader, json);
            return item;
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Unable to create an instance of '{typeof(T).Name}' from the json: {json}", ex);
        }
    }

    private static T DeserializeReader<T>(JsonSerializer serializer, JsonTextReader reader, string json)
    {
        var item = serializer.Deserialize<T>(reader);
        if (item != null)
        {
            return item;
        }

        throw new InvalidCastException($"Unable to create an instance of '{typeof(T).Name}' from the json: {json}");
    }
    internal static bool IsNullOrWhitespace([NotNullWhen(false)] this string? text)
    {
        return string.IsNullOrWhiteSpace(text);
    }
    
    internal static string? GetXmlNodeValue(this string xml, string nodeName)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
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
    
    internal static string EscapeJsonForXml(this string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        var escapes = new Dictionary<string, string>
        {
            { "&", "&amp;" },
            { "<", "&lt;" },
            { ">", "&gt;" },
            { "\"", "&quot;" },
            { "'", "&apos;" }
        };

        foreach (var (key, value) in escapes)
        {
            json = json.Replace(key, value);
        }

        return json;
    }

    internal static string UnescapeXmlToJson(this string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return xml;
        }

        var escapes = new Dictionary<string, string>
        {
            { "<", "&lt;" },
            { ">", "&gt;" },
            { "\"", "&quot;" },
            { "'", "&apos;" },
            { "&", "&amp;" }
        };

        foreach (var (key, value) in escapes)
        {
            xml = xml.Replace(value, key);
        }

        return xml;
    }
    
    internal static string GetSolutionDirectory(this string projectFilePath)
    {
        var solutionSuffixes = new[] { ".sln", ".slnx" };
        var file = new FileInfo(projectFilePath);
        if (!file.Exists)
        {
            return string.Empty;
        }
        
        var directory = file.Directory;
        while (directory != null)
        {
            foreach (var solutionSuffix in solutionSuffixes)
            {
                var solutionFile = directory.GetFiles(solutionSuffix);
                if (solutionFile.Length < 1)
                {
                    continue;
                }
                
                return directory.FullName;
            }
            
            directory = directory.Parent;
        }

        return string.Empty;
    }
}