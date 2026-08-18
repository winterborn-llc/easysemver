# 99 — Known Gaps and Deviations

Consolidated list of every place the implementation diverges from the requirements in this
folder, ordered by severity. "Verified" means confirmed by executing the code; "code-read" means
established by inspection. IDs are never reused, so references from issues and commits keep
resolving.

## Blockers

*None.* G-01 — the one blocker — is resolved; see below.

## Major

*None.*

## Minor

**G-16 — Minimal compilation references degrade foreign type names.** (Code-read)
Cross-project and NuGet types resolve as error symbols in the ad-hoc compilation, so recorded
names may lack namespaces. Stable run-to-run, so diffs work, but theoretically collidable.
*Relates to:* SIG-01.

## Informative / hygiene

**G-17 — Stale documentation.** ✅ Resolved by the doc-12 rewrite of
[README.md](../README.md): folder-based usage, the version-source table and Swift prerequisites
replaced the "Auto-Version" content, the stale package version, the post-build rationale and the
`UsingTask` instructions. The warning about the MSBuild route that briefly replaced them is gone
too, now that the route itself is (G-02/G-03/G-04 withdrawn).

## Withdrawn

Requirements that were dropped rather than met, because the shape of the product changed. IDs are
never reused.

**G-02 — Documented MSBuild integration cannot work.** ⊘ Withdrawn
**G-03 — Packaged targets point at an executable that isn't packed.** ⊘ Withdrawn
**G-04 — Targets file not auto-imported (`NU5129`).** ⊘ Withdrawn

All three were defects in one mechanism: consuming EasySemVer as a `PackageReference` that hooks
itself into a consuming build. On 2026-08-02 that mechanism was withdrawn — EasySemVer is a CLI,
distributed as a `dotnet tool`, as self-contained binaries, and as a GitHub Action
(**INV-01**, **PKG-01/PKG-02**, **ACT-01…ACT-12**). A tool whose unit of work is *a folder* does not belong bolted
to one arbitrary project inside it.

Deleting `Winterborn.EasySemVer.targets` also removed the `NU5129` warning, so the build is now
warning-free. **§20 O-05 is closed by withdrawal, not by repair.**

## Resolved

Retained so the IDs are never reused and so references from issues and commits still resolve.

**G-01 — Baseline serialization throws; no run can complete.** ✅ Resolved by **BAS-02**
The persisted graph is now concrete, public, parameterless-constructible types with
concrete-typed members; the interface view the rules consume comes from *explicit* interface
implementation, which `XmlSerializer` never sees. No interface-typed member remains anywhere on
the persisted graph, and no parallel DTO tree exists to drift.
*Satisfies:* PER-05, SYN-01, TST-05 — `Regression.TestProgramInvocation` passes.

**G-05 — CI workflow is stale and cannot go green.** ✅ Resolved by the §17 workflow rewrite:
`env.project` is `EasySemVer`, the SDK is `10.0.x`, the runner is `macos-latest` so the Swift
suite can run, and the unit and integration suites are separate steps.
ℹ️ Since split out: build-and-test is a reusable workflow still on `macos-latest`, and the release
job that depends on it runs on `ubuntu-latest` (CI-01).
*Satisfies:* CI-01.

**G-06 — CLI directory argument is ignored.** ✅ Resolved by **FLD-01**
[`RunOptions.Parse`](../src/EasySemVer/Settings/RunOptions.cs) uses the argument as the folder
root and validates *that* path. *Satisfies:* CLI-04.

**G-07 — Adding a new method or property classified as Patch.** ✅ Resolved
Fixed by rules R15 (`MethodAdded`) and R16 (`PropertyAdded`), both Minor, each with a test class.
*Satisfies:* CLS-05.

**G-08 — Optional→required parameter classified as Minor.** ✅ Resolved
Fixed by rule R17 (`MethodInputParameterMadeRequired`, Major), which reuses R02's matcher but
fires only in the breaking direction, so required→optional stays Minor via R04.
*Satisfies:* CLS-06.

