using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S22 - a function's parameter labels, types, order, count, or return type changed.</summary>
public class SwiftFunctionSignatureChanged : IEvaluateSwiftSignatures
{
    public string RuleId => "S22";

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
