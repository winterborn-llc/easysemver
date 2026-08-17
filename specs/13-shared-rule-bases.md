# 13 — Shared Rule Bases

**Status: forward-looking specification.** Nothing here is implemented. Docs 01–12 describe the
system as built; this document specifies one refactor inside it, and where it conflicts with an
existing requirement it says so explicitly.

This is a **behaviour-preserving** change. Every rule keeps its name, its impact, its symbols and
its tests. If any test changes, the refactor is wrong.

## 0. What changes

| Aspect | Today | After this work |
|--------|-------|-----------------|
| The "is it on the other side?" loop | hand-written in each rule that needs it | written once, in one generic base per language-neutral shape |
| Agreeing with another language | copy the loop into your `Evaluators/<Language>/` folder | derive from the base and supply an identity |
| Disagreeing | the only option | still available: do not derive, write `FindDifferences` by hand |
| Rule interfaces | `IEvaluateCsharpSignatures`, `IEvaluateSwiftSignatures`, unrelated | **unchanged** — the bases are not in either hierarchy |
| Rule count, names, impacts, symbols | 41 C# + 38 Swift | identical |

## 1. Why

**Rule bodies are the part of a language provider that scales worst.** A provider is written once;
its rules are written once *per rule*, and the rule count does not fall as languages are added. At
the language set contemplated in the multi-language triage — C#, VB.NET, Swift, Objective-C,
TypeScript, Java, Kotlin, Dart, Go, Rust, PHP, Python — a 40-rule average is roughly **500 rule
classes and, under TST-01, 500 test classes**. That is the maintenance cliff, and it arrives long
before any individual parser becomes the bottleneck.

Most of those rules are not 40 different ideas. Reading the current ones, a large share are the
same loop over different nouns:

```
foreach (entity in <one side>.<collection>)
    if (<a language-specific filter>)       continue;
    if (findIn(<other side>, <key>) != null) continue;
    yield return <symbol>;
```

[`Evaluators/Csharp/TypeAdded.cs`](../src/EasySemVer/Evaluators/Csharp/TypeAdded.cs) and
[`Evaluators/Swift/TypeAdded.cs`](../src/EasySemVer/Evaluators/Swift/TypeAdded.cs) are that loop
twice. So are `TypeRemoved` in both languages, `NestedTypeAdded`, `FieldAdded` and
`EnumMemberAdded` in C#, and `MemberAdded` and `EnumCaseAdded` in Swift.

What differs between the two `TypeAdded` classes is worth stating precisely, because it is exactly
what must survive:

| | C# | Swift |
|---|---|---|
| Filter | skips `Kind == Class` — classes have their own rule | none |
| Key | `(Name, Kind)` — a struct becoming an interface is a different type | `Name` — the kind change is `TypeKindChanged`'s job |
| Symbol | `Name` | `Name` |

Those are real semantic differences between two languages, arrived at deliberately. A base that
erased them would be worse than the duplication.

ℹ️ This is not a new pattern in the codebase.
[`UnitRemovedRule`](../src/EasySemVer/Evaluators/UnitRemovedRule.cs) already does exactly this for
unit existence, and its doc comment already states the bargain: *"Subclass and declare a name to
agree with every other language; override `FindDifferences` to disagree."* This document extends a
proven shape from 2 rules to a family, and changes no principle.

## 2. What may be shared, and what may not

**RUL-01 — The shared thing is the set operation, never the concept.** ✅ required
A shared base SHALL abstract only the *mechanics of a difference* — pair a collection against
another by a key, yield what has no counterpart. It SHALL NOT name, model, or constrain what the
things being diffed are.

This is the ML-01 line, and the test for it is mechanical: **a base's type parameters carry no
constraint**. There is no `where TEntity : IHasName`, no `IDeclaration`, no `IMember`. The base
cannot ask an entity for its name, its kind, its visibility, or its members, because a base that
could would be the cross-language abstraction of "member" that ML-01 forbids. It can only ask the
*rule* to turn one into a string, and the rule is language-specific by construction.

