namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IProject
{
    public string Name { get; init; }
    public List<IProjectClass> Classes { get; set; }
}