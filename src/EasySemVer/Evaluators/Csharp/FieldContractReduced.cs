using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R28 - a public field was removed, retyped, or gained readonly. All three break a caller that
/// was reading or assigning it.
/// </summary>
public class FieldContractReduced : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderField in typePair.Older.Fields)
            {
                var newerField = Fields.Find(typePair.Newer, olderField.Name);
                if (newerField == null)
                {
                    return true;
                }

                if (newerField.Type != olderField.Type)
                {
                    return true;
                }

                if (newerField.IsReadOnly && !olderField.IsReadOnly)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
