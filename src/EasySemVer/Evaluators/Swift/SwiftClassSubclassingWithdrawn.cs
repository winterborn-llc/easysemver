using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S04 - a class went from open to public, withdrawing subclassing and overriding.</summary>
public class SwiftClassSubclassingWithdrawn : IEvaluateSwiftSignatures
{
    public string RuleId => "S04";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "is no longer open, so it can no longer be subclassed";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.AccessLevel != SwiftAccessLevels.Open)
            {
                continue;
            }

            if (typePair.Newer.AccessLevel == SwiftAccessLevels.Open)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
