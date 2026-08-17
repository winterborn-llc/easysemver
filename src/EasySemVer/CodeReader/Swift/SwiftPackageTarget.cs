namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>One target as a Package.swift declares it (SWD-01).</summary>
[DebuggerDisplay("{Name}")]
internal class SwiftPackageTarget
{
    internal required string Name { get; init; }

    /// <summary>UNI-04 - what ".testTarget" produces. A unit like any other, with no vote on the API.</summary>
    internal required bool IsTest { get; init; }

    /// <summary>The "path:" argument, if one was written. Package-relative.</summary>
    internal string Path { get; init; } = string.Empty;

    /// <summary>The "sources:" argument: an explicit list of files or directories, target-relative.</summary>
    internal IReadOnlyList<string> Sources { get; init; } = [];

    /// <summary>The "exclude:" argument, target-relative.</summary>
    internal IReadOnlyList<string> Excluded { get; init; } = [];
}
