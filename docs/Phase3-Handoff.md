# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.

## Where things stand (2026-06-30)

- **`runtime` is the foundation and the current frontier — now at ~262 compile errors** (down from
  952 at the start of the campaign, 2769 mid-campaign). It is the bottom of the dependency graph, so
  it gates the entire upper stdlib. It is the **sole failing project**, but read the next bullet.
- **"runtime is the only failing package" is misleading.** `dotnet build` **skips the dependents of
  a failed project** rather than erroring them. So while `runtime` fails, the entire upper stdlib
  (`bufio`, `bytes`, `strings`, `os`, the full `fmt`, `reflect`, …) is *not being compile-checked at
  all*. The true remaining work is "the whole library"; expect the count to grow (un-skipping
  dependents surfaces their own latent defects) once `runtime` greens — **that is progress** (the
  metric is packages-compiling, not raw error count).
- **The era of cheap contained converter one-offs is essentially over.** The campaign cleared a long
  tail of isolated converter bugs (escape/box-naming, shadow-renames, collision-renames, narrow/native
  numeric casts, labeled loops, type-switch dedup, range-var reassignment, blank discards, constant
  overflow, shift-count casts, bitwise-operand casts…). The `git log` + the `go2cs-phase3-progress`
  memory have the full per-defect history. **What remains in `runtime` is dominated by a handful of
  ARCHITECTURAL features** (see *Current frontier*), not one-line emit fixes.
- The `Δslice` "2 errors" blocker from older handoffs is **solved** and was a measurement artifact —
  see the short historical note below; do not chase it.

## Core principle — ALL SHIPS RISE TOGETHER

The goal is **correct, idiomatic conversion**, not "make the C# compile by any means." Three
components must work in tandem and each is first-class:

1. **`go2cs`** (the converter, `src/go2cs/*.go`) — emits the C#.
2. **`golib`** (the runtime library, `src/core/golib/`) — hand-written Go semantics.
3. **`go2cs-gen`** (the Roslyn source generators, `src/gen/go2cs-gen/`) — compile-time Go semantics.

**Do NOT hack or work around `golib` or the generators just to make converted output compile.** When
a package fails, find the *root cause* and fix it in whichever component is actually wrong:
- If the converter emits wrong/uncompilable C# → fix the converter.
- If `golib` is missing a method, has wrong semantics, or a wrong constraint → fix `golib` properly
  (so the emitted Go-idiomatic call is *correct*, not merely compiling).
- If a generator computes the wrong name/shape/accessibility → fix the generator.

The converter producing correct C# is the *ultimate* goal, but `golib` and the generators are not
scaffolding to be bent around — they are the target runtime and must be made genuinely correct. A
"fix" that makes `go2cs` output compile while leaving `golib` behaviorally wrong is a regression in
disguise. **All three rise together to the finish.**

## Workflow (per defect — non-negotiable)

1. **Measure** with the loop (below); bucket errors by frequency; pick the highest-impact *root* defect.
2. **Fix** it in the correct component (converter / golib / generator).
3. **Add a behavioral test** that exercises the construct — extend an existing `src/Tests/Behavioral/*`
   project if one fits, else add a new one. Follow the CLAUDE.md *"Adding a regression test"* steps
   (scaffold + `go.mod` + `.csproj` + register in `src/go2cs.slnx` + `UpdateTestTargets --createTargetFiles`).
4. **Validate no regression** — full behavioral suite green AND zero golden churn (re-transpile all
   behavioral dirs; byte-identical `.cs` ⟹ no regression). Re-baseline goldens only for *intended*
   output changes.
5. **Record the conversion decision** in [`ConversionStrategies.md`](ConversionStrategies.md) if it's
   a new/changed emitted form (per CLAUDE.md).
6. **Commit** (directly to `master` — solo project). One focused commit per root fix.
7. Rinse and repeat until the full library compiles.

## The measurement loop (foundation-up)

