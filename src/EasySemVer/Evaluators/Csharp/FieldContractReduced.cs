using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R28 - a public field was removed, retyped, or gained readonly. All three break a caller that
/// was reading or assigning it.
/// </summary>
public class FieldContractReduced : IEvaluateCsharpSignatures
{
    public string RuleId => "R28";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed, retyped, or made readonly";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderField in typePair.Older.Fields)
            {
                var symbol = $"{typePair.Older.Name}.{olderField.Name}";
                var newerField = Fields.Find(typePair.Newer, olderField.Name);
                if (newerField == null)
                {
                    yield return symbol;
                    continue;
                }

                if (newerField.Type != olderField.Type)
                {
                    yield return symbol;
                    continue;
                }

                if (newerField.IsReadOnly && !olderField.IsReadOnly)
                {
                    yield return symbol;
                }
            }
        }
    }
}
