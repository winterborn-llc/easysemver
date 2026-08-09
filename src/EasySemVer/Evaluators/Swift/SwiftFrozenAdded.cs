using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S15 - @frozen was added, which only promises callers more.</summary>
public class SwiftFrozenAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S15";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "became @frozen";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.IsFrozen || !typePair.Newer.IsFrozen)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
