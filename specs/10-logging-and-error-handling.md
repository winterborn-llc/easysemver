# 10 — Logging and Error Handling

Sources: [`Log.cs`](../src/EasySemVer/Log.cs), [`Program.cs`](../src/EasySemVer/Program.cs).

## Logging

**LOG-01 — Timestamped stdout.** ⚠️
Diagnostic output SHALL go to stdout with lines prefixed
`yyyy-MM-dd HH:mm:ss.fff` (so MSBuild logs interleave meaningfully).
*Deviation:* only messages routed through `Log` get the prefix; several hot paths
(`CsProjFile.GetSolutionProjectFiles`, `SolutionBuilder`, baseline-corruption warning,
top-level exception dump) use bare `Console.WriteLine`. One logging surface should win.
(Gap **G-12**.)

**LOG-02 — Nested-progress indentation.** ❌
`Log` exposes `Indent`/`Outdent`/`ResetIndent` so nested phases can be visually grouped
(3 spaces per level, continuation lines aligned past the timestamp column).
*Current state:* the semantics are inverted (`Indent()` *decreases* the level, `Outdent()`
increases it), the indented message string is computed and then discarded (the raw message is
written instead), and no call sites use the API yet. Effectively unimplemented. (Gap **G-12**.)

**LOG-03 — Progress events.** ✅
A run SHALL log at minimum: the starting/solution directory ("Auto Versioning: …"), each
project file processed, each project loaded for signature extraction, each classification
rule that fired ("Yay differences: <RuleName>"), and the final change type
("Change Type: Major|Minor|Patch").
ℹ️ The "Auto Versioning" line currently prints the *starting* path rather than the resolved
solution root (minor inaccuracy, part of **G-12**'s cleanup).

## Error handling

**ERR-01 — Fail loudly, fail the build.** ✅
Any unhandled exception SHALL be printed in full to stdout and the process SHALL exit `1`
(CLI-06). Under the MSBuild `Exec` integration a non-zero exit fails the consumer's build —
deliberate: a versioning failure on a release build must be impossible to miss.

**ERR-02 — Tolerated failures.** ✅
The only errors deliberately absorbed are baseline-read failures (PER-03/PER-04): missing or
corrupt history downgrades to "first run," never blocks a release.

**ERR-03 — In-process embedding caveat.** ℹ️
`Program.Main` catches and calls `Environment.Exit(1)`, which terminates the *host* process —
this is what crashes the xUnit test host today (see [11-testing.md](11-testing.md)). If
in-process invocation is a supported scenario (the integration test says it is), the
exit-code decision should move to the outermost shell (e.g. `Main` returns `int`), with
`Execute` throwing. Worth fixing alongside G-01.
