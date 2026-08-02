using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S10 - a protocol conformance was added.</summary>
public class SwiftConformanceAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var conformance in typePair.Newer.Conformances)
            {
                if (typePair.Older.Conformances.Contains(conformance))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
