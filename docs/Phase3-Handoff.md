# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.

## Where things stand (2026-06-30)

- **`runtime` is the foundation and the current frontier — now at ~222 compile errors** (down from
  952 at the start of the campaign, 2769 mid-campaign). It is the bottom of the dependency graph, so
  it gates the entire upper stdlib. It is the **sole failing project**, but read the next bullet.
- **Manual conversions live in `src/core` and must be restored over the auto output for measurement.**
  The user hand-finishes certain stdlib files in `src/core` marked `[module: GoManualConversion]` (the
  converter skips re-converting them) or named `*_impl.cs`. A fresh reconvert into an empty scratchpad
  dir does NOT trigger the skip (it checks the destination file), so the overlay must re-copy them —
  `overlay.sh` now does this after the cs/csproj copy. **The canonical unsafe.Pointer model is here:**
  `core/sync/atomic/type.cs` stores `atomic.Pointer<T>` as a managed `ж<T>` (Volatile/Interlocked +
  `nilCanon`), NOT a `nuint` round-trip; `reflectlite/value.cs` uses `object? m_target`. *Where Go
  stores a managed pointer via `unsafe.Pointer`, the C# model holds the `ж<T>`/`object` DIRECTLY* — the
  guiding principle for all S1 work (see the `go2cs-manual-conversions` memory).
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

- [~] **S1 — `unsafe.Pointer` / pointer-conversion modeling** *(re-characterized 2026-06-30; one contained
  fix landed, the bulk is multi-session architectural).* **What landed:** `ef279eab3` — the
  `(*Base)(p)` identical-underlying pointer reinterpret now derefs a genuine box arg before the value
  conversion (runtime/pinner `(*pinnerBits)(newMarkBits(…))`); CS0030 59→58, runtime 262→261, zero churn,
  test `NamedPointerReinterpret`. **CORRECTED CHARACTERIZATION (the original "~80, CS0030 59 + CS0021 12 +
  CS1510 9" estimate over-counted S1):**
  - **CS1510 ×9 is NOT S1 — it is S2** (ref-receiver method on a value-deref rvalue: `(~…).wbBuf.get2()`,
    `(~getg()).schedlink.set(…)`). The `unsafe.Pointer.FromRef(ref X.val)` lines actually **compile** (a
    minimal repro confirms `ref (rvalue).val` on a ref-returning property is legal). Moved to S2.
  - **CS0021 splits:** only `malloc.cs` ×2 is the genuine S1 cast-then-index `(*[2]uint64)(x)[i]` (and it
    compiles-but-CRASHES — `(ж<array<E>>)(uintptr)` does an immediate raw `*(array*)addr` deref of a
    managed type; not runtime-testable). The rest (mgcscavenge/type/proc/traceback) is named-type-over-
    array/map **indexer forwarding** = the S6/`pallocBits`/`winlibcall` family, not S1.
  - **CS0030 bulk (~50: map ×16, iface, lfstack ×5, mstats/profbuf/mgcsweep, runtime2 guintptr/muintptr/
    puintptr, gclinkptr) is the project's explicitly-accepted "memory-layout-dependent, will not work as
    expected" runtime-unsafe code** (CLAUDE.md). These store a *managed pointer as a `uintptr`/`unsafe.Pointer`*,
    which a raw round-trip cannot recover. The goal for them is **COMPILE-ONLY** (unblock dependents); a
    correct runtime test is impossible by design. **The correct model is the user's managed-referent
    approach** (hold `ж<T>` directly — see *Where things stand* + `go2cs-manual-conversions` memory): the
    runtime `guintptr/muintptr/puintptr/gclinkptr/lfstack` types must be **hand-rewritten to hold managed
    refs** (the same play as the promoted `atomic.Pointer<T>`), each a per-type effort. **This is genuinely
    multi-session** and should be done WITH the user's model, NOT via a raw-uintptr round-trip (which
    compiles-but-crashes — exactly the reverted-fix trap). Resume S1 as a dedicated managed-referent
    redesign session once the cheaper S2/S3 buckets are cleared.
