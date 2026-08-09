using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S14 - @frozen was removed from a public struct or enum, so its layout is no longer guaranteed.</summary>
public class SwiftFrozenRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S14";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "is no longer @frozen";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (!typePair.Older.IsFrozen || typePair.Newer.IsFrozen)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
