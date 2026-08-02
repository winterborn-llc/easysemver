using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R40 - a generic constraint was removed or loosened. More type arguments compile than before,
/// so nothing existing breaks.
/// </summary>
public class GenericConstraintLoosened : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (IsLoosened(typePair.Older.GenericParameters, typePair.Newer.GenericParameters))
            {
                return true;
            }
        }

        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            if (IsLoosened(overloadPair.Older.GenericParameters, overloadPair.Newer.GenericParameters))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLoosened(
        IReadOnlyList<ICsharpGenericParameter> older,
        IReadOnlyList<ICsharpGenericParameter> newer)
    {
        // A parameter-count change is R39's business, not this rule's.
        if (older.Count != newer.Count)
        {
            return false;
        }

        foreach (var pair in GenericConstraints.GetPaired(older, newer))
        {
            if (!GenericConstraints.HasExtraConstraint(pair.Older.Constraints, pair.Newer.Constraints))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
