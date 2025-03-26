using System.Diagnostics;
using System.Xml;

namespace Yamamari.Library.AutoVersion;

[DebuggerDisplay("{Version}")]
internal class CsProjFileVersion
{
    internal Version Version { get; set; }
    
    internal CsProjFileVersion(string inputXml)
    {
        var versionText = GetVersionTextFromXml(inputXml);
        this.Version = new Version(versionText);
    }
    
    private static string GetVersionTextFromXml(string inputXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        var version = new Version();
        foreach (var propertyPreference in MagicValues.VersionPropertyNames)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            var value = elements[0]?.InnerText;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var specificVersion = new Version(value);
            if (specificVersion < version)
            {
                continue;
            }
            
            version = specificVersion;
        }

        return version;
    }
}