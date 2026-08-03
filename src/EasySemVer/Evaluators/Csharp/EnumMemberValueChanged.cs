using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R24 - a member kept its name but changed value. Anything already compiled against the old
/// constant, or persisted using it, is now wrong.
/// </summary>
public class EnumMemberValueChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its value";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in EnumMembers.GetPairedEnums(signatures))
        {
            var older = (ICsharpEnum)typePair.Older;
            var newer = (ICsharpEnum)typePair.Newer;
            foreach (var olderMember in older.Members)
            {
                var newerMember = EnumMembers.Find(newer, olderMember.Name);
                if (newerMember == null)
                {
                    continue;
                }

                if (newerMember.Value == olderMember.Value)
                {
                    continue;
                }

                yield return $"{newer.Name}.{olderMember.Name}";
            }
        }
    }
}
