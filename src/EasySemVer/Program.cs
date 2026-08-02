using Winterborn.Library.EasySemVer.CodeReader;
using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Settings;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer;

// ReSharper disable once ClassNeverInstantiated.Global
public static class Program
{
    public static void Main(params string[] args)
    {
        try
        {
            Execute(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(1);
        }
    }
    
    private static void Execute(string[] args)
    {
        var solutionDirectory = GetSolutionDirectory(args);
        var oldSignature = GetOlderSignature(solutionDirectory);
        var newSignature = GetNewerSignature(solutionDirectory);
        var csProjFiles = CsProjFile.GetSolutionProjectFiles(solutionDirectory);
        var signatures = new SignaturesToCompare(solutionDirectory, oldSignature, newSignature);
        var startingVersion = Version.GetVersionFromProjectFiles(csProjFiles);
        var changeType = CompareSignatures.GetChangeType(signatures);
        var newVersion = new Version(startingVersion);
        newVersion.Increment(changeType);
        signatures.Save(newVersion);
    }
    
    private static Solution GetOlderSignature(string solutionDirectory)
    {
        var signaturePath = Path.Combine(solutionDirectory, MagicValues.SignatureFileName);
        if (!File.Exists(signaturePath))
        {
            return new Solution();
        }

        try
        {
            var xml = File.ReadAllText(signaturePath);
            var solution = xml.Deserialize<Solution>();
            return solution;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to deserialize existing signature file {signaturePath}:\n{e}");
            return new Solution();
        }
    }

    private static ISolution GetNewerSignature(string solutionDirectory)
    {
        var files = CsProjFile.GetSolutionProjectFilePaths(solutionDirectory);
        var newSignature = SolutionBuilder.GetSolutionSignatureFromAnalyzer(files);
        return newSignature;
    }
    
    private static string GetSolutionDirectory(string[] args)
    {
        var path = GetDirectoryToUse(args);
        var solutionDirectory = new DirectoryInfo(path);
        while (solutionDirectory != null)
        {
            var slnFiles = Directory.GetFiles(solutionDirectory.FullName, "*.sln", SearchOption.TopDirectoryOnly);
            if (slnFiles.Length > 0)
            {
                Log.WriteLine($"Auto Versioning: {path}");
                return solutionDirectory.FullName;
            }
            
            solutionDirectory = solutionDirectory.Parent;
        }
        
        throw new InvalidOperationException($"Could not find solution directory at {path} - or any of its parents");
    }
    
    private static string GetDirectoryToUse(string[] args)
    {
        if (args.Length > 1)
        {
            throw new InvalidOperationException("EasySemVer requires a single parameter that specifies the directory in which to execute");
        }

        if (args.Length < 1)
        {
            return Environment.CurrentDirectory;
        }
        
        var path = Environment.CurrentDirectory;
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException($"Directory {path} does not exist");
        }

        return path;
    }
}