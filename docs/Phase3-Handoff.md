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

## Session model — one architectural issue per session

The remaining `runtime` work is **a small number of independent ARCHITECTURAL features**, each a
self-contained, session-sized effort (golib/generator design + converter wiring + behavioral
validation). **Take ONE per session, with a fresh context window** — each is large enough to deserve a
dedicated session, and greening one often un-skips dependents and re-shapes the picture. Work the
**Session queue** below top-to-bottom (ordered by impact / how much each gates).

**Every session ends with a closing ritual (non-negotiable):**

1. Land the fix(es) per the per-defect **Workflow** above (root fix + behavioral test + zero golden
   churn + `ConversionStrategies.md` + commit).
2. **Update THIS doc:** check off the completed queue item (`[x]`) with a one-line result note (commits,
   error delta), and refresh the count + date in *Where things stand*.
3. **Rewrite the *Next session prompt* block** (bottom of this doc) into a ready-to-paste kickoff for the
   *next* unchecked item — its goal, characterization, and first concrete step. Commit the doc update.
4. If `runtime` reached **0**, the frontier moves UP the dependency graph — re-measure the now-visible
   upper stdlib (`bufio`/`bytes`/`strings`/`os`/`fmt`/`reflect`/…) and append new queue items for it (S7+).

A green compile is **necessary but NOT sufficient** for these items: the memory documents several fixes
that compiled with zero churn yet were behaviorally wrong (S4 nil-NRE, S5 dead box, the `pallocBits`
forwarding lost-writes) and were correctly reverted. The behavioral test **and its runtime output** is
the real gate. Validate with `run-behavioral.ps1` / `check-no-regression.ps1` (see *Gotchas*).

## Session queue (ordered; full per-defect detail in the `go2cs-phase3-progress` memory)

Re-bucket a fresh reconvert at the start of each session — counts drift ±10 (nondeterminism) and shift
as items land. As of 2026-06-30 (`runtime` = ~262):

- [ ] **S1 — `unsafe.Pointer` / pointer-conversion modeling** *(the keystone: CS0030 ~59 + CS0021 ~12 +
  CS1510 ~9 + part of CS1503; ~80 errors, gates the most).* `unsafe.Pointer → ж<T>` standalone casts;
  **pointer-to-array conversion `ж<array<E>>` ↔ `ж<NamedArray>`** (the `(*[2]uint64)(x)` family — the
  `ImplicitConvGenerator` gap); the **cast-then-index precedence** fix (`(*T)(p)[i]` → CS0021) is PAIRED
  with the conversion and only testable once it exists; `unsafe.Pointer.FromRef(ref X.val)` non-lvalue
  (CS1510). The `unsafe.Pointer`=`nuint` round-trip is a known runtime limitation; the gaps interact —
  **design the model first** (how `ж<T>` / `unsafe.Pointer` / `Span<T>` interconvert), then wire converter
  + golib + `ImplicitConvGenerator` together.
- [ ] **S2 — pointer-deref-chain atomic receivers** *(CS1929 ~32).* `mp.mLockProfile.waitTime.Load()`,
  `sgp.g.selectDone.CompareAndSwap(…)`: the receiver is built with `(~mp)` (value-deref rvalue) but the
  ж-only atomic overload needs `.val` (ref-deref). Extend the `~`-vs-`.val` deref-form distinction to
  pointer-receiver-method receivers. **Delicate copy-vs-box deref area — the memory warns it regressed
  under fatigue; validate every churn.** (The global value-field atomic case is already fixed via
  box-the-global + `Ꮡg.of(T.Ꮡfield)`.)
- [ ] **S3 — TypeGenerator 2-level embedding promotion** *(CS1061 ~26).* `stackWorkBuf` embeds
  `stackWorkBufHdr` which has `nobj`; `workbuf.obj`; promoted fields on `abi.Type` (`.Typ`/`.Itab`).
  Generator work — extend `TypeGenerator`'s promotion to nested (2-level) embedding, rebuild the
  analyzer, regen.
