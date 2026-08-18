# Language expansion — questions for Andrew

Written while adding languages overnight. **Nothing here blocked the work**; each question records a
decision I made so I could keep going, what I chose, and what it would cost to choose otherwise.
Where a choice is cheap to reverse I say so, and where there is a closing window I say that too.

Read the ⏳ ones first — those get more expensive the longer they wait.

---

## 🔴 Read this one first — I shipped a defect

### ✅ Q-00 — the VB work cut an unearned v21.0.0 — *answered: leave it*

**What happened.** Adding VB.NET exposed a real bug in the C# reader:
`GetFullyQualifiedName` cut Roslyn's display string at the first `::`, which for a generic type threw
away everything before its last type argument. `List<CsharpMethodParameter>` was being recorded as
`Winterborn…CsharpMethodParameter>` — dangling bracket, no `List<`. **81 type names in this
repository's own baseline were wrong that way.** Fixing it was right and is VB-07.

**What I got wrong.** Correcting the wording changed 81 recorded strings, R13 read them as
property-type changes, and the next run cut **v21.0.0** — a major release for an API nobody touched.
This is exactly what BAS-07's per-unit signature version exists to prevent, and I should have moved
C#'s `SignatureVersion` to `2` in the same commit. I did not.

**Blast radius.** Every consumer holding a C# baseline written before v20.1.0 takes the same
unearned Major on their first run after upgrading. Once, then never again.

**What I did about it:** nothing, deliberately, and I want you to confirm that. Bumping
`SignatureVersion` now would drop every C# baseline a second time — consumers who already paid the
Major would pay again, while those who have not would trade a Major for a Minor. Doing nothing is
self-limiting. It is also the call G-23 made about v17.0.0: the release is real and immutable, and
what gets fixed is the next one. Written up as **G-26**.

**The thing actually worth your time — now done.** Nothing in the build failed when a provider
changed how it words a signature. `TestSignatureWording` now does: it extracts a fixture dense with
the constructs whose rendering has moved before and compares against a checked-in expected file,
failing with the only two correct responses spelled out. I mutation-tested it by flipping the
global-namespace style back, and it catches exactly the change that caused this.

The judgement call I could not avoid: the expected file is **generated from current behaviour**, so
it asserts the wording did not change, not that it is right. That is the honest scope of a golden
test, and it is what would have caught G-26. If you think today's wording is wrong somewhere,
correcting it is now a deliberate two-line act rather than an accident.

---

## VB.NET — shipped, full signature support

### ✅ Q-01 — VB units are persisted in `<CsharpProject>` elements — *answered: renamed to `<VisualBasicProject>`*

**Answered: Visual Basic gets its own name.** A VB unit now writes `<VisualBasicProject>` (VB-08).
The model stays C#'s per VB-01 — that was the right call and nothing about it changed — but the name
in the file a human reads is VB's own. Someone opening `EasySemVer.xml` and finding their Visual
Basic project described as a `<CsharpProject>` would reasonably conclude the tool had misread it.

Done immediately because the window was closing: no repository had a VB baseline yet, so it was free
exactly once. C#'s element name is deliberately untouched — renaming *that* would re-seed every
existing baseline, which is G-26 chosen on purpose.

**Left as-is:** the language id stays `vb` rather than `visualbasic`. It is already VB's own name
rather than C#'s, which was the actual complaint. Say so if you want the long form and it is a
one-line change — nothing persists it for a Full-tier language except the report's `language` field.

### Q-02 — VB `Module` is modelled as a class

A VB `Module` compiles to a sealed class with static members, and that is exactly how it lands in
the signature. A VB developer would call it a Module.

**Decided:** left as a class. It is what the metadata says, and every rule about classes is correct
for it. Modelling it separately would mean a `CsharpModule` type that only VB ever produces, inside
a model shared with C#, which seems worse than the imprecision.

**Reversible:** yes, cheaply, and no baseline consequence if done before VB has users.

### Q-03 — VB and C# spell primitives differently in the shared model

`Public Enum Colour As Byte` records `Byte`; the C# equivalent records `byte`. This is Roslyn
rendering a type in its own compilation's language, and it is *correct* — a VB developer reads
`Byte`.

