# 02 — Invocation and MSBuild Integration

Sources: [`Program.cs`](../src/EasySemVer/Program.cs),
[`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`Resources/Winterborn.EasySemVer.targets`](../src/EasySemVer/Resources/Winterborn.EasySemVer.targets),
[`README.md`](../README.md).

## Command-line contract

**CLI-01 — Deliverable is a console executable.** ✅
The tool SHALL build as a console executable (`OutputType=Exe`) with assembly name
`Winterborn.Library.EasySemVer`, multi-targeting `net8.0`, `net9.0`, and `net10.0`
(`LangVersion` 14). `public static int Main(params string[] args)` is public so tests can invoke
a run in-process, and returns its exit code rather than calling `Environment.Exit` — see ERR-03.

**CLI-02 — At most one directory argument.** ✅
The tool SHALL accept zero or one directory argument, plus any recognised flags. Two or more
directories SHALL fail the run with the message that a single directory parameter is required.

**CLI-03 — Zero arguments → current directory.** ✅
With no arguments, the starting directory SHALL be the process's current working directory.
This is the primary mode when invoked from an MSBuild target, whose working directory is the
project directory.

**CLI-04 — The directory argument is the folder root.** ✅ *(replaced by FLD-01; G-06 resolved)*
The single directory argument SHALL be the folder root, resolved to a full path. It is used, not
merely validated. Implemented in [`RunOptions.Parse`](../src/EasySemVer/Settings/RunOptions.cs).

**CLI-05 — No solution-root discovery.** ✅ *(retired by FLD-02; G-09 resolved)*
The tool SHALL NOT search ancestors for a `.sln`/`.slnx` and SHALL NOT require one to exist. A
folder containing no solution file is a valid, ordinary input. Both walk-up helpers are deleted.

**CLI-06 — Exit codes.** ✅
The process SHALL exit `0` on success. Any unhandled error SHALL print the full exception to
stdout and exit `1`. (See [10-logging-and-error-handling.md](10-logging-and-error-handling.md)
for why exit 1 is a deliberate build-failure signal.)

**CLI-07 — `--dry-run` previews without writing.** ✅ *(added per §20 O-04)*
`--dry-run` SHALL run discovery, extraction, classification and version resolution, log the
verdict and the version it would produce, and write nothing: no baseline, no version stamps. A
dry run is not a release, so it does not conflict with OVR-03. Without the flag, every successful
invocation increments by at least Patch and rewrites state.

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
NuGet does not auto-import it (`NU5129`, **G-04**). The argument it passes is now honoured
(CLI-04), so once (a) and (b) are fixed the mechanism works. Until then the README documents
direct invocation instead. See §20 **O-05**.

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
