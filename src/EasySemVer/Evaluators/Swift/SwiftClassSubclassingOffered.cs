using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S05 - a class went from public to open.</summary>
public class SwiftClassSubclassingOffered : IEvaluateSwiftSignatures
{
    public string RuleId => "S05";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "became open, so it can now be subclassed";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.AccessLevel == SwiftAccessLevels.Open)
            {
                continue;
            }

            if (typePair.Newer.AccessLevel != SwiftAccessLevels.Open)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
