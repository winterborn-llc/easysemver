using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluation.Swift;

/// <inheritdoc cref="ISwiftSignaturesToCompare"/>
internal class SwiftSignaturesToCompare : ISwiftSignaturesToCompare
{
    public ISwiftModule Older { get; }

    public ISwiftModule Newer { get; }

    public ISwiftTypeHistory[] TypeHistory { get; }

    public SwiftSignaturesToCompare(ISwiftModule older, ISwiftModule newer)
    {
        this.Older = older;
        this.Newer = newer;
        this.TypeHistory = this.GetTypesInBoth();
    }

    /// <summary>
    /// Types are paired by name alone, not by (name, kind): a struct that became a class is the
    /// same declaration with a different kind, and S03 is the rule that says so.
    /// </summary>
    private ISwiftTypeHistory[] GetTypesInBoth()
    {
        var history = new List<ISwiftTypeHistory>();
        foreach (var olderType in this.Older.Types)
        {
            var newerType = FindType(this.Newer, olderType.Name);
            if (newerType == null)
            {
                continue;
            }

            history.Add(new SwiftTypeHistory(olderType, newerType));
        }

        return history.ToArray();
    }

    internal static ISwiftType? FindType(ISwiftModule module, string name)
    {
        foreach (var candidate in module.Types)
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