**G-09 — `.slnx` recognized inconsistently.** ✅ Resolved by **FLD-02**
No code path looks for a solution file at all; both walk-up helpers are deleted.
*Satisfies:* DSC-01/DSC-02, now retired.

**G-10 — Source discovery includes `obj/` generated files.** ✅ Resolved by **FLD-04**
Source enumeration goes through the same `FolderScanner` exclusion list as discovery, so build
output and package caches are skipped at any depth. *Satisfies:* DSC-06/DSC-08.
ℹ️ Fixed alongside a related leak found while testing: extraction walked the compilation's
*merged* global namespace, so public types from referenced assemblies (`Internal.Console` out of
`System.Console.dll`) were entering real baselines. It now walks the source assembly's namespace.

**G-11 — Version edge cases crash.** ✅ Resolved by **MVR-02**
Fewer than three segments normalize to three on parse; a blank version behaves as `0.0.0` for
`ToString` and comparison. `Version.TryParse` was added so a version source with an unparseable
value is skipped with a warning rather than failing the run. *Satisfies:* VER-02/VER-07.

**G-12 — Logging half-implemented.** ✅ Resolved by **ERR-M2**
`Indent`/`Outdent` semantics corrected, the indent-aligned message is the one written,
continuation lines align past the timestamp column, and every path routes through `Log` — there
are no bare `Console.WriteLine` calls left in the tool. *Satisfies:* LOG-01/LOG-02/LOG-03.

**G-13 — Redundant discovery + hidden cwd dependency in classification.** ✅ Resolved by
**FLD-03** and **REN-04**
Discovery runs once per invocation and its result feeds every downstream stage.
`CsharpSignaturesToCompare` carries only the two signatures and the paired-type history; it has
no path, no `Save`, and no way to re-discover anything. *Satisfies:* DSC-07, CLS-07.

**G-14 — Return type stored per method name, not per overload.** ✅ Resolved by **CSX-04**
Return type is recorded per overload; R03 compares both the per-name value and each matched
overload's own. *Satisfies:* SIG-11, R03.

**G-15 — Interfaces, structs, enums, delegates, events, fields are invisible.** ✅ Resolved by
**CSX-01…CSX-05**
Every public type kind is modelled as its own concept, nested types included; fields, events,
enum members, positional record parameters, and the full modifier set are captured; rules
R18–R42 classify changes to all of it. The `TODO`s in the builder are gone.
*Satisfies:* SIG-02, and closes the last two ⚠️ rows of the CLS scenario matrix.

**G-18 — Test hygiene.** ✅ Resolved by **TST-M9**
`Experimental.cs` and its hard-coded absolute path are deleted, as are the `Test.csproj`
references to non-existent content files. *Satisfies:* TST-07.

**G-19 — Dead/vestigial code and an implicit dependency.** ✅ Resolved by **REN-05**
Deleted: `CsProjSignature`, `ExtendIList`, `ExtendDirectoryInfo`, `ExtendFileInfo`,
`ExtendString.GetXmlNodeValue`, the commented-out `AdhocWorkspace` block, and the
`Newtonsoft.Json` attributes on `Solution` (along with `Solution` itself, replaced by the unit
array). *Satisfies:* PKG-07.

**G-20 — ACT-10's release block specified but not in force.** ✅ Resolved by adoption
CLI-10, REP-10, ACT-11 and ACT-12 shipped in one commit; that commit's own run published **v16.0.0**
to nuget.org and cut the GitHub Release with all five PKG-02 archives. The commit after it pinned
ACT-02's `version` default and the README's `uses:` refs to that tag, replaced four hand-written
steps in [`dotnet.yml`](../.github/workflows/dotnet.yml) with ACT-12's two invocations, and added
`ActionRegression.TheDocumentedReleaseStepsAreTheOnesThisRepositoryRuns` — which asserts the README
and the workflow cannot drift, and was confirmed to fail when they do.

