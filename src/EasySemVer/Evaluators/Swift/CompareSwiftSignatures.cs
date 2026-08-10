using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>
/// The Swift rule list and its aggregation, over one paired module at a time (NCL-03). Rules
/// overlap on purpose - a renamed method fires both S16 and S17 - because Major wins anyway and
/// making them mutually exclusive would cost more clarity than it buys (SCL-02).
/// </summary>
internal static class CompareSwiftSignatures
{
    private static readonly IEvaluateSwiftSignatures[] Evaluators =
    [
        new TypeRemoved(),
        new TypeAdded(),
        new TypeKindChanged(),
        new ClassSubclassingWithdrawn(),
        new ClassSubclassingOffered(),
        new ClassMadeFinal(),
        new ClassFinalRemoved(),
        new SuperclassChanged(),
        new ConformanceRemoved(),
        new ConformanceAdded(),
        new GenericParameterCountChanged(),
        new GenericConstraintTightened(),
        new GenericConstraintLoosened(),
        new FrozenRemoved(),
        new FrozenAdded(),
        new MemberRemoved(),
        new MemberAdded(),
        new EnumCaseAdded(),
        new EnumCaseChanged(),
        new ProtocolRequirementAdded(),
        new ProtocolRequirementAddedWithDefault(),
        new FunctionSignatureChanged(),
        new EffectAdded(),
        new EffectRemoved(),
        new DeclarationWithdrawn(),
        new DeclarationDeprecated(),
        new ObjCExposureRemoved(),
        new ObjCExposureAdded(),
        new MutatingAdded(),
        new MutatingRemoved(),
        new DefaultArgumentRemoved(),
        new DefaultArgumentAdded(),
        new ParameterModifierChanged(),
        new MemberStaticnessChanged(),
        new PropertySetterRemoved(),
        new PropertySetterAdded(),
        new PropertyTypeChanged(),
        new OperatorChanged()
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
                    LanguageId = unit.LanguageId,
                    UnitId = unit.UnitId,
                    Rule = "NoComparableBaseline",
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
                    LanguageId = unit.LanguageId,
                    UnitId = unit.UnitId,
                    Rule = evaluator.Rule,
                    Symbol = symbol,
                    Description = evaluator.ChangeDescription,
                    Impact = evaluator.EvaluationImpact
                });
            }
        }

        return findings;
    }
}
