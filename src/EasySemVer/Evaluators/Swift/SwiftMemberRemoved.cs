using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S16 - a public member of an existing type is gone.</summary>
public class SwiftMemberRemoved : IEvaluateSwiftSignatures
{
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
