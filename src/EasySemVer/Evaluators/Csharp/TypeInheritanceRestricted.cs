using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R32 - a type gained sealed, abstract or static, or changed its base class. Each one withdraws
/// something a caller could previously do: derive from it, instantiate it, or rely on the base.
/// </summary>
public class TypeInheritanceRestricted : IEvaluateCsharpSignatures
{
    public string Rule => "TypeInheritanceRestricted";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "restricted what callers may derive from or instantiate";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            // One type is one finding however many of the four ways it tightened, because they
            // all say the same thing about it.
            if (!IsRestricted(typePair))
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }

    private static bool IsRestricted(ICsharpClassHistory typePair)
    {
        if (typePair.Newer.IsSealed && !typePair.Older.IsSealed)
        {
            return true;
        }

        if (typePair.Newer.IsAbstract && !typePair.Older.IsAbstract)
        {
            return true;
        }

        if (typePair.Newer.IsStatic && !typePair.Older.IsStatic)
        {
            return true;
        }

        return typePair.Newer.BaseType != typePair.Older.BaseType;
    }
}
