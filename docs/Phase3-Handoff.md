# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.

## Where things stand (2026-06-30, late)

- **`runtime` is the foundation and the current frontier — now at ~159 compile errors** (down from
  952 at the start of the campaign, 2769 mid-campaign). It is the bottom of the dependency graph, so
  it gates the entire upper stdlib. It is the **sole failing project**, but read the next bullet.
- **2026-06-30 (latest): GENERALIZED cross-package type-reference alias emission (`a1d6db87e`,
  subsuming the unsafe-specific `08946c23d`; CS0246 2 → 0, runtime 161 → 159).** A cross-package type
  renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type,
  `@unsafe.Pointer` for the unsafe.Pointer basic — which resolves only via a file-local alias
  (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias was emitted solely from a
  CANONICAL (unaliased) import, so a file reaching a foreign type without one — via type INFERENCE (a
  same-package function returns the foreign type, so the caller never writes `pkg.` and need not import
  it: `preempt.go` `fd := funcdata()`→unsafe.Pointer), a BLANK import (`_ "pkg"`, alias `_`:
  `symtabinl.go`), or a non-canonical ALIAS — failed CS0246. *The user flagged that my first fix was
  unsafe-specific and asked to generalize it — confirmed generic via a `time.Duration` repro.* Fix
  (converter-only, 3 files): `collectTypePackages` (walked from `getTypeName`) records the import path of
  every foreign named type it emits — recursing pointer/slice/array/map/chan/generic-args/func-signature
  so composite elements register — plus pseudo-path `"unsafe"` for the unsafe.Pointer basic;
  `visitImportSpec` records (in `canonicalAliasImported`) paths whose canonical alias an import already
  emitted (`packageUsingAlias` factors the derivation); `visitFile` supplies the alias for the difference
  (idempotent — no duplicate; a non-canonical alias coexists). This is the type-reference analog of the
  method-call `addMethodPackageNamespaceUsing`. Full suite green (193), existing goldens byte-identical;
  adversarially verified head-to-head (no duplicate aliases across 219 stdlib files; unsafe goldens
  byte-identical). Tests `UnsafePointerInferredNoImport` (Basic arm: scalar/composite/blank) +
  `InferredForeignTypeNoImport` (generic named arm: inferred `*strings.Reader` in an `fmt`-only consumer).
  See ConversionStrategies.md "A cross-package type reference emits its `using`…". **Known-inert
  side-effect (optional future cleanup):** `collectTypePackages` runs on every `getTypeName` call incl.
  non-emitting reasoning, so regenerable stdlib output gains a few UNUSED `using alias = …;` directives —
  compile-inert (not even CS8019, which fires only for unused *namespace* usings) and zero golden churn;
  could later be gated to emission paths only. (CS0118 tracetime `unsafe_package is a type used like a
  variable` is a DISTINCT root — not the alias issue — left untouched.)
- **2026-06-30: S2 `~`-deref-rooted receiver materialization landed (`716de3a64`; CS1510
  9 → 0, runtime 169 → 161).** A pointer-receiver method on a value field reached through a pointer
  RVALUE — a call `getg().schedlink.set(…)`, a method-call chain `q.tail.ptr().schedlink.set(…)`, or a
  pointer-element index `batch[i].schedlink.set(…)` — emitted `(~rvalue).field.method(…)`, an rvalue the
  generated `ref` couldn't bind. Fix (converter-only): new predicate `exprIsValueFieldOfPointerRvalue` +
  a pointer-receiver routing branch in convSelectorExpr, and convUnaryExpr's `&base.field` box-field
  branch now accepts a pointer-returning CALL / pointer-ELEMENT index base — materializing
  `root.of(T.Ꮡfield).method(…)`. A type-CONVERSION CallExpr `(*T)(p)` is EXCLUDED (renders as a C# cast;
  `.of(…)` would mis-bind by precedence → kept its `Ꮡ(…)` form, no StdLibInternalAbi churn). Test
  `PointerRvalueFieldReceiver`; full suite green (191). Also re-baselined 3 cast-paren goldens stale
  since `4261cd21a` (`ccca54886` — MethodSelector/RangeVarReassign/UnsafePointerArgPassing, missed by the
  `42e1fa600` rebaseline; the stale-go2cs.exe false-green hid them).
- **Behavioral-suite hygiene note (2026-06-30):** the "reduce redundant cast parentheses" beautify
  (`4261cd21a`) had over-stripped the outer parens of `string()` conversions, leaving the suite RED
  (UnsafeOperations failed to compile — a variadic-spread `.ꓸꓸꓸ` rebound to the cast's inner operand,
  CS1061). Fixed in `61ce1157a` (a `string` target keeps the wrap; `@string` is member-accessible —
  see ConversionStrategies "Basic-target conversions"). The full behavioral suite is green again (190
  projects); if a future "suite was green" claim conflicts with a fresh run, re-run it — beautify
  commits have twice now landed without a full-suite re-run.
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
  overflow, shift-count casts, bitwise-operand casts, named-numeric `++`/`--`, named-numeric↔basic/named
  conversion through the underlying, cross-ASSEMBLY named-numeric implicit-conversion operators through
  the underlying…). The `git log` + the `go2cs-phase3-progress`
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
as items land. As of 2026-06-30 latest (`runtime` = ~159; the `@unsafe`-alias fix above cleared both
CS0246, 161 → 159): CS0030 45, CS1503 24, CS1061 18, CS0021 10, CS1929 9, CS0029 8, CS0121 6, CS0841 5,
CS0266 5, CS0103 5:

