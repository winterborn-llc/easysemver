using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S18 - an enum case was added. Major, not Minor: a client switching exhaustively stops compiling, which is every client of a package built without library evolution (SCL-01).</summary>
public class SwiftEnumCaseAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var enumPair in SwiftEnums.GetPaired(signatures))
        {
            foreach (var newerCase in enumPair.Newer.Cases)
            {
                if (SwiftMembers.FindCase(enumPair.Older, newerCase.Name) != null)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
