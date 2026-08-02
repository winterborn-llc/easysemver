using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S17 - a public member was added to an existing type.</summary>
public class SwiftMemberAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            foreach (var newerMember in SwiftMembers.GetAll(typePair.Newer))
            {
                if (SwiftMembers.Find(typePair.Older, newerMember) != null)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
