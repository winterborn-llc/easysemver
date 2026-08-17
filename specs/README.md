# EasySemVer — Implementation Specifications

This folder is the **specification** of the EasySemVer library: the concrete requirements for
what the implementation does.

It serves two purposes:

1. **A requirements baseline** for the system as built, so future changes have an explicit
   contract to preserve or consciously change.
2. **An honest status record** — each requirement carries a conformance marker, and
   [99-known-gaps.md](99-known-gaps.md) lists everything that still diverges.

Documents 01–11 were written retroactively against the C#-only, solution-rooted implementation,
then rewritten as [12](12-multi-language-swift-and-folder-model.md) landed. They now describe the
folder-based, multi-language system that exists today.

## Documents

| Doc | Scope |
|-----|-------|
| [01-overview.md](01-overview.md) | Product intent, core concepts, processing pipeline, non-goals |
| [02-invocation-and-distribution.md](02-invocation-and-distribution.md) | CLI contract, exit codes, the `--json` report, how the tool is invoked |
| [03-packaging-and-distribution.md](03-packaging-and-distribution.md) | The `dotnet tool` package, the standalone binaries, CI/publishing |
| [04-folder-discovery.md](04-folder-discovery.md) | Finding the folder root, the packageable units in it, and their source |
| [05-signature-extraction.md](05-signature-extraction.md) | The per-language API-surface models and how each is extracted |
| [06-signature-persistence.md](06-signature-persistence.md) | The `EasySemVer.xml` baseline file: format, read/write rules |
| [07-change-classification.md](07-change-classification.md) | The evaluator rules that map API diffs to Major/Minor/Patch |
| [08-version-model.md](08-version-model.md) | Version parsing, comparison, incrementing, seed resolution |
| [09-version-synchronization.md](09-version-synchronization.md) | Writing the new version back into every version location, and into `{{vnext}}` anywhere else |
| [10-logging-and-error-handling.md](10-logging-and-error-handling.md) | Console output and failure behavior |
| [11-testing.md](11-testing.md) | Required test coverage and current verification results |
| [99-known-gaps.md](99-known-gaps.md) | Consolidated list of deviations, defects, and dead code |
| [12-multi-language-swift-and-folder-model.md](12-multi-language-swift-and-folder-model.md) | The multi-language rework, **implemented**: folder-based invocation, per-language native topologies (`ICsharp*` / `ISwift*`), serializable baseline v2, Swift and Xcode support. Holds the Swift rule table (§13) and the settled decisions (§1). |
| [13-shared-rule-bases.md](13-shared-rule-bases.md) | **Not implemented.** A behaviour-preserving refactor: one generic identity-diff base a rule may derive from to stop hand-writing the "is it on the other side?" loop, with deriving optional and the two rule interfaces unchanged. |

> Doc 12 is the design record for the rework; docs 01–11 and 99 were updated as it landed and
> describe the result. Where a requirement was replaced or retired, the older document says so
> and points at the requirement that replaced it.

## Conventions

- **Requirement IDs** are `PREFIX-nn` (e.g. `CLS-03`), unique within this folder, so they can
  be referenced from issues, commits, and future specs.
- **SHALL** denotes mandatory behavior; **SHOULD** recommended; **MAY** optional.
- Each requirement carries a **status marker**:
  - ✅ **Implemented** — behavior confirmed in code and (where noted) by tests.
  - ⚠️ **Deviation** — the intent is met partially or with a caveat; details inline and in
    [99-known-gaps.md](99-known-gaps.md).
  - ❌ **Not working** — the intent is clear from code/docs, but the current implementation
    does not deliver it.
  - ℹ️ **Informative** — a behavior note, not a requirement.
- Requirements are traced to source with relative links, e.g.
  [`src/EasySemVer/Program.cs`](../src/EasySemVer/Program.cs).

## Verification snapshot

Statuses were verified on **2026-08-03** on macOS with .NET SDK 10.0.100 and Swift 6.3.3:

- `dotnet build` — succeeds, **no warnings** (`NU5129` went with **G-04**'s withdrawal).
- `dotnet test` (unit project `Test`) — **509/509 pass**.
- `dotnet test` (project `IntegrationTest`) — **34/34 pass**: 6 `Regression`,
  6 `JsonReportRegression`, 15 `ActionRegression`, 4 SwiftPM, 3 Xcode. G-01 is dead.
- The Swift and Xcode integration tests need no toolchain and are no longer traited: they read
  Package.swift, project.pbxproj and .swift files as text. They used to dominate the runtime — the
  suite was about 18 minutes with them — and now cost fractions of a second.
