using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S22 - a function's parameter labels, types, order, count, or return type changed.</summary>
public class SwiftFunctionSignatureChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (functionPair.Older.ReturnType != functionPair.Newer.ReturnType)
            {
                return true;
            }

            if (!SwiftParameters.AreTheSame(functionPair.Older.Parameters, functionPair.Newer.Parameters))
            {
                return true;
            }
        }

        return false;
    }
}
