namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpField
{
    public string Name { get; }

    public string Type { get; }

    public bool IsStatic { get; }

    /// <summary>Gaining this is breaking for any caller that assigned the field (R28).</summary>
    public bool IsReadOnly { get; }

    public bool IsConstant { get; }
}
