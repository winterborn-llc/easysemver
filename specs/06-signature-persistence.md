# 06 — Signature Persistence (the Baseline)

How the previous run's signature is stored, loaded, and replaced.
Sources: [`Program.cs`](../src/EasySemVer/Program.cs) (`GetOlderSignature`),
[`SignaturesToCompare.Save`](../src/EasySemVer/Evaluation/SignaturesToCompare.cs),
[`ExtendObject.Serialize`](../src/EasySemVer/Extensions/ExtendObject.cs),
[`ExtendString.Deserialize`](../src/EasySemVer/Extensions/ExtendString.cs),
[`MagicValues`](../src/EasySemVer/Settings/MagicValues.cs).

**PER-01 — Location and name.** ✅
The baseline SHALL live at `<solution root>/EasySemVer.xml`
(`MagicValues.SignatureFileName`). One baseline per solution.

**PER-02 — Baseline is shared state.** ℹ️
The baseline file is designed to be **committed to version control**: it is what makes the
diff meaningful across machines and across time, and it is why the README warns about merge
conflicts and recommends Release-only runs (MSB-03). Nothing enforces this; it is usage
guidance the README should state explicitly (today it only implies it).

**PER-03 — Missing baseline → empty history.** ✅
If the file does not exist, the run SHALL proceed with an **empty** baseline solution. Under
the classification rules this makes a first run register every project as added → **Minor**
(verified in the integration run output: `Change Type: Minor`).

**PER-04 — Corrupt baseline → warn and continue.** ✅
If the file exists but cannot be deserialized, the tool SHALL print a warning including the
path and the exception, and proceed with an empty baseline (identical outcome to PER-03).
A damaged baseline heals itself on the next successful save; it never blocks a release.

**PER-05 — Save the new baseline.** ❌ **Currently broken**
After classification, the tool SHALL serialize the **new** signature as XML and replace the
baseline file, so the next run diffs against it.
*Current state:* serialization goes through `XmlSerializer`, which cannot serialize the
interface-typed object graph (`Solution : List<IProject>` whose items expose `IProjectClass`,
`IMethodList`, … members). Constructing the serializer throws:

```
System.NotSupportedException: Cannot serialize interface Winterborn.Library.EasySemVer.Interfaces.IProject.
   at ... ExtendObject.Serialize(...)
   at ... SignaturesToCompare.Save(...)
```

Verified 2026-08-01 by running the integration test. Because `Program.Main` treats this as
fatal (CLI-06), **no run can currently complete**, and since deserialization uses the same
type shape, round-tripping needs a serializable representation (concrete DTOs, `[XmlInclude]`
types, or a hand-rolled writer/reader) on both sides. This is the single blocking defect in
the pipeline — everything upstream (discovery, extraction, classification, increment) works.
(Gap **G-01**.)

**PER-06 — Persistence ordering.** ✅ ordering / ❌ consequence
Within the persistence step, the baseline SHALL be written **before** project files are
updated. Consequence today: because the baseline write throws (PER-05), the version
write-back ([09-version-synchronization.md](09-version-synchronization.md)) is never reached
— a run mutates nothing and exits 1. (There is no transactional intent documented; if the
ordering ever matters for crash-recovery, it should be decided explicitly in the rework.)

**PER-07 — Format expectations.** ⚠️ (intent, pending PER-05)
The baseline SHALL be a self-contained XML document representing the full signature model of
[05-signature-extraction.md](05-signature-extraction.md), human-diffable in code review
(indented), and stable across runs with no volatile content (timestamps, absolute paths,
machine names). The intended writer (`ExtendObject.Serialize`) produces indented XML with an
XML declaration; the save path re-writes it UTF-8 to disk. No schema/versioning field exists
yet for the baseline format itself — worth adding when PER-05 is fixed, so future format
changes can migrate old baselines instead of discarding them via PER-04.
