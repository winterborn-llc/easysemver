using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S28 - ObjC exposure was added.</summary>
public class SwiftObjCExposureAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S28";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "gained Objective-C exposure";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
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

            yield return pair.Newer.Name;
        }
    }
}
