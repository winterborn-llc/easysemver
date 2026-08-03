using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S22 - a function's parameter labels, types, order, count, or return type changed.</summary>
public class SwiftFunctionSignatureChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its signature";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            // Parameters and return type are one signature, so a function that changed both is
            // still one finding.
            if (functionPair.Older.ReturnType == functionPair.Newer.ReturnType
                && SwiftParameters.AreTheSame(
                    functionPair.Older.Parameters, functionPair.Newer.Parameters))
            {
                continue;
            }

            yield return functionPair.Newer.Name;
        }
    }
}
