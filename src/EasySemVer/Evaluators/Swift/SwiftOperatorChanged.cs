using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S38 - an operator declaration or its precedence group changed.</summary>
public class SwiftOperatorChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var olderOperator in signatures.Older.Operators)
        {
            var newerOperator = FindOperator(signatures.Newer, olderOperator.Name);
            if (newerOperator == null)
            {
                return true;
            }

            if (newerOperator.PrecedenceGroup != olderOperator.PrecedenceGroup)
            {
                return true;
            }

            if (newerOperator.OperatorKind != olderOperator.OperatorKind)
            {
                return true;
            }
        }

        return false;
    }

    private static ISwiftOperator? FindOperator(ISwiftModule module, string name)
    {
        foreach (var candidate in module.Operators)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
