# 02 — Invocation and MSBuild Integration

Sources: [`Program.cs`](../src/EasySemVer/Program.cs),
[`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`Resources/Winterborn.EasySemVer.targets`](../src/EasySemVer/Resources/Winterborn.EasySemVer.targets),
[`README.md`](../README.md).

## Command-line contract

**CLI-01 — Deliverable is a console executable.** ✅
The tool SHALL build as a console executable (`OutputType=Exe`) with assembly name
`Winterborn.Library.EasySemVer`, multi-targeting `net8.0`, `net9.0`, and `net10.0`
(`LangVersion` 14). `Program.Main(params string[] args)` is public so tests can invoke a run
in-process.

**CLI-02 — At most one argument.** ✅
The tool SHALL accept zero or one command-line argument. Two or more arguments SHALL fail the
run with the message that a single directory parameter is required.

**CLI-03 — Zero arguments → current directory.** ✅
With no arguments, the starting directory SHALL be the process's current working directory.
This is the primary mode when invoked from an MSBuild target, whose working directory is the
project directory.

**CLI-04 — One argument = starting directory.** ⚠️ Deviation
The single argument SHALL specify the starting directory for solution discovery.
*Deviation:* the current implementation validates and then ignores the argument's value —
[`GetDirectoryToUse`](../src/EasySemVer/Program.cs) assigns
`Environment.CurrentDirectory` instead of `args[0]`, and its existence check therefore also
tests the wrong path. Passing an argument behaves identically to passing none. (Gap **G-06**.)

**CLI-05 — Solution root discovery.** ✅
From the starting directory, the tool SHALL walk up the directory tree (starting directory
included) and select the first directory whose *top level* contains at least one `*.sln`
file. If no ancestor contains one, the run SHALL fail with an explanatory error.
ℹ️ Note: this search recognizes only `.sln`, while the helper used elsewhere
([`ExtendString.GetSolutionDirectory`](../src/EasySemVer/Extensions/ExtendString.cs)) also
recognizes `.slnx` — see gap **G-09**.

**CLI-06 — Exit codes.** ✅
The process SHALL exit `0` on success. Any unhandled error SHALL print the full exception to
stdout and exit `1`. (See [10-logging-and-error-handling.md](10-logging-and-error-handling.md)
for why exit 1 is a deliberate build-failure signal.)

**CLI-07 — No dry-run / no-op mode.** ✅ (by design, see OVR-03)
Every successful invocation increments the version by at least Patch and rewrites state.
There is no flag to preview the classification without applying it. ℹ️ A future CLI surface
may want `--dry-run`; nothing in the current contract reserves flags.

## MSBuild integration

Two integration mechanisms exist in the repo; both are in transition (gaps **G-02/G-03/G-04**).

**MSB-01 — Packaged targets hook (current direction).** ⚠️
The NuGet package SHALL ship an MSBuild targets file under `buildTransitive/` so consuming
projects run the tool automatically. The shipped
[`Winterborn.EasySemVer.targets`](../src/EasySemVer/Resources/Winterborn.EasySemVer.targets):

- runs `BeforeTargets="BeforeBuild"` (pre-build),
- resolves the executable at `$(MSBuildThisFileDirectory)..\tools\EasySemVer.exe`
  (overridable via the `EasySemVerExePath` property),
- invokes it with `Exec Command="... "$(SolutionDir)""`.

*Deviations:* (a) the package does not actually pack anything into `tools/`, so the resolved
path never exists (**G-03**); (b) the targets filename does not match the package ID, so
NuGet does not auto-import it (`NU5129`, **G-04**); (c) the argument passed
(`$(SolutionDir)`) is currently ignored per CLI-04 — the run still works only because Exec's
working directory is inside the solution.

**MSB-02 — Documented UsingTask hook (legacy).** ❌
[README.md](../README.md) documents integration via
`<UsingTask TaskName="EasySemVer" AssemblyFile="$(OutDir)Winterborn.Library.EasySemVer.dll"/>`
plus a post-build `<EasySemVer />` invocation. The current assembly contains **no MSBuild
`Task` class** (the old `AutoVersion` task was deleted in the rewrite), so this documented
path cannot work. Either a task class must be reintroduced or the README rewritten around
MSB-01. (Gap **G-02**.)

**MSB-03 — Release-only gating.** ℹ️ Recommended usage
Consumers SHOULD condition the hook on `'$(Configuration)' == 'Release'` (or equivalent) so
day-to-day debug builds do not bump versions, per README step 5. The tool itself performs no
configuration check; gating is the consumer's responsibility.

**MSB-04 — Pre- vs post-build timing.** ℹ️
The packaged targets run *before* build; the README's legacy instructions run *after* build.
The design accepts either — per README, "as long as we're consistent, it doesn't matter which
we choose" — because the increment is relative to the persisted baseline, not to the build
artifacts. The signature is extracted from **source**, not from compiled DLLs, so no ordering
constraint exists. (The README's claim that the process "must be post-build so the dlls exist
and can be inspected" describes the old AutoVersion design and is stale — gap **G-17**.)
