using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S09 - a protocol conformance was removed from a public type.</summary>
public class SwiftConformanceRemoved : IEvaluateSwiftSignatures
{
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
