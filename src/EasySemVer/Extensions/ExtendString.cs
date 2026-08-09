using System.Diagnostics.CodeAnalysis;

namespace Winterborn.Tools.EasySemVer.Extensions;

internal static class ExtendString
{
    internal static bool IsNullOrWhitespace([NotNullWhen(false)] this string? text)
    {
        return string.IsNullOrWhiteSpace(text);
    }
}
