using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R27 - a record's positional parameter list changed, which breaks both its primary constructor
/// and every deconstruction of it.
/// </summary>
public class RecordPositionalParametersChanged : IEvaluateCsharpSignatures
{
    public string Rule => "RecordPositionalParametersChanged";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its positional parameters";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
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

            yield return newer.Name;
        }
    }
}
