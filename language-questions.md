# Language expansion — questions for Andrew

Written while adding languages overnight. **Nothing here blocked the work**; each question records a
decision I made so I could keep going, what I chose, and what it would cost to choose otherwise.
Where a choice is cheap to reverse I say so, and where there is a closing window I say that too.

Read the ⏳ ones first — those get more expensive the longer they wait.

---

## VB.NET — shipped, full signature support

### ⏳ Q-01 — VB units are persisted in `<CsharpProject>` elements. Rename now or never?

**Decided:** left as `<CsharpProject>`, matching your "reuse C# types" answer.

**Why it is time-sensitive.** No repository has a VB baseline yet, because VB shipped tonight. Right
now, changing the element name costs nothing. The moment someone runs the tool on a VB project, it
costs them a forced re-seed (a Major-looking release for a rename nobody made — the G-23 shape).

The options, if you want it changed, are: bump VB's `SignatureVersion` and accept one re-seed for
early adopters, or write the element as `<DotNetProject>` for VB only while C# keeps its own name.
The second is odd but free.

**My read:** leave it. The baseline is an internal format, `formatVersion` already gates it, and the
name is visible only to someone reading the XML. Not worth spending anything on.

### Q-02 — VB `Module` is modelled as a class

A VB `Module` compiles to a sealed class with static members, and that is exactly how it lands in
the signature. A VB developer would call it a Module.

**Decided:** left as a class. It is what the metadata says, and every rule about classes is correct
for it. Modelling it separately would mean a `CsharpModule` type that only VB ever produces, inside
a model shared with C#, which seems worse than the imprecision.

**Reversible:** yes, cheaply, and no baseline consequence if done before VB has users.

### Q-03 — VB and C# spell primitives differently in the shared model

`Public Enum Colour As Byte` records `Byte`; the C# equivalent records `byte`. This is Roslyn
rendering a type in its own compilation's language, and it is *correct* — a VB developer reads
`Byte`.

It is harmless in practice: units are keyed `(language, unit id)` and a VB unit is only ever compared
against its own history. The single case where it bites is a project **converted in place** from C#
to VB keeping the same project name — every primitive-typed member then reads as retyped, and the
run is Major. That was already a Major, so I recorded it in the test rather than fixing it.

**No action needed unless you disagree.**

### Q-04 — should a VB test project's signals be VB-specific?

`IsTestCode` reads the same MSBuild signals as C# — `<IsTestProject>`, or a `PackageReference` to
Microsoft.NET.Test.Sdk / xunit / NUnit / MSTest. I could not find a VB-specific convention that
differs.

**Decided:** shared. Flagging only in case you know of one I do not.

---

## Cross-cutting

### ⏳ Q-05 — what does "supported" mean in the README?

This is the one I would most like your answer on, because it shapes how the rest of the languages
get described.

There are two honest tiers, and they are very different products:

| Tier | What the tool does | What it does **not** do |
|---|---|---|
| **Full** | discovers units, reads the public API, classifies Major/Minor/Patch, stamps versions | — |
| **Version-sync** | discovers units, seeds from their versions, stamps the new version everywhere | never reads an API, so never votes on the change type |

C#, VB.NET and Swift are Full. Everything I add after them is Version-sync first, because a
hand-rolled reader that silently under-reports a breaking change is worse than no reader — that is
the G-23/G-24 lesson applied to new languages.

Version-sync is genuinely useful on its own: a Go or Rust or Python repo gets one coherent version
stamped across every manifest that already carries one, driven by whatever *is* being read. But if
the README says "supports Go" without qualification, someone will reasonably expect Go breaking
changes to be detected, and they will not be.

**Decided, so I could keep going:** every language is listed with its tier, in a table, and
version-sync languages say in plain words that they do not vote on the change type.

**What I need from you:** whether you are happy shipping the version-sync tier at all, or whether a
language should stay unlisted until its reader exists. If the latter, the work is still not wasted —
it is the half every reader needs underneath it.

