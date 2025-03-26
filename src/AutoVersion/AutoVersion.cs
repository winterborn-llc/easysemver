using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoVersion : Microsoft.Build.Utilities.Task 
{
    internal string AutoVersionFile => $"{this.SolutionDirectory}/.autoversion.json";
    
    internal CsProjFile[] CsProjFiles { get; set; }
    
    internal string InitialDirectory { get; }
    
    internal string SolutionDirectory { get; }
    
    internal Version StartingVersion { get; }

    // This is used by the real execution of the process
    // ReSharper disable once UnusedMember.Global
    public AutoVersion() : this("")
    {
    }
    
    public AutoVersion(string currentDirectory)
    {
        if (currentDirectory.IsNullOrWhitespace())
        {
            currentDirectory = Environment.CurrentDirectory;
        }
        
        var solutionDir = GetSolutionDirectory(currentDirectory);
        this.SolutionDirectory = solutionDir.FullName;
        this.InitialDirectory = currentDirectory;
        this.CsProjFiles = GetProjectFiles(this.SolutionDirectory);
        this.StartingVersion = GetStartingVersion(this.CsProjFiles);
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

    private static CsProjFile[] GetProjectFiles(string startingDirectory)
    {
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
    
    public override bool Execute()
    {
        try
        {
            this.LogInfo($"Auto versioning {this.SolutionDirectory}");
            var newSignature = this.GetNewSignature();
            var oldSignature = this.GetOldSignature();
            var changeType = CompareSignatures.GetChangeType(this, oldSignature, newSignature);
            this.LogWarn($"Change Type: {changeType.ToString()}");
        
            var version = new Version(this.StartingVersion);
            version.Increment(changeType);
        
            foreach (var csProjFile in this.CsProjFiles)
            {
                csProjFile.Version = new Version(version);
                csProjFile.Save();
            }
        
            File.WriteAllText(this.AutoVersionFile, newSignature.Serialize());
            return true;
        }
        catch (Exception e)
        {
            this.LogFail($"Failed to execute autoversion:\n{e.Message}");
            return false;
        }
    }

    private Signature GetNewSignature()
    {
        var newSignature = SignatureBuilder.GetSignatureFor(this.CsProjFiles);
        return newSignature ?? [];
    }
    
    private Signature GetOldSignature()
    {
        if (!File.Exists(this.AutoVersionFile))
        {
            return new Signature();
        }
        
        try
        {
            var json = File.ReadAllText(this.AutoVersionFile);
            var oldSignature = json.Deserialize<Signature>();
            return oldSignature;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to deserialize old signature: {e.Message}");
        }
        
        return new Signature();
    }
}