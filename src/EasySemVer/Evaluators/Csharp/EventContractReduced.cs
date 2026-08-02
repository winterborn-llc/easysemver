using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R30 - a public event was removed or its handler type changed.</summary>
public class EventContractReduced : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderEvent in typePair.Older.Events)
            {
                var newerEvent = Events.Find(typePair.Newer, olderEvent.Name);
                if (newerEvent == null)
                {
                    return true;
                }

                if (newerEvent.HandlerType != olderEvent.HandlerType)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
