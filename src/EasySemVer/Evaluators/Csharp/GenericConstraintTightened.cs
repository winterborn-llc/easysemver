using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R39 - a generic parameter count changed, or a constraint was added or tightened. Either way
/// some type argument that used to compile no longer does.
/// </summary>
public class GenericConstraintTightened : IEvaluateCsharpSignatures
{
    public string Rule => "GenericConstraintTightened";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "tightened its generic constraints";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (!IsTightened(typePair.Older.GenericParameters, typePair.Newer.GenericParameters))
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }

        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            if (!IsTightened(overloadPair.Older.GenericParameters, overloadPair.Newer.GenericParameters))
            {
                continue;
            }

            yield return overloadPair.Symbol;
        }
    }

    private static bool IsTightened(
        IReadOnlyList<ICsharpGenericParameter> older,
        IReadOnlyList<ICsharpGenericParameter> newer)
    {
        if (older.Count != newer.Count)
        {
            return true;
        }

        foreach (var pair in GenericConstraints.GetPaired(older, newer))
        {
            if (!GenericConstraints.HasExtraConstraint(pair.Newer.Constraints, pair.Older.Constraints))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