- [ ] **S4 — pointer-reassign nil-safe re-alias model** *(CS0029 ~11).* `gp = getg()` where `gp` is a
  deref-aliased `*g` param (`ref var gp = ref Ꮡgp.val`) can't take a `ж<g>`. A box-reassign-then-realias
  (`Ꮡgp = …; gp = ref Ꮡgp.val`) was implemented (−32!) but **REVERTED — it eagerly derefs the box, so a
  nil reassignment NREs** (the behavioral test caught it; compile+churn looked clean). The fix is a
  nil-safe re-alias model (golib `ж<T>.val` nil handling, or a deferred/conditional re-alias). Canonical
  repro documented in memory.
- [ ] **S5 — closure-captured-pointer box + pointer-to-pointer aliasing** *(CS0103 ~6 + part of CS1503).*
  `ᏑmToFlush` in `traceAdvance` (a `*m` whose `&local` is taken inside `systemstack(func(){…})` —
  `convertToHeapTypeDecl` short-circuits boxing for inherently-heap pointer types); also
  `unsafe.Pointer`-param-treated-as-box (`return Ꮡzero` for a `zero unsafe.Pointer` param). The
  decl-side-only fix was tried + REVERTED (dead box: plain `&pointerVar` uses the `Ꮡ(copy)` copy-box, not
  the declared box) — needs a COORDINATED decl+usage fix = the pointer-to-pointer aliasing feature (make
  `&pointerVar` use the declared box when the local is boxed).
- [ ] **S6 — contained sweep** *(the residue; do LAST, or first as a warm-up).* CS0121 `add` overload
  collision (free func vs `RecvGenerator` companion both static `add` in `runtime_package`); CS0119 method
  expression `(*timers).run` → `(ж<timers>).run` (delegate/method-group feature); CS0266 `*byte`
  pointer-walk mis-typing + named-numeric; CS0019 `taggedPointer` named-numeric bitwise; the two
  large-literal latents (>int32 literal as a `uintptr` CALL ARG → CS1503, and as a VAR INIT → CS0266);
  CS0128 `type.cs` escape-hoisted-for-var over-boxing; the CS0841/CS0411/CS0136/CS0117/CS0149 tail. Each
  has a SPECIFIC trap — read memory first; several touch the named-numeric operator-ambiguity area.
- [ ] **S7+ — upper stdlib** *(unlocks only after `runtime` = 0).* Re-measure `bufio`/`bytes`/`strings`/
  `os`/`fmt`/`reflect`/… (currently skipped) and append their queue items here.

## Gotchas (these cost real time — see CLAUDE.md + memory for more)

- **Validate with the standalone behavioral runner, not testhost (2026-06-30).** `src/Tests/Behavioral/
  run-behavioral.ps1 [--filter <Name>] [--phase transpile,compile,target,output] [--update-targets]`
  runs the four phases over all **180** behavioral projects **outside testhost** — the old
  `testhost`/`vstest.console` self-lock (`MSB3027`, 0-byte hangs) is structurally gone. Cold ≈2 min /
  warm ≈80s, all 180 green. For a pure converter no-regression check with no compile/run, use
  **`check-no-regression.ps1`** (re-transpiles every behavioral dir, `git status`es the `.cs`;
  byte-identical ⟹ no regression). These supersede the old `dotnet test --filter` / kill-stale-testhost
  dance — prefer them. (The MSTest `BehavioralTests` runner still exists and works; it's just slower and
  lock-prone.)
- **Run `dotnet build <pkg>.csproj` from the REPO ROOT.** A leftover `cd src/go2cs` (from building the
  converter) makes the relative project path resolve wrong → `MSB1009 "project does not exist"` and a
  **false 0-errors** reading. The working dir persists between Bash calls; many slips this session.
