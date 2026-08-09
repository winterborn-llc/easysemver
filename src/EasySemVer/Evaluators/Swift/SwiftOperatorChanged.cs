using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S38 - an operator declaration or its precedence group changed.</summary>
public class SwiftOperatorChanged : IEvaluateSwiftSignatures
{
    public string RuleId => "S38";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed, or changed kind or precedence group";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var olderOperator in signatures.Older.Operators)
        {
            var newerOperator = FindOperator(signatures.Newer, olderOperator.Name);
            if (newerOperator == null)
            {
                yield return olderOperator.Name;
                continue;
            }

            if (newerOperator.PrecedenceGroup != olderOperator.PrecedenceGroup)
            {
                yield return olderOperator.Name;
                continue;
            }

            if (newerOperator.OperatorKind != olderOperator.OperatorKind)
            {
                yield return olderOperator.Name;
            }
        }
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