- [~] **S2 — pointer-deref-chain receivers** *(main root landed 2026-06-30; sub-roots remain).* **What
  landed:** `7f0075d4f` — a DIRECT-ж method on a value field-chain rooted at a deref-aliased pointer
  PARAMETER or (direct-ж) RECEIVER now routes through the real nested box `Ꮡp.of(T.Ꮡf1).of(…Ꮡf2)`
  (`Δp.scav.index.find()`, `mp.trace.seqlock.Load()`, `h.userArena.readyList.remove(s)`). Two coordinated
  fixes: convUnaryExpr's `&`-machinery recurses through such a chain (+ uses the RAW box name `Ꮡp` not the
  shadow-renamed `ᏑΔp`); convSelectorExpr routes via a new `exprIsValueFieldOfDerefdPointerRoot` GATED to
  direct-ж (a `[GoRecv]` ref method binds directly — no churn). Runtime CS1929 32→16, total 261→243 (−18),
  zero churn, full suite green. Test `FieldChainBoxReceiver` (write-through verified). **REMAINING S2
  sub-roots (16 CS1929 + 9 CS1510 — each distinct, pick one per session):**
  - **Transitive direct-ж propagation (CS1929 ~6: `scavengeIndex.free`×5 in `mpagealloc`, `mgcmark`
    `limiterEvent.start`, `proc` `timers.take`).** The ENCLOSING method (`free(this ref pageAlloc Δp)`) is
    `[GoRecv] ref` (no box `Ꮡp`), yet it calls a direct-ж method on a value field-chain of its receiver →
    it must be PROMOTED to direct-ж (so `Ꮡp` exists). This is a **signature-changing capture-mode change**
    (captureModeOperations.go) — high blast radius, do FRESH (the memory's repeatedly-flagged delicate area).
  - **CS1510 ×9 — `[GoRecv] ref` method on a `~`-value-deref RVALUE receiver** (`(~getg()).schedlink.set(…)`,
    `(~…).wbBuf.get2()`): the receiver root is a `~`-deref of a call/expr (not an ident param/receiver), so
    the box routing above does not apply. Needs the receiver materialized to an addressable/box form.
  - **Indexed-element atomic (CS1929 ×4: `mprof` `bh.val[i].Load()`/`.StoreNoWB()`).** Array element of
    atomic `UnsafePointer` via a pointer — the `daca4f3a1`/`exprIsIndexedValueElement` area; check why it
    isn't firing for `UnsafePointer`.
  - **Embedding promotion (CS1929: `time` `timeTimer.modify/stop/reset` ×3 → needs `ж<timer>`; `type`
    `Δrtype.Uncommon` → needs `ref abi.Type`).** Overlaps S3 (TypeGenerator embedding) — `timeTimer` embeds
    `timer`, `Δrtype` embeds `abi.Type`.
  - **iface `ж<ж<itabTableType>>.find` ×1** — double-box (a pointer field already a box, over-boxed).
- [~] **S3 — named-type/embedding member forwarding** *(CS1061 26→19; named-over-STRUCT done; remainder
  characterized).* **What landed:** `e59b5865a` — a defined type over a STRUCT (`type winlibcall libcall`)
  now forwards the underlying struct's fields as get/set properties over a MUTABLE `m_value`
  (`TypeGenerator`+`InheritedTypeTemplate`), cleared the 7 `winlibcall` `fn/n/args/r1/r2/err` CS1061. PAIRED
  golib fix: `ж<T>.operator ~` now returns `value.val` not `value.m_val` — `(~c).field` on a field-ref box
  was reading a zero-valued copy (compiles-but-wrong; the winlibcall reads `(~c).n` returned 0). Runtime
  243→236, full suite green, zero converter churn. Test `NamedTypeOverStruct`. **NOTE: 2-level struct
  EMBEDDING promotion already works** (`stackWorkBuf`→`stackWorkBufHdr`→`workbufhdr.nobj`, transitive — see
  ConversionStrategies "type embedding"). **REMAINING CS1061 (~19) — distinct roots:**
  - **`Δrtype` (reflect) embeds CROSS-PACKAGE `abi.Type`** (`.Str`/`.TFlag`/`.Kind_`/`.Size_`, ~4). The
    promotion uses `Context.GetStructDeclaration` (SYNTAX-based — same-package or source-referenced), which
    does NOT resolve a METADATA-only referenced assembly (`internal/abi` built as a DLL). Needs
    metadata-based member resolution (`INamedTypeSymbol.GetMembers()`) — a meatier generator extension.
  - **field-on-box deref-missing (~7: arena/mbitmap/mheap/proc/symtab/trace/mwbbuf `box.field`)** — several
    are S1-tied (`(ж<T>)(uintptr)(new @unsafe.Pointer(…)).field`) or `Ꮡ(~x).field` precedence; heterogeneous.
  - **named-over-ARRAY/MAP member forwarding** = kin to the struct case just done, but the ARRAY case
    (`pallocBits`→IArray, CS1503 ×5 + CS0021 indexer) was **tried & REVERTED** (lazy array allocates on a
    throwaway copy → lost writes; needs EAGER shared backing). The MAP comma-ok case (`type.cs seen[tp,ꟷ]`,
    CS0021/CS8130) may be easier (maps are reference types). Also the range/deconstruct CS8130 10 + CS8183 5
    overlap here (`for i := range namedSliceOrSpan` / comma-ok over a named map).
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

This session: re-bucket, then tackle ONE root. Runtime is at ~222. Last session added named-numeric
`++`/`--` operators (CS0266 −7). The recommended next root is **named-numeric ↔ basic numeric conversion
through the underlying** — a CONTAINED sub-root WITHIN the big CS0030 bucket (the rest of CS0030 is the
deferred S1 unsafe.Pointer architecture — skip THAT). CONFIRM with a fresh bucket.

The recommended defect (named-numeric ↔ basic, ~8-9 CS0030): a Go conversion between a named numeric and a
DIFFERENT basic numeric — `uint64(t.Str)` where `Str` is `abi.NameOff`/`TypeOff`/`TextOff` (named int32),
`int64(taggedPointer)`, `long(traceTime)`, and the reverse `TypeOff(ulongExpr)` — emits a plain
`(ulong)namedVal` / `(TypeOff)(ulong)` cast. The `[GoType]` wrapper only converts between the named type
and its EXACT underlying (`NameOff ↔ int32`), so `(ulong)NameOff` is CS0030. The converter must route
through the underlying: `(ulong)(int)namedVal` (named→basic) and `(TypeOff)(int)(ulong)` (basic→named) —
exactly the mirror of the already-handled "converting TO a named numeric" path (convCallExpr.go ~128, the
`NamedNumericConversion` fix) and the `(int)(nuint)(c)` `NamedNumericIntCast` cast. This is the SAME family,
just the named→basic direction (and cross-package named types like `abi.NameOff`). Likely a convCallExpr
conversion-handling fix. **Bonus kin (same root):** `int(c)` on a `num:nuint` named type (`idx`) emits
`(nint)c` (CS0030) — the named→basic value conversion not going through the underlying; a fresh repro
(`int(idx)`) reproduces it. Fixing the through-underlying routing should clear both.

First steps:
1. Reconvert + overlay + build runtime, bucket fresh (overlay.sh restores the core manual files). Read the
   actual CS0030 `'<named>' to '<basic>'` sites (`type.cs` abi offsets, `tracetime.cs`, `tagptr_64bit.cs`),
   and confirm with a `int(idx)` repro.
2. Read the go2cs-phase3-progress memory's named-numeric-conversion notes (NamedNumericConversion /
   NamedNumericIntCast — the through-underlying cast) BEFORE editing.
