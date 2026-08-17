namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Bracket-aware text helpers shared by everything that reads Swift declarations. They assume
/// their input has been through <see cref="SwiftSourceText"/>, so a bracket they see is a real
/// bracket and not one inside a string.
/// </summary>
internal static class SwiftText
{
    /// <summary>
    /// The index of the bracket closing the one at <paramref name="openIndex"/>, or -1 if the text
    /// runs out first. Angle brackets are matched too, because a generic argument list is the one
    /// place a "&gt;" is reliably a bracket - "-&gt;" is stepped over so a return arrow does not
    /// close a generic list that was never open.
    /// </summary>
    internal static int FindMatching(string text, int openIndex)
    {
        var open = text[openIndex];
        var close = GetClosingBracket(open);
        var depth = 0;
        for (var index = openIndex; index < text.Length; index++)
        {
            if (IsReturnArrow(text, index))
            {
                index++;
                continue;
            }

            var character = text[index];
            if (character == open)
            {
                depth++;
                continue;
            }

            if (character != close)
            {
                continue;
            }

            depth--;
            if (depth < 1)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Splits on a separator that is not inside brackets, so a tuple, a closure type or a nested
    /// generic argument survives as one piece. Empty pieces are dropped: a trailing comma in a
    /// parameter list is legal Swift and is not a parameter.
    /// </summary>
    internal static List<string> SplitTopLevel(string text, char separator)
    {
        var pieces = new List<string>();
        var depth = 0;
        var start = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (IsReturnArrow(text, index))
            {
                index++;
                continue;
            }

            switch (text[index])
            {
                case '(' or '[' or '{' or '<':
                    depth++;
                    break;

                case ')' or ']' or '}' or '>':
                    depth--;
                    break;

                default:
                    if (text[index] == separator && depth < 1)
                    {
                        Add(pieces, text[start..index]);
                        start = index + 1;
                    }

                    break;
            }
        }

        Add(pieces, text[start..]);
        return pieces;
    }

    /// <summary>The index of the first occurrence of <paramref name="wanted"/> outside brackets.</summary>
    internal static int IndexOfTopLevel(string text, char wanted, int from = 0)
    {
        var depth = 0;
        for (var index = from; index < text.Length; index++)
        {
            if (IsReturnArrow(text, index))
            {
                index++;
                continue;
            }

            switch (text[index])
            {
                case '(' or '[' or '{' or '<':
                    depth++;
                    continue;

                case ')' or ']' or '}' or '>':
                    depth--;
                    continue;
            }

            if (text[index] == wanted && depth < 1)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// The index of a whole word outside brackets, so that "throws" in a nested closure type is
    /// not read as the enclosing function's, and "throwsAnError" is not read as "throws".
    /// </summary>
    internal static int IndexOfTopLevelWord(string text, string word)
    {
        var depth = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (IsReturnArrow(text, index))
            {
                index++;
                continue;
            }

            switch (text[index])
            {
                case '(' or '[' or '{' or '<':
                    depth++;
                    continue;

                case ')' or ']' or '}' or '>':
                    depth--;
                    continue;
            }

            if (depth >= 1 || !IsWordAt(text, index, word))
            {
                continue;
            }

            return index;
        }

        return -1;
    }

    internal static bool ContainsTopLevelWord(string text, string word)
    {
        return IndexOfTopLevelWord(text, word) >= 0;
    }

    internal static bool IsWordAt(string text, int index, string word)
    {
        if (!text.AsSpan(index).StartsWith(word, StringComparison.Ordinal))
        {
            return false;
        }

        // A leading dot makes it a member access, not the keyword: "x.set(1)" in a computed
        // property's body is not the "set" accessor that would make the property settable.
        if (index > 0 && (IsWordCharacter(text[index - 1]) || text[index - 1] == '.'))
        {
            return false;
        }

        var after = index + word.Length;
        return after >= text.Length || !IsWordCharacter(text[after]);
    }

    internal static bool IsWordCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character == '_';
    }

    /// <summary>
    /// An identifier as Swift writes it, including the backtick-escaped form. Used to tell a named
    /// declaration from an operator one, which is what decides whether argument labels exist.
    /// </summary>
    internal static bool IsIdentifier(string text)
    {
        if (text.Length < 1)
        {
            return false;
        }

        if (text.StartsWith('`') && text.EndsWith('`'))
        {
            return text.Length > 2;
        }

        if (char.IsDigit(text[0]))
        {
            return false;
        }

        foreach (var character in text)
        {
            if (!IsWordCharacter(character))
            {
                return false;
            }
        }

        return true;
    }

    internal static string TrimBackticks(string identifier)
    {
        return identifier.StartsWith('`') && identifier.EndsWith('`') && identifier.Length > 2
            ? identifier[1..^1]
            : identifier;
    }

    private static bool IsReturnArrow(string text, int index)
    {
        return text[index] == '-' && index + 1 < text.Length && text[index + 1] == '>';
    }

    private static char GetClosingBracket(char open)
    {
        return open switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            '<' => '>',
            _ => throw new ArgumentOutOfRangeException(nameof(open), open, "Not an opening bracket")
        };
    }

    private static void Add(List<string> pieces, string piece)
    {
        var trimmed = piece.Trim();
        if (trimmed.Length < 1)
        {
            return;
        }

        pieces.Add(trimmed);
    }
}
