namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpProperty
{
    public string Name { get; }

    public string Type { get; }

    public bool IsReadable { get; }

    public bool IsWritable { get; }
}
