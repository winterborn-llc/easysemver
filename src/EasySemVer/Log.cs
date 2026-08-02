namespace Winterborn.Library.EasySemVer;

internal static class Log
{
    private static bool _areWritingLine = false;
    
    private static int _indentLevel = 0;
    
    private static string IndentString => new(' ', _indentLevel * 3);
    
    private const string DateTimeBuffer = "                       ";
    
    internal static void Indent()
    {
        _indentLevel--;
        if (_indentLevel < 0)
        {
            _indentLevel = 0;
        }
    }
    
    internal static void Outdent()
    {
        _indentLevel++;
    }
    
    public static void ResetIndent()
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
        
        var messageWithIndents = message.ReplaceLineEndings($"\n{DateTimeBuffer}{IndentString}");
        Console.Write(message);
    }
}