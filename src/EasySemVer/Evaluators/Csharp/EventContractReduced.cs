using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R30 - a public event was removed or its handler type changed.</summary>
public class EventContractReduced : IEvaluateCsharpSignatures
{
    public string RuleId => "R30";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed or changed its handler type";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderEvent in typePair.Older.Events)
            {
                var symbol = $"{typePair.Older.Name}.{olderEvent.Name}";
                var newerEvent = Events.Find(typePair.Newer, olderEvent.Name);
                if (newerEvent == null)
                {
                    yield return symbol;
                    continue;
                }

                if (newerEvent.HandlerType != olderEvent.HandlerType)
                {
                    yield return symbol;
                }
            }
        }
    }
}
