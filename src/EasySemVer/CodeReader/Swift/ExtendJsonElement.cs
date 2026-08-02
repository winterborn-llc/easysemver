using System.Text.Json;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

internal static class ExtendJsonElement
{
    internal static string GetStringOrEmpty(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    internal static JsonElement? GetOrNull(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value : null;
    }
}
