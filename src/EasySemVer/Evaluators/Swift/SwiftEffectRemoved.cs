using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S24 - throws or async was removed, which existing call sites tolerate.</summary>
public class SwiftEffectRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S24";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "lost a throws or async effect";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            // Losing both effects at once is still one thing that happened to the function.
            if (functionPair.Older.Throws && !functionPair.Newer.Throws)
            {
                yield return functionPair.Newer.Name;
                continue;
            }

            if (functionPair.Older.IsAsync && !functionPair.Newer.IsAsync)
            {
                yield return functionPair.Newer.Name;
            }
        }
    }
}
