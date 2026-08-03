using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S14 - @frozen was removed from a public struct or enum, so its layout is no longer guaranteed.</summary>
public class SwiftFrozenRemoved : IEvaluateSwiftSignatures
{
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
