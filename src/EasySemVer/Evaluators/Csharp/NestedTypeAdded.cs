using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R41 (addition half) - a public nested type appeared.</summary>
public class NestedTypeAdded : IEvaluateCsharpSignatures
{
    public string RuleId => "R41";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var newerType in signatures.Newer.Types)
        {
            if (newerType.DeclaringType.Length < 1)
            {
                continue;
            }

            var olderType = CsharpSignaturesToCompare.FindTypeOfAnyKind(signatures.Older, newerType.Name);
            if (olderType != null)
            {
                continue;
            }

            yield return newerType.Name;
        }
    }
}
