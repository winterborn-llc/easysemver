namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>The C# native topology's root: one .csproj worth of public API surface.</summary>
public interface ICsharpProject
{
    public string Name { get; }

    public IReadOnlyList<ICsharpClass> Classes { get; }

    public IReadOnlyList<ICsharpInterface> Interfaces { get; }

    public IReadOnlyList<ICsharpStruct> Structs { get; }

    public IReadOnlyList<ICsharpRecord> Records { get; }

    public IReadOnlyList<ICsharpEnum> Enums { get; }

    public IReadOnlyList<ICsharpDelegate> Delegates { get; }

    /// <summary>Every type of every kind, for the rules that do not care which kind it is.</summary>
    public IReadOnlyList<ICsharpType> Types { get; }
}
