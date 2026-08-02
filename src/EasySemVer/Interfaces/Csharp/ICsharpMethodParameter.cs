namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodParameter
{
    public string ParameterName { get; }

    public string ParameterType { get; }

    /// <summary>False iff the parameter is nullable-annotated or declares a default (SIG-08).</summary>
    public bool IsRequired { get; }
}
