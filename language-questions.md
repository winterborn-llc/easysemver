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

### Q-07 — directory exclusions are still global, not per-language

We agreed exclusions should become language-owned and contextual (a `vendor` beside a `go.mod`, a
`target` beside a `Cargo.toml`). **I have not built that yet**, and each new ecosystem makes it more
necessary — `target`, `vendor`, `dist`, `deps` and `blib` are all somebody's real source directory.

**Decided:** for now I am *not* adding any new global exclusions, so nothing new can be silently
swallowed. The cost is that a vendored dependency directory may be discovered as first-party units,
which fails loudly and visibly (new units appear in the baseline) — the failure direction the
`Packages` post-mortem chose deliberately.

This wants doing properly before the language count grows much further.