### Q-06 — a version-sync-only language and the "every run is a release" rule

If a repository contains *only* version-sync languages, nothing ever votes, so every run is a Patch
(and the first is a Minor via NCL-02). That is correct behaviour and it is also a slightly
surprising product: the tool becomes a version stamper rather than a version decider.

**Decided:** correct as-is, and the run log says so explicitly per unit, so it cannot be mistaken for
"no breaking changes found".

---

## Version-sync languages — shipped: JavaScript/TypeScript, Rust, Python, Dart, PHP, Java

### ⏳ Q-08 — Gradle has no honest language id. What should it be?

`build.gradle` / `build.gradle.kts` is shared by **Java, Kotlin and Groovy**, often in the same
module, so a Gradle unit cannot say which language it is. Maven has the same ambiguity in principle
but not in practice, so Java shipped on `pom.xml` alone.

The options as I see them:

1. **A `gradle` "language"** — honest about what it is, dishonest about being a language. The report
   would say `gradle/UnitAdded`, which is at least not wrong.
2. **A `jvm` language** covering Maven and Gradle both — then `java` is the wrong id for the Maven
   provider that already shipped, and renaming it later costs a re-seed.
3. **Guess from the module's source directories** — `src/main/kotlin` vs `src/main/java`. Works
   often, fails silently on mixed modules, which is the failure mode this codebase keeps deciding
   against.

**Decided:** shipped nothing for Gradle. `gradle.properties` support is about four lines once the id
is settled, so this is genuinely blocked on your answer and not on effort.

**My weak preference is (1)**, on the grounds that the unit really is a Gradle project and pretending
otherwise is what creates the silent-failure options.

### Q-09 — Go is unlisted because nothing is writable

`go.mod` has no version field; a Go module's version *is* its git tag. Reading tags already works.
Writing one is doc 12 §20 **O-02**, which you have not confirmed — and it is outward-facing and
effectively irreversible, so I did not decide it for you.

**Decided:** Go stays unlisted rather than shipping a provider that discovers units and changes
nothing.

**If you confirm O-02** (`--tag`, local only, never pushed, off by default), Go becomes worth adding
and so does a `git-tag` write path for PHP and Python, which mostly version by tag too.

### Q-10 — should the Full-tier providers move onto `ManifestLanguageProvider`'s loops?

`ReadVersions` and `WriteVersion` are **character-for-character identical** in the C#, VB and Swift
providers, and now a fourth copy lives on the version-sync base. That is the same duplication the
pairing helpers just removed, one layer up.

**Decided:** left alone. It is a refactor of three shipped Full-tier providers, and doing it
unsupervised on the same night as six new languages is how a version-stamping tool starts writing
versions to the wrong files. It is easy and safe to do deliberately.

### Q-11 — a `package.json` version is matched at up to two spaces of indent

The pattern anchors to `^\s{0,2}"version"` so it cannot match a nested one — a dependency's, or the
`engines` block. Every formatter npm ships indents top-level keys by two, so this holds for
generated and hand-written files alike.

**It would miss** a `package.json` formatted with four-space or tab indentation. The consequence is
a missed version, not a wrong one: the package is discovered, contributes no version source, and is
stamped nowhere. That is the safe direction and it is MVR-04's behaviour anyway.

**Decided:** shipped as-is. Worth revisiting if anyone reports a package that is not being stamped.

### Q-07 — directory exclusions are still global, not per-language

We agreed exclusions should become language-owned and contextual (a `vendor` beside a `go.mod`, a
`target` beside a `Cargo.toml`). **I have not built that yet**, and each new ecosystem makes it more
necessary — `target`, `vendor`, `dist`, `deps` and `blib` are all somebody's real source directory.

**Decided:** for now I am *not* adding any new global exclusions, so nothing new can be silently
swallowed. The cost is that a vendored dependency directory may be discovered as first-party units,
which fails loudly and visibly (new units appear in the baseline) — the failure direction the
`Packages` post-mortem chose deliberately.

This wants doing properly before the language count grows much further.
