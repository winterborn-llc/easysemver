using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S33 - a parameter's inout, variadic or ownership modifier changed. The call site has to change with it, in either direction.</summary>
public class ParameterModifierChanged : IEvaluateSwiftSignatures
{
    public string Rule => "ParameterModifierChanged";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed a parameter modifier";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            foreach (var parameterPair in SwiftParameters.GetPaired(
                         functionPair.Older.Parameters,
                         functionPair.Newer.Parameters))
            {
                if (!DidAnyModifierChange(parameterPair.Older, parameterPair.Newer))
                {
                    continue;
                }

                yield return $"{functionPair.Newer.Name} ({parameterPair.Newer.Label})";
            }
        }
    }

    private static bool DidAnyModifierChange(ISwiftParameter older, ISwiftParameter newer)
    {
        if (older.IsInout != newer.IsInout)
        {
            return true;
        }

        if (older.IsVariadic != newer.IsVariadic)
        {
            return true;
        }

        return older.Ownership != newer.Ownership;
    }
}
