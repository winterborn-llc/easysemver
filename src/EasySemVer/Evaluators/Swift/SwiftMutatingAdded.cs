using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S29 - mutating was added to a member of a value type, so a let binding can no longer call it.</summary>
public class SwiftMutatingAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S29";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "became mutating";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (functionPair.Older.IsMutating || !functionPair.Newer.IsMutating)
            {
                continue;
            }

            yield return functionPair.Newer.Name;
        }
    }
}
