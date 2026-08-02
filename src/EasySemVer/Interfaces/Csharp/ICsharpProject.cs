namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpProject
{
    public string Name { get; init; }
    public List<ICsharpClass> Classes { get; set; }
}