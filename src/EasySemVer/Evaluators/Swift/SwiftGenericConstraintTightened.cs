using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S12 - a generic constraint was added or tightened.</summary>
public class SwiftGenericConstraintTightened : IEvaluateSwiftSignatures
{
    public string RuleId => "S12";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "tightened its generic constraints";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (!IsTightened(typePair.Older.GenericParameters, typePair.Newer.GenericParameters))
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }

        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (!IsTightened(functionPair.Older.GenericParameters, functionPair.Newer.GenericParameters))
            {
                continue;
            }

            yield return functionPair.Newer.Name;
        }
    }

    private static bool IsTightened(
        IReadOnlyList<ISwiftGenericParameter> older,
        IReadOnlyList<ISwiftGenericParameter> newer)
    {
        foreach (var pair in SwiftGenericConstraints.GetPaired(older, newer))
        {
            if (!SwiftGenericConstraints.HasExtraConstraint(pair.Newer.Constraints, pair.Older.Constraints))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
