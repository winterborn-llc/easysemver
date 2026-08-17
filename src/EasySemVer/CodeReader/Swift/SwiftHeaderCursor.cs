using System.Text;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// A cursor over one declaration header, handing out the pieces in the order Swift writes them:
/// attributes, then modifiers, then the keyword, then whatever that keyword takes. Each Read
/// advances only if what it wants is actually there, so a declaration that omits a piece costs
/// nothing to skip.
/// </summary>
internal class SwiftHeaderCursor(string header)
{
    /// <summary>
    /// Words that can precede the keyword. "class" is here as well as being a keyword: in
    /// "class func make()" it is the modifier that means static, and only what follows it says
    /// which of the two it is.
    /// </summary>
    private static readonly string[] ModifierWords =
    [
        "open", "public", "package", "internal", "fileprivate", "private",
        "final", "static", "class", "lazy", "weak", "unowned", "dynamic",
        "mutating", "nonmutating", "borrowing", "consuming",
        "required", "convenience", "override", "indirect", "optional",
        "prefix", "postfix", "infix", "distributed", "isolated", "nonisolated",
        "async", "throws", "rethrows", "reasync"
    ];

    /// <summary>What has to follow "class" for it to have been a modifier rather than a keyword.</summary>
    private static readonly string[] ClassModifierFollowers =
        ["func", "var", "let", "subscript", "init"];

    private const string OperatorCharacters = "+-*/%<>=!&|^~?.";

    private int _index;

    /// <summary>Whether the last <see cref="ReadBracketed"/> found its bracket.</summary>
    internal bool ConsumedBrackets { get; private set; }

    internal IReadOnlyList<string> ReadAttributes()
    {
        var attributes = new List<string>();
        while (true)
        {
            this.SkipWhitespace();
            if (this._index >= header.Length || header[this._index] != '@')
            {
                return attributes;
            }

            var start = this._index;
            this._index++;
            this.SkipWord();

            // "@available(...)" carries its argument list; "@objc" need not.
            if (this._index < header.Length && header[this._index] == '(')
            {
                var closing = SwiftText.FindMatching(header, this._index);
                this._index = closing < 0 ? header.Length : closing + 1;
            }

            attributes.Add(Collapse(header[start..this._index]));
        }
    }

    internal IReadOnlyList<string> ReadModifiers()
    {
        var modifiers = new List<string>();
        while (true)
        {
            this.SkipWhitespace();
            var start = this._index;
            var word = this.PeekWord();
            if (word.Length < 1 || !ModifierWords.Contains(word))
            {
                return modifiers;
            }

            if (word == "class" && !this.IsClassModifier())
            {
                this._index = start;
                return modifiers;
            }

            this._index += word.Length;

            // "private(set)" and "unowned(safe)" are one modifier, parenthesis and all.
            if (this._index < header.Length && header[this._index] == '(')
            {
                var closing = SwiftText.FindMatching(header, this._index);
                this._index = closing < 0 ? header.Length : closing + 1;
            }

            modifiers.Add(header[start..this._index].Trim());
        }
    }

    internal string ReadKeyword()
    {
        this.SkipWhitespace();
        var word = this.PeekWord();
        this._index += word.Length;
        return word;
    }

    internal string ReadIdentifier()
    {
        this.SkipWhitespace();
        if (this._index < header.Length && header[this._index] == '`')
        {
            var closing = header.IndexOf('`', this._index + 1);
            if (closing > 0)
            {
                var quoted = header[(this._index + 1)..closing];
                this._index = closing + 1;
                return quoted;
            }
        }

        var word = this.PeekWord();
        this._index += word.Length;
        return word;
    }

    /// <summary>A function's name, which is an identifier for most of them and an operator for the rest.</summary>
    internal string ReadDeclarationName()
    {
        this.SkipWhitespace();
        if (this._index >= header.Length)
        {
            return string.Empty;
        }

        if (SwiftText.IsWordCharacter(header[this._index]) || header[this._index] == '`')
        {
            return this.ReadIdentifier();
        }

        var start = this._index;
        while (this._index < header.Length && OperatorCharacters.Contains(header[this._index]))
        {
            this._index++;
        }

        return header[start..this._index];
    }

    /// <summary>
    /// A type reference as written, including any qualification and generic arguments, stopping at
    /// the colon or the "where" that would begin the next clause.
    /// </summary>
    internal string ReadTypeReference()
    {
        this.SkipWhitespace();
        var start = this._index;
        var depth = 0;
        while (this._index < header.Length)
        {
            var character = header[this._index];
            if (character is '<' or '(' or '[')
            {
                depth++;
            }
            else if (character is '>' or ')' or ']')
            {
                depth--;
            }
            else if (depth < 1 && (character == ':' || char.IsWhiteSpace(character)))
            {
                break;
            }

            this._index++;
        }

        return Collapse(header[start..this._index]);
    }

    /// <summary>The text inside a bracket pair, if the next thing written is one.</summary>
    internal string ReadBracketed(char open)
    {
        this.SkipWhitespace();
        this.ConsumedBrackets = false;
        if (this._index >= header.Length || header[this._index] != open)
        {
            return string.Empty;
        }

        var closing = SwiftText.FindMatching(header, this._index);
        if (closing < 0)
        {
            return string.Empty;
        }

        var inner = header[(this._index + 1)..closing];
        this._index = closing + 1;
        this.ConsumedBrackets = true;
        return inner.Trim();
    }

    internal bool ConsumeFailableMarker()
    {
        if (this._index >= header.Length || header[this._index] is not ('?' or '!'))
        {
            return false;
        }

        this._index++;
        return true;
    }

