# 07 — Change Classification

How (baseline, current) signature pairs map to a SemVer change type. C# rules are R01–R42 here;
Swift rules are S01–S38 in
[12 §13](12-multi-language-swift-and-folder-model.md). Sources:
[`ChangeClassifier.cs`](../src/EasySemVer/Evaluation/ChangeClassifier.cs),
[`Evaluators/`](../src/EasySemVer/Evaluators) (neutral),
[`Evaluators/Csharp/`](../src/EasySemVer/Evaluators/Csharp),
[`Evaluators/Swift/`](../src/EasySemVer/Evaluators/Swift), and their tests in
[`src/Test/Evaluators/`](../src/Test/Evaluators).

## Architecture

**CLS-01 — Rule-object model, per language.** ✅ *(preserved per-language by ML-04)*
Classification SHALL be expressed as independent rule objects. Each language has its own
contract over its own comparison context; there is deliberately no shared base type:

```csharp
// Interfaces/Csharp/IEvaluateCsharpSignatures.cs
VersionType EvaluationImpact { get; }
string ChangeDescription { get; }
IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures);

// Interfaces/Swift/IEvaluateSwiftSignatures.cs
VersionType EvaluationImpact { get; }
string ChangeDescription { get; }
IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures);
```

A rule yields the symbols it fired on rather than a bare yes/no, and carries one phrase saying
what happened to them; empty means the rule did not fire. That is what lets a run report its
reasoning (CLI-08) — a bool cannot name anything. The neutral existence rules have the same
shape over units instead of symbols.

Rules are registered in a static list per language (`CompareSignatures`, `CompareSwiftSignatures`
— 41 and 38 classes today). Adding a detection capability means a rule class + registration + a test
class (TST-01), with no change to any aggregation logic.

**CLS-01a — Neutral unit-existence rules.** ✅ *(§7)*
"A shippable module appeared or disappeared" means the same thing in every language, so it is
classified once, neutrally, by
[`IEvaluateUnitExistence`](../src/EasySemVer/Interfaces/IEvaluateUnitExistence.cs) rules over the
unit lists: `UnitRemoved` (Major, NCL-01) and `UnitAdded` (Minor, NCL-02).

**CLS-02 — Pairing.** ✅
Before any language rule runs, units SHALL be paired by `(Language, UnitId)` (NCL-03); within a
paired unit, types SHALL be paired by name — for C# by (name, kind). Member-level rules operate
**only on paired types**; added/removed units and types are the existence rules' concern. This
layering prevents double-counting a removed type as also "removing all its members", and means a
language's rules never see a unit that only exists on one side.
ℹ️ In C# the paired-type collection is still called `ClassHistory`, but it pairs types of every
kind, so R01/R10/R15/R16 cover interfaces, structs and records without being restated.

**CLS-03 — Aggregation.** ✅ *(generalized by ML-05)*
The result SHALL be the highest impact across the neutral existence rules **and** every language
provider's findings — `Major > Minor > Patch` — with **Patch as the default**. Every rule is
evaluated; evaluation order must not affect the outcome. Because there is one version per folder
(OVR-02/ML-06), a Swift-only change moves the C# projects' versions too, and vice versa.
Classification produces findings and nothing else: rendering them is a formatter's job, so a
second output format is a second formatter over the same findings and never a second traversal.

**CLS-04 — Defensive null handling.** ✅
If either side is `null`, the result SHALL be **Minor** (fail-safe: assume additive). In practice
a missing baseline is an *empty* (not null) unit list, and first runs classify Minor via NCL-02
(see PER-03). Where the fail-safe has a unit to name — a baseline entry with no signature for its
language — it is reported as an ordinary finding against that unit rather than as an unexplained
verdict; only the whole-run case has nothing to name.

## The rules

