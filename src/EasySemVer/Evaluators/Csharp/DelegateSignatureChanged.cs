using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R26 - a delegate's parameters or return type changed.</summary>
public class DelegateSignatureChanged : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            if (typePair.Newer.Kind != CsharpTypeKinds.Delegate)
            {
                continue;
            }

            var older = (ICsharpDelegate)typePair.Older;
            var newer = (ICsharpDelegate)typePair.Newer;
            if (older.ReturnType != newer.ReturnType)
            {
                return true;
            }

            if (!ParameterLists.AreTheSame(older.Parameters, newer.Parameters))
            {
                return true;
            }
        }

        return false;
    }
}