Because dependents are skipped, work **bottom-up**: get the lowest packages green first so the next
layer becomes measurable. Concretely:

```bash
# 1. Reconvert the whole stdlib (ALWAYS -comments; license headers are required). Build the converter
#    first if any src/go2cs/*.go changed: (cd src/go2cs && go build -o bin/go2cs.exe .)
#    Use -parallel 1 for a DETERMINISTIC result when chasing a specific package; -parallel 4 is
#    faster (~3.5 min) for broad sweeps. (Per-file work is sub-second; cost is the type graph load.)
bin/go2cs.exe -stdlib -comments -parallel 4 -go2cspath scratchpad/recon   # writes scratchpad/recon/core/<pkg>

# 2. Overlay fresh .cs + regenerated .csproj onto src/go-src-converted (keeps golib shared in core).
bash scratchpad/overlay.sh scratchpad/recon/core      # recreate overlay.sh from the measurement-loop memory

# 3. Build a package (deps build first; a failed dep's dependents are SKIPPED). RUN FROM THE REPO ROOT
#    — go-src-converted/Directory.Build.props auto-resolves $(go2csPath); no -p flag needed:
dotnet build src/go-src-converted/runtime/runtime.csproj -c Debug -clp:ErrorsOnly | tee scratchpad/build.log
#    or the whole solution: dotnet build src/go-src-converted.sln -c Debug -clp:ErrorsOnly

# 4. Bucket by error code, then by message/file, to find the highest-frequency ROOT defect:
grep -oE 'error CS[0-9]+' scratchpad/build.log | sort | uniq -c | sort -rn
# Verify the errors are actually IN runtime (full path), not a skipped dependent:
grep -ciE 'go-src-converted[\\/]runtime[\\/]' scratchpad/build.log
```

**Metric = packages-compiling, not raw error count.** Fixing a file-inclusion or a foundational
defect often *raises* the count by un-skipping dependents that then surface their own latent bugs —
that is progress.

**⚠ The converter is NONDETERMINISTIC across reconverts** (Go map-iteration order) — raw counts
fluctuate ±10 between two reconverts of the same source (init-func renumbering, alias-resolution
order). **To attribute a delta to your fix, do NOT trust the raw count: cross-reference each error's
`file:line` against the lines your change actually emits** (e.g. confirm zero errors land on the lines
you touched). A clean fix can show a net +1 from noise while genuinely clearing −2.

## Historical: the `Δslice` blocker is SOLVED (don't chase it)

Older handoffs said runtime was "at 2 errors" (the `Δslice` CS0102). That was a **measurement
artifact**: a duplicate `Δslice` declaration in the single `partial class runtime_package` made
Roslyn *suppress member-body analysis for the whole class*, masking ~1960 real latent errors. Fix
(commit `1d7ecaf41`): the type-side collision-avoidance appends the `ᴛ` marker so the TYPE is
`Δsliceᴛ` while the METHOD stays `Δslice` (converter + generators stay in sync). **Lesson that still
applies:** when a foundational fix *raises* the count, sample the "new" errors — if they're genuine
converter defects unrelated to your change, they were **masked, not caused** (unmasking = progress).
The old "renaming slice causes a CS8785 generator-desync cascade" theory was simply wrong.

## Current frontier — the architectural core (the dominant mass)

Re-bucket a fresh reconvert. As of 2026-06-30 the `runtime` buckets are (full history + per-defect
characterization in the `go2cs-phase3-progress` memory):

**~150 of the 262 are the architectural core — these need FEATURE work (golib/generator design +
full-suite behavioral validation), not one-line emit fixes:**

- **CS0030 ~59 — `unsafe.Pointer`/pointer-conversion modeling.** `unsafe.Pointer → ж<T>` standalone
  casts; **pointer-to-array conversion `ж<array<E>>` ↔ `ж<NamedArray>`** (the `(*[2]uint64)(x)`
  family — the `ImplicitConvGenerator` gap; a cast-then-index precedence fix (CS0021) is PAIRED with
  this and only testable once the conversion exists). The `unsafe.Pointer`=`nuint` round-trip is a
  known runtime limitation; this area has interacting gaps that make isolated behavioral tests hard.