The two-commit shape was not avoidable and is not a defect: ACT-02's Action runs a *published*
binary, so nothing here can consume a capability in the same commit that introduces it.
ℹ️ Retiring the `git add EasySemVer.xml src/*/*.csproj` glob was the point. It was silently wrong
for any project it did not match — a green run, a tag, and a commit with no bump in it.
*Satisfies:* ACT-10. *Relates to:* ACT-02, ACT-11, ACT-12, CLI-10, REP-10.

**G-21 — v16.0.0's action manifest fails to load.** ✅ Resolved by **ACT-13**
The `report` input's description carried `${{ steps.version.outputs.report }}` as a worked example.
GitHub evaluates expressions inside input descriptions, where the `steps` context does not exist, so
`action.yml` failed template validation at *Set up job*: `Unrecognized named-value: 'steps'`. Every
consumer of `winterborn-llc/easysemver@v16.0.0` gets that error before any step runs, and this
repository could not cut the fix because its own workflow pinned to that tag.

Fixed by removing the example (it lives in the README), by `uses: ./` so a manifest is loaded by the
run that publishes it, and by `ActionRegression.NoInputDescriptionCarriesAnExpression` — confirmed
to fail when the example is put back.
ℹ️ **v16.0.0 remains broken as an Action and cannot be repaired**: a published release is immutable
and moving the tag would be a history rewrite. It is superseded, not fixed. The tool binary and the
NuGet package at that version are unaffected — only the manifest.
ℹ️ Nothing was published by the failed run. CI-03's ordering held: it died before the version step,
so no commit, no tag, and nothing on nuget.org.
*Relates to:* ACT-02, ACT-09, ACT-10, ACT-13.

**G-22 — The pinned version and the README refs go stale between releases.** ✅ Resolved by **CI-05**
ACT-02's `version` default and the README's `uses:` refs were hand-edited, in a commit separate from
the release that made them true. They were consequently wrong most of the time: `action.yml` pinned
`v16.1.1` while v16.1.2 was the newest release, and every README example told a reader to copy a tag
one release behind the one the pin would download.

Nothing broke, because an exact tag names a manifest and a binary that shipped together whichever
release that was — which is precisely why it went unnoticed. It became load-bearing the moment a
moving `v16` was wanted: a floating tag over a hand-pinned manifest hands out the newest wrapper
around the oldest tool, and widens the gap on every release.

Fixed by stamping both in the release run itself, staged into the release commit so they are inside
the tag, and by three assertions in `ActionRegression` — that exactly one line in `action.yml` looks
like the pin the rewrite targets, that the README's refs are the major tag of that pin, and that a
file staged before the ACT-11 commit step reaches the commit at all, which is the mechanism the
whole thing rides on and is no part of that step's contract.
ℹ️ Releases before v16.1.2 keep whatever pin they were published with. They are not wrong — each
still names a real release — merely older than the tag that contains them.
*Satisfies:* CI-05. *Relates to:* ACT-02, ACT-10, ACT-11.

**G-23 — Renaming a test method cuts a major release.** ✅ Resolved by **UNI-04**
UNI-02 made every `.csproj` a unit and UNI-03 said the same of Swift test targets, deliberately:
they carry versions, and a unit vanishing is a real change. What neither noticed is that a test
project's *public members* were then part of the folder's API surface, and R02 classifies a removed
public member as Major.

So this repository released **v17.0.0** for two renamed `[Fact]` methods. Nothing about the tool
changed for anyone; the CLI, the package and the Action were byte-identical in behaviour to
v16.1.2. Every consumer with a test project inside the versioned folder had the same defect, and it
fires on the most ordinary act there is — renaming a test.

Fixed by separating "versioned" from "has an API surface": a test project is still discovered, still
seeds the version and still gets the new one written into it, and is simply not compared. Which
code is test code is answered per language, through `ILanguageProvider.IsTestCode` — C# from the
`.csproj`, SwiftPM from `dump-package`, Xcode from the project file's product types — so the fix is
the seam's, not C#'s, and a language added later cannot inherit the defect by omission.
ℹ️ v17.0.0 is not a mistake to be undone — the release is real and immutable, and the major line is
where the tool now lives. What is fixed is that the *next* one will not happen the same way.
ℹ️ The versions of test projects are deliberately still written. They are assemblies that ship in
a build output and get stamped like anything else; what changed is only whose changes get a vote.
*Satisfies:* UNI-04. *Relates to:* ML-02, UNI-02, UNI-03, NCL-03, BAS-01, R02.

