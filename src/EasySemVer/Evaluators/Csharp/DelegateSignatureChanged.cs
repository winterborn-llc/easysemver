using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R26 - a delegate's parameters or return type changed.</summary>
public class DelegateSignatureChanged : IEvaluateCsharpSignatures
{
    public string Rule => "DelegateSignatureChanged";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its signature";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Delegate)
            {
                continue;
            }

            var older = (ICsharpDelegate)typePair.Older;
            var newer = (ICsharpDelegate)typePair.Newer;

            // Return type and parameters are one signature, so a delegate that changed both is
            // still one finding.
            if (older.ReturnType == newer.ReturnType
                && ParameterLists.AreTheSame(older.Parameters, newer.Parameters))
            {
                continue;
            }

            yield return newer.Name;
        }
    }
}