- **CS1503 ~33 — `Span`/`IByteSeq`/argument coercions** + the `pallocBits`→IArray generator forwarding
  (an attempted forwarding lost zero-value writes — reverted; needs a value-semantics design).
- **CS1929 ~32 — pointer-deref-chain atomic receivers.** `mp.mLockProfile.waitTime.Load()`,
  `sgp.g.selectDone.CompareAndSwap(…)`: the receiver is built with `(~mp)` (value-deref rvalue) but
  the ж-only atomic overload needs `.val` (ref-deref). Needs the `~`-vs-`.val` deref-form distinction
  extended to pointer-receiver-method receivers. **This is the delicate copy-vs-box deref area the
  memory repeatedly warns regressed under fatigue — do with full churn validation.** (The global
  value-field atomic case was already fixed via box-the-global + `Ꮡg.of(T.Ꮡfield)` routing.)
- **CS1061 ~26 — TypeGenerator embedding promotion.** Concentrated in **2-level nested embedding**
  (`stackWorkBuf` embeds `stackWorkBufHdr` which has `nobj`; `workbuf.obj`) and promoted fields on
  `abi.Type` (`.Typ`/`.Itab`). Generator work (rebuild the analyzer + regen).

**Smaller remaining contained-ish candidates (each has a SPECIFIC documented trap — read memory first):**

- **CS8130 ~12 / CS0021 ~12** — `Span` deconstruction / range-over-Span; cast-then-index precedence
  (paired with the CS0030 pointer-array conversion above).
- **CS0029 ~11 — pointer-reassign nil-safe re-alias.** `gp = getg()` where `gp` is a deref-aliased
  `*g` param (`ref var gp = ref Ꮡgp.val`) can't take a `ж<g>`. A box-reassign-then-realias
  (`Ꮡgp = …; gp = ref Ꮡgp.val`) was implemented (−32!) but **REVERTED — it eagerly derefs the box, so
  a nil reassignment NREs** (the behavioral test caught it; compile+churn looked clean). Blocked on a
  nil-safe re-alias model (golib `ж<T>.val` nil handling). The canonical repro is documented.
- **CS1510 ~9 — `unsafe.Pointer.FromRef(ref X.val)` non-lvalue** (`ref (Ꮡ(copy)).val`) — the
  unsafe.Pointer/FromRef family.
- **CS0266 ~9 — named-numeric arithmetic / `*byte` pointer-walk mis-typing** (`dst := span.heapBits()`
  emitted as a value `byte` not a `ж<byte>` pointer). Fiddly multi-path.
- **CS0121 ~6 — `add` overload collision.** The converted free function `add(unsafe.Pointer, uintptr)`
  and the `RecvGenerator` companion `add(ж<notInHeap>, uintptr)` are both static `add` in
  `runtime_package`; a call resolves ambiguously. Generator/naming concern.
- **CS0103 ~6 — box-not-declared.** Mostly `unsafe.Pointer`-param-treated-as-box (`return Ꮡzero` for a
  `zero unsafe.Pointer` param; `v.val =`/`Ꮡv` for a reassigned pointer param), plus the
  **closure-captured-pointer box** (`ᏑmToFlush` in `traceAdvance`: a `*m` whose `&local` is taken
  inside a `systemstack(func(){…})` — `convertToHeapTypeDecl` short-circuits boxing for inherently-heap
  pointer types). A decl-side-only fix was tried + REVERTED (dead box, because plain `&pointerVar`
  uses the `Ꮡ(copy)` copy-box, not the declared box — needs a COORDINATED decl+usage fix = the
  pointer-to-pointer aliasing feature).
