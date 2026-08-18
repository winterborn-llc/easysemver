# 14 — Language Expansion

The requirements for languages added after C# and Swift. Doc 12 built the seam and asserted that
adding a language costs one provider and one registration line; this document is that claim being
cashed in, one language at a time, and the record of what each one cost.

Open decisions raised while implementing are in
[language-questions.md](../language-questions.md) at the repository root, not here — this document
carries what was decided, that file carries what is still worth asking.

## 1. Support tiers

**LNG-01 — Every language declares a tier, and the tier is published.** ✅ required
A language is supported at exactly one of two tiers, and the README, the run log and this document
SHALL all name it:

| Tier | Discovers units | Seeds and stamps versions | Reads the API | Votes on the change type |
|---|:--:|:--:|:--:|:--:|
| **Full** | ✅ | ✅ | ✅ | ✅ |
| **Version-sync** | ✅ | ✅ | ❌ | ❌ |

Version-sync is not a degraded Full; it is UNI-04 applied to a whole language — the same mechanism
that lets a test project carry a version without its members being a contract. Its units are
discovered, seed MVR-03 and are written by MVR-05, and are absent from extraction, classification
and the baseline.

**LNG-02 — A language ships at Version-sync until its reader is trustworthy.** ✅ required
No language SHALL ship at Full on a reader that has not been tested against real declarations of
every kind it claims to model. A reader that under-reports public surface silently converts a
breaking change into a Patch, on a run nobody is watching — which is G-23 and G-24's shared failure
mode, and the reason both cost a major version to undo.

ℹ️ The asymmetry is the whole argument. Version-sync is *visibly* incomplete: it says so per unit in
the log and in this table. A bad reader is *invisibly* wrong, and the tool's entire job is to be
trusted with a number nobody checks.

**LNG-03 — A Version-sync language SHALL say so at runtime.** ✅ required
Each such unit SHALL be logged as versioned without its API being read, so a build log cannot be
mistaken for "no breaking changes found". The existing UNI-04 line already says exactly this and is
reused rather than restated.

## 2. Current tiers

| Language | Tier | Units | Version sources | Notes |
|---|---|---|---|---|
| C# | **Full** | `.csproj` | `AssemblyVersion`, `PackageVersion`, `FileVersion` | Roslyn, doc 05 |
| VB.NET | **Full** | `.vbproj` | as C# | §3 — shares C#'s model |
| Swift | **Full** | SwiftPM target, Xcode target | 7 sources, MVR-03 | doc 12 |
| JavaScript / TypeScript | Version-sync | `package.json` | `"version"` | §4 |
| Rust | Version-sync | `Cargo.toml` | `[package] version` | §4 |
| Python | Version-sync | `pyproject.toml` | `[project]` / `[tool.poetry] version` | §4 |
| Dart | Version-sync | `pubspec.yaml` | top-level `version:` | §4 |
| PHP | Version-sync | `composer.json` | `"version"`, usually absent | §4 |
| Java | Version-sync | `pom.xml` | `/project/version` | §4, Maven only |
| Go | Version-sync | `go.mod` | git tag only, `--tag` to write | §5 |
| C / C++ | Version-sync | `CMakeLists.txt` | `project(… VERSION …)` | §4, likely permanent |
| Ruby | Version-sync | `*.gemspec` | gemspec literal, `VERSION` in version.rb | §4, likely permanent |
| Perl | Version-sync | `Makefile.PL`, `Build.PL`, `dist.ini` | `dist.ini` version, `$VERSION` in every .pm | §4, permanent |
| Gradle (Java / Kotlin / Groovy) | Version-sync | `build.gradle`, `build.gradle.kts` | script literal, `gradle.properties` | §4, LNG-09 |

## 4. The version-sync ecosystems

**LNG-04 — A provider may declare a unit surfaceless at discovery.** ✅ required
`HasPublicApiSurface` SHALL be narrowed by `IsTestCode`, never widened by it:

```csharp
unit.HasPublicApiSurface = unit.HasPublicApiSurface && !provider.IsTestCode(unit);
```

A unit arrives claiming a surface (UNI-01 defaults it true), so this preserves UNI-04 exactly for
every Full-tier provider. What it adds is a way for a version-sync provider to say so **once, at
discovery**, instead of having to call all of its production code "test code" to get the same
effect — which would be a lie in the log and in `IsTestCode`'s contract.

**LNG-05 — One manifest, one unit, one pattern.** ✅ required
A version-sync language SHALL be a subclass of `ManifestLanguageProvider` declaring only its
language id, its unit kind and its manifest filename. Its version convention SHALL be one
registration line naming the pattern that finds a **literal** version in that manifest.

