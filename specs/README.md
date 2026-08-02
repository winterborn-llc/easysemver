# EasySemVer — Implementation Specifications

This folder is a **retroactive specification** of the EasySemVer library: the concrete
requirements for what the current implementation does (and was evidently intended to do).
It was reverse-engineered from the source, the README, the test suite, and a verification
run of the full build + test pipeline.

It is intended to serve two purposes:

1. **A requirements baseline** for the system as built, so future changes (e.g. the planned
   multi-language, folder-based rework) have an explicit contract to preserve or consciously
   change.
2. **An honest status record** — each requirement carries a conformance marker, because the
   current code is mid-rewrite (AutoVersion → EasySemVer) and not everything works yet.

## Documents

| Doc | Scope |
|-----|-------|
| [01-overview.md](01-overview.md) | Product intent, core concepts, processing pipeline, non-goals |
| [02-invocation-and-msbuild-integration.md](02-invocation-and-msbuild-integration.md) | CLI contract, exit codes, MSBuild hook-in |
| [03-packaging-and-distribution.md](03-packaging-and-distribution.md) | NuGet package contents, CI/publishing |
| [04-solution-discovery.md](04-solution-discovery.md) | Finding the solution root, projects, and source files |
| [05-signature-extraction.md](05-signature-extraction.md) | The API-surface model and how it is extracted from C# |
| [06-signature-persistence.md](06-signature-persistence.md) | The `EasySemVer.xml` baseline file: read/write rules |
| [07-change-classification.md](07-change-classification.md) | The evaluator rules that map API diffs to Major/Minor/Patch |
| [08-version-model.md](08-version-model.md) | Version parsing, comparison, incrementing, seed resolution |
| [09-version-synchronization.md](09-version-synchronization.md) | Writing the new version back into every `.csproj` |
| [10-logging-and-error-handling.md](10-logging-and-error-handling.md) | Console output and failure behavior |
| [11-testing.md](11-testing.md) | Required test coverage and current verification results |
| [99-known-gaps.md](99-known-gaps.md) | Consolidated list of deviations, defects, and dead code |
| [12-multi-language-swift-and-folder-model.md](12-multi-language-swift-and-folder-model.md) | **Forward-looking.** The multi-language rework: folder-based invocation, per-language native topologies (`ICsharp*` / `ISwift*`), serializable baseline v2, and Swift support |

> Docs 01–11 and 99 describe the system **as built**. Doc 12 describes the system **to be
> built** and states which of the requirements above it replaces or retires; when the two
> disagree, doc 12 wins and docs 01–11 are updated as that work lands.

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

Statuses were verified on **2026-08-01** against the uncommitted working tree (post-rename
`EasySemVer` sources) by building and testing a copy of the repo:

- `dotnet build` — succeeds (warnings `NU5129`, `CS8604`).
- `dotnet test` (unit project `Test`) — **65/65 pass**.
- `dotnet test` (project `IntegrationTest`) — **aborts**: the tool's persistence step throws
  `System.NotSupportedException: Cannot serialize interface ... IProject`, `Program.Main`
  calls `Environment.Exit(1)`, and the test host crashes. See
  [99-known-gaps.md](99-known-gaps.md) item **G-01**.
