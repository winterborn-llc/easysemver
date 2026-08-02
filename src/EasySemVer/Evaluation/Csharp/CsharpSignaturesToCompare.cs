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
        this.ClassHistory = this.GetClassesInBoth();
    }

    private ICsharpClassHistory[] GetClassesInBoth()
    {
        var history = new List<ICsharpClassHistory>();
        foreach (var olderClass in this.Older.Classes)
        {
            var newerClass = FindClass(this.Newer, olderClass.Name);
            if (newerClass == null)
            {
                continue;
            }

            history.Add(new CsharpClassHistory(olderClass, newerClass));
        }

        return history.ToArray();
    }

    internal static ICsharpClass? FindClass(ICsharpProject project, string name)
    {
        foreach (var candidate in project.Classes)
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
