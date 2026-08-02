using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S28 - ObjC exposure was added.</summary>
public class SwiftObjCExposureAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var pair in SwiftMembers.GetPairedDeclarations(signatures))
        {
            if (pair.Older.ObjCExposure.Length > 0)
            {
                continue;
            }

            if (pair.Newer.ObjCExposure.Length < 1)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