**G-25 — This repository's own release could not pass `vnext-token-name`.** ✅ Resolved by adoption
TOK-01 stamps the default token everywhere it appears under the folder root, and this repository's
README, specs and source comments *document* that token — so its release step has to name a token
nobody writes (CLI-13), or the version is stamped over every mention of it and committed, one
release at a time, until the documentation says nothing.

The input could not ship in the commit that introduced the flag. ACT-02's step runs the
**published** binary, and v20.0.1 rejected `--vnext-token-name` by name — which is not a
hypothetical: the first push did pass it, and the run died at the version step with
`EasySemVer does not recognise the option --vnext-token-name`. CI-03's ordering held and nothing was
released: no build, no commit, no tag, nothing on nuget.org.

The commit after it removed the input, its release published **v20.0.2** — the first binary that
understands the flag — and the commit after *that* put the input back. Same two-step as G-20, same
cause: an Action running a published binary cannot consume a capability in the commit that
introduces it. Expect it again for anything this repository's own pipeline consumes.

**The window between those two commits was not safe, and it was entered.** The reasoning at the
time was that the binary which could not be told about the token was the binary that did not have
the feature — true of the *pin*, and irrelevant, because the release in the middle rewrites the pin
(CI-05). An unrelated push landed four minutes later, ran the freshly published v20.0.2 against a
workflow that could not yet name the token, and **v20.0.3 committed the version over every mention
of `{{vnext}}`** in the README, `readme-contributors.md`, three specs and three source comments —
"mark the spot with `20.0.3`" — which is precisely the failure CLI-13 exists to prevent, arriving
by the one route nobody was watching.

The text was restored in a forward commit; the release commit stands, because published history is
not rewritten here. What the incident says for next time is narrow and worth stating plainly:
**during a two-step adoption, the exposure starts the moment the middle release publishes, not when
the second commit lands** — so hold the branch, or land the second commit before anything else can.
*Satisfies:* CLI-13. *Relates to:* ACT-02, ACT-10, CI-05, TOK-01, G-20.

**G-24 — Reading Swift needed a Swift toolchain, and that failed in the field.** ✅ Resolved by
**SIG-20**
D-02 chose the toolchain's symbol graph over a hand-rolled parser, and the reasoning was sound: the
compiler has resolved the program, so it knows the type of `public let store = Store()`, it knows
which members a conformance synthesises, and it knows what a macro expanded to. A parser knows none
of that. What the decision priced in was a build — §20 O-03 measured it, at 4.1 s per Xcode project
and ~1.3 s per SwiftPM package, and concluded that nothing there proved the tool impractical.

The clock was the wrong thing to measure. Four processes ran per versioned run — `swift package
dump-package` and `xcodebuild -list -json` to discover targets, `swift build` and `xcodebuild build`
to extract signatures — and the first two resolve the project's package dependencies before they
will answer at all. So a versioning run required, transitively: a Swift toolchain; Xcode, for any
repository containing an `.xcodeproj`; a network; and credentials for every private package the
project depends on. D-03 made every one of those a hard failure. Clients hit it: runs failing on
machines that were offline, behind a proxy, or without access to a private dependency — during
versioning, which has no business needing any of it.

Fixed by reading the source. Targets come from the text of `Package.swift` and from
`project.pbxproj`; signatures come from the target's `.swift` files. Nothing runs a process.

**What that costs, stated plainly.** The graph was more accurate and the reader is an
approximation. It cannot see an inferred type, a macro-generated declaration, or which branch of an
`#if` is live; it guesses at a superclass when the first inheritance entry is a foreign protocol.
SIG-27 lists all of it. Every one of those errs towards reporting more public surface than exists,
never less, and every one is *stable* — the same source produces the same answer on every machine —
so none of them can churn a baseline.

