# 99 — Known Gaps and Deviations

Consolidated list of every place the implementation diverges from the requirements in this
folder, ordered by severity. "Verified" means confirmed by executing the code on 2026-08-01
(see [README.md](README.md) verification snapshot); "code-read" means established by
inspection.

## Blockers

**G-01 — Baseline serialization throws; no run can complete.** (Verified)
`SignaturesToCompare.Save` → `ExtendObject.Serialize` uses `XmlSerializer` on
`Solution : List<IProject>`; `XmlSerializer` cannot serialize interface-typed members and
throws `NotSupportedException: Cannot serialize interface ... IProject` before anything is
written. Consequences: baseline never persists (PER-05), version write-back never runs
(SYN-01), process exits 1 (would fail any consuming build), integration test host crashes
(TST-05). Everything upstream — discovery, extraction, classification, increment — works.
Fix requires a serializable representation on both the write **and** read sides (concrete
DTO graph, or a hand-rolled XML mapper); note `ExtendString.Deserialize<Solution>` has the
mirror-image problem and currently only "works" because it never receives a file.
*Violates:* PER-05, SYN-01, TST-05.

## Major

**G-02 — Documented MSBuild integration cannot work.** (Code-read)
README instructs `UsingTask` + `<EasySemVer />`, but the assembly contains no MSBuild `Task`
class (deleted with the old `AutoVersion` project). Either restore a task wrapper or rewrite
the README around the packaged-targets/Exec mechanism. *Violates:* MSB-02.

**G-03 — Packaged targets point at an executable that isn't packed.** (Code-read)
`Winterborn.EasySemVer.targets` resolves `..\tools\EasySemVer.exe`, but the `.csproj` packs
no `tools/` folder. *Violates:* PKG-04.

**G-04 — Targets file not auto-imported (`NU5129`).** (Verified at build)
The buildTransitive targets file must be named `Winterborn.Library.EasySemVer.targets`
(match the package ID) for NuGet to import it into consumers. *Violates:* PKG-05.

**G-05 — CI workflow is stale and cannot go green.** (Code-read)
[`dotnet.yml`](../.github/workflows/dotnet.yml): `env.project: AutoVersion` (nupkg push path
no longer exists), SDK `9.0.x` vs `net10.0` targets, and the test step includes the
currently-aborting integration test (G-01). *Violates:* CI-01.

## Minor

**G-06 — CLI directory argument is ignored.** (Code-read)
`Program.GetDirectoryToUse` validates and returns `Environment.CurrentDirectory` instead of
`args[0]`. Works under MSBuild only because Exec's cwd is inside the solution.
*Violates:* CLI-04.

**G-09 — `.slnx` recognized inconsistently.** (Code-read)
`Program.GetSolutionDirectory` matches only `*.sln`; the string-extension variant matches
`.sln` and `.slnx`. An `.slnx`-only solution fails at startup. *Violates:* DSC-02.

**G-10 — Source discovery includes `obj/` generated files.** (Code-read)
`**/*.cs` under the project directory with no artifact filtering. Harmless today (generated
files declare no public namespace-level classes) but fragile. *Violates:* intent of DSC-06.

**G-11 — Version edge cases crash.** (Code-read)
Fewer than three segments → `Increment` throws (index past list end, e.g. `"1.0" + Patch`);
empty version → `ToString`/comparison throws. Seeds must be written as exactly three
segments until fixed. *Violates:* VER-02/VER-07 robustness intent.

**G-12 — Logging half-implemented.** (Code-read)
`Log.Indent`/`Outdent` semantics are inverted; the indent-aligned message is computed then
discarded; no call sites use indentation; several paths bypass `Log` with bare
`Console.WriteLine`; the "Auto Versioning" line prints the starting path, not the resolved
solution root. *Violates:* LOG-01/LOG-02/LOG-03.

**G-13 — Redundant discovery + hidden cwd dependency in classification.** (Verified via log)
Project files are enumerated four times per run; `CompareSignatures` rebuilds a
`SignaturesToCompare` with an empty path, silently re-discovering from the current working
directory. *Violates:* DSC-07, CLS-07.

## Informative / hygiene

**G-14 — Return type stored per method name, not per overload.** (Code-read)
`MethodType` comes from the first overload encountered; a return-type change on another
overload can go undetected. *Relates to:* SIG-07, R03.

**G-15 — Interfaces, structs, enums, delegates, events, fields are invisible.** (Code-read)
Removing a public interface or renaming an enum member classifies as Patch. Marked `TODO` in
`SolutionBuilder`; now the largest remaining correctness-of-classification gap.
*Relates to:* SIG-02/SIG-10, CLS matrix.

**G-16 — Minimal compilation references degrade foreign type names.** (Code-read)
Cross-project/NuGet types resolve as error symbols; recorded names may lack namespaces.
Stable run-to-run, but theoretically collidable. *Relates to:* SIG-01.

**G-17 — Stale documentation.** (Code-read)
README: old product name ("Auto-Version"), old package version in examples, "must be
post-build so the dlls exist" rationale (extraction is source-based; packaged targets run
pre-build), `UsingTask` instructions (see G-02). *Relates to:* MSB-02/MSB-04.

**G-18 — Test hygiene.** (Code-read)
`Experimental.Debug` carries a hard-coded absolute machine path;
`Test.csproj` references non-existent `SampleCsProj.xml`/`TargetCsProj.xml` content files.
*Relates to:* TST-07.

**G-19 — Dead/vestigial code and an implicit dependency.** (Verified by grep)
Never used: `CsProjSignature`, `ExtendIList.AddIfNew`, `ExtendDirectoryInfo.GetSubDirectory`,
`ExtendString.GetXmlNodeValue`, the commented-out `AdhocWorkspace` path in `SolutionBuilder`
(and `ExtendFileInfo.GetFileText`, referenced only from that dead block). `Solution.cs`
carries Newtonsoft.Json attributes with no direct package reference (compiles via a
transitive dependency) even though persistence is XML. *Relates to:* PKG-07.

## Resolved

Retained so the IDs are never reused and so references from issues/commits still resolve.

**G-07 — Adding a new method or property classified as Patch.** ✅ Resolved
Fixed by adding rules R15 [`MethodAdded`](../src/EasySemVer/Evaluators/MethodAdded.cs) and
R16 [`PropertyAdded`](../src/EasySemVer/Evaluators/PropertyAdded.cs) (both Minor), registered
in `CompareSignatures`, each with a test class. Per CLS-02 they inspect `ClassHistory` pairs
only, so members of a brand-new class remain R05's concern. *Satisfies:* CLS-05.

**G-08 — Optional→required parameter classified as Minor (breaking change missed).**
✅ Resolved — fixed by adding rule R17
[`MethodInputParameterMadeRequired`](../src/EasySemVer/Evaluators/MethodInputParameterMadeRequired.cs)
(Major). It reuses R02's count/names/types overload matcher but fires only on the breaking
direction (`IsRequired` false→true), so required→optional stays Minor via R04 — locked by a
regression test. R02 itself was deliberately left requiredness-blind; folding `IsRequired`
into its matcher would have made the non-breaking direction Major too. *Satisfies:* CLS-06.
