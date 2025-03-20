using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.Signatures;

namespace Yamamari.Library.AutoVersion;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoVersion : Microsoft.Build.Utilities.Task 
{
    internal string AutoVersionFile => $"{this.SolutionDirectory}/.autoversion.json";
    
    internal CsProjFile[] CsProjFiles { get; set; }
    
    internal string InitialDirectory { get; }
    
    internal string SolutionDirectory { get; }
    
    internal Version StartingVersion { get; }

    public AutoVersion() : this("")
    {
    }
    
    public AutoVersion(string currentDirectory)
    {
        if (currentDirectory.IsNullOrWhitespace())
        {
            currentDirectory = Environment.CurrentDirectory;
        }
        
        this.InitialDirectory = currentDirectory;
        var solutionDir = GetSolutionDirectory(this.InitialDirectory);
        if (solutionDir == null)
        {
            throw new InvalidOperationException($"Could not find solution directory.");
        }
        
        this.SolutionDirectory = solutionDir.FullName;
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
    
    private static DirectoryInfo? GetSolutionDirectory(string startingDirectory)
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

        return null;
    }
    
    public override bool Execute()
    {
        this.LogInfo($"Auto versioning {this.SolutionDirectory}");
        var newSignature = this.GetNewSignature();
        var oldSignature = this.GetOldSignature();
        var changeType = SignatureComparer.GetChangeType(oldSignature, newSignature);
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

    private Signature GetNewSignature()
    {
        var newSignature = SignatureBuilder.GetSignatureFor(this, this.CsProjFiles);
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