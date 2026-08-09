namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

/// <summary>One <c>@available</c> clause on a declaration (SWM-03). Drives S25 and S26.</summary>
public interface ISwiftAvailability
{
    /// <summary>The platform, or "*" for all of them.</summary>
    public string Domain { get; }

    public string Introduced { get; }

    public string Deprecated { get; }

    public string Obsoleted { get; }

    public bool IsDeprecated { get; }

    public bool IsUnavailable { get; }

    public string RenamedTo { get; }
}
