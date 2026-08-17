using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// The "@available" attribute, in both spellings Swift allows: the short form that lists platforms
/// and the versions they were introduced in, and the long form that says what happened to one
/// platform. S19 and S22 turn on the difference between them.
/// </summary>
internal static class SwiftAvailabilityText
{
    private const string AttributeName = "@available";

    /// <summary>Long-form keys written bare, with no value after them.</summary>
    private static readonly string[] BareKeys = ["deprecated", "unavailable", "noasync"];

    internal static List<SwiftAvailability> Read(IReadOnlyList<string> attributes)
    {
        var availability = new List<SwiftAvailability>();
        foreach (var attribute in attributes)
        {
            if (!attribute.StartsWith(AttributeName, StringComparison.Ordinal))
            {
                continue;
            }

            var open = attribute.IndexOf('(');
            var close = SwiftText.FindMatching(attribute, open < 0 ? attribute.Length - 1 : open);
            if (open < 0 || close < 0)
            {
                continue;
            }

            ReadArguments(SwiftText.SplitTopLevel(attribute[(open + 1)..close], ','), availability);
        }

        return availability;
    }

    private static void ReadArguments(List<string> arguments, List<SwiftAvailability> availability)
    {
        if (arguments.Count < 1)
        {
            return;
        }

        if (IsLongForm(arguments))
        {
            availability.Add(ReadLongForm(arguments));
            return;
        }

        foreach (var argument in arguments)
        {
            // The trailing "*" says "and every other platform", which is not a platform.
            if (argument == "*")
            {
                continue;
            }

            var parts = SwiftText.SplitTopLevel(argument, ' ');
            availability.Add(new SwiftAvailability
            {
                Domain = parts.Count > 0 ? parts[0] : string.Empty,
                Introduced = parts.Count > 1 ? parts[1] : string.Empty
            });
        }
    }

    /// <summary>
    /// The two forms are told apart by their second argument: the long form's is a key, with or
    /// without a value, where the short form's is another platform.
    /// </summary>
    private static bool IsLongForm(List<string> arguments)
    {
        if (arguments.Count < 2)
        {
            return false;
        }

        return arguments[1].Contains(':') || BareKeys.Contains(arguments[1]);
    }

    private static SwiftAvailability ReadLongForm(List<string> arguments)
    {
        var availability = new SwiftAvailability { Domain = arguments[0] };
        for (var index = 1; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            var separator = argument.IndexOf(':');
            if (separator < 0)
            {
                availability.IsDeprecated |= argument == "deprecated";
                availability.IsUnavailable |= argument == "unavailable";
                continue;
            }

            Apply(availability, argument[..separator].Trim(), Unquote(argument[(separator + 1)..]));
        }

        return availability;
    }

    private static void Apply(SwiftAvailability availability, string key, string value)
    {
        switch (key)
        {
            case "introduced":
                availability.Introduced = value;
                return;

            case "deprecated":
                availability.Deprecated = value;
                availability.IsDeprecated = true;
                return;

            case "obsoleted":
                availability.Obsoleted = value;
                return;

            case "renamed":
                availability.RenamedTo = value;
                return;
        }
    }

    private static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length > 1 && trimmed.StartsWith('"') && trimmed.EndsWith('"')
            ? trimmed[1..^1]
            : trimmed;
    }
}
