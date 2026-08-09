using System.Xml;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Settings;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Csharp;

/// <summary>
/// One .csproj on disk: its version properties, and the ability to write them back. Discovering
/// which .csproj files exist is the provider's job (FLD-03), not this type's.
/// </summary>
[DebuggerDisplay("{ProjectName}")]
public class CsProjFile
{
    public Version Version { get; private set; }

    public string ProjectName { get; }

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
        this.Version = new CsProjFileVersion(this.ProjectXml).Version;
    }

    /// <summary>SYN-02/SYN-03 - update every occurrence that already exists; never add one.</summary>
    public void Save(Version version)
    {
        this.Version = version;
        this.ProjectXml = UpdateXmlForVersion(this.ProjectXml, this.Version);
        File.WriteAllText(this.ProjectFilePath, this.ProjectXml);
    }

    public CsharpProject GetProject()
    {
        return CsharpUnitBuilder.GetProjectSignature(this.ProjectFilePath);
    }

    private static string UpdateXmlForVersion(string projectXml, Version newVersion)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(projectXml);

        foreach (var propertyPreference in MagicValues.VersionPropertyNames)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i]!.InnerText = newVersion;
            }
        }

        var xml = CleanXmlContent(xmlDoc.InnerXml);
        return xml;
    }

    private static string CleanXmlContent(string inputXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);

        var textWriter = new StringWriter();
        var writer = new XmlTextWriter(textWriter);
        writer.Formatting = Formatting.Indented;
        xmlDoc.WriteTo(writer);
        var xml = textWriter.ToString();
        return xml;
    }
}