ℹ️ The distinction that makes this legal is worth keeping in view, because a later base could
easily cross it. `IdentityDiffRule<ICsharpType>` and `IdentityDiffRule<ISwiftType>` are two closed
generic types with no supertype in common and no shared vocabulary; the C# rule and the Swift rule
still meet nowhere. A hypothetical `IdentityDiffRule<IDeclaration>` would be a different
proposition entirely, and is refused.

**RUL-02 — The bases are outside both rule hierarchies.** ✅ required
`IEvaluateCsharpSignatures` and `IEvaluateSwiftSignatures`
([Interfaces/Csharp/](../src/EasySemVer/Interfaces/Csharp/IEvaluateCsharpSignatures.cs),
[Interfaces/Swift/](../src/EasySemVer/Interfaces/Swift/IEvaluateSwiftSignatures.cs)) SHALL be
unchanged, SHALL gain no base type, and SHALL remain unrelated to each other. A rule implements its
language's interface and *separately* derives from a base for help with the diffing. The base is a
utility reachable by inheritance, not a place the two languages meet.

CLS-01's statement that "there is deliberately no shared base type" therefore stands as written,
and doc 07 needs no amendment beyond a pointer here.

**RUL-03 — Scoping stays in the rule.** ✅ required
A base SHALL receive **two collections** and nothing else. Deciding *which* collections — the
unit's own types, the paired types' fields, the paired enums' cases, the paired functions — SHALL
remain in the rule.

This is the requirement that keeps the base honest. `signatures.ClassHistory`,
`signatures.TypeHistory`, `SwiftEnums.GetPaired` and `SwiftMembers.GetPairedFunctions` are
language-specific traversals of language-specific topologies; a base that knew how to obtain them
would know what a type and a member are, in violation of RUL-01. Handing the base two collections
costs the rule one `foreach` it was going to write anyway, and buys the base its ignorance.

It also solves the reporting problem for free. Symbols are qualified differently per rule —
`TypeAdded` yields `Widget`, `FieldAdded` yields `Widget.Count` — and the qualifier comes from the
scope the rule is already standing in. So the base yields **entities**, and the rule formats the
symbol with the scope it holds:

```csharp
foreach (var pair in signatures.ClassHistory)
{
    foreach (var field in this.FindAdded(pair.Older.Fields, pair.Newer.Fields))
    {
        yield return $"{pair.Newer.Name}.{field.Name}";
    }
}
```

ℹ️ An earlier draft had the base project the symbol too, via a `GetSymbol` override. It cannot: at
the point the base has an entity it does not have the enclosing type, and passing one in would mean
the base knew entities have enclosing types. Yielding entities is both simpler and the only shape
that is actually available.

## 3. The identity-diff base

**RUL-04 — One base, two directions.** ✅ required
There SHALL be a single generic base carrying the identity diff, in
`src/EasySemVer/Evaluators/`, alongside the existing neutral rule bases:

```csharp
public abstract class IdentityDiffRule<TEntity>
{
    /// Abstract, never defaulted from the class name - see RUL-06.
    public abstract string Rule { get; }

    /// The key two entities are the same entity by. Language-specific by construction.
    protected abstract string GetIdentity(TEntity entity);

    /// Entities this rule does not speak for. Defaults to all of them.
    protected virtual bool Includes(TEntity entity) => true;

    /// Every included entity in `present` with no counterpart in `absent`, by identity.
    protected IEnumerable<TEntity> FindMissing(
        IEnumerable<TEntity> present,
        IEnumerable<TEntity> absent);
}
```

`FindAdded(older, newer)` and `FindRemoved(older, newer)` SHALL be provided as named, ordered
wrappers over `FindMissing`, because `FindMissing(newer, older)` and `FindMissing(older, newer)`
differ only in argument order and a transposition would be a silent inversion of a rule's meaning.

**RUL-05 — Impact and description are inheritable defaults.** ✅ required
The base SHALL carry `EvaluationImpact` and `ChangeDescription` as `virtual` members with the
family's usual answer — Minor / "was added" for the added direction, Major / "was removed" for the
removed direction — so that a rule which agrees declares only its identity and its name. A rule
that disagrees overrides, exactly as `UnitRemovedRule` already permits.

