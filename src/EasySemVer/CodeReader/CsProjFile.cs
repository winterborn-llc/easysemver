using System.Xml;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Settings;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.CodeReader;

[DebuggerDisplay("{ProjectName}")]
public class CsProjFile
{
    public Version Version { get; set; }

    public string ProjectName { get; }

    public string SolutionDirectory { get; }

    public string ProjectFilePath { get; }

    public string ProjectXml { get; private set; }

    public IProject GetProject()
    {
        return SolutionBuilder.GetProjectSignature(this.ProjectFilePath);
    }

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
    }

    public void Save(Version version)
    {
        this.Version = version;
        this.ProjectXml = UpdateXmlForVersion(this.ProjectXml, this.Version);
        File.WriteAllText(this.ProjectFilePath, this.ProjectXml);
    }

    public FileInfo[] GetCsFiles()
    {
        var csFile = new FileInfo(this.ProjectFilePath);
        var csDirectory = csFile.Directory;
        var csFiles = csDirectory!.GetFiles("*.cs", SearchOption.AllDirectories);
        return csFiles;
    }
    
    internal static string[] GetSolutionProjectFilePaths(string startingDirectory)
    {
        var paths = new List<string>();
        var csProjFiles = GetSolutionProjectFiles(startingDirectory);
        foreach (var csProjFile in csProjFiles)
        {
            paths.Add(csProjFile.ProjectFilePath);
        }
        
        return paths.ToArray();
    }
    
    internal static CsProjFile[] GetSolutionProjectFiles(string startingDirectory)
    {
        if (startingDirectory.IsNullOrWhitespace())
        {
            startingDirectory = Environment.CurrentDirectory.GetSolutionDirectory();
        }
        
        var csProjFiles = new List<CsProjFile>();
        var projectFilePaths = Directory.GetFiles(startingDirectory, "*.csproj", SearchOption.AllDirectories);
        foreach (var projectFilePath in projectFilePaths)
        {
            Console.WriteLine($"Processing project file: {projectFilePath}");
            var csProjFile = new CsProjFile(projectFilePath);
            csProjFiles.Add(csProjFile);
        }
        
        return csProjFiles.ToArray();
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