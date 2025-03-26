using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoVersion : Microsoft.Build.Utilities.Task
{
    public override bool Execute()
    {
        try
        {
            this.Execute(Environment.CurrentDirectory);
            return true;
        }
        catch (Exception e)
        {
            this.LogFail(e.Message);
            return false;
        }
    }
    
    private void Execute(string currentDirectory)
    {
        if (currentDirectory.IsNullOrWhitespace())
        {
            currentDirectory = Environment.CurrentDirectory;
        }
        
        var solutionDir = GetSolutionDirectory(currentDirectory);
        var solutionDirectory = solutionDir.FullName;
        var csProjFiles = GetProjectFiles(solutionDirectory);
        var startingVersion = GetStartingVersion(csProjFiles);
        
        this.LogInfo($"Auto versioning {solutionDirectory}");
        var newSignature = GetNewSignature(csProjFiles);
        var oldSignature = GetOldSignature(csProjFiles);
        var changeType = CompareSignatures.GetChangeType(this, oldSignature, newSignature);
        this.LogWarn($"Change Type: {changeType.ToString()}");

        var version = new Version(startingVersion);
        version.Increment(changeType);

        foreach (var csProjFile in csProjFiles)
        {
            csProjFile.Version = new Version(version);
            csProjFile.Save();
        }
    }

    private static Version GetStartingVersion(params CsProjFile[] csProjFiles)
    {
        var startingVersion = new Version("0.0.0");
        foreach (var csProjFile in csProjFiles)
        {
            if (csProjFile.Version < startingVersion)
            {
                continue;
            }
            
            startingVersion = csProjFile.Version;
        }

        return startingVersion;
    }

    internal static CsProjFile[] GetProjectFiles(string startingDirectory)
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
    
    private static DirectoryInfo GetSolutionDirectory(string startingDirectory)
    {
        var dir = new DirectoryInfo(startingDirectory);
        while (dir != null)
        {
            if (dir.GetFiles().Any(f => f.Name.EndsWith(".sln") || f.Name.EndsWith(".slnx")))
            {
                return dir;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not find solution directory.");
    }

    private static Signature GetNewSignature(params CsProjFile[] csProjFiles)
    {
        var newSignature = SignatureBuilder.GetSignatureFor(csProjFiles);
        return newSignature ?? [];
    }
    
    private static Signature GetOldSignature(params CsProjFile[] csProjFiles)
    {
        var signature = new Signature();
        foreach (var csProjFile in csProjFiles)
        {
            signature.Add(csProjFile.ProjectLatest);
        }
        
        return signature;
    }
}