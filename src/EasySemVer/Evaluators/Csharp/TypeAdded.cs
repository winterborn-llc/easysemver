using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R19 - a public interface, struct, record, enum or delegate appeared.</summary>
public class TypeAdded : IEvaluateCsharpSignatures
{
    public string RuleId => "R19";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var newerType in signatures.Newer.Types)
        {
            if (newerType.Kind == CsharpTypeKinds.Class)
            {
                continue;
            }

            var olderType = CsharpSignaturesToCompare.FindType(
                signatures.Older, newerType.Name, newerType.Kind);
            if (olderType != null)
            {
                continue;
            }

            yield return newerType.Name;
        }
    }
}
