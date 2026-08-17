using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// The property traversal the facet rules share (RUL-09). Swift has had
/// <c>SwiftMembers.GetPairedProperties</c> since it was written and its rules are six lines each
/// because of it; C# reached the same shape only for overloads
/// (<see cref="Overloads.GetMatchedOverloads"/>) and left every property rule re-implementing the
/// pairing inline. This finishes that job.
/// </summary>
internal static class Properties
{
    /// <summary>
    /// Every property present on both sides of a paired type, in the older side's declared order
    /// so that findings come out in the order they always have.
    /// <para>
    /// <c>DeclaringType</c> is the <em>newer</em> type, because that is the name every consumer
    /// qualifies its symbol with. Handing back the entities and their scope rather than a formatted
    /// string is RUL-03: the qualifier differs per rule, and only the rule knows it.
    /// </para>
    /// </summary>
    internal static IEnumerable<(ICsharpType DeclaringType, ICsharpProperty Older, ICsharpProperty Newer)>
        GetPaired(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            // Walking the list rather than Keys: the order is the same, and Keys builds and
            // discards a string[] every time it is touched.
            foreach (var olderProperty in typePair.Older.Properties)
            {
                if (!typePair.Newer.Properties.Contains(olderProperty.Name))
                {
                    continue;
                }

                yield return (
                    typePair.Newer,
                    olderProperty,
                    typePair.Newer.Properties[olderProperty.Name]);
            }
        }
    }
}
