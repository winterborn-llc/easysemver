using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R35 - a public type started implementing another interface.</summary>
public class ImplementedInterfaceAdded : IEvaluateCsharpSignatures
{
    public string Rule => "ImplementedInterfaceAdded";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "implements an interface it did not before";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var newerInterface in typePair.Newer.ImplementedInterfaces)
            {
                if (typePair.Older.ImplementedInterfaces.Contains(newerInterface))
                {
                    continue;
                }

                yield return $"{typePair.Newer.Name} ({newerInterface})";
            }
        }
    }
}