- [x] **Empty `struct{}` lift poisoning a `map[K]struct{}` parameter** *(landed 2026-06-30, `ccab3e458`;
  cleared the type.cs `typesEqual` cluster — CS8130 ×2 + CS0021 ×2 + CS1503 — 175 → 169).* The handoff's
  "anonymous-map-param lifting / implement `visitMapType`" diagnosis was **WRONG** — `visitMapType` is
  still a stub and was never the issue. Real root: `convStructType` lifted EVERY `struct{}` composite to
  a `[GoType("dyn")]` named type, including the EMPTY one. For `seen[k] = struct{}{}` the enclosing
  assignment passes the LHS ident (`seen`) into the conversion context to name the lift — so the empty
  struct was lifted to `typesEqual_seen` AND registered under `seen`'s OWN type, the map
  `map[_typePair]struct{}`, in the lifted-type registry. That poisoned every later reference to the map
  type: the parameter rendered as the phantom struct (not `map<_typePair, EmptyStruct>`), so comma-ok
  deconstruction (CS8130) and the two-arg indexer (CS0021) vanished and real-map call sites mismatched
  (CS1503). **Fix (converter-only, `convStructType.go`, +12 lines):** an empty struct short-circuits to
  golib `EmptyStruct` before any lift, mirroring the `!isEmptyStruct` guard `extractStructType` already
  applies everywhere else. Behavioral test `EmptyStructMapSet`; zero golden churn; full suite green. See
  ConversionStrategies.md "...empty struct `struct{}` is never lifted...".
