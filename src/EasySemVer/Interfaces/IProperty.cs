namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IProperty
{
    public string Name { get; init; }
    public string Type { get; init; }
    public bool IsReadable { get; init; }
    public bool IsWritable { get; init; }
}