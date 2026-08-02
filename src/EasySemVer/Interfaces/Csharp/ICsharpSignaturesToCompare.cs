using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpSignaturesToCompare
{
    public ISolution Older { get; }
    public ISolution Newer { get; }
    public ICsharpClassHistory[] ClassHistory { get; }
    public void Save(Version version);
}