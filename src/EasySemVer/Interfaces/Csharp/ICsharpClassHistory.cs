namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>
/// One type as the baseline recorded it, alongside the same type now (CLS-02). It pairs types of
/// any kind, not only classes: an interface losing a method and a class losing a method are the
/// same breaking change, so the member rules should not have to be written twice.
/// </summary>
public interface ICsharpClassHistory
{
    public ICsharpType Older { get; }

    public ICsharpType Newer { get; }
}
