# 13 — Shared Rule Bases

**Status: implemented and complete, 2026-08-17 — as two pairing helpers, not as a base class.** §7
is built and green (§9), and every step in §8 is done.

**How it got here: census complete, base class withdrawn by its own criterion.** RUL-10 required a full
reading of all 79 registered rules before any base was written, and specified withdrawal if the
identity-diff family came back under roughly a third of the rule set. It came back at **29% raw,
19% effective** (§6). RUL-01…RUL-08 are therefore **withdrawn, 2026-08-17**, and kept below with
their reasoning intact rather than deleted, per this folder's convention.

What the census found instead is that the duplication is real but sits somewhere else — in the
**pairing traversal**, not the diff loop. RUL-09, written here as a mere prerequisite, turns out to
be the whole opportunity, and §7 promotes it. It needs no generic base, no inheritance, and comes
nowhere near ML-01.

This remains a **behaviour-preserving** change. Every rule keeps its name, its impact, its symbols
and its tests. If any test changes, the refactor is wrong.

## 0. What changes

| Aspect | Today | After this work |
|--------|-------|-----------------|
| C# member pairing | hand-rolled inline in each rule that needs it, ~8 lines a time | `Properties.GetPaired` / `Fields.GetPaired`, matching Swift's existing shape |
| C# facet rules | 15–20 lines, most of it pairing | 6 lines, like Swift's |
| ~~The "is it on the other side?" loop~~ | ~~hand-written per rule~~ | ~~one generic base~~ — **withdrawn, §6** |
| Rule interfaces | `IEvaluateCsharpSignatures`, `IEvaluateSwiftSignatures`, unrelated | **unchanged** |
| Rule count, names, impacts, symbols | 41 C# + 38 Swift | identical |

## 1. Why *(original motivation — retained)*

**Rule bodies are the part of a language provider that scales worst.** A provider is written once;
its rules are written once *per rule*, and the rule count does not fall as languages are added. At
the language set contemplated in the multi-language triage — C#, VB.NET, Swift, Objective-C,
TypeScript, Java, Kotlin, Dart, Go, Rust, PHP, Python — a 40-rule average is roughly **500 rule
classes and, under TST-01, 500 test classes**. That is the maintenance cliff, and it arrives long
before any individual parser becomes the bottleneck.

That premise survives the census unchanged. What did not survive is the assumption about *which*
duplication the cliff is made of.

> **Qualified, 2026-08-17 (census).** Reading 14 rules suggested the repeated thing was the
> key-matched lookup. Reading all 79 shows it is the traversal that produces the pairs the lookup
> runs over — a different problem, with a much duller solution. See §6 and §7.

## 2. What may be shared, and what may not

> **Withdrawn, 2026-08-17 (census).** RUL-01 and RUL-02 are retained in full because the ML-01
> boundary they draw is correct and will be needed again the next time a shared base is proposed.
> Only their application to an identity-diff base is withdrawn.

**RUL-01 — The shared thing is the set operation, never the concept.** ⚠️ *withdrawn as applied*
A shared base SHALL abstract only the *mechanics of a difference* — pair a collection against
another by a key, yield what has no counterpart. It SHALL NOT name, model, or constrain what the
things being diffed are.

This is the ML-01 line, and the test for it is mechanical: **a base's type parameters carry no
constraint**. There is no `where TEntity : IHasName`, no `IDeclaration`, no `IMember`. The base
cannot ask an entity for its name, its kind, its visibility, or its members, because a base that
could would be the cross-language abstraction of "member" that ML-01 forbids.

ℹ️ Retained as the standing test for any future proposal. A base whose type parameter carries a
constraint is refused on sight; the argument does not need re-running.

**RUL-02 — The bases are outside both rule hierarchies.** ⚠️ *withdrawn as applied*
`IEvaluateCsharpSignatures` and `IEvaluateSwiftSignatures` SHALL be unchanged, SHALL gain no base
type, and SHALL remain unrelated to each other.

