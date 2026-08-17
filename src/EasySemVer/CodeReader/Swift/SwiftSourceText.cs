using System.Text;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// A Swift file with its comments and string literals blanked out (SWE-07). Everything downstream
/// matches brackets and splits on commas, and a brace inside a string or a comment would throw all
/// of that off - <c>let brace = "}"</c> would end the enclosing type three declarations early.
/// <para>
/// Blanked, not removed: every character keeps its index and every newline survives, so a span
/// found in the blanked text addresses exactly the same span of the original file, and the
/// original is what declaration text is finally read from.
/// </para>
/// </summary>
internal static class SwiftSourceText
{
    internal static string Blank(string source)
    {
        var blanked = new StringBuilder(source);
        var index = 0;
        while (index < source.Length)
        {
            var character = source[index];

            if (character == '/' && Next(source, index) == '/')
            {
                index = BlankLineComment(source, blanked, index);
                continue;
            }

            if (character == '/' && Next(source, index) == '*')
            {
                index = BlankBlockComment(source, blanked, index);
                continue;
            }

            if (character is '"' or '#')
            {
                var afterString = BlankString(source, blanked, index);
                if (afterString > index)
                {
                    index = afterString;
                    continue;
                }
            }

            index++;
        }

        return blanked.ToString();
    }

    private static char Next(string source, int index)
    {
        return index + 1 < source.Length ? source[index + 1] : '\0';
    }

    private static int BlankLineComment(string source, StringBuilder blanked, int start)
    {
        var index = start;
        while (index < source.Length && source[index] != '\n')
        {
            blanked[index] = ' ';
            index++;
        }

        return index;
    }

    /// <summary>Swift block comments nest, so a depth counter is not optional here.</summary>
    private static int BlankBlockComment(string source, StringBuilder blanked, int start)
    {
        var depth = 0;
        var index = start;
        while (index < source.Length)
        {
            if (source[index] == '/' && Next(source, index) == '*')
            {
                depth++;
                Blank(blanked, source, index, 2);
                index += 2;
                continue;
            }

            if (source[index] == '*' && Next(source, index) == '/')
            {
                depth--;
                Blank(blanked, source, index, 2);
                index += 2;
                if (depth < 1)
                {
                    return index;
                }

                continue;
            }

            Blank(blanked, source, index, 1);
            index++;
        }

        return index;
    }

    /// <summary>
    /// Every string literal Swift has: plain, multiline, and raw at any pound level. Returns the
    /// index just past the closing delimiter, or <paramref name="start"/> if this was not a string
    /// after all - a bare "#" is the start of "#if" far more often than of a raw string.
    /// </summary>
    private static int BlankString(string source, StringBuilder blanked, int start)
    {
        var pounds = 0;
        var index = start;
        while (index < source.Length && source[index] == '#')
        {
            pounds++;
            index++;
        }

        if (index >= source.Length || source[index] != '"')
        {
            return start;
        }

        var isMultiline = source.AsSpan(index).StartsWith("\"\"\"");
        var delimiter = isMultiline ? "\"\"\"" : "\"";
        var closing = delimiter + new string('#', pounds);

        Blank(blanked, source, start, index - start + delimiter.Length);
        index += delimiter.Length;

        while (index < source.Length)
        {
            // An escape only escapes when it is not raw, or when it carries the pound level.
            if (source[index] == '\\' && HasPounds(source, index + 1, pounds))
            {
                var afterEscape = index + 1 + pounds;
                if (afterEscape < source.Length && source[afterEscape] == '(')
                {
                    var afterInterpolation = BlankInterpolation(source, blanked, afterEscape);
                    Blank(blanked, source, index, afterEscape - index);
                    index = afterInterpolation;
                    continue;
                }

                // "\\" must not be read as an escaped quote, so the pair is consumed together.
                Blank(blanked, source, index, Math.Min(2 + pounds, source.Length - index));
                index += 1 + pounds + 1;
                continue;
            }

            if (source[index] == '"' && source.AsSpan(index).StartsWith(closing))
            {
                Blank(blanked, source, index, closing.Length);
                KeepDelimiters(blanked, start, index + closing.Length);
                return index + closing.Length;
            }

            Blank(blanked, source, index, 1);
            index++;
        }

        return index;
    }

    /// <summary>
    /// An interpolation holds an arbitrary expression, strings and all: <c>"\(a["]"])"</c> is one
    /// literal, and reading the quote inside it as a terminator would blank the rest of the file.
    /// The expression is blanked wholesale - no declaration ever lives inside one.
    /// </summary>
    private static int BlankInterpolation(string source, StringBuilder blanked, int openParen)
    {
        var depth = 0;
        var index = openParen;
        while (index < source.Length)
        {
            switch (source[index])
            {
                case '"' or '#':
                    var afterString = BlankString(source, blanked, index);
                    if (afterString > index)
                    {
                        index = afterString;
                        continue;
                    }

                    break;

                case '(':
                    depth++;
                    break;

                case ')':
                    depth--;
                    if (depth < 1)
                    {
                        Blank(blanked, source, index, 1);
                        return index + 1;
                    }

                    break;
            }

            Blank(blanked, source, index, 1);
            index++;
        }

        return index;
    }

    /// <summary>
    /// A blanked literal keeps a quote at each end, so that it is still visibly something. It
    /// matters at the end of a line: "var name: String = """ blanked to nothing would leave a
    /// trailing "=", and a declaration ending in one is read as continuing onto the next line.
    /// </summary>
    private static void KeepDelimiters(StringBuilder blanked, int start, int end)
    {
        blanked[start] = '"';
        blanked[end - 1] = '"';
    }

    private static bool HasPounds(string source, int index, int pounds)
    {
        for (var offset = 0; offset < pounds; offset++)
        {
            if (index + offset >= source.Length || source[index + offset] != '#')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Newlines survive blanking: a line comment ends at one, and line numbers are useful.</summary>
    private static void Blank(StringBuilder blanked, string source, int start, int length)
    {
        for (var index = start; index < start + length && index < source.Length; index++)
        {
            if (source[index] != '\n')
            {
                blanked[index] = ' ';
            }
        }
    }
}
