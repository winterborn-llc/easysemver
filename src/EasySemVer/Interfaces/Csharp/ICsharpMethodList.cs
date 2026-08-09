namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

/// <summary>A class's methods, addressable by name (SIG "named collections").</summary>
public interface ICsharpMethodList : IEnumerable<ICsharpMethod>
{
    public bool Contains(string name);

    public ICsharpMethod this[string name] { get; }

    public string[] Keys { get; }
}
