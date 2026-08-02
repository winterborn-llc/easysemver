using Winterborn.Library.EasySemVer.DataObject.Swift;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// An enum case's associated values are not reported as a function signature, so they are read
/// from the declaration the toolchain emitted: <c>case green(shade: Int, Double)</c>.
/// </summary>
internal static class SwiftEnumCaseText
{
    internal static List<SwiftParameter> GetAssociatedValues(string declaration)
    {
        var values = new List<SwiftParameter>();
        var start = declaration.IndexOf('(');
        var end = declaration.LastIndexOf(')');
        if (start < 0 || end <= start)
        {
            return values;
        }

        foreach (var value in declaration[(start + 1)..end].Split(','))
        {
            var text = value.Trim();
            if (text.Length < 1)
            {
                continue;
            }

            var separator = text.IndexOf(": ", StringComparison.Ordinal);
            values.Add(separator < 0
                ? new SwiftParameter { Label = string.Empty, Type = text }
                : new SwiftParameter { Label = text[..separator], Type = text[(separator + 2)..].Trim() });
        }

        return values;
    }
}