Unit identity is the manifest's folder-root-relative **directory** (ML-03), normalised to `.` at the
root. Two packages of the same name in different folders stay distinct, and nothing machine-specific
reaches the baseline.

**LNG-06 — Manifests are edited textually, and only the first match.** ✅ required
Version write-back SHALL replace the matched span and nothing else, leaving quoting, key order and
formatting untouched. SYN-04's DOM rewrite is right for a `.csproj`, whose formatting MSBuild owns;
it is hostile to a `package.json`, where reserialising would restyle a file the team reads daily.

Only the **first** match is replaced, and every pattern is anchored to the manifest's own top-level
scope. **A manifest mentions versions it does not own** — a dependency's, a parent POM's, an
`engines` constraint — and rewriting one of those pins somebody else's package to this repository's
number, silently, in a committed and published file.

ℹ️ This is the requirement most likely to be broken by a well-meaning pattern change, so it is
tested per language rather than in general:
`TestVersionSyncLanguages.ADependencysVersionIsNeverRewritten` gives every fixture a second version
belonging to something else and asserts it survives.

**LNG-07 — Maven is read as XML, not matched.** ✅ required
`pom.xml` SHALL be read with `XDocument` and only `/project/version` — the root's direct child —
SHALL be read or written. A pom names `<version>` for its parent and for every dependency, so LNG-06's
anchoring is not available to it. A module inheriting its version from a parent has no such element
and is read-skipped and write-skipped (MVR-04); the parent is where that version lives, and the
parent is its own unit.

**LNG-08 — TypeScript is not a separate language here.** ℹ️
A TypeScript package is an npm package: same manifest, same `version` key, same publish. They are
one language because they are one *unit*. If a reader is written it will read `.d.ts`, which is what
both of them ship.

**LNG-09 — Gradle is a language id, because the module is not.** ✅ required
*(Was "deliberately unsupported" for the length of one commit; superseded 2026-08-17.)*

A Gradle module does not say whether it is Java, Kotlin or Groovy, frequently contains more than
one, and inferring it from `src/main/kotlin` is a guess that fails silently on exactly the mixed
modules hardest to notice. So the unit is registered as what it demonstrably is — a **Gradle
project** — rather than as a language nobody can prove it is.

ℹ️ The original objection was that picking the wrong id would be expensive to correct. It is not,
for this tier: a version-sync unit is absent from the baseline (UNI-04), so **no repository persists
the id** and renaming it costs no re-seed. Only a report consumer matching on it would notice, and a
version-sync language emits no findings to match. The decision that looked irreversible was free,
which is why it did not have to wait.

Only a **literal** version is read — `version = "1.2.3"` at column zero of the build script, or in
`gradle.properties` beside it. A script that computes its version has nothing to match: it is a
Groovy or Kotlin program, and SWD-01's reasoning about `Package.swift` applies to it in full.

**LNG-11 — Some languages are version-sync permanently, not provisionally.** ℹ️
LNG-02 frames the tier as a waiting room, and for most of its occupants it is. Three are not:

- **C++** — a header does not say what it declares until the preprocessor has run, and the
  preprocessor needs include paths, which need the build system. That is the toolchain dependency
  G-24 removed, arriving through a different door.
- **Ruby** — `private` is a method call, not a declaration, and a class's surface can be assembled at
  load time. There is no honest static answer to what a gem's public API is.
- **Perl** — only perl can parse Perl: source filters and prototypes let a program change its own
  grammar as it is read. `@EXPORT_OK` and a list of sub names is not an API worth classifying against.

Recording this matters because otherwise each will look like an unfinished job to whoever reads the
tier table next, and they will try.

**LNG-12 — A directory is a unit, not a manifest file.** ✅ required
Where a language has several possible manifests, a directory holding more than one SHALL still be
one unit. Perl is the case: a distribution carrying both a `Makefile.PL` and a `dist.ini` is one
package, and discovering it twice would version it twice and read as two units appearing the first
time anyone upgraded. Manifests are searched in declared name order then path order, so the same one
claims the directory on every machine (BAS-04).

**LNG-10 — Go's only version location is a git tag.** ✅ required *(superseded its own "not listed"
status, 2026-08-17, when O-02 was confirmed)*
`go.mod` carries no version field; a Go module's version *is* its git tag, because `go get` resolves
against the tag and nothing else. Go is therefore the only language here whose single version source
is outside its manifest, and the only one that writes nothing at all unless `--tag` is passed.

