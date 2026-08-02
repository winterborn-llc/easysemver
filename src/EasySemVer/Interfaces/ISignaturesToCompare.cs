using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Interfaces;

public interface ISignaturesToCompare
{
    public ISolution Older { get; }
    public ISolution Newer { get; }
    public IProjectClassHistory[] ClassHistory { get; }
    public void Save(Version version);
}