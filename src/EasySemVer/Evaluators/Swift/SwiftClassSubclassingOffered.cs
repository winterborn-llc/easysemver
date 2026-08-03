using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S05 - a class went from public to open.</summary>
public class SwiftClassSubclassingOffered : IEvaluateSwiftSignatures
{
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
