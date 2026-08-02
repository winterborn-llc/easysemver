using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendString
{
    internal static T Deserialize<T>(this string json)
    where T : class
    {
        var stringReader = new StringReader(json);

        var reader = new XmlTextReader(stringReader);
        var serializer = new XmlSerializer(typeof(T));
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

    private static T DeserializeReader<T>(XmlSerializer serializer, XmlTextReader reader, string json)
    where T : class
    {
        var item = serializer.Deserialize(reader) as T;
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

    internal static string GetSolutionDirectory(this string projectFilePath)
    {
        var solutionSuffixes = new[] { ".sln", ".slnx" };
        var file = new FileInfo(projectFilePath);
        var directory = file.Directory;
        while (directory != null)
        {
            foreach (var solutionSuffix in solutionSuffixes)
            {
                var solutionFiles = directory.GetFiles().Where(f => f.FullName.EndsWith(solutionSuffix));
                if (!solutionFiles.Any())
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