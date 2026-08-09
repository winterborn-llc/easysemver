namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodParameter
{
    public string ParameterName { get; }

    public string ParameterType { get; }

    /// <summary>False iff the parameter is nullable-annotated or declares a default (SIG-08).</summary>
    public bool IsRequired { get; }

    /// <summary>"None" | "Ref" | "Out" | "In" | "RefReadOnlyParameter". Changing it is breaking (R37).</summary>
    public string RefKind { get; }

    public bool IsParams { get; }
}
