# 08 — Version Model

The version value type and how the run's starting version is determined.
Sources: [`Version.cs`](../src/EasySemVer/DataObject/Version.cs),
[`CsProjFileVersion.cs`](../src/EasySemVer/CodeReader/CsProjFileVersion.cs); tests in
[`TestVersion.cs`](../src/Test/TestVersion.cs) and
[`TestExtractingVersionFromCsProjFile.cs`](../src/Test/TestExtractingVersionFromCsProjFile.cs).

**VER-01 — Format.** ✅
A version SHALL be a dot-separated sequence of non-negative integers, canonically three
segments `MAJOR.MINOR.PATCH`. The parser accepts any segment count; any non-numeric segment
SHALL fail the run with a parse error identifying the input. Pre-release/build-metadata
suffixes (`-beta`, `+sha`) are **not** supported (parse error by the same rule).

**VER-02 — Defaults and blanks.** ⚠️
The default version is `0.0.0`. A `null`/blank input SHALL produce an empty version.
*Deviation:* an empty version is unusable — `ToString()` and comparisons against it throw
(index out of range). Callers today always construct from a non-blank string or the default,
so this is latent. (Gap **G-11**.)

**VER-03 — Ordering and equality.** ✅
Versions SHALL compare by Major, then Minor, then Patch, with absent segments treated as `0`
(`"1.0" == "1.0.0"`). Segments beyond the third are ignored for comparison. Equality is by
value; implicit conversions to/from `string` exist in both directions.

**VER-04 — Increment semantics.** ✅ (tested)
`Increment(changeType)` SHALL bump the segment corresponding to the change type and reset all
lower-significance segments to zero:
`1.0.2 + Patch → 1.0.3`, `1.0.2 + Minor → 1.1.0`, `1.1.1 + Major → 2.0.0`.

**VER-05 — Overflow rollover.** ✅ (tested)
If the target segment already holds `int.MaxValue`, the increment SHALL be applied to the
next-more-significant segment instead (one level of rollover):
`1.0.2147483647 + Patch → 1.1.0`, `1.2147483647.123 + Minor → 2.0.0`.
If Major itself is at `int.MaxValue`, the run SHALL fail with an overflow error.

**VER-06 — Seed version resolution.** ✅ (tested; README step 4)
The run's starting version SHALL be the **highest** version present anywhere in the solution:

- Per project: for each of `AssemblyVersion`, `PackageVersion`, `FileVersion`, read the
  **first** occurrence of that element in the `.csproj` XML (anywhere in the document —
  PropertyGroup conditions are not evaluated); blank/absent values are skipped; the
  project's version is the highest of the values found, or `0.0.0` if none.
- Per solution: the highest of the per-project versions (`0.0.0` if no project declares any).

This is the mechanism that re-synchronizes drifted projects: whatever the highest counter in
the solution is, everyone gets `highest + increment` (see
[09-version-synchronization.md](09-version-synchronization.md)).

**VER-07 — Robustness edges.** ⚠️
Versions with fewer than three segments parse successfully (VER-01) but crash `Increment`
when the target segment index exceeds the list (e.g. `"1.0" + Patch`). Versions with more
than three segments are tolerated: extra segments ride along and are zeroed when a
more-significant segment is bumped. Seeds SHOULD be written as exactly three segments until
G-11 is fixed.
