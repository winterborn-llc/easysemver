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

## The version token

Source: [`VersionTokens.cs`](../src/EasySemVer/Persistence/VersionTokens.cs).

**TOK-01 — `{{vnext}}` becomes the new version.** ✅ *(added 2026-08-16)*
After every version location has been written, the run SHALL replace every occurrence of the token
under the folder root with the same version, in files of any kind, and SHALL report each file it
changed as a written file (REP-10).

SYN-01 through SYN-04 cover every location whose *shape* this tool knows: an element in a
`.csproj`, an assignment in a `project.pbxproj`, a constant in a `.swift` file. A release also puts
its number in places that have no shape to know — a changelog heading, a Helm chart's
`appVersion`, a docs page, an installer script — and until now every one of those was somebody's
hand-written `sed` in a release workflow, running against a version scraped from an output. This is
that job, done once, by the thing that already knows the number.

ℹ️ It runs last, after the version locations, and its files join theirs in `writtenFiles`. A
workflow staging that list (ACT-11) therefore commits the stamped changelog with the bump, which is
the only ordering that makes a release commit whole.

**TOK-02 — The token is `{{` + a name + `}}`, and the name defaults to `vnext`.** ✅
The delimiters SHALL be fixed and only the name SHALL be configurable (CLI-13). A caller needs a
way to say "not that word"; letting the delimiters move as well would buy nothing and give a run a
second way to match nothing.

`vnext` rather than `version` because the word must not turn up in ordinary prose by accident, and
because it says *which* version it means: the one this run is about to publish.

**TOK-03 — Every occurrence, in every file.** ✅
Within the folder root, **every** occurrence SHALL be replaced, exactly as SYN-02 treats a version
location. A half-stamped document with no way to tell which half is worse than an unstamped one.

**TOK-04 — The same walk, and the same exclusions.** ✅
The search SHALL use the discovery walk and FLD-04's exclusion list, so `node_modules`, `bin` and a
dependency checkout cost nothing here and cannot be stamped. It SHALL NOT rewrite `EasySemVer.xml`:
the baseline is this tool's own file, written moments earlier by the same run (PER-06), and editing
it here would be a run altering the history the next run reads back.

**TOK-05 — The replacement consumes the token.** ℹ️
After a run the file carries the version, not the placeholder. That is the requirement, not a
limitation of it — what a released changelog must contain is the number — and it is the property to
understand before adopting the feature: a file that wants stamping every release has to be marked
every release, which for a changelog is what writing the next entry does anyway.

**TOK-06 — A dry run names the files and changes none.** ✅
`--dry-run` (CLI-07) SHALL perform the search and log each file it would stamp, and SHALL write
nothing and report an empty `writtenFiles`. The scan is read-only and this is the one write in a run
that consumes what it replaces, so "which files would this take my token out of" is exactly the
question the mode exists to answer.

**TOK-07 — Only text, and everything else survives byte for byte.** ✅
A file whose first 8000 bytes contain a NUL SHALL be treated as binary and skipped, as SHALL a file
that is not valid UTF-8; both are skipped silently, because a repository holding a few hundred
images would otherwise emit a few hundred log lines carrying no decision for a reader to make. A
file that *is* rewritten SHALL be byte-identical outside the replaced token, a leading byte-order
mark included — unlike a `.csproj`, this is somebody's prose, and a diff that should carry one line
must not carry a re-encoding.

ℹ️ A file that cannot be read at all — a dangling symlink, a locked file — is named in the log and
skipped rather than failing the run. The walk reaches every file in the folder, and one unreadable
file is not a reason to lose a release.
ℹ️ The binary sniff reads its own 8000 bytes rather than the whole file, so a repository of large
assets is not loaded into memory to be told what it obviously is.
