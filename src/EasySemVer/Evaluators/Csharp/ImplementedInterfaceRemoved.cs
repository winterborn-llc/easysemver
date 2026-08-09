using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R34 - a public type stopped implementing an interface, so every cast to it fails.</summary>
public class ImplementedInterfaceRemoved : IEvaluateCsharpSignatures
{
    public string RuleId => "R34";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "no longer implements an interface it used to";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderInterface in typePair.Older.ImplementedInterfaces)
            {
                if (typePair.Newer.ImplementedInterfaces.Contains(olderInterface))
                {
                    continue;
                }

                // The type is the subject; the interface it dropped goes in the symbol so the
                // line says which one without the description having to be per-finding.
                yield return $"{typePair.Newer.Name} ({olderInterface})";
            }
        }
    }
}