| Rule | Was | Fires when… | Impact |
|------|-----|-------------|:------:|
| MethodsContinueToExist | R01 | a method name present in the old paired class is absent from the new one | **Major** |
| MethodInputParameterOverrideRemoved | R02 | an old overload has no new overload with the same parameter count, names, and types, in order (requiredness deliberately ignored — a requiredness-only change is not a *removed* overload; direction-sensitive handling is R17's job) | **Major** |
| MethodReturnType | R03 | a paired method name's recorded return type differs | **Major** |
| MethodOverrideAdded | R04 | a method that exists on both sides gains an overload whose canonical signature string (SIG-09, requiredness included) is absent from the old side | **Minor** |
| ProjectClassAdded | R05 | a paired project contains a class name absent from the old side | **Minor** |
| ProjectClassesContinueToExist | R06 | a class in an old paired project is absent from the new side | **Major** |
| ~~R07~~ | *retired* | ~~a project in the old signature is absent from the new one~~ — re-homed to C#'s own **UnitRemoved**, which covers every language. The ID is retired, not reused. | — |
| PropertyEditabilityEnhanced | R08 | a paired property goes not-writable → writable | **Minor** |
| PropertyEditabilityReduced | R09 | a paired property goes writable → not-writable | **Major** |
| PropertiesContinueToExist | R10 | a property name in an old paired class is absent from the new one | **Major** |
| PropertyReadabilityEnhanced | R11 | a paired property goes not-readable → readable | **Minor** |
| PropertyReadabilityReduced | R12 | a paired property goes readable → not-readable | **Major** |
| PropertyType | R13 | a paired property's type name differs | **Major** |
| ~~R14~~ | *retired* | ~~the new signature contains a project name absent from the old one~~ — re-homed to C#'s own **UnitAdded**. The ID is retired, not reused. | — |
| MethodAdded | R15 | a paired class contains a method name absent from the old side | **Minor** |
| PropertyAdded | R16 | a paired class contains a property name absent from the old side | **Minor** |
| MethodInputParameterMadeRequired | R17 | for overloads matched on parameter count, names, and types (R02's matcher), a parameter is `IsRequired = false` in the old side and `IsRequired = true` in the new one | **Major** |

R15/R16 are the additive complements of R01/R10 and, per CLS-02, only inspect `ClassHistory`
pairs — members of a brand-new class are R05's concern, not theirs.
R17 is deliberately **directional**: only optional→required fires. The reverse
(required→optional) is non-breaking and stays Minor via R04.

### R18–R42 — the fidelity expansion (CSX-05)

Added when the C# model grew to the full topology; each is a class in
[`Evaluators/Csharp/`](../src/EasySemVer/Evaluators/Csharp) with its own test class.

| Rule | Was | Fires when… | Impact |
|------|-----|-------------|:------:|
| TypeRemoved | R18 | a public interface / struct / record / enum / delegate is removed (classes are R06) | **Major** |
| TypeAdded | R19 | a public interface / struct / record / enum / delegate is added (classes are R05) | **Minor** |
| InterfaceRequirementAdded | R20 | an interface gains a requirement with **no** default implementation | **Major** |
| InterfaceRequirementAddedWithDefault | R21 | an interface gains a requirement **with** a default implementation | **Minor** |
| EnumMemberRemoved | R22 | an enum member is removed or renamed | **Major** |
| EnumMemberAdded | R23 | an enum member is added | **Minor** |
| EnumMemberValueChanged | R24 | an enum member's explicit value changes | **Major** |
| EnumUnderlyingTypeChanged | R25 | an enum's underlying type changes | **Major** |
| DelegateSignatureChanged | R26 | a delegate's parameters or return type change | **Major** |
| RecordPositionalParametersChanged | R27 | a record's positional parameter list changes | **Major** |
| FieldContractReduced | R28 | a public field is removed, retyped, or gains `readonly` | **Major** |
| FieldAdded | R29 | a public field is added | **Minor** |
| EventContractReduced | R30 | a public event is removed or its handler type changes | **Major** |
| EventAdded | R31 | a public event is added | **Minor** |
| TypeInheritanceRestricted | R32 | a type gains `sealed`, `abstract` or `static`, or changes base class | **Major** |
| TypeInheritanceRelaxed | R33 | a type loses `sealed` or `abstract` | **Minor** |
| ImplementedInterfaceRemoved | R34 | an implemented interface is removed from a public type | **Major** |
| ImplementedInterfaceAdded | R35 | an implemented interface is added to a public type | **Minor** |
| MemberOverridabilityReduced | R36 | a member loses `virtual`/`abstract`, or gains `abstract`/`sealed` | **Major** |
| ParameterModifierChanged | R37 | a parameter's `ref`/`out`/`in`/`params` modifier changes | **Major** |
| MemberStaticnessChanged | R38 | a member's static-vs-instance-ness changes | **Major** |
| GenericConstraintTightened | R39 | generic parameter count changes, or a constraint is added/tightened | **Major** |
| GenericConstraintLoosened | R40 | a generic constraint is removed or loosened | **Minor** |
| NestedTypeRemoved | R41 | a public nested type is removed | **Major** |
| NestedTypeAdded | R41 | a public nested type is added | **Minor** |
| PropertySetterBecameInitOnly | R42 | a property's `set` accessor changes to `init` | **Major** |
| UnitRemoved | R07 | a project the baseline recorded is absent from this run | **Major** |
| UnitAdded | R14 | a project exists this run that the baseline never saw | **Minor** |

ℹ️ R41 was one id shared by two classes, one per direction. Now that a rule is named rather than
numbered they are simply two rules, and the shared-id exception is gone.

All 43 live rule classes are covered by dedicated test classes asserting impact, a no-change negative
case, and a positive case; directional pairs additionally assert the non-firing direction
([11-testing.md](11-testing.md)).

## Scenario → outcome matrix

The observable contract, as produced by the rules above. ⚠️ rows are where the current rule
set diverges from SemVer intent (details in [99-known-gaps.md](99-known-gaps.md)).

| Scenario | Outcome | Via |
|----------|:-------:|-----|
| First run (no baseline) | Minor | NCL-02 |
| Nothing changed / implementation-only change | Patch | default |
| Unit added (any language) | Minor | NCL-02 |
| Unit removed or renamed (any language) | Major | NCL-01 (+NCL-02 on rename) |
| Class added to existing project | Minor | R05 |
| Class removed, renamed, or moved to another namespace | Major | R06 (+R05) |
| New method added to existing class | Minor | R15 |
| New property added to existing class | Minor | R16 |
| Overload added to an existing method | Minor | R04 |
| Method removed (all overloads) | Major | R01 |
| One overload removed | Major | R02 |
| Parameter added/removed/renamed/retyped on an existing overload | Major | R02 (+R04) |
| Return type changed, on any overload | Major | R03 (G-14 resolved by CSX-04) |
| Optional parameter made required | Major | R17 (R04 also fires; Major wins) |
| Required parameter made optional | Minor | R04 |
| Property type changed | Major | R13 |
| Property setter removed | Major | R09 |
| Property setter added | Minor | R08 |
| Property `set` changed to `init` | Major | R42 (G-15's last ⚠️ row, closed by CSX-03) |
| Property getter removed / added | Major / Minor | R12 / R11 |
| Public interface / struct / record / enum / delegate removed / added | Major / Minor | R18 / R19 |
| Enum member removed, renamed or revalued / added | Major / Minor | R22, R24 / R23 |
| Public field or event removed or retyped / added | Major / Minor | R28, R30 / R29, R31 |
| Nested type removed / added | Major / Minor | R41 |
| Type sealed, made abstract, or rebased | Major | R32 |
| Interface implementation dropped / added | Major / Minor | R34 / R35 |

**CLS-05 — Additions to existing classes SHALL be Minor.** ✅
Per SemVer, adding new public functionality (a brand-new method or property on an existing
class) is a Minor change. R15 (`MethodAdded`) and R16 (`PropertyAdded`) detect this as the
additive complements of R01/R10 — R04 could not, since it requires the method name to already
exist, nor could R08/R11, which require the property to already exist. (Was gap **G-07**.)

**CLS-06 — Tightening a contract SHALL be Major.** ✅
Making an optional parameter required is breaking and classifies Major via R17
(`MethodInputParameterMadeRequired`). R02's matcher is intentionally left requiredness-blind:
folding `IsRequired` into it would classify the non-breaking reverse (required→optional) as a
removed overload, i.e. Major. R17 instead reuses R02's count/names/types matcher and fires
only in the breaking direction, so required→optional stays Minor via R04.
(Was gap **G-08**.)

**CLS-07 — Classification depends only on its inputs.** ✅ *(REN-04; G-13 resolved)*
`CsharpSignaturesToCompare` carries exactly `(ICsharpProject older, ICsharpProject newer)` plus
that unit's paired-type history. It knows nothing about file paths, unit enumeration or saving,
and cannot re-discover anything; the working-directory fallback that made that possible is gone,
along with the duplicate I/O. `SwiftSignaturesToCompare` has the same shape.

**CLS-08 — Overlapping rules are fine.** ℹ️ *(SCL-02)*
A renamed member fires both the removal and the addition rule; Major wins per CLS-03. Rules SHALL
NOT be made mutually exclusive at the cost of clarity — aggregation already handles it.
