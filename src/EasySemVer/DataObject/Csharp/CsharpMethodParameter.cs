using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{DebugText}")]
internal class CsharpMethodParameter : ICsharpMethodParameter
{
    private string DebugText
    {
        get
        {
            var prefix = this.IsRequired ? "[" : "";
            var suffix = this.IsRequired ? "]" : "";
            return $"{prefix}{this.ParameterType} {this.ParameterName}{suffix}";
        }
    }
    
    public string ParameterName { get; init; } = string.Empty;

    public string ParameterType { get; init; } = string.Empty;

    public bool IsRequired { get; init; } = true;
}