**RUL-06 — `Rule` stays abstract, in every base, forever.** ✅ required
The rule name is half of the published `(language, rule)` key in the JSON report (REP-02) and is
therefore a contract. It SHALL NOT be derived from the class name by any base, at any point,
because a base that filled it in would make a class rename silently break a consumer — which is
the precise reason `UnitRemovedRule` already documents for keeping it abstract there.

**RUL-07 — Deriving is optional, and not deriving is not a defect.** ✅ required
Nothing SHALL require a rule to use a base. A rule whose diffing does not fit — because it compares
two collections at once, or unions two traversals, or treats several facets as one finding —
implements its language's interface directly and is complete and correct. No test, lint, or review
gate SHALL treat a hand-written `FindDifferences` as a smell.

ℹ️ Four shapes already in the codebase are outside the family and are expected to stay there:
`MemberStaticnessChanged` (C#) unions two traversals over different collections;
[`EffectAdded`](../src/EasySemVer/Evaluators/Swift/EffectAdded.cs) treats `throws` and `async` as
one thing that happened, so a generic facet loop would report it twice;
[`FunctionSignatureChanged`](../src/EasySemVer/Evaluators/Swift/FunctionSignatureChanged.cs) treats
return type and parameters as one signature; and the C# overload rules match overload *sets*, not
entities. These are the "unique cases", and they are the reason RUL-07 exists.

## 4. Considered and rejected: a facet-change base

**RUL-08 — There SHALL NOT be a base for the directional-facet family.** ℹ️
A second family is visible in the rules — "iterate pairs, fire when one facet flipped one way":
[`ClassMadeFinal`](../src/EasySemVer/Evaluators/Swift/ClassMadeFinal.cs),
`PropertySetterAdded`/`Removed`, `FrozenAdded`/`Removed`, `MutatingAdded`/`Removed`,
`PropertyEditabilityEnhanced`/`Reduced`. It looks like the same opportunity. It is not, and it was
measured rather than assumed:

- The **identity-diff** family's shared part is the key-matched lookup — roughly ten lines,
  including the `HashSet` that stops it being quadratic, and it is the part most likely to be
  written subtly differently twice.
- The **facet** family's shared part is `foreach (pair) { if (!predicate) continue; yield
  pair.Newer; }` — four lines, of which the predicate *is the rule*. `ClassMadeFinal` is six lines
  of body in total.

A base would replace four lines of obvious loop with an inheritance relationship and an override,
and would not remove a single test. LINQ already expresses the residue. Swift's pair helpers
already return `(Older, Newer)` value tuples
([`SwiftMembers.GetPairedFunctions`](../src/EasySemVer/Evaluators/Swift/SwiftMembers.cs)), so the
rules that want it can write `.Where(...).Select(p => p.Newer.Name)` today with no new type at all.

ℹ️ Recorded here rather than left unsaid because the family is genuinely tempting and will be
proposed again. The general principle: **share the part that is easy to get subtly wrong twice, not
the part that is merely typed twice.**

## 5. Prerequisite — C# member pairing

**RUL-09 — C# SHALL gain the paired-member helpers Swift already has.** ✅ required
Swift's rules obtain paired members from `SwiftMembers.GetPairedFunctions`,
`GetPairedProperties` and `SwiftEnums.GetPaired`. C# has the equivalent only for overloads
(`Overloads.GetMatchedOverloads`); its property and field rules hand-roll the pairing inline —
[`PropertyEditabilityEnhanced`](../src/EasySemVer/Evaluators/Csharp/PropertyEditabilityEnhanced.cs)
walks `Older.Properties.Keys` and re-checks `Contains` on the newer side, and
`MemberStaticnessChanged` repeats the same lines.

C# SHALL gain `Properties.GetPaired` and `Fields.GetPaired` in `Evaluators/Csharp/`, matching
Swift's `(Older, Newer)` tuple shape, before the base work lands. This is a prerequisite and not a
detail: without it, adopting the base in C# would leave the two halves of each rule at different
levels of abstraction, and the resulting diff would be much harder to confirm as
behaviour-preserving.

ℹ️ This is worth doing on its own merits regardless of whether §3 proceeds.

## 6. Census before code

**RUL-10 — The family membership SHALL be established by reading all 79 rules.** ✅ required
This document's family assignments come from reading a **sample** — nine identity-diff rules, six
facet rules, four bespoke ones. The full census across all 41 C# and 38 Swift rules SHALL be the
first work item, and its result SHALL be recorded here as a table of (rule, family, notes) before
any base class is written.

If the census finds the identity-diff family is smaller than roughly a third of the rule set, the
base does not pay and this document SHALL be withdrawn rather than implemented. Say so in the
summary either way.

ℹ️ Stated as a requirement because the sample is the weakest evidence in this document and the
whole case rests on it. The measured-cost discipline that closed §20 O-03 in doc 12 applies here
too: the refactor is justified by a number nobody has produced yet.

## 7. Testing

**RUL-11 — The base gets its own tests; the rules keep theirs.** ✅ required
`IdentityDiffRule<TEntity>` SHALL have a dedicated test class exercising the diff against a
purpose-built dummy entity type — not against `ICsharpType` or `ISwiftType`, so the test cannot
quietly become a test of one language. It SHALL cover: no difference, one added, one removed, an
entity excluded by `Includes`, two entities sharing an identity, and empty collections on each
side.

TST-01 is **unchanged**: every rule keeps its own test class asserting declared impact, a
no-difference negative, and a representative positive. The base removes duplicated production code,
not duplicated tests — a derived rule's identity function and filter are exactly what its test
exists to pin down, and they are the part the base cannot see.

**RUL-12 — The refactor is proved by the suite not changing.** ✅ required
No test file SHALL be edited as part of adopting a base in an existing rule. The full unit suite
(509 tests at the doc-12 verification snapshot) SHALL pass unmodified before and after each rule is
migrated. A migration that requires a test edit has changed behaviour and SHALL be reverted.

## 8. Implementation order

Each step ends green: build clean, full suite passing, working tree committable.

- **B1 — Census (§6).** Read all 79 rules, record the family table in this document. Decide
  go/no-go.
- **B2 — C# pairing helpers (§5).** Behaviour-preserving on its own; lands independently.
- **B3 — The base and its tests (§3, RUL-11).** No rule uses it yet.
- **B4 — Migrate Swift's identity-diff rules.** Swift first because its pair helpers already exist,
  so the diff is smallest and the base gets exercised before C# depends on it.
- **B5 — Migrate C#'s identity-diff rules.**
- **B6 — Docs.** CLS-01 in [07](07-change-classification.md) gains a pointer here; the
  contributor guide gains "deriving is optional" as the standing answer.

## 9. Acceptance criteria

1. `dotnet build` clean, no new warnings.
2. Full unit suite green with **no test file modified** (RUL-12).
3. Every rule's name, impact, description and yielded symbols are byte-identical before and after —
   demonstrated by an unchanged `--json` report over a fixture with a representative diff.
4. No base class in `Evaluators/` declares a type constraint on any type parameter (RUL-01).
5. `IEvaluateCsharpSignatures` and `IEvaluateSwiftSignatures` are unchanged (RUL-02).
6. At least one rule per language remains a hand-written `FindDifferences` and is not marked as
   debt (RUL-07).
7. The census table from RUL-10 is present in this document.

## 10. Open items

**O-07 — Does a new language get the bases, or a template?** A base helps a language that has
already modelled its topology. It does nothing for the 60% of a provider that is the reader. If the
real cost of language number five is still the parser, the bases are a modest saving on the cheap
half, and the more valuable artifact might be a documented worked example — "here is Swift's
provider, annotated" — rather than more shared code. Recommend landing §3 and then reassessing
against the *next* language actually added, not against the hypothetical twelve. **Confirm.**

**O-08 — Do the bases belong to a language after all?** RUL-02 keeps them neutral, in
`Evaluators/`. An alternative is one copy per language folder, duplicated on purpose, so that a
language can change its diffing without a shared file becoming a coordination point between
languages. That is the position ML-04 takes for rules themselves, and it is not obviously wrong
here. The case for neutral is that the identity diff is genuinely the same operation and a
`HashSet` bug fixed once should stay fixed. Flagging it because it is the one place this document
argues against ML-04's instinct, and doing so knowingly. **Confirm.**