- **The standalone runner sidesteps the `*Tests.cs` rebuild.** `run-behavioral.ps1 --filter <Name>`
  builds and runs the project directly (no MSTest assembly), so a NEW project just works — no
  `-p:go2csPath` build dance. *(Only if you fall back to the MSTest path:* `UpdateTestTargets
  --createTargetFiles` adds a `Check<Name>()` to the four `*Tests.cs` for a NEW project, staling
  `--no-build`; build once with `dotnet build …/BehavioralTests.csproj -c Debug
  -p:go2csPath=H:/Projects/go2cs/src/` — FORWARD slashes — then `dotnet test --no-build --filter`.)*
- **`replace_all` on a func def does not touch its call sites** — rename both, or `go run` errors
  "undefined: oldName".
- **Reboots/compactions are survivable.** Converter edits and `scratchpad/recon` persist; just rebuild
  `go2cs.exe` and re-overlay. `overlay.sh` itself dies with the session — recreate it from the
  `go2cs-measurement-loop` memory.
- **`getSanitizedFunctionName` / converter↔generator name agreement is the invariant** — any
  name-shape change can cascade through the generators. Treat it as radioactive.
- **Don't commit `go-src-converted` regens.** It's regenerable; the unit of work is the converter/golib/
  generator fix. Restore with `git checkout HEAD -- src/go-src-converted && git clean -fdq -- src/go-src-converted`.

## Next session prompt

> Paste this block to start the next session. **Each session rewrites this block for its successor** as
> the final step of the closing ritual (point to the next unchecked queue item).

```
Continue Phase 3 of go2cs. Read docs/Phase3-Handoff.md and CLAUDE.md first — they have the goal, the
ALL-SHIPS-RISE principle, the per-defect Workflow, the measurement loop, and the session queue.

This session tackles ONE queue item: S1 — unsafe.Pointer / pointer-conversion modeling (the keystone,
~80 of runtime's ~262 errors: CS0030 ~59 + CS0021 ~12 + CS1510 ~9 + part of CS1503; it gates the most).

The work (design the model BEFORE editing — the gaps interact):
- unsafe.Pointer <-> ж<T> <-> Span<T> interconversion. Decide how golib models these together. The
  unsafe.Pointer = nuint round-trip is a known runtime limitation — work WITH it, don't fight it.
- The pointer-to-array conversion ж<array<E>> <-> ж<NamedArray> (the (*[2]uint64)(x) family) is an
  ImplicitConvGenerator gap — likely the linchpin; the cast-then-index precedence fix ((*T)(p)[i] ->
  CS0021) is PAIRED with it and only becomes testable once the conversion exists.
- unsafe.Pointer.FromRef(ref X.val) non-lvalue (CS1510) is in the same family.
Fix each at its ROOT across converter / golib / go2cs-gen as needed (all ships rise — do NOT bend golib
or the generators just to compile). A green compile is necessary but NOT sufficient: these have bitten
us with compiles-but-wrong (lost writes, NRE) — the behavioral test AND its runtime output is the gate.

First steps:
1. Reconvert + overlay + build runtime, bucket fresh (see the measurement loop). Confirm the S1 bucket
   sizes and read the actual CS0030/CS0021/CS1510 sites in runtime to ground the design.
2. Read the go2cs-phase3-progress memory's unsafe.Pointer / pointer-array / cast-then-index notes
   (prior attempts + the documented traps) before designing.
3. Implement per the Workflow (root fix + behavioral test via run-behavioral.ps1 --filter + zero churn
   via check-no-regression.ps1 + ConversionStrategies.md + one focused commit per root fix).

Closing ritual (REQUIRED at the end): update docs/Phase3-Handoff.md — check off S1 with a result note,
refresh the runtime count/date — then rewrite this "Next session prompt" block to point at S2
(pointer-deref-chain atomic receivers, CS1929). Commit the doc update. Then stop and hand me the S2
prompt to kick off the following session.
```

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` reports **0 errors** across all ~305 packages, with
the green baseline (`src/go2cs.slnx`) and the full behavioral suite still passing. Promote packages
into the baseline (`src/core/<pkg>`) as they go green and stabilize (see CLAUDE.md). Then stop and
stand by — this is THE milestone.
