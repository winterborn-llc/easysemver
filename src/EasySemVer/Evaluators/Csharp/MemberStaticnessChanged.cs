using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R38 - a member moved between static and instance. Every call site has to be rewritten, in
/// either direction.
/// </summary>
public class MemberStaticnessChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            if (overloadPair.Older.IsStatic == overloadPair.Newer.IsStatic)
            {
                continue;
            }

            return true;
        }

        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var name in typePair.Older.Properties.Keys)
            {
                if (!typePair.Newer.Properties.Contains(name))
                {
                    continue;
                }

                if (typePair.Older.Properties[name].IsStatic ==
                    typePair.Newer.Properties[name].IsStatic)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
