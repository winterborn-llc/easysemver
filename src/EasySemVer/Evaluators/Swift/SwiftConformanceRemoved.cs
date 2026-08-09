using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S09 - a protocol conformance was removed from a public type.</summary>
public class SwiftConformanceRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S09";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "no longer conforms to a protocol it used to";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var conformance in typePair.Older.Conformances)
            {
                if (typePair.Newer.Conformances.Contains(conformance))
                {
                    continue;
                }

                // The type is the subject; the protocol it dropped rides along in the symbol so
                // the line names it without the description having to vary per finding.
                yield return $"{typePair.Newer.Name} ({conformance})";
            }
        }
    }
}
