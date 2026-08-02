namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IProjectClassHistory
{
    public IProjectClass Older { get; }
    public IProjectClass Newer { get; }
}