ℹ️ Now satisfied trivially: §7 introduces no base at all. CLS-01's "there is deliberately no shared
base type" stands as written, and doc 07 needs no amendment.

**RUL-03 — Scoping stays in the rule.** ✅ **required — survives, and is now the point**
A shared helper SHALL receive **two collections** and nothing else. Deciding *which* collections —
the unit's own types, the paired types' fields, the paired enums' cases, the paired functions —
SHALL remain in the rule.

> **Reinterpreted, 2026-08-17 (census).** Written as a constraint on the base, this is the census's
> central finding read backwards. If scoping cannot be shared and scoping is where the duplication
> lives, then the base was solving the smaller half of the problem. §7 shares the scoping *within* a
> language, where ML-01 has no opinion, and shares nothing across languages.

Symbols are qualified differently per rule — `TypeAdded` yields `Widget`, `FieldAdded` yields
`Widget.Count` — and the qualifier comes from the scope the rule is already standing in. Any helper
therefore yields **entities or pairs**, never formatted symbols, and the rule does the formatting.

**RUL-04 — One base, two directions.** ❌ *withdrawn, 2026-08-17*
**RUL-05 — Impact and description are inheritable defaults.** ❌ *withdrawn, 2026-08-17*

The proposed `IdentityDiffRule<TEntity>` with `FindAdded` / `FindRemoved` wrappers is not built.
§6 shows it would serve 15 rules of 79 at a saving of roughly eight lines each, in exchange for a
generic base class, its test class, and an inheritance relationship in fifteen files.

**RUL-06 — `Rule` stays abstract, in every base, forever.** ✅ **required — retained**
The rule name is half of the published `(language, rule)` key in the JSON report (REP-02) and is
therefore a contract. It SHALL NOT be derived from the class name by any base or helper, at any
point, because doing so would make a class rename silently break a consumer — the reason
[`UnitRemovedRule`](../src/EasySemVer/Evaluators/UnitRemovedRule.cs) already documents for keeping
it abstract there.

ℹ️ Retained independently of everything else in this document. It is a standing constraint on the
two existing neutral bases and on anything added later.

**RUL-07 — Deriving is optional, and not deriving is not a defect.** ✅ **required — retained**
Nothing SHALL require a rule to use a base or a helper. A rule whose diffing does not fit — because
it compares two collections at once, or unions two traversals, or treats several facets as one
finding — implements its language's interface directly and is complete and correct. No test, lint,
or review gate SHALL treat a hand-written `FindDifferences` as a smell.

ℹ️ The census makes this load-bearing rather than defensive: **22 of 79 rules are in family C** and
are expected to stay there permanently.

## 3. Considered and rejected: a facet-change base

**RUL-08 — There SHALL NOT be a base for the directional-facet family.** ✅ **confirmed, for a
different reason than the one originally given**

The original argument was that the facet family's shared part is four lines of loop, of which the
predicate is the rule — measured against `ClassMadeFinal` (six lines of body) and its Swift
siblings.

> **Right answer, wrong evidence, 2026-08-17 (census).** The sampled rules were the *cheap* ones.
> Family B is not small — at **34 of 79 it is the largest family in the codebase**, and C#'s members
> of it run 15–20 lines, not six. The four-line claim held only for Swift, and only because Swift
> already has `GetPairedFunctions` / `GetPairedProperties` while C# hand-rolls the same traversal
> inline.
>
> The conclusion survives intact, and is now better supported: what makes a C# facet rule long is
> not the facet, it is the missing pairing helper. Give C# the helper and its facet rules collapse
> to Swift's six-line shape — at which point a base would once again be saving four lines. **The
> helper captures the whole saving; the base would capture what is left after the helper, which is
> nothing.**

ℹ️ The general principle stands and is worth keeping: **share the part that is easy to get subtly
wrong twice, not the part that is merely typed twice.** The census refines it — before deciding
which part that is, check whether the expensive part is the comparison or the traversal that feeds
it.

## 4. Testing *(RUL-11 revised, RUL-12 unchanged)*

