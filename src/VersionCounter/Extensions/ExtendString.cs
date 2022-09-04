namespace Yamamari.Library.VersionCounter.Extensions;

internal static class ExtendString
{
    internal static IList<int> ConvertToVersionArray(this string text)
    {
        var segments = new List<int>();
        var textParts = text.Split(".");
        foreach (var textPart in textParts)
        {
            if (!int.TryParse(textPart, out var number))
            {
                throw new InvalidCastException($"'{textPart}' is an invalid version segment value");
            }

            segments.Add(number);
        }

        return segments;
    }
}