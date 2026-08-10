# Contributing to EasySemVer

This is the guide for working *on* EasySemVer. For using it, see the [README](README.md).

Read [the rules of the design](#the-rules-of-the-design) before you propose a shape that differs
from the one that is here. Most of those rules exist to prevent a specific failure that has
already happened once, and the ones about releases exist because a version that goes out wrong
cannot be recalled.

## The specs are the contract

[`specs/`](specs/) is the requirement set, not a description written afterwards. Every requirement
has an id — `CLS-03`, `ML-02`, `ACT-11` — and a status marker, code is traced back to it with
relative links, and [99-known-gaps.md](specs/99-known-gaps.md) is the honest list of everything
that still diverges. Start at [specs/README.md](specs/README.md); doc
[01](specs/01-overview.md) is the shortest path into the whole design, and
[12](specs/12-multi-language-swift-and-folder-model.md) is the record of the multi-language rework
and holds the settled decisions behind most of what follows.

A change either conforms to the requirements or changes them. Both are fine; doing neither is not.
If behaviour moves, the spec doc moves in the same commit — including its status marker, and the
gaps list if the change opens or closes one.

Requirement ids are how the code explains itself. A comment saying `NCL-03` is pointing at a
paragraph that says why, so that the reasoning survives the person who had it. Cite them the same
way when you add code.

---

# The architecture

## A run, end to end

One pass, in [`Evaluation/VersioningRun.cs`](src/EasySemVer/Evaluation/VersioningRun.cs):

```
Program.Main
  └─ RunOptions.Parse            the folder root and the flags (Settings/)
  └─ LanguageProviders.Create    every language, and the version conventions they share
  └─ VersioningRun.Execute
       ├─ provider.Discover      one walk of the tree per language → packageable units
       ├─ provider.IsTestCode    units that carry a version but no contract
       ├─ provider.Extract       fills each unit's signature — fatal on failure
       ├─ BaselineFile.Read      the previous run's units, or empty, or fatal
       ├─ ChangeClassifier       splits units by language, asks each provider for findings
       ├─ GetSeedVersion         highest version in any source in any unit, else 0.0.0
       ├─ BaselineFile.Write     before anything is stamped
       ├─ provider.WriteVersion  the one new version into every existing location
       └─ Publish                text, JSON and GitHub Actions renderings of one document
```

Two orderings in there are load-bearing rather than incidental. The baseline is written *before*
any version is stamped, so a crash mid-write cannot leave a stamped version with no history behind
it. The JSON report is written *last*, so a report exists only if everything it describes actually
happened.

`Program.Main` returns its exit code rather than calling `Environment.Exit`, so a run invoked
in-process — which is how the integration tests invoke it — cannot take the host down with it.

## The language seam

The whole architecture is one plugin boundary:
[`ILanguageProvider`](src/EasySemVer/Interfaces/ILanguageProvider.cs). A language contributes
discovery, test-code detection, extraction, classification, version reading, version writing, and
baseline serialization — and nothing else in the tool knows it exists.

| Folder | What lives there |
|--------|------------------|
| [`Providers/`](src/EasySemVer/Providers) | One provider per language, plus the two registries |
| [`Interfaces/Csharp/`](src/EasySemVer/Interfaces/Csharp), [`Interfaces/Swift/`](src/EasySemVer/Interfaces/Swift) | Each language's own topology, as interfaces |
| [`DataObject/Csharp/`](src/EasySemVer/DataObject/Csharp), `DataObject/Swift/` | The implementations of those |
| [`CodeReader/Csharp/`](src/EasySemVer/CodeReader/Csharp), `CodeReader/Swift/` | Extraction: source in, signature out |
| [`Evaluators/Csharp/`](src/EasySemVer/Evaluators/Csharp), `Evaluators/Swift/` | One class per rule, plus that language's rule list |
| [`Evaluation/`](src/EasySemVer/Evaluation) | The neutral core: the run, the classifier, the pairing, the walk |
| [`Persistence/`](src/EasySemVer/Persistence) | The baseline file, which treats every signature as opaque |
| [`Reporting/`](src/EasySemVer/Reporting) | Three formatters over one report document |
| [`Process/`](src/EasySemVer/Process) | The one place anything shells out |

**The neutral core knows exactly three things**: the packageable unit, the `VersionType` verdict,
and the `Version` value. There is no shared abstraction of "type" or "member" and there never will
be. A Swift protocol is a protocol; a C# record is a record; neither is expressed in the other's
vocabulary, because every previous attempt at a common model lost the distinctions that decide
whether a change is breaking.

`ChangeClassifier` runs no rules of its own. It applies the test-code filter, splits the units by
language, and hands each provider its own slice — so a provider never sees another language's work
and never has to check. Even the two rules every language agrees on, "a unit appeared" and "a unit
vanished", are owned per language: they subclass
[`UnitExistence`](src/EasySemVer/Evaluators/UnitExistence.cs) rather than being neutral, so that
agreeing costs a subclass and disagreeing costs an override.

**Adding a language is one provider, one set of folders, and one line** in
[`LanguageProviders.cs`](src/EasySemVer/Providers/LanguageProviders.cs) — with no edit to the core.
That is the acceptance test for whether the seam is still right. If your language needs a change
inside `Evaluation/` or `Persistence/` to work, the seam is wrong and *that* is the bug.

Version conventions have the same shape and their own registry:
[`VersionSourceFactories`](src/EasySemVer/Providers/VersionSourceFactories.cs). A new place a
version can live — some other manifest, some other constant — is a class implementing
`IDiscoverVersionSources` and one line there, not an edit to a provider.

## How each language is read

**C#** is parsed from source with Roslyn
([`CsharpUnitBuilder`](src/EasySemVer/CodeReader/Csharp/CsharpUnitBuilder.cs)) — a compilation over
the unit's own syntax trees, never a reference to a built assembly. That matters more than it
looks: framework references would come from whichever runtime the tool happened to run on, so a
symbol reached through metadata would make the baseline a property of the machine that wrote it.
A test asserts that a unit's signature contains what the unit's own source declares and nothing
else.

**Swift** comes from the toolchain's own symbol graph — `swift build` for SwiftPM,
`xcodebuild` for Xcode targets, both with symbol-graph flags, read back by
[`SymbolGraphReader`](src/EasySemVer/CodeReader/Swift/SymbolGraphReader.cs). There is no
hand-rolled Swift parser and there will not be one. One build produces every target's graph in a
package, so they are extracted together and cached rather than rebuilt per unit.

Everything that shells out — swift, xcodebuild, git — goes through
[`IRunProcess`](src/EasySemVer/Interfaces/IRunProcess.cs). That is the seam that lets the tests
stand in for all three without any of them being installed, and the reason the C# suite runs
anywhere.

## Rules

A rule is a small class. It declares what it is, and yields the symbols it found:

```csharp
/// <summary>
/// R23 - an enum member was added. Unlike Swift's S18 this is Minor: C# has no exhaustiveness
/// requirement on a switch over an enum, so existing callers keep compiling.
/// </summary>
public class EnumMemberAdded : IEvaluateCsharpSignatures
{
    public string Rule => "EnumMemberAdded";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures) { … }
}
```

It is registered in its language's `CompareSignatures` list —
[C#](src/EasySemVer/Evaluators/Csharp/CompareSignatures.cs),
[Swift](src/EasySemVer/Evaluators/Swift/CompareSwiftSignatures.cs) — and that is the whole of
wiring it up. The run takes the highest impact anything reported, defaulting to Patch.

Findings are keyed `(language, rule)`, so a rule name must be unique within its language but may
repeat across languages. That pair is a **published contract**: it is what a consumer's script
matches on, so renaming a rule is a breaking change to the report and needs the spec table updated
with it.

The comment above the class carries the reasoning and the spec id, as in the example. A rule whose
severity is surprising — and several are — is a rule whose comment has to say why, because the next
person's instinct will be to "fix" it.

---

# Working on it

## The test suites

| | |
|-|-|
| [`src/Test`](src/Test) | Unit tests. Nothing launched, no toolchain needed. |
| [`src/IntegrationTest`](src/IntegrationTest) | End to end: the real tool, the real Action scripts pulled out of `action.yml`, real git repositories with real remotes. |

```bash
dotnet test src/Test/Test.csproj
dotnet test src/IntegrationTest/IntegrationTest.csproj
```

Rule tests are built from **hand-constructed signature graphs**
([`Build`](src/Test/Build.cs), [`BuildSwift`](src/Test/Swift/BuildSwift.cs)), never from live
extraction. That is deliberate: it means a failing rule test tells you the rule is wrong, and
nothing else. Every rule has a dedicated test class asserting at minimum its declared impact, a
no-difference case that does not fire, and a representative difference that does; a rule whose
correctness is *directional* asserts the non-firing direction too. A rule without one of those is
not finished. See [specs/11](specs/11-testing.md).

Two things to know before the suite surprises you:

- The **Swift and Xcode suites shell out to a real toolchain**, so a full local run wants macOS
  with Xcode configured. CI keeps that requirement inside a single job, which is what lets the
  release job run on Linux.
- The **integration regression versions this repository on purpose** — it invokes the tool against
  the real tree twice and asserts the second run moves Patch by exactly one. It leaves the working
  copy dirty, and it legitimately fails on the first run after a real Major or Minor change.

`ActionRegression` is worth understanding before you touch `action.yml`: it extracts the `run:`
scripts out of the manifest and executes them, so a test can never pass against a script the Action
no longer ships. Only the release download is stubbed.

## Documentation is executed

The two release steps in the [README](README.md) are asserted against
[`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml) byte for byte. The steps this
repository releases itself with *are* the steps it tells other repositories to copy, because
documentation that is not executed rots quietly and the failure lands in someone else's workflow,
on a copy-paste, in a repository we never see.

The release also rewrites `action.yml`'s version pin and the README's `uses:` refs to the release
it is cutting. **Do not hand-edit either.** A stale pin used to be a routine bug here; it is now a
test failure.

## Releasing

You do not release. A push to `main` does: it runs the full suite,
versions the repository with its own previous release, repoints the pins, commits, tags, publishes
to nuget.org over OIDC, builds the five self-contained binaries, attaches them to a draft release,
and publishes it once every archive is on it.

The Action deliberately downloads a **published** binary rather than the one that run builds, so a
change that breaks versioning breaks the *next* release rather than silently mis-versioning the one
that introduced it. The manifest is the opposite — `uses: ./`, from the commit being released —
because v16.0.0 shipped a manifest that failed to load and the pinned workflow could not cut its
own fix.

## Pull requests

- **Branch, push, and let CI run.** [`ci.yml`](.github/workflows/ci.yml) builds and tests every
  branch but `main`, using the same reusable job that gates the release — a branch cannot go green
  against a weaker suite than the one guarding `main`. Fork PRs are not built today: `ci.yml` has
  no `pull_request` trigger, and adding one is the change to make when this repository starts
  taking them.
- **Do not touch versions by hand.** Not the `.csproj` properties, not `EasySemVer.xml`, not
  `action.yml`'s `default: v…`, not the README's `uses:` refs. The release owns all of them, and a
  hand-edit is either overwritten or a merge conflict.
- **Bring the spec with you.** New behaviour gets a requirement; changed behaviour edits the one it
  breaks; a known limitation goes in the gaps list rather than being left for the next person to
  rediscover.
- **Say why in the commit.** Subjects are imperative and sentence case — "Match an overload on its
  generic arity as well as its parameters". The body carries the reasoning, and so do the comments:
  this codebase explains *why* at the point the decision lives, and a change that strips that
  reasoning out is a change that gets undone by accident later.

---

# The rules of the design

Settled, and load-bearing.

**The folder is the unit.** The directory the tool is pointed at *is* the root: no walk-up, no
solution file, no project graph. Inside it, a **packageable unit** is anything independently
shippable — a `.csproj`, a SwiftPM target, an Xcode target — and units are the atoms of add/remove
detection and of version write-back, not of versioning itself. One version per folder root, seeded
from the highest value found anywhere inside it, incremented once, and written back to every
location that already exists. A Swift-only change moves the C# projects' versions too. Per-unit and
per-language version streams are out of scope, and asking for them is asking for a different tool.

**Every run is a release.** There is no "no change" outcome: a successful run always increments by
at least a Patch. Gating is the caller's job.

**Never create a version location.** The tool only updates what a team already seeded. A `.podspec`
with no literal version, a target with no `MARKETING_VERSION`, are read-skipped and write-skipped.
Creating one would be deciding, on someone else's behalf, that a file is a version's home.

**Declarations, never behaviour.** Rules compare the API as written. Nothing infers what code
*does*, and nothing tries to read an API assembled at runtime. A rule that needs to interpret logic
to fire is a rule that will be wrong quietly, which is worse than absent.

**The baseline is deterministic.** Two runs over unchanged source on two machines produce
byte-identical `EasySemVer.xml`. Anything machine-dependent — absolute paths, timestamps, toolchain
versions, file-system enumeration order — is a defect, because it makes every checkout report a
change that is not one. Sorting is the provider's job, at the point it writes.

**Failure is fatal and loud.** Exit 1, print the exception, write nothing. No partial baselines, no
skip-and-warn: a baseline missing a unit under-reports the *next* change, silently, on a run nobody
is watching. The same reasoning makes an unreadable baseline fatal while a missing one is not.

**One document, three renderings.** The console log, the JSON report and the GitHub Actions outputs
are formatters over the same report — never a second traversal of the signatures. A fourth surface
is a fourth formatter, and a `jq` incantation in a workflow is a sign one is missing.

**The verdict is never in the exit code.** "This change is Major" must stay distinguishable from
"the tool fell over".
