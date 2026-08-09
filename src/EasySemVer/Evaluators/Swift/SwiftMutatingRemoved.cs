using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S30 - mutating was removed.</summary>
public class SwiftMutatingRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S30";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "is no longer mutating";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (!functionPair.Older.IsMutating || functionPair.Newer.IsMutating)
            {
                continue;
            }

            yield return functionPair.Newer.Name;
        }
    }
}
