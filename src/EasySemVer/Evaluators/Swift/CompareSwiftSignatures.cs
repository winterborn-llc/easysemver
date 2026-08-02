using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>
/// The Swift rule list and its aggregation, over one paired module at a time (NCL-03). Rules
/// overlap on purpose - a renamed method fires both S16 and S17 - because Major wins anyway and
/// making them mutually exclusive would cost more clarity than it buys (SCL-02).
/// </summary>
internal static class CompareSwiftSignatures
{
    private static readonly IEvaluateSwiftSignatures[] Evaluators =
    [
        new SwiftTypeRemoved(),
        new SwiftTypeAdded(),
        new SwiftTypeKindChanged(),
        new SwiftClassSubclassingWithdrawn(),
        new SwiftClassSubclassingOffered(),
        new SwiftClassMadeFinal(),
        new SwiftClassFinalRemoved(),
        new SwiftSuperclassChanged(),
        new SwiftConformanceRemoved(),
        new SwiftConformanceAdded(),
        new SwiftGenericParameterCountChanged(),
        new SwiftGenericConstraintTightened(),
        new SwiftGenericConstraintLoosened(),
        new SwiftFrozenRemoved(),
        new SwiftFrozenAdded(),
        new SwiftMemberRemoved(),
        new SwiftMemberAdded(),
        new SwiftEnumCaseAdded(),
        new SwiftEnumCaseChanged(),
        new SwiftProtocolRequirementAdded(),
        new SwiftProtocolRequirementAddedWithDefault(),
        new SwiftFunctionSignatureChanged(),
        new SwiftEffectAdded(),
        new SwiftEffectRemoved(),
        new SwiftDeclarationWithdrawn(),
        new SwiftDeclarationDeprecated(),
        new SwiftObjCExposureRemoved(),
        new SwiftObjCExposureAdded(),
        new SwiftMutatingAdded(),
        new SwiftMutatingRemoved(),
        new SwiftDefaultArgumentRemoved(),
        new SwiftDefaultArgumentAdded(),
        new SwiftParameterModifierChanged(),
        new SwiftMemberStaticnessChanged(),
        new SwiftPropertySetterRemoved(),
        new SwiftPropertySetterAdded(),
        new SwiftPropertyTypeChanged(),
        new SwiftOperatorChanged()
    ];

    internal static VersionType GetChangeType(
        string unitId,
        ISwiftModule? older,
        ISwiftModule? newer)
    {
        if (older is null || newer is null)
        {
            return VersionType.Minor;
        }

        var signatures = new SwiftSignaturesToCompare(older, newer);
        var changeType = VersionType.Patch;
        foreach (var evaluator in Evaluators)
        {
            if (!evaluator.AreDifferencesPresent(signatures))
            {
                continue;
            }

            Log.WriteLine($"{evaluator.GetType().Name}: {evaluator.EvaluationImpact} in {unitId}");
            changeType = changeType.GetHigherImpact(evaluator.EvaluationImpact);
        }

        return changeType;
    }
}
