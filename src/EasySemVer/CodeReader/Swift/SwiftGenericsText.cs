using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Generic parameter lists and "where" clauses, rendered into the constraint strings S34-S36
/// compare. A constraint is written as its kind and its right-hand side - "conformance Equatable" -
/// and the set is sorted, so that reordering a where clause is not a change and tightening one is.
/// </summary>
internal static class SwiftGenericsText
{
    private const string Conformance = "conformance";

    private const string SameType = "sameType";

    /// <summary>
    /// The parameters of "&lt;T: Equatable, U&gt;", with any constraints a where clause adds to
    /// them folded in. A where clause naming something other than a parameter by itself - a nested
    /// "T.Element" - constrains an associated type rather than the parameter, and is left to the
    /// extension constraint text rather than attributed to T.
    /// </summary>
    internal static List<SwiftGenericParameter> ReadParameters(string genericList, string whereClause)
    {
        var parameters = new List<SwiftGenericParameter>();
        foreach (var piece in SwiftText.SplitTopLevel(genericList, ','))
        {
            var constraints = new List<string>();
            var name = SplitConstraint(piece, constraints);
            if (name.Length < 1)
            {
                continue;
            }

            foreach (var clause in SwiftText.SplitTopLevel(whereClause, ','))
            {
                SplitConstraint(clause, constraints, onlyFor: name);
            }

            constraints.Sort(StringComparer.Ordinal);
            parameters.Add(new SwiftGenericParameter
            {
                Name = name,
                Constraints = string.Join(", ", constraints)
            });
        }

        return parameters;
    }

    /// <summary>
    /// A where clause as one string, in the shape the symbol graph used to report an extension's
    /// constraints: subject, kind, requirement.
    /// </summary>
    internal static string ReadConstraints(string whereClause)
    {
        var rendered = new List<string>();
        foreach (var clause in SwiftText.SplitTopLevel(whereClause, ','))
        {
            var separator = FindSeparator(clause, out var kind);
            if (separator < 0)
            {
                continue;
            }

            var subject = clause[..separator].Trim();
            var requirement = clause[(separator + (kind == SameType ? 2 : 1))..].Trim();
            rendered.Add($"{subject} {kind} {SwiftHeaderCursor.Collapse(requirement)}");
        }

        rendered.Sort(StringComparer.Ordinal);
        return string.Join(", ", rendered);
    }

    /// <summary>
    /// Splits "T: Equatable" into its subject and its constraint, adding the constraint to
    /// <paramref name="constraints"/>. Returns the subject, or nothing when the clause is not
    /// about <paramref name="onlyFor"/>.
    /// </summary>
    private static string SplitConstraint(
        string clause,
        List<string> constraints,
        string onlyFor = "")
    {
        var separator = FindSeparator(clause, out var kind);
        if (separator < 0)
        {
            var bare = clause.Trim();
            return onlyFor.Length > 0 ? string.Empty : bare;
        }

        var subject = clause[..separator].Trim();
        if (onlyFor.Length > 0 && subject != onlyFor)
        {
            return string.Empty;
        }

        var requirement = clause[(separator + (kind == SameType ? 2 : 1))..].Trim();
        constraints.Add($"{kind} {SwiftHeaderCursor.Collapse(requirement)}");
        return subject;
    }

    private static int FindSeparator(string clause, out string kind)
    {
        var sameType = clause.IndexOf("==", StringComparison.Ordinal);
        if (sameType >= 0)
        {
            kind = SameType;
            return sameType;
        }

        kind = Conformance;
        return SwiftText.IndexOfTopLevel(clause, ':');
    }
}
