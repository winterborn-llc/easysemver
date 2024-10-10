using Microsoft.Build.Framework;

namespace Yamamari.AutoVersion;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoVersion : Microsoft.Build.Utilities.Task 
{
    [Required]
    public string ProjectFile { get; set; }
    
    public AutoVersion()
    {
        this.ProjectFile = string.Empty;
    }
    
    public override bool Execute()
    {
        Console.WriteLine($"Testing {DateTime.Now}");
        var xml = File.ReadAllText(this.ProjectFile);
        var signature = SignatureBuilder.GetSignatureFor(this, this.ProjectFile, xml);
        IncrementFileVersion.HandleFile(this.ProjectFile);
        return true;
    }
}