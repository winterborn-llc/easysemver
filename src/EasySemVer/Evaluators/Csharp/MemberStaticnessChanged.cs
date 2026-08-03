using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R38 - a member moved between static and instance. Every call site has to be rewritten, in
/// either direction.
/// </summary>
public class MemberStaticnessChanged : IEvaluateCsharpSignatures
{
    public string RuleId => "R38";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "moved between static and instance";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            if (overloadPair.Older.IsStatic == overloadPair.Newer.IsStatic)
            {
                continue;
            }

            yield return overloadPair.Symbol;
        }

        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var name in typePair.Older.Properties.Keys)
            {
                if (!typePair.Newer.Properties.Contains(name))
                {
                    continue;
                }

                if (typePair.Older.Properties[name].IsStatic ==
                    typePair.Newer.Properties[name].IsStatic)
                {
                    continue;
                }

                yield return $"{typePair.Newer.Name}.{name}";
            }
        }
    }
}
