namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>The C# native topology's root: one .csproj worth of public API surface.</summary>
public interface ICsharpProject
{
    public string Name { get; }

    public IReadOnlyList<ICsharpClass> Classes { get; }
}
