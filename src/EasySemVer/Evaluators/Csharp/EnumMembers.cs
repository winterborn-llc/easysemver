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

    /// <summary>
    /// Every member present on both sides of a paired enum, in the older side's declared order
    /// (RUL-09). <c>DeclaringEnum</c> is the newer one, for the same reason
    /// <see cref="Properties.GetPaired"/> hands back the newer type.
    /// </summary>
    internal static IEnumerable<(ICsharpEnum DeclaringEnum, ICsharpEnumMember Older, ICsharpEnumMember Newer)>
        GetPairedMembers(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in GetPairedEnums(signatures))
        {
            var older = (ICsharpEnum)typePair.Older;
            var newer = (ICsharpEnum)typePair.Newer;
            foreach (var olderMember in older.Members)
            {
                var newerMember = Find(newer, olderMember.Name);
                if (newerMember == null)
                {
                    continue;
                }

                yield return (newer, olderMember, newerMember);
            }
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