It is harmless in practice: units are keyed `(language, unit id)` and a VB unit is only ever compared
against its own history. The single case where it bites is a project **converted in place** from C#
to VB keeping the same project name — every primitive-typed member then reads as retyped, and the
run is Major. That was already a Major, so I recorded it in the test rather than fixing it.

**No action needed unless you disagree.**

### Q-04 — should a VB test project's signals be VB-specific?

`IsTestCode` reads the same MSBuild signals as C# — `<IsTestProject>`, or a `PackageReference` to
Microsoft.NET.Test.Sdk / xunit / NUnit / MSTest. I could not find a VB-specific convention that
differs.

**Decided:** shared. Flagging only in case you know of one I do not.

---

## Cross-cutting

### ✅ Q-05 — what does "supported" mean in the README? — *answered: keep the tier as shipped*

This is the one I would most like your answer on, because it shapes how the rest of the languages
get described.

There are two honest tiers, and they are very different products:

| Tier | What the tool does | What it does **not** do |
|---|---|---|
| **Full** | discovers units, reads the public API, classifies Major/Minor/Patch, stamps versions | — |
| **Version-sync** | discovers units, seeds from their versions, stamps the new version everywhere | never reads an API, so never votes on the change type |

C#, VB.NET and Swift are Full. Everything I add after them is Version-sync first, because a
hand-rolled reader that silently under-reports a breaking change is worse than no reader — that is
the G-23/G-24 lesson applied to new languages.

Version-sync is genuinely useful on its own: a Go or Rust or Python repo gets one coherent version
stamped across every manifest that already carries one, driven by whatever *is* being read. But if
the README says "supports Go" without qualification, someone will reasonably expect Go breaking
changes to be detected, and they will not be.

**Answered: ship it as-is.** Every language is listed with its tier, and the README says in plain
words that a Versioned language never votes on the change type — including the consequence that a
repository containing only Versioned languages sees a Patch on every run.

### Q-06 — a version-sync-only language and the "every run is a release" rule

If a repository contains *only* version-sync languages, nothing ever votes, so every run is a Patch
(and the first is a Minor via NCL-02). That is correct behaviour and it is also a slightly
surprising product: the tool becomes a version stamper rather than a version decider.

**Decided:** correct as-is, and the run log says so explicitly per unit, so it cannot be mistaken for
"no breaking changes found".

---

## Version-sync languages — shipped: JavaScript/TypeScript, Rust, Python, Dart, PHP, Java,
## C/C++, Ruby, Perl, Gradle

### ✅ Q-08 — Gradle has no honest language id. What should it be? — *shipped as `gradle`*

`build.gradle` / `build.gradle.kts` is shared by **Java, Kotlin and Groovy**, often in the same
module, so a Gradle unit cannot say which language it is. Maven has the same ambiguity in principle
but not in practice, so Java shipped on `pom.xml` alone.

The options as I see them:

1. **A `gradle` "language"** — honest about what it is, dishonest about being a language. The report
   would say `gradle/UnitAdded`, which is at least not wrong.
2. **A `jvm` language** covering Maven and Gradle both — then `java` is the wrong id for the Maven
   provider that already shipped, and renaming it later costs a re-seed.
3. **Guess from the module's source directories** — `src/main/kotlin` vs `src/main/java`. Works
   often, fails silently on mixed modules, which is the failure mode this codebase keeps deciding
   against.

**Decided, and shipped: option (1), `gradle`.** I initially held this back as blocked on you, then
realised the thing I was protecting against does not exist.

The fear was that a wrong id would be expensive to correct. It is not, at this tier: a version-sync
unit is **absent from the baseline** (UNI-04), so no repository stores the string, and renaming it
costs nobody a re-seed. The only observer is a report consumer matching on `language`, and a
version-sync language emits no findings to match. A decision that looked irreversible turned out to
be free, so it did not need your night.

**Still worth your opinion**, just not urgently: if you would rather it were `jvm`, or split by
guessing at source directories, say so and it is a one-line change.

