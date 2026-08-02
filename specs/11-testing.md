# 11 — Testing Requirements

Test projects: [`src/Test`](../src/Test) (unit, xUnit) and
[`src/IntegrationTest`](../src/IntegrationTest) (end-to-end, xUnit). Internals are exposed to
both via `InternalsVisibleTo`
([`AssemblySettings.cs`](../src/EasySemVer/Settings/AssemblySettings.cs)).

**TST-01 — Every classification rule is tested.** ✅ 17/17
Each evaluator SHALL have a dedicated test class
([`TestSignatureEvaluators/`](../src/Test/TestSignatureEvaluators)) asserting at minimum:
1. the declared `EvaluationImpact` (locks the rule's severity),
2. a no-difference case returns `false`,
3. a representative difference case returns `true`,
using hand-built `Solution` object graphs (not source parsing) so rules are tested in
isolation from extraction.
ℹ️ Rules whose correctness is *directional* SHALL additionally test the non-firing direction:
[`TestMethodInputParameterMadeRequired`](../src/Test/TestSignatureEvaluators/TestMethodInputParameterMadeRequired.cs)
asserts required→optional returns `false` (locks CLS-06's non-breaking half), and
`TestMethodAdded`/`TestPropertyAdded` assert members of a brand-new class do not fire
(locks the CLS-02 layering).

**TST-02 — Version arithmetic.** ✅
[`TestVersion`](../src/Test/TestVersion.cs) SHALL cover segment extraction and the increment
table, including the `int.MaxValue` rollover cases (VER-04/VER-05).

**TST-03 — csproj version extraction.** ✅
[`TestExtractingVersionFromCsProjFile`](../src/Test/TestExtractingVersionFromCsProjFile.cs)
SHALL cover: a project with no version elements → `0.0.0`, and a project with differing
`AssemblyVersion`/`PackageVersion`/`FileVersion` → the highest wins (VER-06).

**TST-04 — Self-signature smoke test.** ✅
[`TestSelfSignature`](../src/Test/TestSelfSignature.cs) SHALL run real extraction against the
test project's own sources and assert the test class appears under its fully-qualified name
with the expected method, `void` return type, and a single overload (SIG-01…SIG-07 in one
end-to-end pass).

**TST-05 — Integration regression.** ❌ currently aborts
[`Regression.TestProgramInvocation`](../src/IntegrationTest/Regression.cs) SHALL invoke
`Program.Main()` twice against the real repository: the first run establishes/refreshes the
baseline; the second run — with no code changes in between — SHALL increment **Patch by
exactly 1** on the `Test` project's csproj (locks OVR-03 + the whole pipeline). The test's
own comment notes it legitimately fails on the first run after a real Major/Minor change.
*Current state:* the run dies at PER-05 (`NotSupportedException`), `Environment.Exit(1)`
kills the xUnit host, and the run reports "test host process crashed" rather than a failure
(gap **G-01**; see ERR-03 for the exit-code aspect).
ℹ️ Side effect to be aware of: when working, this test **mutates the working tree** (bumps
csproj versions, rewrites `EasySemVer.xml`) — that is by design, but CI and local runs must
expect dirty files afterwards.

**TST-06 — Verification snapshot (2026-08-01).** ℹ️
Executed on a scratch copy of the working tree, .NET SDK 10.0.100:

| Suite | Result |
|-------|--------|
| `dotnet build` | ✅ success — warnings `NU5129` (PKG-05), `CS8604` (null-hygiene in `SolutionBuilder`) |
| `Test` (unit) | ✅ **65/65 passed** (53 at first snapshot + 12 for R15–R17) |
| `IntegrationTest` | ❌ aborted — test host crashed via `Environment.Exit(1)` after `NotSupportedException` in baseline serialization |

**TST-07 — Test hygiene.** ⚠️
[`Experimental.cs`](../src/Test/Experimental.cs) contains a developer scratch test with a
hard-coded absolute path (`/Users/andrew/code/...`) that runs as part of the suite, and
[`Test.csproj`](../src/Test/Test.csproj) references content files `SampleCsProj.xml` /
`TargetCsProj.xml` that do not exist in the repo. Neither currently breaks the suite; both
are cleanup items (gap **G-18**).
