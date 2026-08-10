using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R23 - an enum member was added. Unlike Swift's S18 this is Minor: C# has no exhaustiveness
/// requirement on a switch over an enum, so existing callers keep compiling.
/// </summary>
public class EnumMemberAdded : IEvaluateCsharpSignatures
{
    public string Rule => "EnumMemberAdded";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in EnumMembers.GetPairedEnums(signatures))
        {
            var older = (ICsharpEnum)typePair.Older;
            var newer = (ICsharpEnum)typePair.Newer;
            foreach (var newerMember in newer.Members)
            {
                if (EnumMembers.Find(older, newerMember.Name) != null)
                {
                    continue;
                }

                yield return $"{newer.Name}.{newerMember.Name}";
            }
        }
    }
}
