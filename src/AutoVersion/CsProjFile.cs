using System.Diagnostics;
using System.Xml;
using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion;

[DebuggerDisplay("{ProjectName}")]
internal class CsProjFile
{
    public Version Version { get; set; }

    public string ProjectName { get; }

    public string SolutionDirectory { get; }

    public string ProjectFilePath { get; }

    public string ProjectXml { get; private set; }
    
    public Project ProjectLatest { get; }
    
    public Project ProjectActual { get; }

    public CsProjFile(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException(projectFilePath);
        }

        this.ProjectFilePath = projectFilePath;
        this.ProjectName = new FileInfo(projectFilePath).Name;
        this.ProjectXml = File.ReadAllText(this.ProjectFilePath);
        this.SolutionDirectory = projectFilePath.GetSolutionDirectory();
        var csProjVersion = new CsProjFileVersion(this.ProjectXml);
        this.Version = csProjVersion.Version;
        this.ProjectLatest = new CsProjFileLatest(this).Project;
        this.ProjectActual = new CsProjFileActual(this).Project;
    }

    public void Save()
    {
        this.ProjectXml = UpdateXmlForSignature(this.ProjectXml, this.ProjectActual);
        this.ProjectXml = UpdateXmlForVersion(this.ProjectXml, this.Version);
        File.WriteAllText(this.ProjectFilePath, this.ProjectXml);
    }

    private static string UpdateXmlForSignature(string inputXml, Project latest)
    {
        var jsonForXml = latest.Serialize(); //.EscapeJsonForXml();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);

        var autoVersionNodes = xmlDoc.GetElementsByTagName(MagicValues.AutoVersionPropertyName);
        if (autoVersionNodes.Count == 1)
        {
            autoVersionNodes[0]!.InnerText = jsonForXml;
            var autoXml = CleanXmlContent(xmlDoc.InnerXml);
            return autoXml;
        }

        var root = xmlDoc["Project"];
        if (root == null)
        {
            return inputXml;
        }
        
        var autoVersionNode = xmlDoc.CreateNode(XmlNodeType.Element, MagicValues.AutoVersionPropertyName, "");
        if (autoVersionNode == null)
        {
            return inputXml;
        }
        
        autoVersionNode.InnerText = jsonForXml;

        var autoVersionPropertyGroup = xmlDoc.CreateNode(XmlNodeType.Element, "PropertyGroup", "");
        autoVersionPropertyGroup.AppendChild(autoVersionNode);

        root.AppendChild(autoVersionPropertyGroup);

        var xml = CleanXmlContent(xmlDoc.InnerXml);
        return xml;
    }

    private static string UpdateXmlForVersion(string inputXml, Version newVersion)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);

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