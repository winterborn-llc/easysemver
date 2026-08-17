namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>One target of an .xcodeproj (SWD-02), and the Swift files that build into it.</summary>
[DebuggerDisplay("{Name}")]
internal class XcodeTarget
{
    /// <summary>
    /// UNI-04 - unit tests and UI tests both. A UI test bundle's symbols are no more a contract
    /// than a unit test bundle's, and it is the same .xctest either way.
    /// </summary>
    private static readonly string[] TestProductTypes =
    [
        "com.apple.product-type.bundle.unit-test",
        "com.apple.product-type.bundle.ui-testing"
    ];

    internal required string Name { get; init; }

    internal required string ProductType { get; init; }

    internal required IReadOnlyList<string> SourceFiles { get; init; }

    internal bool IsTest => TestProductTypes.Contains(this.ProductType);
}
