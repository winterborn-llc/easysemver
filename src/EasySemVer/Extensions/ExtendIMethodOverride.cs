using System.Text;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendIMethodOverride
{
    public static string GetMethodSignature(this IMethodOverride method)
    {
        var signatureSoFar = new StringBuilder();
        foreach (var input in method)
        {
            if (signatureSoFar.Length > 0)
            {
                signatureSoFar.Append(", ");
            }

            var prefix = "";
            var suffix = "";
            if (input.IsRequired)
            {
                prefix = "[";
                suffix = "]";
            }
            
            signatureSoFar.Append($"{prefix}{input.ParameterType} {input.ParameterName}{suffix}");
        }
        
        return signatureSoFar.ToString();
    }
}