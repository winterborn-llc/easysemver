using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S33 - a parameter's inout, variadic or ownership modifier changed. The call site has to change with it, in either direction.</summary>
public class SwiftParameterModifierChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            foreach (var parameterPair in SwiftParameters.GetPaired(
                         functionPair.Older.Parameters,
                         functionPair.Newer.Parameters))
            {
                if (parameterPair.Older.IsInout != parameterPair.Newer.IsInout)
                {
                    return true;
                }

                if (parameterPair.Older.IsVariadic != parameterPair.Newer.IsVariadic)
                {
                    return true;
                }

                if (parameterPair.Older.Ownership != parameterPair.Newer.Ownership)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
