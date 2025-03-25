using System.Text;

namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class MethodOverride : List<MethodOverrideInput>
{
    private string _methodSignature = "-";
    
    public string MethodSignature
    {
        get
        {
            this.SetMethodSignature();
            return this._methodSignature;
        }
    }
    
    private void SetMethodSignature()
    {
        if (this._methodSignature != "-")
        {
            return;
        }

        var prefix = "";
        var suffix = "";
        var signatureSoFar = new StringBuilder();
        foreach (var input in this)
        {
            if (signatureSoFar.Length > 0)
            {
                signatureSoFar.Append(", ");
            }

            if (input.IsRequired)
            {
                prefix = "[";
                suffix = "]";
            }
            
            signatureSoFar.Append($"{prefix}{input.ParameterType} {input.ParameterName}{suffix}");
        }
        
        this._methodSignature = signatureSoFar.ToString();
    }
}