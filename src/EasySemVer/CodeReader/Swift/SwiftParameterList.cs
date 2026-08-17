using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// A parameter list as written, turned into the modelled parameters. Everything that makes a
/// parameter part of the contract is here: its label, its type, whether it carries a default,
/// and the ownership and variadic markers that change how a caller may pass it.
/// </summary>
internal static class SwiftParameterList
{
    /// <summary>
    /// Markers that sit in front of a parameter's type and change the calling convention. "inout"
    /// is modelled separately as well because it is the one a caller cannot ignore.
    /// </summary>
    private static readonly string[] OwnershipKeywords =
        ["inout", "borrowing", "consuming", "__owned", "__shared"];

    /// <summary>
    /// <paramref name="labelsAreOmitted"/> is what a subscript needs: "subscript(index: Int)" is
    /// called as "widget[0]", so the one name written there is the internal one and the label is
    /// omitted. Every other parameter list works the other way round.
    /// </summary>
    internal static List<SwiftParameter> Read(string parameterList, bool labelsAreOmitted = false)
    {
        var parameters = new List<SwiftParameter>();
        foreach (var piece in SwiftText.SplitTopLevel(parameterList, ','))
        {
            parameters.Add(ReadOne(piece, labelsAreOmitted));
        }

        return parameters;
    }

    private static SwiftParameter ReadOne(string text, bool labelsAreOmitted)
    {
        var separator = SwiftText.IndexOfTopLevel(text, ':');
        if (separator < 0)
        {
            // An enum case's associated value may be a bare type with no label at all.
            return new SwiftParameter { Type = SwiftHeaderCursor.Collapse(text) };
        }

        var names = SwiftText.SplitTopLevel(text[..separator], ' ');
        var onlyName = names.Count == 1;
        var parameter = new SwiftParameter
        {
            Label = onlyName && labelsAreOmitted
                ? "_"
                : names.Count > 0 ? SwiftText.TrimBackticks(names[0]) : string.Empty,
            InternalName = onlyName && labelsAreOmitted
                ? SwiftText.TrimBackticks(names[0])
                : names.Count > 1 ? SwiftText.TrimBackticks(names[^1]) : string.Empty
        };

        ReadType(parameter, text[(separator + 1)..]);
        return parameter;
    }

    private static void ReadType(SwiftParameter parameter, string text)
    {
        var type = SwiftHeaderCursor.Collapse(text);

        var defaultValue = SwiftText.IndexOfTopLevel(type, '=');
        if (defaultValue >= 0)
        {
            parameter.HasDefault = true;
            type = type[..defaultValue].Trim();
        }

        foreach (var keyword in OwnershipKeywords)
        {
            if (!type.StartsWith(keyword + " ", StringComparison.Ordinal))
            {
                continue;
            }

            parameter.Ownership = keyword;
            parameter.IsInout = keyword == "inout";
            type = type[(keyword.Length + 1)..].Trim();
            break;
        }

        if (type.EndsWith("...", StringComparison.Ordinal))
        {
            parameter.IsVariadic = true;
            type = type[..^3].Trim();
        }

        parameter.Type = type;
    }
}
