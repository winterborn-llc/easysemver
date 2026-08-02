using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.CodeReader;

[DebuggerDisplay("{CsProjFile.ProjectName}")]
internal class CsProjSignature
{
    internal CsProjFile CsProjFile { get; }
    
    internal IProject Project { get; }
    
    internal CsProjSignature(CsProjFile csProjFile)
    {
        this.CsProjFile = csProjFile;
        if (!File.Exists(csProjFile.ProjectFilePath))
        {
            throw new FileNotFoundException(csProjFile.ProjectFilePath);
        }

        this.Project = SolutionBuilder.GetProjectSignature(csProjFile.ProjectFilePath);
    }
}