using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces;
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

    internal static IReadOnlyList<ChangeFinding> GetFindings(
        IPackageableUnit unit,
        ISwiftModule? older,
        ISwiftModule? newer)
    {
        // CLS-04: fail safe towards additive. There is a unit to name here, so the fail-safe is
        // reported as an ordinary finding rather than as an unexplained verdict.
        if (older is null || newer is null)
        {
            return
            [
                new ChangeFinding
                {
                    Language = unit.Language,
                    UnitId = unit.UnitId,
                    RuleName = nameof(CompareSwiftSignatures),
                    RuleId = "CLS-04",
                    Symbol = unit.UnitId,
                    Description = "has no comparable baseline signature, so it is treated as additive",
                    Impact = VersionType.Minor
                }
            ];
        }

        var signatures = new SwiftSignaturesToCompare(older, newer);
        var findings = new List<ChangeFinding>();
        foreach (var evaluator in Evaluators)
        {
            foreach (var symbol in evaluator.FindDifferences(signatures))
            {
                findings.Add(new ChangeFinding
                {
                    Language = unit.Language,
                    UnitId = unit.UnitId,
                    RuleName = evaluator.GetType().Name,
                    RuleId = evaluator.RuleId,
                    Symbol = symbol,
                    Description = evaluator.ChangeDescription,
                    Impact = evaluator.EvaluationImpact
                });
            }
        }

        return findings;
    }
}
