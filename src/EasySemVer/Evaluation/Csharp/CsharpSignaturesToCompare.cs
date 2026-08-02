using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluation.Csharp;

/// <inheritdoc cref="ICsharpSignaturesToCompare"/>
internal class CsharpSignaturesToCompare : ICsharpSignaturesToCompare
{
    public ICsharpProject Older { get; }

    public ICsharpProject Newer { get; }

    public ICsharpClassHistory[] ClassHistory { get; }

    public CsharpSignaturesToCompare(ICsharpProject older, ICsharpProject newer)
    {
        this.Older = older;
        this.Newer = newer;
        this.ClassHistory = this.GetTypesInBoth();
    }

    /// <summary>
    /// CLS-02 - pair up the types that exist on both sides before any member rule runs, so a
    /// removed type is never also counted as "everything in it was removed". Pairing is by
    /// (name, kind): a struct that became a class is not the same type (R03's Swift twin S03).
    /// </summary>
    private ICsharpClassHistory[] GetTypesInBoth()
    {
        var history = new List<ICsharpClassHistory>();
        foreach (var olderType in this.Older.Types)
        {
            var newerType = FindType(this.Newer, olderType.Name, olderType.Kind);
            if (newerType == null)
            {
                continue;
            }

            history.Add(new CsharpClassHistory(olderType, newerType));
        }

        return history.ToArray();
    }

    internal static ICsharpType? FindType(ICsharpProject project, string name, string kind)
    {
        foreach (var candidate in project.Types)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            if (candidate.Kind != kind)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    /// <summary>Whether a type of this name exists at all, regardless of kind.</summary>
    internal static ICsharpType? FindTypeOfAnyKind(ICsharpProject project, string name)
    {
        foreach (var candidate in project.Types)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
