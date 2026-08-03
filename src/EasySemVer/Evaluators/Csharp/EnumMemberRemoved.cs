using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R22 - an enum member was removed or renamed. Either way callers stop compiling.</summary>
public class EnumMemberRemoved : IEvaluateCsharpSignatures
{
    public string RuleId => "R22";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in EnumMembers.GetPairedEnums(signatures))
        {
            var older = (ICsharpEnum)typePair.Older;
            var newer = (ICsharpEnum)typePair.Newer;
            foreach (var olderMember in older.Members)
            {
                if (EnumMembers.Find(newer, olderMember.Name) != null)
                {
                    continue;
                }

                yield return $"{older.Name}.{olderMember.Name}";
            }
        }
    }
}