    /// <summary>Everything after a ":", up to the "=" or "where" that would start the next clause.</summary>
    internal string ReadTypeAnnotation()
    {
        this.SkipWhitespace();
        if (this._index >= header.Length || header[this._index] != ':')
        {
            return string.Empty;
        }

        this._index++;
        var rest = header[this._index..];
        var end = rest.Length;

        var whereClause = SwiftText.IndexOfTopLevelWord(rest, "where");
        if (whereClause >= 0)
        {
            end = whereClause;
        }

        var assignment = IndexOfAssignment(rest);
        if (assignment >= 0 && assignment < end)
        {
            end = assignment;
        }

        this._index += end;
        return Collapse(rest[..end]);
    }

    /// <summary>Everything after an "=": a default value, a raw value, a typealias's right-hand side.</summary>
    internal string ReadInitialiser()
    {
        this.SkipWhitespace();
        var assignment = this._index < header.Length ? IndexOfAssignment(header[this._index..]) : -1;
        if (assignment != 0)
        {
            return string.Empty;
        }

        var value = header[(this._index + 1)..];
        this._index = header.Length;
        return Collapse(value);
    }

    internal string ReadWhereClause()
    {
        this.SkipWhitespace();
        var rest = header[this._index..];
        var whereClause = SwiftText.IndexOfTopLevelWord(rest, "where");
        if (whereClause < 0)
        {
            return string.Empty;
        }

        this._index = header.Length;
        return Collapse(rest[(whereClause + "where".Length)..]);
    }

    /// <summary>
    /// The effects and return type that follow a parameter list, and any "where" clause after
    /// them. All three are optional and can only appear in this order.
    /// </summary>
    internal (bool IsAsync, bool Throws, string ReturnType, string WhereClause) ReadEffectsAndReturnType()
    {
        var rest = header[Math.Min(this._index, header.Length)..];
        this._index = header.Length;

        var whereClause = string.Empty;
        var whereAt = SwiftText.IndexOfTopLevelWord(rest, "where");
        if (whereAt >= 0)
        {
            whereClause = Collapse(rest[(whereAt + "where".Length)..]);
            rest = rest[..whereAt];
        }

        var returnType = string.Empty;
        var arrow = IndexOfReturnArrow(rest);
        if (arrow >= 0)
        {
            returnType = Collapse(rest[(arrow + 2)..]);
            rest = rest[..arrow];
        }

        return (
            SwiftText.ContainsTopLevelWord(rest, "async"),
            SwiftText.ContainsTopLevelWord(rest, "throws")
            || SwiftText.ContainsTopLevelWord(rest, "rethrows"),
            returnType,
            whereClause);
    }

    /// <summary>
    /// The first "-&gt;" that is not inside brackets. A return type can itself be a function type,
    /// so the first one is the one that separates the signature from what it returns.
    /// </summary>
    private static int IndexOfReturnArrow(string text)
    {
        var depth = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '-' && index + 1 < text.Length && text[index + 1] == '>')
            {
                if (depth < 1)
                {
                    return index;
                }

                index++;
                continue;
            }

            switch (text[index])
            {
                case '(' or '[' or '<':
                    depth++;
                    break;

                case ')' or ']' or '>':
                    depth--;
                    break;
            }
        }

        return -1;
    }

    /// <summary>An "=" that assigns, rather than one that is part of "==", "&gt;=" or "!=".</summary>
    private static int IndexOfAssignment(string text)
    {
        var depth = 0;
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            switch (character)
            {
                case '(' or '[' or '{':
                    depth++;
                    continue;

                case ')' or ']' or '}':
                    depth--;
                    continue;
            }

            if (character != '=' || depth >= 1)
            {
                continue;
            }

            var previous = index > 0 ? text[index - 1] : ' ';
            var next = index + 1 < text.Length ? text[index + 1] : ' ';
            if (OperatorCharacters.Contains(previous) || next == '=')
            {
                continue;
            }

            return index;
        }

        return -1;
    }

    private bool IsClassModifier()
    {
        var after = this._index + "class".Length;
        while (after < header.Length && char.IsWhiteSpace(header[after]))
        {
            after++;
        }

        foreach (var follower in ClassModifierFollowers)
        {
            if (SwiftText.IsWordAt(header, after, follower))
            {
                return true;
            }
        }

        return false;
    }

    private string PeekWord()
    {
        var end = this._index;
        while (end < header.Length && SwiftText.IsWordCharacter(header[end]))
        {
            end++;
        }

        return header[this._index..end];
    }

    private void SkipWord()
    {
        while (this._index < header.Length && SwiftText.IsWordCharacter(header[this._index]))
        {
            this._index++;
        }
    }

    private void SkipWhitespace()
    {
        while (this._index < header.Length && char.IsWhiteSpace(header[this._index]))
        {
            this._index++;
        }
    }

    /// <summary>
    /// One space between words, none around brackets and colons. A declaration broken across lines
    /// and the same declaration on one line are the same declaration, and a baseline that
    /// disagreed would turn reformatting into an API change.
    /// </summary>
    internal static string Collapse(string text)
    {
        const string noSpaceBefore = ",;:)]>";
        const string noSpaceAfter = "([<";

        var collapsed = new StringBuilder();
        var wasWhitespace = false;
        foreach (var character in text.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                wasWhitespace = true;
                continue;
            }

            if (wasWhitespace
                && collapsed.Length > 0
                && !noSpaceBefore.Contains(character)
                && !noSpaceAfter.Contains(collapsed[^1]))
            {
                collapsed.Append(' ');
            }

            wasWhitespace = false;
            collapsed.Append(character);
        }

        return collapsed.ToString();
    }
}
