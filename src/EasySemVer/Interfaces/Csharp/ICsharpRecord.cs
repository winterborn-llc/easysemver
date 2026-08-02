namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpRecord : ICsharpType
{
    /// <summary>Changing the positional parameter list breaks deconstruction and construction (R27).</summary>
    public IReadOnlyList<ICsharpMethodParameter> PositionalParameters { get; }

    /// <summary>True for <c>record struct</c>.</summary>
    public bool IsValueType { get; }
}
