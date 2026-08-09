using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R03 - a method's return type changed. Checked per overload as well as per method name, so a
/// change on anything but the first overload can no longer hide (CSX-04, closes G-14).
/// </summary>
public class MethodReturnType : IEvaluateCsharpSignatures
{
    public string RuleId => "R03";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its return type";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
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

                // The method-level type stands in for every overload, so reporting it as well as
                // the overload that changed would say the same thing twice.
                if (oldMethod.MethodType != newMethod.MethodType)
                {
                    yield return $"{newType.Name}.{oldMethodName}";
                    continue;
                }

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

                    yield return
                        $"{newType.Name}.{oldMethodName}({oldOverride.GetMethodSignature()})";
                }
            }
        }
    }
}
