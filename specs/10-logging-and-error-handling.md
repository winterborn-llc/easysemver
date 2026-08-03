# 10 — Logging and Error Handling

Sources: [`Log.cs`](../src/EasySemVer/Log.cs), [`Program.cs`](../src/EasySemVer/Program.cs).

## Logging

**LOG-01 — Timestamped stdout.** ✅ *(ERR-M2; G-12 resolved)*
Diagnostic output SHALL go to stdout with lines prefixed `yyyy-MM-dd HH:mm:ss.fff` (so build logs
interleave meaningfully). Every path routes through `Log`; there are no bare `Console.WriteLine`
calls left in the tool.

**LOG-02 — Nested-progress indentation.** ✅ *(ERR-M2; G-12 resolved)*
`Log` exposes `Indent`/`Outdent`/`ResetIndent` so nested phases are visually grouped: three
spaces per level, with continuation lines of a multi-line message — a stack trace, a tool's
stderr — aligned past the timestamp column so they stay attached to their entry. `Indent`
increases the level and `Outdent` decreases it, and the indented string is the one written.

**LOG-03 — Progress events.** ✅ *(ERR-M3)*
A run SHALL log at minimum: the folder root, the unit count per language, each unit as it is
read, each firing rule with its unit and impact, the aggregate change type, the seed version, the
new version, and each file written. A build-log reader can act on all of it. A dry run replaces
the firing-rule summary with the per-change report CLI-08 requires; a release run keeps the
summary, because its job is to write versions and the detail is a flag away.

## Error handling

**ERR-01 — Fail loudly, fail the build.** ✅
Any unhandled exception SHALL be printed in full to stdout and the process SHALL exit `1`
(CLI-06). Under the MSBuild `Exec` integration a non-zero exit fails the consumer's build —
deliberate: a versioning failure on a release build must be impossible to miss.

**ERR-02 — Tolerated failures.** ✅
The only errors deliberately absorbed are baseline-read failures (PER-03/PER-04): missing or
corrupt history downgrades to "first run," never blocks a release.

**ERR-03 — In-process embedding.** ✅ **Resolved**
`Program.Main` returns `int` rather than calling `Environment.Exit`, so invoking a run in-process
— which the integration tests do — no longer takes the calling host down with it. The runtime
uses the returned value as the process exit code, so the CLI contract (CLI-06) is unchanged.

**ERR-04 — Swift extraction failure is loud and fatal.** ✅ *(SWE-05)*
A Swift extraction failure SHALL name the unit, the exact command that was run, and the tool's
own stderr, so the failure can be reproduced from the build log alone. It is fatal by design
(D-03): no baseline, no version stamp, no partial state.
