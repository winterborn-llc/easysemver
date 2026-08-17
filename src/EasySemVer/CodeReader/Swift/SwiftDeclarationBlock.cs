namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// One declaration as it appears in a source file: everything up to its opening brace, plus the
/// span of the body if it has one. Bodies are spans rather than text so that a nested declaration
/// can be sliced from the original file, which is where default values and raw values are read
/// from - the blanked copy has had those removed along with every other literal.
/// </summary>
[DebuggerDisplay("{Header}")]
internal class SwiftDeclarationBlock
{
    internal required string Header { get; init; }

    /// <summary>-1 when the declaration has no braces at all: a stored property, a requirement.</summary>
    internal required int BodyStart { get; init; }

    internal required int BodyEnd { get; init; }

    internal bool HasBody => this.BodyStart >= 0;
}