**RUL-11 — Helpers get their own tests; the rules keep theirs.** ✅ required *(revised
2026-08-17: was written for the base class)*
Each new pairing helper SHALL have a dedicated test class covering: both sides empty, one side
empty, a member present on one side only, a member present on both, and two members sharing a name.
Pairing is where an off-by-one or an inverted argument order hides silently, and it is now shared by
a dozen rules apiece.

TST-01 is **unchanged**: every rule keeps its own test class asserting declared impact, a
no-difference negative, and a representative positive. Helpers remove duplicated production code,
not duplicated tests — a rule's facet predicate is exactly what its test exists to pin down, and it
is the part no helper can see.

**RUL-12 — The refactor is proved by the suite not changing.** ✅ required
No test file SHALL be edited as part of adopting a helper in an existing rule. The full unit suite
(509 tests at the doc-12 verification snapshot) SHALL pass unmodified before and after each rule is
migrated. A migration that requires a test edit has changed behaviour and SHALL be reverted.

## 5. *(reserved — was "Prerequisite: C# member pairing", now §7)*

## 6. The census

**RUL-10 — The family membership SHALL be established by reading all 79 rules.** ✅ **done,
2026-08-17**

All 41 C# and 38 Swift registered rules were read from
[`CompareSignatures`](../src/EasySemVer/Evaluators/Csharp/CompareSignatures.cs) and
[`CompareSwiftSignatures`](../src/EasySemVer/Evaluators/Swift/CompareSwiftSignatures.cs). Families:

- **A — identity diff.** Walk one side's collection; yield what has no counterpart on the other by
  key. Nothing about the paired entity is examined.
- **B — paired facet.** Pair by identity first, then compare one facet or value on the pair.
- **C — bespoke.** Multiple traversals unioned, several facets fused into one finding, set-matching
  over overloads, or already delegating to a shared per-language helper.

### C# — 41 rules

| Rule | Family | Note |
|---|:--:|---|
| MethodsContinueToExist | A | keyed list; `Contains` is already O(1) and one line |
| MethodAdded | A | keyed list, as above |
| PropertiesContinueToExist | A | keyed list, as above |
| PropertyAdded | A | keyed list, as above |
| ImplementedInterfaceRemoved | A | `.Contains` over a string list |
| ImplementedInterfaceAdded | A | `.Contains` over a string list |
| TypeRemoved | A | linear `FindType`, key `(Name, Kind)`, filter `Kind != Class` |
| TypeAdded | A | as above |
| ProjectClassesContinueToExist | A | linear `FindType` over `.Classes` |
| ProjectClassAdded | A | as above |
| NestedTypeRemoved | A | linear `FindTypeOfAnyKind`, filter on `DeclaringType` |
| NestedTypeAdded | A | as above |
| EnumMemberRemoved | A | scoped to paired enums, linear `EnumMembers.Find` |
| EnumMemberAdded | A | as above |
| FieldAdded | A | scoped to paired types, linear `Fields.Find` |
| EventAdded | A | scoped to paired types, linear `Events.Find` |
| PropertyEditabilityEnhanced | B | **pairs inline** — ~8 lines before the facet is reached |
| PropertyEditabilityReduced | B | pairs inline |
| PropertyReadabilityEnhanced | B | pairs inline |
| PropertyReadabilityReduced | B | pairs inline |
| PropertyType | B | pairs inline |
| PropertySetterBecameInitOnly | B | pairs inline |
| EnumMemberValueChanged | B | pairs inline via `EnumMembers.Find` |
| EnumUnderlyingTypeChanged | B | over `ClassHistory`, cast to `ICsharpEnum` |
| MemberOverridabilityReduced | B | over `Overloads.GetMatchedOverloads` — helper exists |
| ParameterModifierChanged | B | over `Overloads.GetMatchedOverloads` |
| TypeInheritanceRestricted | B | four ways to tighten, one finding, via a private predicate |
| RecordPositionalParametersChanged | B | kind filter, then composite parameter-list compare |
| DelegateSignatureChanged | B | kind filter, then return type + parameters as one signature |
| MethodInputParameterOverrideRemoved | C | overload-set matching |
| MethodInputParameterMadeRequired | C | overload-set matching plus requiredness |
| MethodReturnType | C | method-level check, then per-overload |
| MethodOverrideAdded | C | overload-signature-set membership |
| FieldContractReduced | C | removal, retype and readonly-gain fused into one rule |
| EventContractReduced | C | removal and handler change fused |
| TypeInheritanceRelaxed | C | two facets, one finding (`continue` after yield) |
| MemberStaticnessChanged | C | unions overloads and properties |
| GenericConstraintTightened | C | unions types and overloads |
| GenericConstraintLoosened | C | unions types and overloads |
| InterfaceRequirementAdded | C | already delegates to `InterfaceRequirements.GetAddedRequirements` |
| InterfaceRequirementAddedWithDefault | C | same helper, opposite flag |

