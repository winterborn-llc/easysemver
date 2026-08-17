namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>One declaration, as found in the file and as understood.</summary>
[DebuggerDisplay("{Header}")]
internal class SwiftSourceDeclaration
{
    internal required SwiftDeclarationBlock Block { get; init; }

    internal required SwiftDeclarationHeader Header { get; init; }
}
