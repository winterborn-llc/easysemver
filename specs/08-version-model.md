# 08 — Version Model

The version value type and how the run's starting version is determined.
Sources: [`Version.cs`](../src/EasySemVer/DataObject/Version.cs),
[`CsProjFileVersion.cs`](../src/EasySemVer/CodeReader/Csharp/CsProjFileVersion.cs),
[`CodeReader/Swift/`](../src/EasySemVer/CodeReader/Swift) for the Swift and Xcode sources; tests in
[`TestVersion.cs`](../src/Test/TestVersion.cs) and
[`TestExtractingVersionFromCsProjFile.cs`](../src/Test/TestExtractingVersionFromCsProjFile.cs).

**VER-01 — Format.** ✅
A version SHALL be a dot-separated sequence of non-negative integers, canonically three
segments `MAJOR.MINOR.PATCH`. The parser accepts any segment count; any non-numeric segment
SHALL be rejected. Pre-release/build-metadata suffixes (`-beta`, `+sha`) are **not** supported.
ℹ️ `Version.TryParse` is the form the version sources use, so an unparseable value in one
location is skipped with a warning rather than failing the run (MVR-03).

**VER-02 — Defaults, blanks and short versions.** ✅ *(MVR-02; G-11 resolved)*
The default version is `0.0.0`. A `null`/blank input SHALL behave as `0.0.0`, and a version with
fewer than three segments SHALL be normalized to three on parse (`"1.0"` → `1.0.0`) rather than
crashing `Increment`. With several ecosystems feeding seeds, two-segment values
(`MARKETING_VERSION = 1.2`) are routine input, not a latent edge.

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

**VER-06 — Seed version resolution.** ✅ *(generalized by MVR-03)*
The run's starting version SHALL be the **highest** version present in **any** version source in
**any** unit, in any language:

| Language | Source | Read | Write |
|----------|--------|:----:|:-----:|
| C# | `.csproj` `AssemblyVersion`, `PackageVersion`, `FileVersion` — first occurrence of each element anywhere in the document; highest of the three | ✅ | ✅ |
| Swift/Xcode | `MARKETING_VERSION` in `project.pbxproj`; highest if several configurations set it | ✅ | ✅ |
| Swift/Xcode | `CFBundleShortVersionString` in `Info.plist` | ✅ | ✅ |
| Swift | `s.version` in a `.podspec` | ✅ | ✅ |
| Swift | a `*Version.swift` constant | ✅ | ✅ |
| any | git tags matching `v?MAJOR.MINOR.PATCH` | ✅ | ❌ never (§20 O-02) |

Blank, absent and unparseable values are skipped with a warning, never fatal. This is the
mechanism that re-synchronizes drifted counters: whatever the highest is, everyone gets
`highest + increment` (see [09-version-synchronization.md](09-version-synchronization.md)).

ℹ️ Two deliberate departures from the table as originally specified, both reported at the time:
`MARKETING_VERSION` is read from the pbxproj literal rather than through
`xcodebuild -showBuildSettings -json`, because only a literal can be written back (MVR-04) and
that avoids an extra xcodebuild per project; and build counters `CURRENT_PROJECT_VERSION` /
`CFBundleVersion` are neither read nor written (MVR-06, §20 O-01).

**VER-07 — Robustness edges.** ✅ *(G-11 resolved)*
Versions with fewer than three segments are normalized on parse and increment correctly.
Versions with more than three segments are tolerated: extra segments ride along and are zeroed
when a more-significant segment is bumped.
