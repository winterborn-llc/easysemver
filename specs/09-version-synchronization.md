# 09 — Version Synchronization (csproj write-back)

How the incremented version is applied to the solution.
Sources: [`CsProjFile.Save` / `UpdateXmlForVersion`](../src/EasySemVer/CodeReader/CsProjFile.cs),
[`SignaturesToCompare.Save`](../src/EasySemVer/Evaluation/SignaturesToCompare.cs).

**SYN-01 — One version, every project.** ✅ design / ❌ currently unreachable
After incrementing, the tool SHALL write the same new version into **every** discovered
`.csproj` (DSC-03) — including test and internal projects — so all counters in the solution
stay in lock-step (OVR-02). *Current state:* the write-back is never reached because the
baseline save that precedes it throws (PER-05/PER-06, gap **G-01**). The write-back code
itself is sound and unit-exercisable.

**SYN-02 — Properties updated.** ✅
For each project the tool SHALL set the inner text of **every occurrence** of the elements
`AssemblyVersion`, `PackageVersion`, and `FileVersion`
([`MagicValues.VersionPropertyNames`](../src/EasySemVer/Settings/MagicValues.cs)) to the new
version string.

**SYN-03 — Never create properties.** ✅
Elements not present in a `.csproj` SHALL NOT be added (OVR-04). A project that declares only
`FileVersion` gets only `FileVersion` updated; a project declaring none is scanned for its
signature but receives no version writes.
ℹ️ Asymmetry with reads: reading takes the *first* occurrence per element (VER-06), writing
updates *all* occurrences — so duplicated/conditional version elements converge to one value
after the first write.

**SYN-04 — File rewrite semantics.** ✅ with side effect
Updates are applied by loading the project XML into a DOM, mutating it, and rewriting the
whole file with normalized indented formatting. ℹ️ Side effects consumers must expect:
original whitespace/attribute formatting is not preserved verbatim, and the file is rewritten
on **every run** (the version always changes, OVR-03). Comments are preserved by the DOM.

**SYN-05 — No transactionality.** ℹ️
Writes are sequential, one file at a time, with no staging or rollback. A failure mid-loop
would leave some projects on the new version and some on the old; the next run self-corrects
via VER-06 (highest wins). This is accepted behavior, not an oversight to "fix" silently —
if stronger guarantees are ever needed, they belong in this spec first.

**SYN-06 — In-memory model stays consistent.** ✅
After a save, the in-memory project model SHALL reflect the written version and XML (the
integration test reads versions back through fresh `CsProjFile` instances, which is the
authoritative check).
