namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpProperty
{
    public string Name { get; init; }
    public string Type { get; init; }
    public bool IsReadable { get; init; }
    public bool IsWritable { get; init; }
}