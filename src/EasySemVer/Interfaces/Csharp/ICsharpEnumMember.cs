namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpEnumMember
{
    public string Name { get; }

    /// <summary>The constant's value as written by the compiler; a change to it is breaking (R24).</summary>
    public string Value { get; }
}