**C#: A 16 · B 13 · C 12.**

### Swift — 38 rules

| Rule | Family | Note |
|---|:--:|---|
| TypeRemoved | A | linear `FindType`, key `Name` |
| TypeAdded | A | as above |
| MemberRemoved | A | scoped to `TypeHistory`, `SwiftMembers.Find` |
| MemberAdded | A | as above |
| EnumCaseAdded | A | scoped to paired enums |
| ConformanceRemoved | A | `.Contains` over a string list |
| ConformanceAdded | A | `.Contains` over a string list |
| TypeKindChanged | B | over `TypeHistory` |
| ClassSubclassingWithdrawn | B | over `TypeHistory` |
| ClassSubclassingOffered | B | over `TypeHistory` |
| ClassMadeFinal | B | over `TypeHistory` |
| ClassFinalRemoved | B | over `TypeHistory` |
| SuperclassChanged | B | over `TypeHistory` |
| FrozenRemoved | B | over `TypeHistory` |
| FrozenAdded | B | over `TypeHistory` |
| MutatingAdded | B | over `GetPairedFunctions` |
| MutatingRemoved | B | over `GetPairedFunctions` |
| FunctionSignatureChanged | B | composite: return type + parameters as one signature |
| PropertySetterRemoved | B | over `GetPairedProperties` |
| PropertySetterAdded | B | over `GetPairedProperties` |
| PropertyTypeChanged | B | over `GetPairedProperties` |
| DeclarationWithdrawn | B | over `GetPairedDeclarations` |
| DeclarationDeprecated | B | over `GetPairedDeclarations` |
| ObjCExposureRemoved | B | over `GetPairedDeclarations` |
| ObjCExposureAdded | B | over `GetPairedDeclarations` |
| DefaultArgumentRemoved | B | nested: `GetPairedFunctions` then `SwiftParameters.GetPaired` |
| DefaultArgumentAdded | B | nested, as above |
| ParameterModifierChanged | B | nested, as above |
| EnumCaseChanged | C | removal, raw value and associated values fused |
| OperatorChanged | C | removal, precedence and kind fused |
| EffectAdded | C | `throws` and `async`, one finding |
| EffectRemoved | C | `throws` and `async`, one finding |
| MemberStaticnessChanged | C | unions functions and properties |
| GenericParameterCountChanged | C | unions types and functions |
| GenericConstraintTightened | C | unions types and functions |
| GenericConstraintLoosened | C | unions types and functions |
| ProtocolRequirementAdded | C | already delegates to `SwiftProtocolRequirements.GetAddedRequirements` |
| ProtocolRequirementAddedWithDefault | C | same helper, opposite flag |

**Swift: A 7 · B 21 · C 10.**

### Verdict

| Family | Count | Share |
|---|--:|--:|
| A — identity diff | 23 | **29%** |
| B — paired facet | 34 | **43%** |
| C — bespoke | 22 | 28% |

