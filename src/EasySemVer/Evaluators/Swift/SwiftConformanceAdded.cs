using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S10 - a protocol conformance was added.</summary>
public class SwiftConformanceAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S10";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "conforms to a protocol it did not before";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var conformance in typePair.Newer.Conformances)
            {
                if (typePair.Older.Conformances.Contains(conformance))
                {
                    continue;
                }

                yield return $"{typePair.Newer.Name} ({conformance})";
            }
        }
    }
}
