using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

internal static class SwiftEnums
{
    /// <summary>Paired types that are enums on both sides; a kind change is S03's business.</summary>
    internal static IEnumerable<(ISwiftEnum Older, ISwiftEnum Newer)> GetPaired(
        ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older is not ISwiftEnum older || typePair.Newer is not ISwiftEnum newer)
            {
                continue;
            }

            yield return (older, newer);
        }
    }

    internal static bool AreAssociatedValuesTheSame(ISwiftEnumCase older, ISwiftEnumCase newer)
    {
        if (older.AssociatedValues.Count != newer.AssociatedValues.Count)
        {
            return false;
        }

        for (var i = 0; i < older.AssociatedValues.Count; i++)
        {
            if (older.AssociatedValues[i].Label != newer.AssociatedValues[i].Label)
            {
                return false;
            }

            if (older.AssociatedValues[i].Type != newer.AssociatedValues[i].Type)
            {
                return false;
            }
        }

        return true;
    }
}
