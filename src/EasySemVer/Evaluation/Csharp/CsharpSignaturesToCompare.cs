using System.Text;
using System.Xml;
using Winterborn.Library.EasySemVer.CodeReader;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;
using Winterborn.Library.EasySemVer.Settings;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Evaluation.Csharp;

internal class CsharpSignaturesToCompare : ICsharpSignaturesToCompare
{
    private string SolutionPath { get; }

    private string SignaturePath => Path.Combine(this.SolutionPath, MagicValues.SignatureFileName);
    
    public ISolution Older { get; }

    public ISolution Newer { get; }
    
    private CsProjFile[] ProjectFiles { get; }
    
    public ICsharpClassHistory[] ClassHistory {get; }

    public CsharpSignaturesToCompare(string solutionDirectory, ISolution older, ISolution newer)
    {
        this.Older = older;
        this.Newer = newer;
        this.SolutionPath = solutionDirectory;
        this.ProjectFiles = CsProjFile.GetSolutionProjectFiles(solutionDirectory);
        this.ClassHistory = GetClassesInBoth();
    }
    
    public void Save(Version version)
    {
        if (this.SignaturePath.IsNullOrWhitespace())
        {
            return;
        }
        
        var latestXml = this.Newer.Serialize();
        var xmlWriter = new XmlTextWriter(this.SignaturePath, Encoding.UTF8);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(latestXml);
        xmlDoc.Save(xmlWriter);

        foreach (var csProjFile in this.ProjectFiles)
        {
            csProjFile.Save(version);
        }
    }
    
    private CsharpClassHistory[] GetClassesInBoth()
    {
        var list = new List<CsharpClassHistory>();
        foreach (var oldProject in this.Older)
        {
            var newProject = this.Newer.FirstOrDefault(p => p.Name == oldProject.Name);
            if (newProject == null)
            {
                continue;
            }
            
            foreach (var oldClass in oldProject.Classes)
            {
                var newClass = newProject.Classes.FirstOrDefault(c => c.Name == oldClass.Name);
                if (newClass == null)
                {
                    continue;
                }

                var classes = new CsharpClassHistory(oldClass, newClass);
                list.Add(classes);
            }
        }

        return list.ToArray();
    }
}