using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R36 - a member lost virtual or abstract, or gained abstract or sealed. Losing virtual breaks
/// existing overrides; gaining abstract breaks existing subclasses that did not override.
/// </summary>
public class MemberOverridabilityReduced : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            var older = overloadPair.Older;
            var newer = overloadPair.Newer;
            if (older.IsVirtual && !newer.IsVirtual)
            {
                return true;
            }

            if (older.IsAbstract && !newer.IsAbstract)
            {
                return true;
            }

            if (newer.IsAbstract && !older.IsAbstract)
            {
                return true;
            }

            if (newer.IsSealed && !older.IsSealed)
            {
                return true;
            }
        }

        return false;
    }
}