3. Implement per the Workflow (convCallExpr conversion routing); gate with a behavioral test (named↔basic
   both directions, output-compared) + zero churn via check-no-regression.ps1 + ConversionStrategies.md +
   one focused commit.

ALTERNATE roots if the above stalls (all characterized):
- Range/deconstruct cluster remainder: (A) Span-range (debuglog/os_windows `foreach (var (i,_) in span)` —
  needs Span-VARIABLE tracking + indexed-for; do NOT switch to golib slice<T> which COPIES → lost writes);
  (B) named-over-MAP comma-ok (type.cs `seen[tp,ꟷ]` — note `typesEqual_seen` is actually lifted as
  `[GoType("dyn")]` STRUCT, not a map type — an anonymous-map-param-lifting bug; visitMapType is a stub).

NOTE — other open roots (all characterized in the queue above):
- S3 remainder: `Δrtype` embeds CROSS-PACKAGE `abi.Type` (~4 CS1061) — needs metadata-based member resolution
  in TypeGenerator (`GetStructDeclaration` only resolves source/same-package); field-on-box deref-missing (~7,
  several S1-tied).
- S2 remainder (CS1929 ~16 + CS1510 ~9): transitive direct-ж PROMOTION for a `[GoRecv]` method calling a
  direct-ж method on its receiver's field-chain (signature-changing capture-mode work, do FRESH);
  `~`-deref-rooted CS1510; indexed-element atomic (mprof `bh.val[i]`).
- S4 (CS0029 ~8) pointer-reassign nil-safe re-alias (a box-reassign was tried & REVERTED — NREs on nil).
- S1 CS0030 bulk (~50): the accepted memory-layout-dependent runtime-unsafe code — the ONLY correct fix is
  the user's managed-referent model (hand-rewrite guintptr/muintptr/… to hold `ж<T>` directly), a dedicated
  multi-session redesign, NOT a raw uintptr round-trip (compiles-but-crashes trap).

Closing ritual (REQUIRED at the end): update docs/Phase3-Handoff.md — check off the item with a result note,
refresh the runtime count/date — then rewrite this "Next session prompt" block to point at the next
unchecked item (re-bucket to pick the new top root). Commit the doc update. Then stop and hand me that
prompt to kick off the following session.
```

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` reports **0 errors** across all ~305 packages, with
the green baseline (`src/go2cs.slnx`) and the full behavioral suite still passing. Promote packages
into the baseline (`src/core/<pkg>`) as they go green and stabilize (see CLAUDE.md). Then stop and
stand by — this is THE milestone.
