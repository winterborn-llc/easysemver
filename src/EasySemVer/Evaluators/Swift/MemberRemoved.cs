using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S16 - a public member of an existing type is gone.</summary>
public class MemberRemoved : IEvaluateSwiftSignatures
{
    public string Rule => "MemberRemoved";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var olderMember in SwiftMembers.GetAll(typePair.Older))
            {
                if (SwiftMembers.Find(typePair.Newer, olderMember) != null)
                {
                    continue;
                }

                yield return olderMember.Name;
            }
        }
    }
}
