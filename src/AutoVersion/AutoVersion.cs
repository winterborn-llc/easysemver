using Microsoft.Build.Framework;
using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.Signatures;

namespace Yamamari.Library.AutoVersion;

// ReSharper disable once ClassNeverInstantiated.Global
public class AutoVersion : Microsoft.Build.Utilities.Task 
{
    [Required]
    public string ProjectFile { get; set; }
    
    public string AutoVersionFile => $"{this.ProjectFile}.autoversion";
    
    public AutoVersion()
    {
        this.ProjectFile = string.Empty;
    }
    
    public override bool Execute()
    {
        this.LogInfo($"Auto versioning {this.ProjectFile}");
        var newSignature = this.GetNewSignature();
        var oldSignature = this.GetOldSignature();
        var changeType = SignatureComparer.GetChangeType(oldSignature, newSignature);
        
        this.LogWarn($"Change Type: {changeType.ToString()}");
        IncrementFileVersion.HandleFile(this.ProjectFile, changeType);
        File.WriteAllText(this.AutoVersionFile, newSignature.Serialize());
        return true;
    }

    private Signature GetNewSignature()
    {
        var xml = File.ReadAllText(this.ProjectFile);
        var newSignature = SignatureBuilder.GetSignatureFor(this, this.ProjectFile, xml);
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