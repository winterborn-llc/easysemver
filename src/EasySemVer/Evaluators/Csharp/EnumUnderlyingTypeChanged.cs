using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R25 - an enum's underlying type changed, breaking every cast and every layout.</summary>
public class EnumUnderlyingTypeChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its underlying type";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in EnumMembers.GetPairedEnums(signatures))
        {
            var older = (ICsharpEnum)typePair.Older;
            var newer = (ICsharpEnum)typePair.Newer;
            if (older.UnderlyingType == newer.UnderlyingType)
            {
                continue;
            }

            yield return newer.Name;
        }
    }
}
