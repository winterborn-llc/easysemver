using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R18 - a public interface, struct, record, enum or delegate is gone. Classes are R06's
/// concern; this rule covers everything G-15 used to make invisible.
/// A type that changed kind counts here too, because pairing is by (name, kind).
/// </summary>
public class TypeRemoved : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var olderType in signatures.Older.Types)
        {
            if (olderType.Kind == CsharpTypeKinds.Class)
            {
                continue;
            }

            var newerType = CsharpSignaturesToCompare.FindType(
                signatures.Newer, olderType.Name, olderType.Kind);
            if (newerType != null)
            {
                continue;
            }

            yield return olderType.Name;
        }
    }
}
