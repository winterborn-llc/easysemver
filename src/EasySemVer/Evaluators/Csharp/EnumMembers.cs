using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>Shared lookups for the enum rules R22-R25.</summary>
internal static class EnumMembers
{
    internal static IEnumerable<ICsharpClassHistory> GetPairedEnums(
        ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Enum)
            {
                continue;
            }

            yield return typePair;
        }
    }

    internal static ICsharpEnumMember? Find(ICsharpEnum enumeration, string name)
    {
        foreach (var member in enumeration.Members)
        {
            if (member.Name != name)
            {
                continue;
            }

            return member;
        }

        return null;
    }
}
