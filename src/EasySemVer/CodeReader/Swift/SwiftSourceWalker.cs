namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Splits a Swift file into declarations without parsing Swift (SWE-07). It never looks inside a
/// function body - nothing declared in one is API - so the only structure it has to get right is
/// where each declaration's header ends, which is at its opening brace, its semicolon, or the end
/// of its line.
/// </summary>
internal class SwiftSourceWalker(string source)
{
    private readonly string _blanked = SwiftSourceText.Blank(source);

    /// <summary>
    /// The keywords that make a run of text a declaration rather than the modifiers and attributes
    /// leading up to one. Until one of these has been seen, a newline continues the declaration -
    /// which is what lets an "@available(...)" sit on its own line above what it applies to.
    /// </summary>
    private static readonly string[] DeclarationKeywords =
    [
        "associatedtype",
        "case",
        "class",
        "deinit",
        "enum",
        "extension",
        "func",
        "import",
        "init",
        "let",
        "macro",
        "operator",
        "precedencegroup",
        "protocol",
        "struct",
        "subscript",
        "typealias",
        "var",
        "actor"
    ];

    /// <summary>
    /// A header ending in one of these is mid-thought, so the newline after it is not the end of
    /// the declaration: an inheritance list or a parameter list broken across lines is ordinary
    /// formatting, not two declarations.
    /// </summary>
    private const string ContinuationCharacters = ",:=&|+-*/?";

    internal string Blanked => this._blanked;

    internal IReadOnlyList<SwiftDeclarationBlock> ReadFile()
    {
        return this.Read(0, this._blanked.Length);
    }

    internal IReadOnlyList<SwiftDeclarationBlock> Read(int start, int end)
    {
        var blocks = new List<SwiftDeclarationBlock>();
        var index = start;
        while (index < end)
        {
            index = this.SkipToDeclaration(index, end);
            if (index >= end)
            {
                break;
            }

            index = this.ReadOne(index, end, blocks);
        }

        return blocks;
    }

    /// <summary>
    /// Whitespace, stray closing braces, and the conditional-compilation directives.
    /// <para>
    /// A "#if" line is skipped rather than evaluated, which means every branch of it is read.
    /// Reading one branch would need the build configuration, which is exactly the toolchain
    /// question this reader exists to avoid asking; reading all of them over-reports a platform's
    /// surface rather than under-reporting it, and the duplicate names that an "#if/#else" pair
    /// produces are collapsed when the declarations are added to the module.
    /// </para>
    /// </summary>
    private int SkipToDeclaration(int index, int end)
    {
        while (index < end)
        {
            var character = this._blanked[index];
            if (char.IsWhiteSpace(character) || character is '}' or ';' or ',')
            {
                index++;
                continue;
            }

            if (character == '#')
            {
                index = this.EndOfLine(index, end);
                continue;
            }

            return index;
        }

        return end;
    }

    private int ReadOne(int start, int end, List<SwiftDeclarationBlock> blocks)
    {
        var depth = 0;
        var hasKeyword = false;
        for (var index = start; index < end; index++)
        {
            var character = this._blanked[index];
            switch (character)
            {
                case '(' or '[':
                    depth++;
                    continue;

                case ')' or ']':
                    depth--;
                    continue;
            }

            if (depth >= 1)
            {
                continue;
            }

            if (!hasKeyword)
            {
                hasKeyword = this.HasDeclarationKeywordAt(index);
            }

            switch (character)
            {
                case '{':
                    return this.AddWithBody(start, index, end, blocks);

                case ';':
                    Add(blocks, this.Slice(start, index), bodyStart: -1, bodyEnd: -1);
                    return index + 1;

                case '\n' when hasKeyword && this.IsEndOfDeclaration(start, index, end):
                    Add(blocks, this.Slice(start, index), bodyStart: -1, bodyEnd: -1);
                    return index + 1;
            }
        }

        Add(blocks, this.Slice(start, end), bodyStart: -1, bodyEnd: -1);
        return end;
    }

    private int AddWithBody(int start, int braceIndex, int end, List<SwiftDeclarationBlock> blocks)
    {
        var closing = SwiftText.FindMatching(this._blanked, braceIndex);
        if (closing < 0 || closing > end)
        {
            closing = end - 1;
        }

        Add(blocks, this.Slice(start, braceIndex), braceIndex + 1, closing);
        return closing + 1;
    }

    /// <summary>
    /// A newline ends the declaration unless the text so far is unfinished, or unless what follows
    /// is the body after all - "func f() -&gt; Int" on one line and "{" on the next is one
    /// declaration, and so is a "where" clause hanging below its signature.
    /// </summary>
    private bool IsEndOfDeclaration(int start, int newlineIndex, int end)
    {
        var header = this._blanked[start..newlineIndex].TrimEnd();
        if (header.Length > 0 && ContinuationCharacters.Contains(header[^1]))
        {
            return false;
        }

        if (header.EndsWith("->", StringComparison.Ordinal))
        {
            return false;
        }

        var next = this.SkipWhitespace(newlineIndex, end);
        if (next >= end)
        {
            return true;
        }

        return this._blanked[next] != '{' && !SwiftText.IsWordAt(this._blanked, next, "where");
    }

    private bool HasDeclarationKeywordAt(int index)
    {
        foreach (var keyword in DeclarationKeywords)
        {
            if (SwiftText.IsWordAt(this._blanked, index, keyword))
            {
                return true;
            }
        }

        return false;
    }

    private int SkipWhitespace(int index, int end)
    {
        while (index < end && char.IsWhiteSpace(this._blanked[index]))
        {
            index++;
        }

        return index;
    }

    private int EndOfLine(int index, int end)
    {
        while (index < end && this._blanked[index] != '\n')
        {
            index++;
        }

        return index;
    }

    /// <summary>
    /// Headers come from the original file, not the blanked copy: a default value and an enum
    /// case's raw value are both string literals often enough that reading them from the blank
    /// would lose them.
    /// </summary>
    private string Slice(int start, int end)
    {
        return source[start..Math.Min(end, source.Length)].Trim();
    }

    private static void Add(
        List<SwiftDeclarationBlock> blocks,
        string header,
        int bodyStart,
        int bodyEnd)
    {
        if (header.Length < 1)
        {
            return;
        }

        blocks.Add(new SwiftDeclarationBlock
        {
            Header = header,
            BodyStart = bodyStart,
            BodyEnd = bodyEnd
        });
    }
}
