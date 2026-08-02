using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R32 - a type gained sealed, abstract or static, or changed its base class. Each one withdraws
/// something a caller could previously do: derive from it, instantiate it, or rely on the base.
/// </summary>
public class TypeInheritanceRestricted : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
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

            if (typePair.Newer.BaseType != typePair.Older.BaseType)
            {
                return true;
            }
        }

        return false;
    }
}
