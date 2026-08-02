using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S09 - a protocol conformance was removed from a public type.</summary>
public class SwiftConformanceRemoved : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var conformance in typePair.Older.Conformances)
            {
                if (typePair.Newer.Conformances.Contains(conformance))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
