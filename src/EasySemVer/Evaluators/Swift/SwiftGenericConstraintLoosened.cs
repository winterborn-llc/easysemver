using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S13 - a generic constraint was removed or loosened.</summary>
public class SwiftGenericConstraintLoosened : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (IsLoosened(typePair.Older.GenericParameters, typePair.Newer.GenericParameters))
            {
                return true;
            }
        }

        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (IsLoosened(functionPair.Older.GenericParameters, functionPair.Newer.GenericParameters))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLoosened(
        IReadOnlyList<ISwiftGenericParameter> older,
        IReadOnlyList<ISwiftGenericParameter> newer)
    {
        // A parameter-count change is S11's business, not this rule's.
        if (SwiftGenericConstraints.DidCountChange(older, newer))
        {
            return false;
        }

        foreach (var pair in SwiftGenericConstraints.GetPaired(older, newer))
        {
            if (!SwiftGenericConstraints.HasExtraConstraint(pair.Older.Constraints, pair.Newer.Constraints))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
