namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodOverrides : IEnumerable<ICsharpMethodOverride>
{
    /// <summary>Whether any overload renders to this canonical signature string (SIG-09).</summary>
    public bool Contains(string methodSignature);
}
