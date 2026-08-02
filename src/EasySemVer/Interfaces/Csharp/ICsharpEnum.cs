namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpEnum : ICsharpType
{
    /// <summary>e.g. "int", "byte". Changing it is breaking for anything that casts (R25).</summary>
    public string UnderlyingType { get; }

    public IReadOnlyList<ICsharpEnumMember> Members { get; }
}
