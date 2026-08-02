using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S23 - throws or async was added to an existing declaration, so every call site has to change.</summary>
public class SwiftEffectAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (!functionPair.Older.Throws && functionPair.Newer.Throws)
            {
                return true;
            }

            if (!functionPair.Older.IsAsync && functionPair.Newer.IsAsync)
            {
                return true;
            }
        }

        return false;
    }
}
