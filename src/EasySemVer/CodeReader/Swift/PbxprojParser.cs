using System.Text;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// A project.pbxproj, read as what it is: an OpenStep property list of dictionaries, arrays and
/// strings. Nothing in .NET parses one, and it is small enough a format to read directly - four
/// kinds of token, one of which is a comment.
/// <para>
/// This exists so that an .xcodeproj can be understood without running <c>xcodebuild</c>. The
/// project file already had to be read for MARKETING_VERSION (MVR-04) and for product types
/// (UNI-04); reading it properly rather than by line-scanning is what lets the target list and the
/// per-target source lists come from it too.
/// </para>
/// </summary>
internal static class PbxprojParser
{
    /// <summary>
    /// The parsed root. Values are nested <see cref="Dictionary{TKey,TValue}"/>,
    /// <see cref="List{T}"/> and <see cref="string"/>, which is every type the format has.
    /// </summary>
    internal static Dictionary<string, object> Parse(string text)
    {
        var index = 0;
        SkipTrivia(text, ref index);

        // The file is one dictionary, opened by a brace after the encoding comment.
        if (index >= text.Length || text[index] != '{')
        {
            throw new InvalidDataException("The project file does not begin with a dictionary");
        }

        index++;
        return ParseDictionary(text, ref index);
    }

    private static Dictionary<string, object> ParseDictionary(string text, ref int index)
    {
        var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
        while (true)
        {
            SkipTrivia(text, ref index);
            if (index >= text.Length)
            {
                return dictionary;
            }

            if (text[index] == '}')
            {
                index++;
                return dictionary;
            }

            var key = ParseToken(text, ref index);
            SkipTrivia(text, ref index);
            if (index >= text.Length || text[index] != '=')
            {
                // Not a key-value pair after all; step over it rather than losing the rest.
                index++;
                continue;
            }

            index++;
            dictionary[key] = ParseValue(text, ref index);

            SkipTrivia(text, ref index);
            if (index < text.Length && text[index] == ';')
            {
                index++;
            }
        }
    }

    private static List<object> ParseArray(string text, ref int index)
    {
        var values = new List<object>();
        while (true)
        {
            SkipTrivia(text, ref index);
            if (index >= text.Length)
            {
                return values;
            }

            if (text[index] == ')')
            {
                index++;
                return values;
            }

            values.Add(ParseValue(text, ref index));

            SkipTrivia(text, ref index);
            if (index < text.Length && text[index] == ',')
            {
                index++;
            }
        }
    }

    private static object ParseValue(string text, ref int index)
    {
        SkipTrivia(text, ref index);
        if (index >= text.Length)
        {
            return string.Empty;
        }

        switch (text[index])
        {
            case '{':
                index++;
                return ParseDictionary(text, ref index);

            case '(':
                index++;
                return ParseArray(text, ref index);

            default:
                return ParseToken(text, ref index);
        }
    }

    /// <summary>
    /// A bare word or a quoted string. Quoting is presentational in this format - Xcode quotes a
    /// value when it contains a space or a special character - so the quotes come off here and the
    /// two spellings of the same value read back the same.
    /// </summary>
    private static string ParseToken(string text, ref int index)
    {
        if (text[index] == '"')
        {
            return ParseQuoted(text, ref index);
        }

        var start = index;
        while (index < text.Length && !IsDelimiter(text[index]))
        {
            index++;
        }

        return text[start..index];
    }

    private static string ParseQuoted(string text, ref int index)
    {
        var value = new StringBuilder();
        index++;
        while (index < text.Length && text[index] != '"')
        {
            if (text[index] == '\\' && index + 1 < text.Length)
            {
                index++;
            }

            value.Append(text[index]);
            index++;
        }

        index++;
        return value.ToString();
    }

    private static bool IsDelimiter(char character)
    {
        return char.IsWhiteSpace(character) || character is '=' or ';' or ',' or '{' or '}'
            or '(' or ')' or '"';
    }

    /// <summary>Whitespace and the "/* ... */" comments Xcode writes beside every identifier.</summary>
    private static void SkipTrivia(string text, ref int index)
    {
        while (index < text.Length)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index++;
                continue;
            }

            if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '*')
            {
                var close = text.IndexOf("*/", index + 2, StringComparison.Ordinal);
                index = close < 0 ? text.Length : close + 2;
                continue;
            }

            if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '/')
            {
                var newline = text.IndexOf('\n', index);
                index = newline < 0 ? text.Length : newline + 1;
                continue;
            }

            return;
        }
    }
}
