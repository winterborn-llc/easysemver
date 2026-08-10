using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R36 - a member lost virtual or abstract, or gained abstract or sealed. Losing virtual breaks
/// existing overrides; gaining abstract breaks existing subclasses that did not override.
/// </summary>
public class MemberOverridabilityReduced : IEvaluateCsharpSignatures
{
    public string Rule => "MemberOverridabilityReduced";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed what subclasses may override";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            if (!IsOverridabilityReduced(overloadPair))
            {
                continue;
            }

            yield return overloadPair.Symbol;
        }
    }

    private static bool IsOverridabilityReduced(Overloads.OverloadPair overloadPair)
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

        return newer.IsSealed && !older.IsSealed;
    }
}
