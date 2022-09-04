using System.Xml;
using Yamamari.Library.VersionCounter.Extensions;

namespace Yamamari.Library.VersionCounter;

// <AssemblyVersion>1.0.1</AssemblyVersion>
// <PackageVersion>1.0.1</PackageVersion>
// <FileVersion>1.0.1</FileVersion>

internal class VersionHandler
{
    private static readonly string[] PropertyPreferences = 
        { "AssemblyVersion", "PackageVersion", "FileVersion" };
    
    public string SourceXml { get; }
    
    public string TargetXml { get; }
    
    public Version SourceVersion { get; }
    
    public Version TargetVersion { get; }
    
    public VersionHandler(string xml, bool isSignificant = false)
    {
        this.SourceXml = xml;
        this.SourceVersion = GetOriginalVersion(this.SourceXml);
        this.TargetVersion = this.SourceVersion.GetNextIncrement(isSignificant);
        this.TargetXml = UpdateVersion(this.SourceXml, this.TargetVersion);
    }
    
    internal static Version GetOriginalVersion(string inputXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        
        foreach (var propertyPreference in PropertyPreferences)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            var value = elements[0]?.InnerText;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }
            
            var version = new Version(value);
            return version;
        }

        return new Version("1.0.0");
    }
    
    internal static string UpdateVersion(string inputXml, Version newVersion)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        
        foreach (var propertyPreference in PropertyPreferences)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i]!.InnerText = newVersion.ToString();
            }
        }
        
        var xml = xmlDoc.InnerXml;
        return xml;
    }
}