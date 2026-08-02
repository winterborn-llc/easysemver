using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R27 - a record's positional parameter list changed, which breaks both its primary constructor
/// and every deconstruction of it.
/// </summary>
public class RecordPositionalParametersChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Record)
            {
                continue;
            }

            var older = (ICsharpRecord)typePair.Older;
            var newer = (ICsharpRecord)typePair.Newer;
            if (ParameterLists.AreTheSame(older.PositionalParameters, newer.PositionalParameters))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
