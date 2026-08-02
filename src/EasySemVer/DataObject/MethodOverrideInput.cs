using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{DebugText}")]
internal class MethodOverrideInput : IMethodOverrideInput
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