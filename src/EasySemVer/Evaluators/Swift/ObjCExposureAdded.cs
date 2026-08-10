using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S28 - ObjC exposure was added.</summary>
public class ObjCExposureAdded : IEvaluateSwiftSignatures
{
    public string Rule => "ObjCExposureAdded";

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