This is the same bargain the C# reader has always struck. G-16 records that cross-project and NuGet
types stay as error symbols carrying their written names, because resolving them properly would
mean building. A stable approximation that needs nothing installed beats a resolved answer that
needs a working build environment, for a tool whose whole job is to run inside someone else's
pipeline and decide a number.

ℹ️ Two things improved on the way past. A member reached through a protocol extension is now
recorded as having a default implementation, so adding one fires S21 (Minor) instead of S20 (Major)
— the graph only reported the relationship for members that were also requirements, which made a
new defaulted method a false Major. And the accuracy that was lost was partly notional: the symbol
graph was already being filtered for `::SYNTHESIZED::` members precisely so that the compiler's
derived members would *not* reach the baseline (SIG-23), which is exactly what source-reading gives
for free.

ℹ️ Swift signatures re-seed once. They are dropped by the per-unit signature version (BAS-07), not
by a `formatVersion` bump, so a repository's C# history survives and a repository with no Swift in
it sees nothing at all. Bumping the file version would have handed every C#-only consumer a release
it did not earn — which is the G-23 mistake wearing a different hat.
ℹ️ CI moved from `macos-latest` to `ubuntu-latest` with this, and the seven Swift and Xcode
integration tests went from needing a Mac with Xcode to running in about 0.3 s anywhere.
*Satisfies:* SIG-20, SIG-27, BAS-07. *Relates to:* D-02, D-03, D-06, SWD-01, SWD-02, SWE-01,
SWE-05, SWE-06, O-03, G-16, G-23.

**G-26 — Fixing corrupt generic type names cut an unearned major release.** ⚠️ Open, and
deliberately not undone
`ExtendINamedTypeSymbol.GetFullyQualifiedName` cut a Roslyn display string at the first `::`, which
for a generic type threw away everything before its last type argument. A property of type
`List<CsharpMethodParameter>` was recorded as
`Winterborn.Tools.EasySemVer.DataObject.Csharp.CsharpMethodParameter>` — a name with a dangling
angle bracket, no `List<`, and no way to tell it apart from a property of the element type. **81
type names in this repository's own baseline were wrong that way.**

It was found by adding VB.NET: VB writes `Global.` where C# writes `global::`, so every VB type
would have entered the baseline under a prefix no rule could match. Asking Roslyn to omit the global
namespace (`SymbolDisplayGlobalNamespaceStyle.Omitted`) fixes both without either language being
special-cased, and is VB-07.

**What was missed is BAS-07.** Correcting the wording changed 81 recorded strings, R13 read them as
property-type changes, and the run cut **v21.0.0** — a major release for an API nobody touched.
That is precisely the case the per-unit signature version exists for, and C#'s
`SignatureVersion` should have gone to `2` in the same commit. It did not, so every consumer holding
a C# baseline written before v20.1.0 takes the same unearned Major on their first run after
upgrading.

ℹ️ The delay is worth understanding, because it is a safety feature working. The fix shipped in
v20.1.0's *source*, but that release was computed by the previously published binary (PKG — the
Action downloads a published binary, never the one the run builds). The corrected names first
reached a baseline on the next run, which is when the Major fired. A change that mis-versions
breaks the release *after* the one that introduced it, exactly as intended.

**Not fixed by bumping now, on purpose.** Bumping `SignatureVersion` at this point would drop every
C# baseline again: consumers who have already taken the unearned Major would take a second unearned
bump for the same cause, while those who have not would trade a Major for a Minor. Doing nothing is
self-limiting — each consumer pays exactly one unearned Major, once, on first upgrade, and never
again. That is the same call G-23 made about v17.0.0: the release is real and immutable, and what
gets fixed is the next one.

⚠️ **The lesson is the requirement, and it is still not enforced.** Nothing in the build fails when
a provider changes how it words a signature without bumping its `SignatureVersion`. A test that
extracts a fixture and compares against a checked-in expected signature would have caught this at
the moment the wording changed, rather than one release later. That test does not exist.
*Relates to:* BAS-07, VB-07, SIG-01, G-16, G-23, G-24.
