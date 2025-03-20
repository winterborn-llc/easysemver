using System.Xml;

namespace Yamamari.Library.AutoVersion;

internal class CsProjFile
{
    private static readonly string[] PropertyPreferences = 
        { "AssemblyVersion", "PackageVersion", "FileVersion" };

    public Version Version { get; set; }

    public string ProjectName { get; } = string.Empty;
    
    public string ProjectFilePath { get; }
    
    public string ProjectXml { get; private set; }

    public CsProjFile(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException(projectFilePath);
        }
        
        this.ProjectFilePath = projectFilePath;
        this.ProjectName = new FileInfo(projectFilePath).Name;
        this.ProjectXml = File.ReadAllText(this.ProjectFilePath);
        this.Version = GetOriginalVersion(this.ProjectXml);
    }
    
    public void Save()
    {
        this.ProjectXml = GetUpdatedXml(this.ProjectXml, this.Version);
        File.WriteAllText(this.ProjectFilePath, this.ProjectXml);
    }

    private static string GetUpdatedXml(string inputXml, Version newVersion)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        
        foreach (var propertyPreference in PropertyPreferences)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i]!.InnerText = newVersion;
            }
        }
        
        var textWriter = new StringWriter();
        var writer = new XmlTextWriter(textWriter);
        writer.Formatting = Formatting.Indented;
        xmlDoc.WriteTo(writer);
        var xml = textWriter.ToString();
        return xml;
    }
    
    internal static Version GetOriginalVersion(string inputXml)
    {
        var versionText = GetVersionTextFromXml(inputXml);
        var version = new Version(versionText);
        return version;
    }

    private static string GetVersionTextFromXml(string inputXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        var version = new Version();
        foreach (var propertyPreference in PropertyPreferences)
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