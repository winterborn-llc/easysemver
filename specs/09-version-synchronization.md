# 09 — Version Synchronization (write-back)

How the incremented version is applied across the folder.
Sources: [`VersioningRun.cs`](../src/EasySemVer/Evaluation/VersioningRun.cs),
[`CsProjFile.cs`](../src/EasySemVer/CodeReader/Csharp/CsProjFile.cs),
[`CodeReader/Swift/`](../src/EasySemVer/CodeReader/Swift),
[`IVersionSource`](../src/EasySemVer/Interfaces/IVersionSource.cs).

**SYN-01 — One version, every location.** ✅ *(generalized by MVR-05; G-01 resolved)*
After incrementing, the tool SHALL write the same new version into **every** version location in
**every** discovered unit, across all languages — including test projects and test targets — so
all counters in the folder stay in lock-step (OVR-02/ML-06). The write-back is now reached: the
baseline save that precedes it works (PER-05).

**SYN-02 — Every occurrence updated.** ✅
Within a location, **every occurrence** SHALL be set to the new version: all three `.csproj`
elements ([`MagicValues.VersionPropertyNames`](../src/EasySemVer/Settings/MagicValues.cs)), every
`MARKETING_VERSION` assignment in a `project.pbxproj`, and so on. Conditional or duplicated
declarations therefore converge to one value after the first write.

**SYN-03 — Never create a location.** ✅ *(generalized by MVR-04)*
A version location that does not already exist SHALL NOT be created. A project declaring only
`FileVersion` gets only `FileVersion` updated; a `.podspec` without a literal `version`, an Xcode
target without `MARKETING_VERSION`, a plist whose value interpolates a build setting, a package
with no version file — all are read-skipped and write-skipped. Opting in stays an explicit act by
the consuming team.
ℹ️ Asymmetry with reads: reading takes the *first* occurrence per csproj element (VER-06), and
the *highest* across sources; writing updates *all* occurrences.

**SYN-04 — File rewrite semantics.** ✅ with side effect
`.csproj` and `Info.plist` updates are applied by loading the XML into a DOM, mutating it, and
rewriting the file; `project.pbxproj`, `.podspec` and Swift version files are updated by targeted
in-place replacement of the literal, leaving the rest of the file byte-identical.
ℹ️ Side effect consumers must expect: for the XML files, original whitespace and attribute
formatting are not preserved verbatim, and the file is rewritten on **every run** (the version
always changes, OVR-03). Comments are preserved by the DOM.

**SYN-05 — No transactionality.** ℹ️
Writes are sequential, one file at a time, with no staging or rollback. A failure mid-loop
would leave some projects on the new version and some on the old; the next run self-corrects
via VER-06 (highest wins). This is accepted behavior, not an oversight to "fix" silently —
if stronger guarantees are ever needed, they belong in this spec first.

**SYN-06 — Reads are the authoritative check.** ✅
The integration tests read versions back off disk through fresh readers after a run, rather than
trusting any in-memory state.

**SYN-07 — Nothing is written when anything failed.** ✅ *(BAS-06/SWE-05)*
If any discovered unit failed extraction, neither the baseline nor any version location is
written. A failed run leaves the working tree byte-identical.
