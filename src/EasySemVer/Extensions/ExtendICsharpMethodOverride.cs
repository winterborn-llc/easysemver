using System.Text;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Extensions;

internal static class ExtendICsharpMethodOverride
{
    /// <summary>
    /// SIG-09 - the canonical rendering of one overload: the comma-joined parameter list with
    /// required parameters bracketed. Note it includes requiredness while R02's matcher
    /// deliberately ignores it (CLS-06).
    /// </summary>
    internal static string GetMethodSignature(this ICsharpMethodOverride method)
    {
        var signatureSoFar = new StringBuilder();
        foreach (var input in method.Parameters)
        {
            if (signatureSoFar.Length > 0)
            {
                signatureSoFar.Append(", ");
            }

            var prefix = string.Empty;
            var suffix = string.Empty;
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