**Family A is 29%, below RUL-10's threshold — and the effective figure is worse.** Eight of the 23
(`MethodsContinueToExist`, `MethodAdded`, `PropertiesContinueToExist`, `PropertyAdded`, both
`ImplementedInterface*`, both `Conformance*`) diff a keyed list or a string list where `Contains`
is already O(1) and a single line. A base saves them nothing. That leaves **15 rules, 19%**, where
the base would replace roughly eight lines apiece — about 120 lines, against a generic base class,
a test class, and an inheritance relationship in fifteen files.

**RUL-04/RUL-05 are withdrawn on that number**, per the criterion agreed before it was measured.

**Family B is the finding.** At 34 rules it is the largest family, and it splits cleanly by
language: Swift's 21 are already short because `GetPairedFunctions`, `GetPairedProperties`,
`GetPairedDeclarations` and `SwiftParameters.GetPaired` exist. C#'s 13 are long because seven of
them re-implement the same `foreach (name in Older.X.Keys) { if (!Newer.X.Contains(name)) continue;
… }` traversal inline before they reach the one line that is actually the rule.

ℹ️ The codebase already reached this conclusion twice without stating it.
`InterfaceRequirements.GetAddedRequirements` and `SwiftProtocolRequirements.GetAddedRequirements`
are the same idea — a per-language helper parameterised by a flag, collapsing two rules into two
one-line bodies — arrived at independently in each language. §7 generalises what the code found on
its own.

## 7. C# member pairing — the actual work

**RUL-09 — C# SHALL gain the paired-member helpers Swift already has.** ✅ required *(promoted
from prerequisite to primary deliverable, 2026-08-17)*

C# SHALL gain, in `Evaluators/Csharp/`, matching Swift's `(Older, Newer)` value-tuple shape:

- [`Properties.GetPaired`](../src/EasySemVer/Evaluators/Csharp/Properties.cs) — every property
  present on both sides of a paired type
- [`EnumMembers.GetPairedMembers`](../src/EasySemVer/Evaluators/Csharp/EnumMembers.cs) — the same
  for enum members

Both yield `(DeclaringType, Older, Newer)`, carrying the **newer** type because that is the name
every consumer qualifies its symbol with, and carrying entities rather than a formatted string per
RUL-03.

**Eight rules collapse onto them** — `PropertyEditabilityEnhanced`, `PropertyEditabilityReduced`,
`PropertyReadabilityEnhanced`, `PropertyReadabilityReduced`, `PropertyType`,
`PropertySetterBecameInitOnly`, `EnumMemberValueChanged`, and the property half of
`MemberStaticnessChanged` — each losing its inline traversal and keeping only its predicate, which
is the rule.

> **`Fields.GetPaired` not built, 2026-08-17.** It was specified here and has **no consumer**.
> `FieldAdded` is family A, and `FieldContractReduced` is family C precisely because it fuses the
> missing case with two facet checks, so it needs the unpaired side that `GetPaired` by definition
> drops. Writing it would have been a helper with one caller, itself. It is trivially addable the
> day a field facet rule exists.
>
> ℹ️ Recorded rather than quietly dropped: the third helper was in this document because the
> symmetry looked right, not because the census found a caller for it. The census counted rules by
> shape, not helpers by demand, and this is the one place that difference showed.

ℹ️ `MemberStaticnessChanged` is a family-C rule and was not in the original list of seven. Its
property half is the same inline traversal character-for-character, so leaving it behind would have
been the only remaining copy of the thing this work exists to delete.

ℹ️ `Overloads.GetMatchedOverloads` already exists and is already used this way by
`MemberOverridabilityReduced` and `ParameterModifierChanged`. This finishes a job C# started and
left half-done; it is not a new pattern.

**RUL-13 — Helpers are per-language, and stay that way.** ✅ required
The helpers SHALL live in `Evaluators/Csharp/` and be `internal`. They SHALL NOT be generalised,
promoted to `Evaluators/`, or shared with Swift. Two languages having a `GetPaired` of the same
shape is ML-04 working as intended, not duplication to be removed — and it sidesteps ML-01 entirely,
because nothing crosses a language boundary.

