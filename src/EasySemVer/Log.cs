namespace Winterborn.Library.EasySemVer;

/// <summary>
/// The one logging surface (LOG-01). With several languages, many units and shelled-out tools in
/// play, the nesting actually earns its keep: root, then language, then unit, then firing rule.
/// </summary>
internal static class Log
{
    private const int SpacesPerIndentLevel = 3;

    private const string DateTimeBuffer = "                        ";

    private static bool _areWritingLine;

    private static int _indentLevel;

    private static string IndentString => new(' ', _indentLevel * SpacesPerIndentLevel);

    internal static void Indent()
    {
        _indentLevel++;
    }

    internal static void Outdent()
    {
        _indentLevel--;
        if (_indentLevel < 0)
        {
            _indentLevel = 0;
        }
    }

    internal static void ResetIndent()
    {
        _indentLevel = 0;
    }

    internal static void WriteLine(string message)
    {
        Write(message);
        EndLine();
    }

    internal static void EndLine()
    {
        _areWritingLine = false;
        Console.Write("\n");
    }

    internal static void Write(string message)
    {
        if (!_areWritingLine)
        {
            Console.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ");
            Console.Write(IndentString);
            _areWritingLine = true;
        }

        // Continuation lines line up past the timestamp column, so a multi-line message - a stack
        // trace, a tool's stderr - stays visually attached to its entry.
        var messageWithIndents = message.ReplaceLineEndings($"\n{DateTimeBuffer}{IndentString}");
        Console.Write(messageWithIndents);
    }
}
