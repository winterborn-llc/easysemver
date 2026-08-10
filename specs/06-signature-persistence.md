# 06 — Signature Persistence (the Baseline)

How the previous run's signatures are stored, loaded, and replaced.
Sources: [`BaselineFile.cs`](../src/EasySemVer/Persistence/BaselineFile.cs),
[`ExtendObject.cs`](../src/EasySemVer/Extensions/ExtendObject.cs),
[`MagicValues.cs`](../src/EasySemVer/Settings/MagicValues.cs).

**PER-01 — Location and name.** ✅
The baseline SHALL live at `<folder root>/EasySemVer.xml` (`MagicValues.SignatureFileName`).
One baseline per folder.

**PER-02 — Baseline is shared state.** ℹ️
The baseline file is designed to be **committed to version control**: it is what makes the diff
meaningful across machines and across time, and it is why the README recommends gating runs on
release builds. The README now states this explicitly.

**PER-03 — Missing baseline → empty history.** ✅
If the file does not exist, the run SHALL proceed with an **empty** baseline. Under the
classification rules this makes a first run register every unit as added → **Minor** (NCL-02).

**PER-04 — Unreadable baseline → fail the run.** ✅
If the file exists but cannot be read — malformed XML, wrong root element, or a `formatVersion`
that is absent or not the current one (BAS-03) — the tool SHALL fail the run, exiting 1 with the
path, the underlying exception, and the remedy. It SHALL write nothing first: no baseline, no
version stamped (PER-06), so the damaged file is left as it was found.

This is deliberately not self-healing. A missing baseline (PER-03) is a first run and "everything
is new" is the honest verdict; a baseline that is present and unreadable is history the run was
supposed to classify against, and proceeding past it publishes a version with nothing behind it —
formerly as a warning inside a run that still exited 0, which is where a warning goes unread. A
release cannot be recalled once a package manager has it, so the run stops instead.

Deleting the baseline is the documented way through, and the failure message says so. It costs one
release classified against an empty history — the same cost the old fallback imposed, except that
someone chooses it knowingly.

**PER-05 — Save the new baseline.** ✅ **Resolved** *(was G-01, the blocking defect)*
After classification, the tool SHALL serialize the new signatures and replace the baseline file.

*How it was fixed:* by shape, not by attribute tricks. The persisted graph now consists of
concrete, public, parameterless-constructible types whose members are concrete-typed
(`List<CsharpClass>`, not `List<ICsharpClass>`); the interface view the rules consume is supplied
by **explicit** interface implementation, which `XmlSerializer` does not see. There is no
interface-typed member anywhere on the persisted graph and no separate DTO tree to keep in sync.

**PER-06 — Persistence ordering.** ✅
Within the persistence step, the baseline SHALL be written **before** any version is stamped. It
SHALL NOT be written at all if any discovered unit failed extraction (BAS-06/SWE-05), so a failed
run mutates nothing.

**PER-07 — Format.** ✅ *(BAS-01…BAS-04)*
The baseline SHALL be a self-contained, indented XML document whose root is
`<EasySemVer formatVersion="4">` and whose content is a **flat array of packageable units**, each
carrying its language, unit id, unit kind, folder-root-relative path, and that language's native
signature payload:

```xml
<EasySemVer formatVersion="4">
   <Unit language="csharp" unitId="EasySemVer" unitKind="csproj" path="src/EasySemVer/EasySemVer.csproj">
      <CsharpProject name="EasySemVer"> … </CsharpProject>
   </Unit>
   <Unit language="swift" unitId="Sources/Widgets:Widgets" unitKind="swiftpm-target" path="Sources/Widgets">
      <SwiftModule name="Widgets"> … </SwiftModule>
   </Unit>
</EasySemVer>
```

The envelope is language-agnostic: the payload element is produced and consumed by the owning
provider, so `Persistence/` names no language type and adding a language requires no edit here.

**PER-08 — Determinism.** ✅ *(BAS-04)*
The file SHALL contain no absolute paths, timestamps, machine names, toolchain versions or raw
tool output. Units SHALL be written sorted by `(Language, UnitId)`, and every collection inside a
unit signature sorted by its entity's identity key. Two runs over unchanged source on two
machines SHALL produce byte-identical files. Symbol-graph ordering is not guaranteed by the
toolchain, so the sorting is EasySemVer's job, not the tool's.

**PER-09 — Atomic write.** ✅ *(BAS-06)*
The baseline SHALL be written to a temporary file in the same directory and moved into place, so
a half-written file can never replace a good one.

**PER-10 — No migration between format versions.** ℹ️ *(BAS-03)*
No migration path exists or is wanted; an unknown version degrades to PER-04, which costs one
Minor bump and heals on the next run.
- **v1** never had a valid file anywhere: because of G-01 the writer never succeeded.
- **v2 → v3** was bumped when extraction stopped recording metadata types (SIG-03). A v2 baseline
  holds framework symbols that a v3 run will never produce, so diffing the two would report their
  disappearance as a Major change. Rejecting it outright is the cheaper error.
