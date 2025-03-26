using System.Diagnostics;
using System.Xml;
using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion;

[DebuggerDisplay("{CsProjFile.ProjectName}")]
internal class CsProjFileLatest
{
    internal CsProjFile CsProjFile { get; }

    internal Project Project { get; }

    internal CsProjFileLatest(CsProjFile csProjFile)
    {
        this.CsProjFile = csProjFile;
        if (!File.Exists(this.CsProjFile.ProjectFilePath))
        {
            throw new FileNotFoundException(this.CsProjFile.ProjectFilePath);
        }

        this.Project = this.GetLatestSignature();
    }

    internal Project GetLatestSignature()
    {
        try
        {
            var xml = File.ReadAllText(this.CsProjFile.ProjectFilePath);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var elements = xmlDoc.GetElementsByTagName(MagicValues.AutoVersionPropertyName);
            if (elements.Count != 1)
            {
                return new Project();
            }

            var element = elements[0];
            var json = element!.InnerText; //.UnescapeXmlToJson();
            var project = json.Deserialize<Project>();
            return project;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Project();
        }
    }
}