namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// What every public Swift declaration carries, whatever kind it is. Availability and ObjC
/// exposure live here because both are properties of a declaration rather than of a type
/// (SWM-03, SWM-04).
/// </summary>
public interface ISwiftDeclaration
{
    /// <summary>
    /// The declaration's identity within its module: its path components joined with dots, and
    /// for functions the full Swift name including argument labels (SWE-03). Never the mangled
    /// USR - mangling changes between toolchain versions and would churn the baseline.
    /// </summary>
    public string Name { get; }

    /// <summary>"public" or "open". Nothing less visible enters the signature (SWE-02).</summary>
    public string AccessLevel { get; }

    public IReadOnlyList<ISwiftAvailability> Availability { get; }

    /// <summary>Empty, "@objc", or "@objc(CustomName)". Losing it breaks ObjC and KVO clients.</summary>
    public string ObjCExposure { get; }
}
