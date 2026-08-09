using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R41 (removal half) - a public nested type is gone. Nested types are recorded flat under their
/// Outer.Inner name, so this is the same lookup as a top-level type with a declaring type set.
/// </summary>
public class NestedTypeRemoved : IEvaluateCsharpSignatures
{
    public string RuleId => "R41";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var olderType in signatures.Older.Types)
        {
            if (olderType.DeclaringType.Length < 1)
            {
                continue;
            }

            var newerType = CsharpSignaturesToCompare.FindTypeOfAnyKind(signatures.Newer, olderType.Name);
            if (newerType != null)
            {
                continue;
            }

            yield return olderType.Name;
        }
    }
}
