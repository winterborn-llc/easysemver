using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R33 - a type lost sealed or abstract, which only widens what callers may do.</summary>
public class TypeInheritanceRelaxed : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "widened what callers may derive from or instantiate";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            // Losing sealed and losing abstract say the same thing, so a type that did both is
            // still one finding.
            if (typePair.Older.IsSealed && !typePair.Newer.IsSealed)
            {
                yield return typePair.Newer.Name;
                continue;
            }

            if (typePair.Older.IsAbstract && !typePair.Newer.IsAbstract)
            {
                yield return typePair.Newer.Name;
            }
        }
    }
}