Without the flag a Go module is still discovered and still seeds the run from its highest existing
tag. It simply has nowhere to write, which MVR-04 already treats as an ordinary outcome.

## 5. Directory exclusions

**FLD-06 — A language declares what to skip, with the evidence.** ✅ required *(decided 2026-08-17)*
A provider MAY declare directories the walk should skip, each as a name plus the **sibling markers**
that prove it is that directory rather than one that merely shares its name. Declarations are
unioned across every registered provider and applied to the whole walk — a dependency tree should be
invisible to every language, not only to the one that recognised it.

A marker is looked for in the excluded directory's **parent**, because the marker identifies the
package the directory belongs to: a `go.mod` sits beside `vendor`, not inside it.

| Name | Vouched for by | Declared by |
|---|---|---|
| `vendor` | `go.mod` | Go |
| `vendor` | `composer.json` | PHP |
| `target` | `Cargo.toml` | Rust |
| `target` | `pom.xml` | Java |
| `venv` | `pyproject.toml`, `setup.py`, `setup.cfg` | Python |
| `blib` | `Makefile.PL`, `Build.PL`, `dist.ini` | Perl |
| `__pycache__`, `site-packages` | *unconditional* | Python |

**This is the `Packages` post-mortem written down as a mechanism.** That entry was removed because
the name alone did not identify the thing, and every name in the table above is in exactly the same
position — build output or vendored source in one ecosystem, ordinary code in another. Excluding
them globally would reintroduce the silent-swallow failure at a rate that grows with the language
count.

**FLD-07 — The pre-existing global list is frozen, not distributed.** ✅ required
`MagicValues.ExcludedDirectoryNames` keeps `bin`, `obj`, `build`, `DerivedData`, `Pods`, `Carthage`
and `node_modules` as unconditional, and the leading-dot rule is unchanged.

Moving those to their owning languages would be more consistent and is **deliberately not done**:
`bin` is currently skipped in every repository, and making it conditional on a neighbouring
`.csproj` would change what existing repositories discover. Freezing the old list while requiring
every *new* exclusion to be contextual solves the actual problem — the list growing with the
language count — at no risk to anyone already using the tool.

ℹ️ CLI-12 outranks everything, declared or frozen: a name the caller passed to `--do-not-exclude` is
kept whichever rule would have excluded it.

ℹ️ `DirectoryExclusions` is declared **virtual on `ManifestLanguageProvider`** rather than left to
the interface default. Interface mapping is fixed at the type that implements the interface, so a
subclass adding its own property with nothing to override would be adding a member interface
dispatch never reaches — the default would keep winning, silently, and every declared exclusion
would be ignored. `TestContextualExclusions.DeclaredExclusionsActuallyReachTheProvider` is what says
so.

## 6. Writing git tags

**TAG-01 — Tag writing is opt-in, local, and never pushed.** ✅ required *(doc 12 §20 O-02,
confirmed 2026-08-17)*
`--tag` SHALL permit a run to create a local `v<version>` tag. It SHALL default to off, and it SHALL
NOT push, ever.

Reading a tag as a seed has always been safe. Writing one is the only **outward-facing** act this
tool can take, which is why it does not simply follow MVR-05 like every other location. The
local-only rule is what makes it acceptable: a local tag is deletable by whoever ran the command, a
pushed one is not. Publishing is left to whatever already publishes releases — in this repository,
the Action's own tag step, which runs *after* the tests rather than before them.

**TAG-02 — An existing tag is left alone, not an error.** ✅ required
A repeated run recomputes the same version from the same source and therefore wants a tag that
already exists. `git tag` fails on that. Failing the run there would make a repeat invocation an
error over a tag that already says exactly what this run wanted it to say, so the tag is checked
first and skipped if present.

ℹ️ Everything else about tags stays as it was: a folder that is not a checkout is an ordinary input,
not a failure, and the tag list is still read once per run rather than once per unit.

## 3. VB.NET

**VB-01 — VB reuses C#'s signature model, rules and evaluators.** ✅ required
A VB unit's signature SHALL be a `CsharpProject`, and VB SHALL classify with `CompareSignatures` —
all forty-one C# rules, unmodified.

This is a deliberate, bounded exception to ML-01, and it is bounded by a fact rather than by taste:
**VB and C# compile to one metadata format.** Roslyn produces the same `INamedTypeSymbol` graph from
both, and the two languages break compatibility in the same ways — a removed public member, a
retyped property, a narrowed accessor, a sealed class. The "native topology" ML-01 protects is, for
these two languages, genuinely the same topology: the CLR's.

