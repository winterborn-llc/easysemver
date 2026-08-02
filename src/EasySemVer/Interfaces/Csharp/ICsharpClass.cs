namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpClass
{
    /// <summary>Namespace-qualified name, "global::" stripped (SIG-04).</summary>
    public string Name { get; }

    public ICsharpMethodList Methods { get; }

    public ICsharpPropertyList Properties { get; }
}