### ✅ Q-09 — Go is unlisted because nothing is writable — *answered: O-02 confirmed, Go shipped*

`go.mod` has no version field; a Go module's version *is* its git tag. Reading tags already works.
Writing one is doc 12 §20 **O-02**, which you have not confirmed — and it is outward-facing and
effectively irreversible, so I did not decide it for you.

**Answered: O-02 confirmed as specified.** Shipped as TAG-01: `--tag` is opt-in, off by default,
creates a **local** `v<version>` and never pushes. An existing tag is left alone rather than failing,
so a re-run is safe (TAG-02).

Go shipped with it, and is the only language whose sole version location is outside its manifest.
Without `--tag` a Go module is still discovered and still seeds from its highest existing tag — it
just has nowhere to write, which MVR-04 already treats as ordinary.

**Still open, and smaller:** PHP and Python mostly version by tag too, and neither is registered
against the tag source yet. Say the word and it is two registration lines each.

### ✅ Q-10 — should the Full-tier providers move onto shared loops? — *answered: hoisted to `UnitVersions`*

`ReadVersions` and `WriteVersion` are **character-for-character identical** in the C#, VB and Swift
providers, and now a fourth copy lives on the version-sync base. That is the same duplication the
pairing helpers just removed, one layer up.

**Answered: hoisted.** Both loops now live in `UnitVersions`, used by all four providers. A static
helper rather than a base class, because the Full-tier providers are not otherwise related and
giving three shipped providers a common ancestor to share eight lines would be a bigger change than
the duplication cost. Behaviour-preserving, and proved the way the pairing helpers were: the suite
passes unmodified.

### ✅ Q-11 — `package.json` matched at up to two spaces of indent — *answered: leave it*

The pattern anchors to `^\s{0,2}"version"` so it cannot match a nested one — a dependency's, or the
`engines` block. Every formatter npm ships indents top-level keys by two, so this holds for
generated and hand-written files alike.

**It would miss** a `package.json` formatted with four-space or tab indentation. The consequence is
a missed version, not a wrong one: the package is discovered, contributes no version source, and is
stamped nowhere. That is the safe direction and it is MVR-04's behaviour anyway.

**Decided:** shipped as-is. Worth revisiting if anyone reports a package that is not being stamped.

### ✅ Q-07 — directory exclusions are still global, not per-language — *answered: built, FLD-06*

We agreed exclusions should become language-owned and contextual (a `vendor` beside a `go.mod`, a
`target` beside a `Cargo.toml`). **I have not built that yet**, and each new ecosystem makes it more
necessary — `target`, `vendor`, `dist`, `deps` and `blib` are all somebody's real source directory.

**Answered: built as FLD-06.** A provider declares a name plus the sibling markers that prove it is
that directory — `vendor` beside a `go.mod`, `target` beside a `Cargo.toml`, `venv` beside a
`pyproject.toml`. Unconditional is reserved for names that cannot mean anything else
(`__pycache__`, `site-packages`).

**And then the legacy list was migrated too, on your instruction.** FLD-07 now says there is no
global list at all: `bin` and `obj` belong to C#/VB and need a project file beside them, `build`
belongs to C++/Gradle/Swift, `Pods` and `Carthage` need their Podfile and Cartfile. Only
`node_modules` and `DerivedData` stayed unconditional, because neither name can mean anything else.

**What it changed:** a `bin`, `build`, `Pods` or `Carthage` with nothing vouching for it is now
walked where it used to be skipped everywhere. The common cases are untouched — MSBuild puts `bin`
beside the project file — but a centralised output directory or a `build/` of shell scripts is now
entered. That costs a walk and finds nothing, because build output holds no manifests; the reverse
mistake hides a unit forever. Verified on this repository: `bin` and `obj` are still skipped, the
skip count fell from 18 to 11, and the newly-walked directories produced no units.

**One thing the migration exposed.** Exclusions now come from the registered providers, so any
caller that walks without passing them gets *none* — silently. Production was fine, but three test
classes were walking unprotected trees and passing for the wrong reason. `Test.Exclusions` now sets
up a run's worth of state the way a run does, and the tests use it.
