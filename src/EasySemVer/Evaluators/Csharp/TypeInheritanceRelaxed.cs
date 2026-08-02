using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R33 - a type lost sealed or abstract, which only widens what callers may do.</summary>
public class TypeInheritanceRelaxed : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Older.IsSealed && !typePair.Newer.IsSealed)
            {
                return true;
            }

            if (typePair.Older.IsAbstract && !typePair.Newer.IsAbstract)
            {
                return true;
            }
        }

        return false;
    }
}
