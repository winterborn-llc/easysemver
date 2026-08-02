using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S24 - throws or async was removed, which existing call sites tolerate.</summary>
public class SwiftEffectRemoved : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (functionPair.Older.Throws && !functionPair.Newer.Throws)
            {
                return true;
            }

            if (functionPair.Older.IsAsync && !functionPair.Newer.IsAsync)
            {
                return true;
            }
        }

        return false;
    }
}