ℹ️ This is the answer to §10 O-08, which asked whether the shared machinery belonged to a language.
It does. The question dissolves once the machinery is a per-language static rather than a neutral
base.

**RUL-14 — A new language gets the pattern, not the code.** ✅ **implemented, 2026-08-17**
The contributor guide SHALL record that a provider is expected to write its own pairing helpers for
its own topology, before it writes the rules that consume them, and that the rules should read as a
traversal plus a predicate. Swift's `SwiftMembers` is the worked example.

Recorded in [readme-contributors.md](../readme-contributors.md) as *"A rule is one traversal and one
predicate"*, under **Rules** — the section a new provider's author is already reading when they
write their first rule, rather than in a spec they would have to know to look for.

ℹ️ This is the honest answer to what a twelfth language inherits: a documented shape, not a library.
The census shows why — 43% of rules pair over a topology only that language has, and no amount of
generic machinery makes `GetPairedDeclarations` mean anything outside Swift.

## 8. Implementation order

Each step ends green: build clean, full suite passing, working tree committable.

- ~~**B1 — Census.**~~ ✅ done, §6.
- ~~**B2 — The C# helpers and their tests (§7, RUL-11).**~~ ✅ done, 2026-08-17.
- ~~**B3 — Migrate the C# rules onto them.**~~ ✅ done, 2026-08-17 — eight rules.
- ~~**B4 — Contributor guide (RUL-14).**~~ ✅ done, 2026-08-17.
- ~~**B3–B5 — base class and rule migration.**~~ ❌ withdrawn, §6.

## 9. Acceptance criteria

Verified 2026-08-17 on .NET SDK 10.0.100.

1. ✅ `dotnet build -c Release` — **0 warnings, 0 errors**.
2. ✅ Unit suite **652/652** (637 before this work, plus 15 new helper tests). **No existing test
   file was modified** — the only test changes are two added files (RUL-12).
3. ✅ Integration suite **79/79** unmodified, including `JsonReportRegression`, so every rule's
   name, impact, description and yielded symbols are unchanged.
4. ✅ No new type under `Evaluators/` (neutral); `Properties` is `internal` in `Evaluators/Csharp/`
   and `GetPairedMembers` went onto the existing `EnumMembers` (RUL-13).
5. ✅ `IEvaluateCsharpSignatures` and `IEvaluateSwiftSignatures` unchanged (RUL-02).
6. ✅ All eight migrated rules read as one traversal plus one predicate; no `Contains`-then-index
   pairing loop remains in any of them.

ℹ️ The suite grew from the 509 recorded in doc 12's snapshot to 637 before this work began. The
baseline for RUL-12 is the 637, measured immediately before the first edit.

## 10. Open items

**O-07 — Does a new language get the bases, or a template?** ✅ **resolved by the census — a
template.** RUL-14. The census settles it: 43% of rules pair over a topology only their own language
has, so there is no shared code to inherit, and the valuable artifact is Swift's provider read as a
worked example. The original recommendation — land the base, then reassess against the next real
language — is moot, because there is no base to land.

**O-08 — Do the bases belong to a language after all?** ✅ **resolved — yes, and the question
dissolves.** RUL-13. With the neutral base withdrawn, the shared machinery is a per-language
`internal static`, which is exactly ML-04's position and creates no coordination point between
languages.

**O-09 — Is family C worth revisiting for its own reasons?** ℹ️ **new, 2026-08-17.** Twenty-two
rules are bespoke, and four pairs among them are suspiciously alike: `FieldContractReduced` /
`EventContractReduced` fuse removal-plus-change identically, as do Swift's `EnumCaseChanged` /
`OperatorChanged`; and both languages independently union two traversals for their generic-constraint
and staticness rules. None of that is *shared* work — each pair is within one language — but the
fused shape is worth examining on its own merits, because a rule that reports "removed" and "retyped"
under one name is harder to read in the `--json` report than two rules would be. This is a
classification question, not a refactoring one, and belongs in doc 07 rather than here. **Raise
separately; do not fold into this work.**
