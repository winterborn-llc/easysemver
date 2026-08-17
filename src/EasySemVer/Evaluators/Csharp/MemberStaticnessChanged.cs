using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R38 - a member moved between static and instance. Every call site has to be rewritten, in
/// either direction.
/// </summary>
public class MemberStaticnessChanged : IEvaluateCsharpSignatures
{
    public string Rule => "MemberStaticnessChanged";

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

        foreach (var pair in Properties.GetPaired(signatures))
        {
            if (pair.Older.IsStatic == pair.Newer.IsStatic)
            {
                continue;
            }

            yield return $"{pair.DeclaringType.Name}.{pair.Newer.Name}";
        }
    }
}
