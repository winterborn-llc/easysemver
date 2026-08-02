namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>A class's properties, addressable by name (SIG "named collections").</summary>
public interface ICsharpPropertyList : IEnumerable<ICsharpProperty>
{
    public bool Contains(string name);

    public ICsharpProperty this[string name] { get; }

    public string[] Keys { get; }
}