- [x] **Cross-assembly named-numeric implicit-conversion operators** *(landed 2026-06-30, `93bbf6ce5`).*
  A `GoImplicitConv` numeric operator whose body constructs a named-numeric declared in ANOTHER assembly
  was doubly broken: `(NameOff)src.val` (`ulong`→foreign `int32`-named) has no cross-assembly route C#
  selects (CS0030 — the same cast to a LOCAL named type compiles), and where the foreign type would host
  the operator (`partial struct NameOff`, reached via a local alias like runtime's `global using nameOff =
  abi.NameOff` so the cross-package dot is hidden and it records `Inverted`) it declared a phantom empty
  local type (CS1729). **Fix (`ImplicitConvGenerator` + template, contained):** when the `new`-constructed
  side (LH type: source when `Inverted`, else target) is foreign, construct through its underlying basic —
  `new global::go.@internal.abi_package.NameOff((int)src.val)` — and relocate the host into the LOCAL type
  when the source side is foreign. Gated to the foreign-constructed case; same-assembly operators emit
  byte-identically (muintptr↔Δhex unchanged). Cleared 3×CS0030 + 3×CS1729 in runtime (`nameOff`/`typeOff`/
  `textOff` ↔ `Δhex`); 199→181. **No behavioral test** — the trigger is inherently cross-assembly and the
  single-assembly behavioral harness cannot host a foreign named numeric (`internal/*` types are
  un-importable from a test module; baseline stubs expose none; a two-module test hits an unrelated
  converter namespace-mapping gap — `go.<pkgname>_package` vs the consumer's `go2cs.<seg>_package`). Guard
  is the runtime build; full suite stayed green (186/186). See ConversionStrategies.md "Generated
  conversion operators between named numerics of different assemblies".
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
  sub-roots (11 CS1929 + 9 CS1510 — each distinct, pick one per session):**
  - [x] **Transitive direct-ж promotion via MULTI-LEVEL receiver field-chain** *(landed 2026-06-30,
    `f7392e778`; cleared the `scavengeIndex.free`×5 cluster, CS1929 16→11, runtime 181→175).* The
    capture-mode pre-pass's `bodyCallsCaptureModeMethodOnReceiverField` only matched a ONE-level field
    (`recvName.field.method`), so `func (p *pageAlloc) free(){ p.scav.index.free(…) }` (a TWO-level value
    field-chain) was never promoted to direct-ж — its `[GoRecv] ref` receiver has no box `Ꮡp` for the
    routing. Fix (contained, converter-only): generalized the detection to walk the FULL value field-chain
    `recvName.f1.…fn.method` (new `selectorRootsAtReceiverValueFieldChain`, value fields only — a pointer
    hop stops the walk). The existing routing (`exprIsValueFieldOfDerefdPointerRoot`, already multi-level)
    then binds `Ꮡp.of(pageAlloc.Ꮡscav).of(…Ꮡindex).free(…)`; the transitive fixpoint cascades the
    promotion up to the caller (`mheap.freeSpanLocked` → `h.pages.free(…)`). Zero behavioral churn, full
    suite green (186). Test `FieldChainBoxReceiver` extended with `deep.bumpDeep` (`d.mid.c.inc()`, no
    other direct-ж trigger). The `limiterEvent.start`/`timers.take` cases originally grouped here are NOT
    this shape — they are `(~ptrCall).field.method(…)` (deref-of-call root, the CS1510 receiver-
    materialization family), still open.
  - [x] **CS1510 ×9 — `[GoRecv] ref` method on a `~`-value-deref RVALUE receiver** *(landed 2026-06-30,
    `716de3a64`; CS1510 9 → 0, runtime 169 → 161).* `(~getg()).schedlink.set(…)`, `(~batch[i]).schedlink.set(…)`,
    `(~q.tail.ptr()).schedlink.set(…)`, `(~Δp.chunkOf(ci)).scavenged.setRange(…)`, `(~getg().m.p.ptr()).wbBuf.get2()`.
    The receiver root is a pointer-returning CALL or a pointer-ELEMENT index — an rvalue that ALREADY is the
    `ж<T>` box, so it materializes straight through it: `getg().of(g.Ꮡschedlink).set(…)`. Fix (converter-only):
    new `exprIsValueFieldOfPointerRvalue` (value field rooted at a NON-ident, NON-selector pointer-to-struct
    expr) + a pointer-receiver routing branch (convSelectorExpr), and convUnaryExpr's `&base.field` box-field
    branch extended to CALL/INDEX bases — EXCLUDING type-conversion CallExprs (`(*T)(p)` renders as a C# cast;
    `.of(…)` mis-binds by precedence, so S1 reinterprets keep their `Ꮡ(…)` form). Test
    `PointerRvalueFieldReceiver`; zero churn; full suite green (191). See ConversionStrategies.md "The base
    may also be a pointer rvalue…".
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

This session: re-bucket, then tackle ONE root. Runtime is at ~159. Last session GENERALIZED the
cross-package type-reference alias emission (CONVERTER-only, `a1d6db87e`, subsuming the unsafe-specific
`08946c23d`; CS0246 2 → 0, runtime 161 → 159). A cross-package type renders in short-alias form (`pkg.Type`
like `time.Duration`, or `@unsafe.Pointer`), resolving ONLY via a file-local alias (`using time =
time_package;`), previously emitted solely from a CANONICAL (unaliased) import. A file reaching a foreign
type without one — type INFERENCE (a same-package func returns it, so the caller never writes `pkg.`:
`preempt.go` `fd := funcdata()`→unsafe.Pointer), a BLANK import (`_ "pkg"`, alias `_`: `symtabinl.go`), or a
non-canonical ALIAS — hit CS0246. (The user flagged the first fix was unsafe-specific and asked to
generalize; confirmed generic via a `time.Duration` repro.) Fix (3 files): `collectTypePackages` (walked from
`getTypeName`) records the import path of every foreign named type it emits — recursing pointer/slice/array/
map/chan/generic-args/func-sig so composite elements register — plus pseudo-path `"unsafe"` for the
unsafe.Pointer basic; `visitImportSpec` records canonical-imported paths (`canonicalAliasImported`,
`packageUsingAlias` factors the derivation); `visitFile` supplies the alias for the difference (idempotent —
no duplicate; a non-canonical alias coexists). Type-reference analog of the method-call
`addMethodPackageNamespaceUsing`. Tests `UnsafePointerInferredNoImport` (Basic arm: scalar/composite/blank) +
`InferredForeignTypeNoImport` (generic named arm: inferred `*strings.Reader` in an `fmt`-only consumer); full
suite green (193), goldens byte-identical, adversarially verified head-to-head (no dup aliases across 219
stdlib files). Known-inert side-effect: a few UNUSED alias directives in regenerable stdlib output
(compile-inert — not even CS8019; zero golden churn; optional future cleanup = gate `collectTypePackages` to
emission paths). FORCE `cd src/go2cs && go build -o bin/go2cs.exe .` before any "suite green"
claim — the standalone runner only rebuilds the exe when a `.go` is newer, so a committed converter change
false-greens on a stale binary. After any emitted-form change run `run-behavioral.ps1 --update-targets` (post
fresh build) for ALL affected goldens.

⚠ **mprof indexed-element atomic (CS1929 ×4, mprof.cs 303/313/333/335) is NOT a clean root — S1/named-over-array
ENTANGLED; do NOT pick it for the autonomous loop** *(classified 2026-06-30 next-session start, did not attempt).*
`bh.val[i].Load()`/`.StoreNoWB()` where `bh` is `*buckhashArray` and `buckhashArray` is `[N]atomic.UnsafePointer`
— a NAMED-over-array. The element box would be `bh.at<atomic.UnsafePointer>(i)`, but golib `ж<T>.at<TElem>`
requires `val is IArray<TElem>`, which a named-over-array struct does NOT implement with eager shared backing (the
REVERTED `pallocBits`→IArray trap — lazy alloc on a throwaway copy → lost writes). AND the surrounding
`(ж<bucket>)(uintptr)(bh.val[i].Load())` is the S1 runtime-unsafe managed-referent / unsafe.Pointer reinterpret
(compile-ONLY goal). Needs the user's named-over-array eager-shared-backing model (multi-session S3-array + S1),
NOT a contained tweak. Park it.

Recommended NEXT root — re-bucket fresh and pick the cleanest CONTAINED one (VERIFY it isn't itself cross-package /
named-over-array entangled before committing):
- **S2/S3 embedding: `time` `timeTimer.modify/stop/reset` (CS1929 ×3, time.cs ~315/327/341)** — `timeTimer`
  embeds `timer` (value field, SAME package); a promoted pointer-receiver method `t.modify(…)` / `Ꮡt.stop()`
  emits the receiver as `Ꮡt` (`ж<timeTimer>`) but the extension wants `ж<timer>` (CS1929). Fix: route the
  receiver through the embedded-field box `Ꮡt.of(timeTimer.Ꮡtimer).modify(…)`. That exact field-box accessor
  `t.of(timeTimer.Ꮡtimer)` is ALREADY emitted for field access on neighbouring lines (newTimer's
  `racerelease(new @unsafe.Pointer(t.of(timeTimer.Ꮡtimer)))`), so the &-machinery already knows the embedded
  field — the gap is only that convSelectorExpr's box-routing branches (capture-mode / pointer-receiver, ~648–760)
  box as `Ꮡt` WITHOUT descending into the embedded field when the method is PROMOTED via embedding
  (`v.info.Selections[sel].Index()` has >1 element: the embed-field hop(s) then the method). Contained
  converter-only feature; VERIFY same-package (NOT the metadata-only `Δrtype`/`abi.Type` case). Existing embedding
  tests: `PointerEmbeddingPromotion` / `NestedEmbeddingPromotion`. Start in convSelectorExpr.go + convUnaryExpr's
  &-machinery.
- **CS0136 local-shadowing (×4, lock_sema `ns`, proc `Δtrace`/`i`×2)** — an inner-scope local collides with an
  enclosing-scope C# local of the same name (`variableAnalysisOperations.go` shadow-rename). Verify it's a genuine
  shadow-rename miss, not analysis-order noise, before committing.
- **S3 `Δrtype` embeds CROSS-PACKAGE `abi.Type` (CS1061 ×4 + CS1929 ×1, type.cs 34/35/42/46/78 + mbitmap 1899)** —
  metadata-based member resolution in TypeGenerator (`GetStructDeclaration` only resolves source/same-package;
  `internal/abi` is metadata-only). Meatier/architectural-ish; ~6 errors.
- **S6 CS0121 `add` overload (mprof.cs 245/258/267, map.cs 184/191/195)** — free `add(unsafe.Pointer,uintptr)` vs
  the `(*notInHeap).add` companion, both static in `runtime_package`. NOTE (found this session): the call sites
  pass `(uintptr)@unsafe.Pointer.FromRef(ref b)` — a uintptr that converts implicitly to BOTH overloads (the
  ambiguity), so it is SUBTLER than the sibling `UnsafePointerArgPassing` case (which passes the struct directly
  and already resolves). The `(uintptr)` round-trip is S1-modeling territory — verify it isn't entangled first.

OTHER characterized roots (pick by fresh bucket if drift shifts the top):
- S2/S3 EMBEDDING promotion (CS1929 ×4): `time` `timeTimer.modify/stop/reset` (×3, `timeTimer` embeds `timer`
  → needs `ж<timer>`) + `type.cs:42` `Δrtype.Uncommon` (`Δrtype` embeds `abi.Type`). The `Δrtype` one is the
  CROSS-package metadata case below; the `time` one is same-package embedding promotion.
- S3 `Δrtype` (reflect) embeds CROSS-PACKAGE `abi.Type` (CS1061 ×4: type.cs `.Str`/`.TFlag`/`.Kind_` 34/35/46/78
  + the `.Uncommon` CS1929 + mbitmap.cs:1899 `ж<abi.Type>.Size_`) — needs metadata-based member resolution in
  TypeGenerator (`GetStructDeclaration` only resolves source/same-package; `internal/abi` is a metadata-only DLL
  ref). Meatier generator extension; ~6 errors.
- S4 (CS0029 ~8: mgcstack ×2, mheap ×2, panic/proc/string/tracetime) pointer-reassign nil-safe re-alias
  (a box-reassign was tried & REVERTED — NREs on nil; needs a nil-safe re-alias model, not a naive box swap).
- S1 CS0030 bulk (~45): the accepted memory-layout-dependent runtime-unsafe code — the ONLY correct fix is
  the user's managed-referent model (hand-rewrite guintptr/muintptr/… to hold `ж<T>` directly), a dedicated
  multi-session redesign, NOT a raw uintptr round-trip (compiles-but-crashes trap).

First steps:
1. Reconvert + overlay + build runtime, bucket fresh (overlay.sh = measurement-loop memory body, PLUS copy
   src/core manual files — *_impl.cs and the GoManualConversion .cs — over go-src-converted, else
   internal/abi etc. fail on unimplemented partials; the memory's overlay.sh OMITS this, the handoff is
   right). Re-bucket; DO NOT re-pick the mprof indexed-element atomic (classified entangled above — park it).
2. Pick the cleanest CONTAINED root from the candidates above (S6 `add`-overload warm-up, `time` embedding, or
   `Δrtype` metadata) — VERIFY it isn't cross-package / named-over-array entangled before committing. Implement
   per the Workflow; gate with a behavioral test + adversarial verify + zero churn via check-no-regression.ps1
   + ConversionStrategies.md + one focused commit.

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
