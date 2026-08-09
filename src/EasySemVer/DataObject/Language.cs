namespace Winterborn.Tools.EasySemVer.DataObject;

/// <summary>
/// The languages EasySemVer knows how to version. One <see cref="Language"/> maps to exactly one
/// <see cref="Interfaces.ILanguageProvider"/>; adding a language means adding a member here, a
/// provider, and a registration line - nothing else in the neutral core moves (ML-02).
/// </summary>
public enum Language
{
    Csharp,
    Swift
}
