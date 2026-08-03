using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R05 - the unit gained a public class the baseline did not have.</summary>
public class ProjectClassAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var newerClass in signatures.Newer.Classes)
        {
            var olderClass = CsharpSignaturesToCompare.FindType(
                signatures.Older, newerClass.Name, newerClass.Kind);
            if (olderClass != null)
            {
                continue;
            }

            yield return newerClass.Name;
        }
    }
}
