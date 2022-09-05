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
        IncrementFileVersion.HandleFile(this.ProjectFile);
        return true;
    }
}