- **CS0019 ~1 — `taggedPointer & ((1<<bits)-1)`** (NAMED `num:nuint` bitwise; the bare-native cases
  are fixed). Casting to the underlying risks the `(Tag)c`-is-CS0030 + named-numeric operator-ambiguity
  traps.
- **CS0119 ~1 — Go method expression `(*timers).run`** emitted as `(ж<timers>).run` (invalid C#).
  Unimplemented feature (method expression → C# delegate/static method group).
- Tail: CS0841/CS0411/CS0136/CS0117/CS0149/CS0128 (≤4 each) — e.g. CS0128 `type.cs` `i`/`Ꮡi` is the
  escape-HOISTED-for-var collision (two sibling boxed `for i` loops in a switch-case both hoisting
  `ref var i = ref heap` to method scope); elusive to repro.
- **Two latent large-literal bugs flagged this session (NOT yet a runtime error each):** a >int32
  literal as a CALL ARG to a `uintptr` param (CS1503) and as a uintptr VAR INIT (CS0266) — same root
  family as the bitwise-operand large-literal fix, different contexts.

**Triage guidance:** the architectural core (CS0030/CS1503/CS1929/CS1061) is now the realistic path to
zero — `unsafe.Pointer`/Span first, since it gates the largest buckets and many smaller ones. Within a
"delicate" bucket, the safe sub-cases are often already done — re-check memory before assuming a whole
bucket is off-limits. Do NOT grind risky numeric/pointer-reassign/deref-form edits under fatigue; the
memory documents several that compiled + had zero churn yet were behaviorally wrong (nil-NRE, dead box,
lost writes) and were correctly reverted. These need a **stable test host** for runtime validation.

## Gotchas (these cost real time — see CLAUDE.md + memory for more)

- **Run `dotnet build <pkg>.csproj` from the REPO ROOT.** A leftover `cd src/go2cs` (from building the
  converter) makes the relative project path resolve wrong → `MSB1009 "project does not exist"` and a
  **false 0-errors** reading. The working dir persists between Bash calls; many slips this session.
- **New-test build when `*Tests.cs` changed.** `UpdateTestTargets --createTargetFiles` adds a
  `Check<Name>()` to the four `*Tests.cs` for a NEW project, so `--no-build` is then stale. Build the
  test assembly with the explicit property once — `dotnet build src/Tests/Behavioral/BehavioralTests/
  BehavioralTests.csproj -c Debug -p:go2csPath=H:/Projects/go2cs/src/` (FORWARD slashes) — then
  `dotnet test --no-build --filter`. Extending an EXISTING project leaves `*Tests.cs` untouched →
  `--no-build` stays valid.
- **`replace_all` on a func def does not touch its call sites** — rename both, or `go run` errors
  "undefined: oldName".
- **Full `dotnet test` testhost hangs** intermittently in long sessions (environmental; hung twice this
  session — 0-byte output). Kill stale `testhost`/`vstest.console`/`dotnet`,
  `dotnet build-server shutdown`, then re-run filtered. Validate via the baseline build + filtered
  behavioral batches + the zero-churn re-transpile check.
- **Reboots/compactions are survivable.** Converter edits and `scratchpad/recon` persist; just rebuild
  `go2cs.exe` and re-overlay. `overlay.sh` itself dies with the session — recreate it from the
  `go2cs-measurement-loop` memory.
- **`getSanitizedFunctionName` / converter↔generator name agreement is the invariant** — any
  name-shape change can cascade through the generators. Treat it as radioactive.
- **Don't commit `go-src-converted` regens.** It's regenerable; the unit of work is the converter/golib/
  generator fix. Restore with `git checkout HEAD -- src/go-src-converted && git clean -fdq -- src/go-src-converted`.

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` reports **0 errors** across all ~305 packages, with
the green baseline (`src/go2cs.slnx`) and the full behavioral suite still passing. Promote packages
into the baseline (`src/core/<pkg>`) as they go green and stabilize (see CLAUDE.md). Then stop and
stand by — this is THE milestone.