The cost is named rather than hidden: VB signatures live in types spelled `Csharp*` and persist into
`<CsharpProject>` elements. That is one language described in another's vocabulary, which ML-01
forbids, and it is permitted **here and nowhere else**. A language that does not compile to CLR
metadata gets its own topology no matter how similar it looks — the test is the metadata, not the
resemblance.

ℹ️ The alternative was measured before it was rejected. A parallel `Interfaces/Vb/`, `DataObject/Vb/`
and `Evaluators/Vb/` would have duplicated roughly forty rule classes and forty test classes that
could never legitimately disagree, because any disagreement would be one of them being wrong about
the metadata both languages emit.

**VB-02 — The language-specific half is a parse front end.** ✅ required
`VbUnitBuilder` SHALL supply Visual Basic syntax trees and a `VisualBasicCompilation`, and SHALL
share everything downstream via `CsharpUnitBuilder.AppendCompilation`. Extraction below the
compilation SHALL remain language-neutral symbol code.

ℹ️ This is the whole of VB support: one file of about sixty lines, plus a provider. It is what
doc 12's acceptance criterion 8 predicted, and the first time it has been tested by a language the
seam was not designed around.

**VB-03 — The root namespace is explicitly emptied.** ✅ required
`VisualBasicCompilationOptions` SHALL be built `.WithRootNamespace(string.Empty)`.

Roslyn defaults VB's root namespace to the **assembly name**, so every type would be recorded as
`Widgets.Widgets.Gadget`, and renaming a project would read as the removal of its entire API and the
addition of another. C# has no equivalent default. Asserted by
`TestVbExtraction.TypeNamesAreNotPrefixedWithTheAssemblyName`.

**VB-04 — VB owns its own unit-existence rules.** ✅ required
`Evaluators/Vb/UnitAdded` and `UnitRemoved`, subclassing the shared bases per ML-04. The signature
model is shared; anything keyed by language is not, because the report's key is `(language, rule)`
and `"vb"/"UnitAdded"` is a different finding from `"csharp"/"UnitAdded"`.

**VB-05 — Test-code signals are MSBuild's, as for C#.** ✅ required
`IsTestCode` SHALL read the `.vbproj` with `CsProjTestProject` — an explicit `<IsTestProject>`, or a
`PackageReference` to Microsoft.NET.Test.Sdk, xunit, NUnit or MSTest (UNI-04, G-23). Declared on the
provider rather than inherited, so `TestLanguageSeam` can see VB answered.

**VB-06 — Primitive spellings are VB's own, and that is not a defect.** ℹ️
`Public Enum Colour As Byte` records `Byte` where C# records `byte`: Roslyn renders a type in its
own compilation's language. It is stable — a VB unit is only ever compared against its own history —
and it is what a VB developer reads.

The one case it bites is a project converted in place from C# to VB under the same project name:
every primitive-typed member reads as retyped, and the run is Major. The rewrite was already a
Major, so this is recorded rather than fixed.

**VB-08 — A VB unit is named for VB in the baseline.** ✅ required *(decided 2026-08-17)*
A VB unit's signature SHALL be written as `<VisualBasicProject>`, not `<CsharpProject>`.

The model is C#'s (VB-01) and stays C#'s; what does not follow is the name in the file a human
reads. Someone opening `EasySemVer.xml` and finding their Visual Basic project described as a
`<CsharpProject>` would reasonably conclude the tool had misread it.

ℹ️ This is a BAS-07 event and it was free exactly once — before any repository had a VB baseline,
which is why it was decided immediately rather than deferred. After that it would have cost every VB
consumer a forced re-seed. C#'s element name is deliberately untouched: renaming *that* would re-seed
every existing baseline, which is G-26 chosen on purpose.

### Fix carried by this work

**VB-07 — The global namespace is omitted, not string-stripped.** ✅ required — *corrects a latent C# defect*
`GetFullyQualifiedName` SHALL obtain names with `SymbolDisplayGlobalNamespaceStyle.Omitted`.

It previously cut the string at `::`, which handled C#'s `global::Widgets.Gadget` and silently let
VB's `Global.Widgets.Gadget` through — every VB type would have entered the baseline under a
`Global.` prefix that no rule and no reader would match. Stripping a leading `Global.` instead would
corrupt a C# type genuinely declared in a namespace named `Global`. Asking Roslyn to omit it needs no
per-language knowledge and is why this is a fix rather than a VB special case.

ℹ️ C# output is unchanged — the full C# extraction suite passes untouched — so no baseline re-seeds.
