using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S27 - ObjC exposure was removed from a public declaration, breaking Objective-C and KVO clients (SWM-04).</summary>
public class SwiftObjCExposureRemoved : IEvaluateSwiftSignatures
{
    public string RuleId => "S27";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "lost or changed its Objective-C exposure";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var pair in SwiftMembers.GetPairedDeclarations(signatures))
        {
            if (pair.Older.ObjCExposure.Length < 1)
            {
                continue;
            }

            if (pair.Newer.ObjCExposure == pair.Older.ObjCExposure)
            {
                continue;
            }

            yield return pair.Newer.Name;
        }
    }
}
