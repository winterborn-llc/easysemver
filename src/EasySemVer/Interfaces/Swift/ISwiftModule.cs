namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// The Swift native topology's root: one target's public API surface (D-05). This is Swift's
/// object model as a Swift developer reads it, deliberately not a translation of C#'s (D-04).
/// </summary>
public interface ISwiftModule
{
    public string Name { get; }

    public IReadOnlyList<ISwiftType> Types { get; }

    public IReadOnlyList<ISwiftExtension> Extensions { get; }

    public IReadOnlyList<ISwiftFunction> GlobalFunctions { get; }

    public IReadOnlyList<ISwiftProperty> GlobalVariables { get; }

    public IReadOnlyList<ISwiftTypeAlias> TypeAliases { get; }

    public IReadOnlyList<ISwiftOperator> Operators { get; }
}
