using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R37 - a parameter's ref, out, in or params modifier changed. The call site has to change with
/// it, so this is breaking in either direction.
/// </summary>
public class ParameterModifierChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var overloadPair in Overloads.GetMatchedOverloads(signatures))
        {
            var older = overloadPair.Older.Parameters;
            var newer = overloadPair.Newer.Parameters;
            for (var i = 0; i < older.Count; i++)
            {
                if (older[i].RefKind != newer[i].RefKind)
                {
                    return true;
                }

                if (older[i].IsParams != newer[i].IsParams)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
