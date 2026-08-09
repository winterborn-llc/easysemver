using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S07 - a class lost final, which only widens what callers may do.</summary>
public class SwiftClassFinalRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S07";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "is no longer final";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (!typePair.Older.IsFinal || typePair.Newer.IsFinal)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
