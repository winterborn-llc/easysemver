using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S04 - a class went from open to public, withdrawing subclassing and overriding.</summary>
public class SwiftClassSubclassingWithdrawn : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.AccessLevel != SwiftAccessLevels.Open)
            {
                continue;
            }

            if (typePair.Newer.AccessLevel == SwiftAccessLevels.Open)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
