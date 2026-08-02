using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R03 - a method's return type changed. Checked per overload as well as per method name, so a
/// change on anything but the first overload can no longer hide (CSX-04, closes G-14).
/// </summary>
public class MethodReturnType : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            var oldType = typePair.Older;
            var newType = typePair.Newer;
            foreach (var oldMethodName in oldType.Methods.Keys)
            {
                if (!newType.Methods.Contains(oldMethodName))
                {
                    continue;
                }

                var oldMethod = oldType.Methods[oldMethodName];
                var newMethod = newType.Methods[oldMethodName];
                if (oldMethod.MethodType != newMethod.MethodType)
                {
                    return true;
                }

                if (DidAnyOverloadChangeReturnType(oldMethod, newMethod))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DidAnyOverloadChangeReturnType(
        ICsharpMethod oldMethod,
        ICsharpMethod newMethod)
    {
        foreach (var oldOverride in oldMethod.Overrides)
        {
            var newOverride = Overloads.FindMatch(oldOverride, newMethod);
            if (newOverride == null)
            {
                continue;
            }

            if (newOverride.ReturnType == oldOverride.ReturnType)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
