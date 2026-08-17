namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// One Swift file, walked once and then read from as often as needed. Bodies are read on demand
/// rather than up front, because the only bodies that are ever opened are those of types and
/// extensions - a function's is never API.
/// </summary>
internal class SwiftSourceFile
{
    private readonly SwiftSourceWalker _walker;

    internal SwiftSourceFile(string sourceText)
    {
        this._walker = new SwiftSourceWalker(sourceText);
        this.TopLevel = Parse(this._walker.ReadFile());
    }

    internal IReadOnlyList<SwiftSourceDeclaration> TopLevel { get; }

    internal IReadOnlyList<SwiftSourceDeclaration> ReadBody(SwiftDeclarationBlock block)
    {
        return block.HasBody
            ? Parse(this._walker.Read(block.BodyStart, block.BodyEnd))
            : [];
    }

    /// <summary>
    /// A declaration's body as text, for the two questions that are answered by looking at one:
    /// which accessors a property has, and whether they are async or throwing. It comes from the
    /// blanked copy, since nothing in a body is read for its content.
    /// </summary>
    internal string ReadBodyText(SwiftDeclarationBlock block)
    {
        return block.HasBody
            ? this._walker.Blanked[block.BodyStart..block.BodyEnd]
            : string.Empty;
    }

    private static List<SwiftSourceDeclaration> Parse(IReadOnlyList<SwiftDeclarationBlock> blocks)
    {
        var declarations = new List<SwiftSourceDeclaration>();
        foreach (var block in blocks)
        {
            declarations.Add(new SwiftSourceDeclaration
            {
                Block = block,
                Header = SwiftDeclarationHeader.Parse(block.Header)
            });
        }

        return declarations;
    }
}
