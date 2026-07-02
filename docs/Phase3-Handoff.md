# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.
>
> **⚠ Strategy correction (2026-07-01) — the milestone is a clean COMPILE, not operational.** Operational
> correctness is Phase 4 (Go unit tests). The CS0030/S1 "architectural wall" is a **FORK to SORT**, not a
> stop: native-type ops → convert; managed-referent (`ж<T>` model) → model; raw-metal dragons →
> `[module: GoManualConversion]` stub. Do NOT promote `go-src-converted → core` on a clean compile
> (deferred to post-Go-test, maybe never); copy the hand-owned manual/`*_impl.cs` files BACK into
> `go-src-converted` religiously. See [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md)
> *The corrected end-state* and [`Phase3-AutonomousLoop.md`](Phase3-AutonomousLoop.md) *S1 is a FORK*.

## Where things stand (2026-07-02)

- **`runtime` is the foundation and the current frontier — now at 51 errors, EXACT and
  REPRODUCIBLE** (down from 952 at the start of the campaign). It is the bottom of the dependency
  graph and the **sole failing project**, but read the next bullets.
- **2026-07-02 (latest): promoted embed call on a [GoRecv] ref receiver (`308debde7`; CS0103 −2,
  53 → 51) + the benchmarks-session merge (`8ea5253e5` + `02470cc93`).**
  1. *Converter root:* a pointer-receiver method promoted through a VALUE embed, called on the
     enclosing method's OWN non-direct-ж receiver (`sc.setEmpty()` in `(*scavChunkData).alloc/free`),
     emitted the box descent `Ꮡsc.of(…)` — but a `this ref` receiver has NO box (CS0103). Fix: the
     embedded field of a ref receiver is addressable, so emit the explicit field call
     `sc.scavChunkFlags.setEmpty()` (binds the `[GoRecv] ref` overload, write-through). Includes the
     rendered==raw hardening in convUnaryExpr's `&recv.field` arm (a pointer LOCAL shadowing the
     receiver name emitted `Ꮡ`+raw — nonexistent box, or the WRONG target inside a direct-ж method).
     Whole-stdlib reconvert diff: exactly 5 files — the fix also pre-cleared the same latent CS0103 in
     archive/zip, gcimporter, go/types, image. Test: EmbeddedValuePointerMethod extended (ref-receiver
     form + shadow control, output vs Go). **CS0103 remnant: 1 (mprof.cs:1119 `Ꮡb.stk().at<uintptr>(0)`
     — a spurious Ꮡ on a pointer LOCAL root in `&b.stk()[0]` element-address; b IS the box → should be
     `b.stk().at<uintptr>(0)`. NEXT.)**
  2. *Benchmarks merge:* the "Go/C# runtime performance benchmarks" session landed
     `src/Tests/Performance/` (PerfSort/PerfStartup/PerfString + PerformanceRunner + run-performance.ps1,
     Go vs C# JIT + Native AOT) plus golib perf: allocation-free append span path (span overload is the
     implementation; single-element fast path) and byte-wise `@string` CompareTo/GetHashCode/operators
     (Go byte order — also MORE correct than UTF-16 ordinal for supplementary chars) + a REAL sort.cs
     bug fix (the `Sort(this IntSlice)` convenience overloads were infinitely self-recursive; cast to
     `Interface<T>` disambiguates) + time.UnixNano. **The append optimization was reconciled by hand
     with the slice-aliasing rewrite** (branch predated `86566b9ef`): fast-path/span writes + the
     4-arg shared-view return within cap, m_low-relative copy + detached 0-based view beyond cap.
     Gates: SliceAliasing 4-phase green; full suite 215/215 (Output 195); runtime count unchanged (51).
- **2026-07-02: CS0121 ELIMINATED — the `uintptr → ж<T>` reinterpret operator is now
  EXPLICIT (`d0a935138`; 59 → 53).** The free `add(unsafe.Pointer, uintptr)` vs `(*notInHeap).add`
  static-overload pair was ambiguous at every free-call site passing a boxless-receiver pin
  (`(uintptr)@unsafe.Pointer.FromRef(ref b)` — map.go keys/overflow/setoverflow, mprof.go walkers ×6)
  because the raw-address reinterpret operator was IMPLICIT, so a `uintptr` argument converted to
  both first params. Explicit is right independent of the fix: a silent deref-an-arbitrary-address
  copy-box should never happen implicitly, and all emitted reinterprets already use explicit cast
  syntax `(ж<T>)(uintptr)(p)`. Box→address stays implicit; numeric `uintptr ↔ Pointer` untouched.
  Golib-only (no reconvert); A/B: every other bucket byte-identical; suite 215/215 green. Test:
  `FuncVsMethodOverload` (output vs Go — the overload pair + the ambiguous pin shape + both
  method-call forms). Doc: ConversionStrategies *Converting a Go pointer to unsafe.Pointer* —
  explicit-by-design paragraph.
- **2026-07-02: golib slice ALIASING landed (`86566b9ef`, cherry-picked from the spun-off
  session's `89398c93f`) — reslicing SHARES the backing array.** `slice<T>` now stores `m_capacity`
  (relative to `m_low`); every reslice (range indexer, `.slice(low,high,max)`, `Slice()`) is a
  bounds-checked shared view (`Reslice`, Go-style `SliceBoundsOutOfRange` panics); append writes the
  shared backing in place within capacity and detaches beyond it. Kills the range-sub-slice-detach
  latent AND two more (offset-view beyond-cap append copied from index 0; IReadOnlyList indexer
  double-offset). The `.slice()` extension's bounds are now RELATIVE to the view (were absolute into
  the backing array — consistent only because everything used to detach to low=0). Zero emission
  change (golib-only): corpus byte-identical 214/214; full suite green with output comparisons
  (Output 194 — the new `SliceAliasing` test covers copy-into-offset-view write-through, compound
  reslices, capped 3-index views, in-place vs detaching append). Doc: ConversionStrategies *Slices
  and Arrays* addendum (came with the branch).
- **2026-07-01: CONVERTER OUTPUT IS NOW BYTE-DETERMINISTIC (`32fd49a45`) — the campaign's
  ±10 jitter is DEAD.** Two consecutive full-stdlib reconverts now `diff -rq` to **ZERO files across
  all 305 packages** (previously dozens flipped), and the error count is stable at 59 both runs (3
  CS0019 were phantom errors manufactured by the converter's own race). The "abi.ΔString flips with
  map order" characterization from iteration 29 led to THREE order-dependent mechanisms, all fixed:
  1. **Per-file goroutines (main.go)** — files converted CONCURRENTLY over shared package globals
     (`-parallel` gates packages, which is forced to 1; per-FILE concurrency was unconditional):
     `initFuncCounter` claimed initΔN in completion order; `getGlobalTempVarName` was an
     UNSYNCHRONIZED map (real data race); and `loadImportedTypeAliases` marked an imported
     package_info "parsed" BEFORE parsing finished, so a racing file skipped the wait and emitted the
     imported renamed const BARE (`abi.String` → CS0019 ×3, present or absent by scheduling luck).
     Files now convert sequentially in sorted order — for FREE: 3m42s concurrent vs 3m39s sequential
     (the cost is go/packages loading, not emission).
  2. **The stdlib queue (stdLibConverter.go)** — DFS roots iterated a map (queue order flipped
     run-to-run), and GOROOT-VENDORED dependency edges were silently DROPPED (`golang.org/x/…`
     import vs `vendor/golang.org/x/…` key; isStdLib's dot-check rejected it), so bidirule could
     convert BEFORE bidi and emit `bidi.Direction` bare (bidi's package_info didn't exist yet).
     Sorted roots + vendored-key resolution + set-membership gate; queue order now
     bidi → bidirule → norm → idna (dependency-correct every run).
  3. **Multi-box re-alias order (visitAssignStmt.go)** — `(Ꮡx, Ꮡy) = (Ꮡy, Ꮡx)` emitted its
     `n = ref Ꮡn.val` refreshers in map order (math/big int.cs flipped). Sorted.
  Verification: recon23-vs-recon24 zero-diff; corpus byte-identical 213/213 (no golden churn — the
  strongest over-fire proof class); suite green. **Downstream effects: (a) reconverts are now
  RE-USABLE as goldens — measurement noise is gone; (b) `initΔN`/`_ᴛN` indices settled to canonical
  file order (one-time churn absorbed in the committed corpus, which was already byte-identical);
  (c) x/net + x/text vendor packages now convert AFTER their deps, so their imported-alias emissions
  (`bidiꓸClass` etc.) are complete and correct.** ConversionStrategies has a new *Deterministic
  Output* section recording the guarantee.
- **2026-07-01: unsafe.Pointer param returned whole is a plain value return (`ecfc7dbbf`;
  CS0103 −4).** The return-path pointer-param boxing (`return p` → `Ꮡp`) fired on the UnsafePointer
  BASIC, but such a param renders as a plain VALUE with no box → CS0103 `Ꮡzero`/`Ꮡv`/`Ꮡfd` (map.go
  mapaccess1/2_fat, mem_windows, panic readvarintUnsafe tuple). Gate: the returned param's own type must
  be a genuine `*types.Pointer`. Test: UnsafePointerParamPin extension (whole/tuple/genuine-*T control).
  Suite green (213). CS0103's remaining 3 = different sub-shapes (a value-receiver box miss
  mgcscavenge ×2, a receiver materialization mprof ×1).
- **2026-07-01: string-literal spread wrapped as @string (`c5c446110`; CS1061 −1, 64 → 63) +
  an HONEST REVERT.** (1) `append(b, "runtime error: "...)` (error.go) rendered the literal `"…"u8`
  (ReadOnlySpan — no spread property); the spread emission now wraps a direct string-literal source as
  `((@string)"…"u8).ꓸꓸꓸ` — the same wrap `string(r)...` uses. Test: StringConvPostfix extension.
  (2) **The double-pointer single (proc.cs `&(*pprev).alllink`) was attempted and REVERTED**: the
  of-chain advance emission was correct, but the walk WRITES `*pprev = …` through `&allm` — the
  pre-existing &GLOBAL COPY-BOX latent — so the C# walk cannot be output-faithful until that model
  lands. RECLASSIFIED: entangled with the copy-box latent family (rides with the &global model, not a
  standalone fix). (3) Repaired NestedFieldElementAddr's sed-mangled package_info.cs (the documented
  sed gotcha — the corpus check flagged it twice before the pattern was recognized). Suite green (213).
- **2026-07-01: resolve cross-package embedded types via the semantic model (`38212b635`;
  CS1061 −4, runtime 68 → 64).** Field promotion resolved the embedded type's SYNTAX only — in a real
  MSBuild build project references are METADATA references (never CompilationReference), so cross-package
  embed promotion had plausibly NEVER worked: `type rtype struct { *abi.Type }` generated an EMPTY
  promoted-accessors section (t.TFlag/t.Str/t.Kind_ all CS1061). Fix (StructTypeTemplate): fall back to
  the type's metadata symbol (`GetTypeByMetadataName` on the normalized nested name
  `go.internal.abi_package+Type`), enumerate public instance fields; accessors unchanged in form — TRUE
  REFS through the embed (`ref Type.val.TFlag`), write-through reaches the target (no copy syntactically
  possible). Guarded by the CrossPkgUser Phase-4 extension (pointer-embed + value-embed across a real
  assembly boundary, write-through vs Go, 10 lines). **KNOWN RESIDUAL: promoted METHOD calls through a
  cross-package embed (also syntax-resolved; zero runtime sites) — call through the embed explicitly.**
  Suite green (213).
- **2026-07-01: route nested-field element addresses through the of-chain (`a342d25e7`;
  CS1061 −3, runtime 71 → 68).** The `&field[i]` routing gated on the IMMEDIATE base only; a chain rooted
  at a pointer through NESTED value fields (`&pp.wbBuf.buf[0]` mwbbuf.go, `&mp.trace.buf[gen%2]` trace.go)
  and a NESTED-INDEX 2-D base (`&cache.entries[ck][i]` symtab.go — an IndexExpr the selector gate never
  saw) fell to the naive `Ꮡ` prefix (CS1061). Fix: walk intermediate selectors to the chain ROOT + a new
  inner-index arm; the proven recursive &-machinery renders the multi-hop chains
  (`pp.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)`, `cache.at(…Ꮡentries, ck).at<nint>(i)`). Bonus: a heap-boxed
  value-root 2-D index also fixed (was CS1061). Corpus byte-diff IDENTICAL (212); skeptic
  CONFIRMED-PARITY on 6 chain variants (incl. aliasing identity + 3-deep chains); in the newly-gated
  territory every old emission was a COMPILE ERROR — silent divergence impossible. Test
  `NestedFieldElementAddr` (write-through vs Go); suite green (213). **Latents logged: a ZERO-VALUED
  struct's array-field backing is null (any zero-value struct array-field access NREs — significant for
  Phase 4); the intermediate-IndexExpr chain `&ptr.items[i].buf[j]` keeps its pre-existing CS1061.**
- **2026-07-01: precise already-dereferenced test for a selector base (`ccfb952b0`; CS1061
  −3, runtime 74 → 71).** The selector auto-deref skip was a whole-subtree scan for ANY StarExpr — a
  conversion star buried in a call ARGUMENT (`stringStructOf((*string)(unsafe.Pointer(p))).n`, arena.go)
  falsely counted as an already-deref'd base → `.n` on the ж box (CS1061); an EXTRA-PAREN conversion base
  (`((*specialWeakHandle)(…)).handle`, mheap.go) missed the conversion branch through the parens — the
  THIRD instance of the extra-paren blind-spot pattern. Fix: inspect only the base's own outermost shape
  (paren-unwrapped); the branch dispatch also unwraps parens. **Verification (disclosed): the corpus
  byte-diff was IDENTICAL across all 211 prior projects — no previously-compiling emission changed —
  plus output-verified reads.** A WRITE through a conversion base hits the copy box (documented
  reinterpret-seam contract; probed: Go 11 vs C# 3, excluded+documented; runtime sites are reads). Test
  `PointerSelectorDeref`; suite green (212). **DISCOVERED pre-existing latent: `&s` on a string LOCAL
  misses escape-boxing (`s := "hello"; f(&s)` references Ꮡs without the heap<> decl) — unrelated pass,
  logged.**
- **2026-07-01: the pallocBits/pMask named-collection family LANDED (`adc8546cc`; CS1503 −9 +
  CS0021 −3 cascade, runtime 86 → 74).** Two coupled roots: (1) GENERATOR `IArrayViewTypeTemplate` —
  a defined-over-array-backed-defined type (`type pallocBits pageBits`) now implements IArray<elem> as a
  view; every member AND the wrapper's `val` (the converter emits `b.val[i] = v` in pointer-receiver
  methods) routes through an ensuring `view` accessor (materialize the lazy backing on the wrapper's OWN
  mutable m_value, return a copy sharing the heap T[]). Corpus-surgical: exactly ONE recipient
  (pallocBits) across all 669 [GoType] structs. (2) GOLIB `copy<T1,T2>(in slice<T1>, ISlice<T2>)` — a
  named-slice source now binds; same-type copies via window-span CopyTo (memmove, Go-overlap-safe).
  **TWO SKEPTICS EACH FOUND A REAL VALUE BUG in the first cut (the family's lost-writes history earned
  its verifier budget): the copy overload double-offset window-relative indexers with +Low (panic on any
  nonzero-Low operand), and the virgin-wrapper pointer-receiver fill loop silently dropped every write
  (lazy backing on a by-value temp). Both fixed + output-pinned.** Test `NamedArrayWrapper` (8 output
  lines vs Go incl. nonzero-Low src+dst copies and an overlapping memmove copy). Suite green (211), zero
  churn (structural). **Scope notes:** Go-legal named-DST copy (`copy(pmDst, …)`) still doesn't bind —
  ZERO corpus occurrences, report-only; virgin-reinterpret-write remains the documented lazy-backing
  edge; pre-existing latents found by the skeptics (range sub-slice DETACHES on nonzero low —
  slice.this[Range] copies, a skeptic filed a task chip; array/wrapper assignment shares backing vs Go
  value-copy; empty named-slice composite `pm{}` → CS0029) logged for future iterations.
- **2026-07-01: forward field-box accessors on a struct-inherited wrapper as TRUE REFS
  (`02a610466`; CS0117 −3 — bucket ELIMINATED, runtime 89 → 86; FIRST GENERATOR-side root of the
  campaign).** `&p.x` on a `*pinnerBits` (`type pinnerBits gcBits`, runtime pinner.go) emits
  `Δp.of(pinnerBits.Ꮡx)` — the accessor names the WRAPPER, but the `Ꮡx` static existed only on the
  underlying `gcBits` → CS0117. Fix (`InheritedTypeTemplate`): alongside the forwarded get/set
  properties, emit each forwarded FIELD's box accessor as a true ref THROUGH `m_value` —
  `public static ref uint8 Ꮡx(ref pinnerBits instance) => ref instance.m_value.x;` — a genuine no-copy
  ref chain (the copy trap sank the earlier pallocBits forwarding). Write-through output-proven twice
  (repro `15 3`; `NamedTypeOverStruct` extension `bump(&c.a)` → the ORIGINAL observes `17 37` vs Go).
  Suite green (210); zero golden churn is STRUCTURAL (the generator runs at C# build time — the
  transpiled .cs is unchanged). **⚠ IMPLICATION for the pallocBits→IArray family: this validates the
  true-ref-through-m_value pattern — the IArray forwarding for an array-inherited wrapper could follow
  the same play (forward the interface members as refs/views over `m_value`, never a copy), keeping the
  `[GoType("pageBits")]` conversion intact. The family may be more approachable than "fraught" now.**
- **2026-07-01: qualify a box-accessor type shadowed by its variable's lambda CAPTURE
  (`d133c769b`; CS1061 −2, runtime 91 → 89).** `boxAccessorType`'s collision check compared the owning
  type name to the `.of()` RECEIVER only; inside a closure the captured variable renames to `mʗ1`, so
  rwmutex `lockSlow`'s `systemstack(func(){ notesleep(&m.park) })` emitted `mʗ1.of(m.Ꮡpark)` — the bare
  `m` bound the still-visible ENCLOSING `ж<m>` local → CS1061. Fix: also qualify when the receiver is
  `typeName + ʗ…` (the capture marker) → `mʗ1.of(runtime_package.m.Ꮡpark)`. Qualification is
  name-resolution only (same static accessor), so mis-resolution would fail to COMPILE, not run wrong —
  the corpus byte-diff (210 projects, only the intended test changed) + stdlib bucket A/B (exactly the 2
  target sites, no new codes) stood as the adversarial evidence (verifier round dialed to zero for this
  one-comparison extension, disclosed to the user live). Guarded by a further `CollisionFieldBoxAccessor`
  extension (`capturedLocalNamedAfterType`, write-through vs Go); suite green (210). Landed interactively
  with the user (reduced loop cadence); ALSO this iteration instituted the reconvert timeout discipline
  (`212f70904`) after the user flagged zombie 2-hour pollers.
- **2026-07-01: pin a deref-aliased pointer param/receiver by its ref-local, not a phantom
  `.val` (`016ce07ef`; CS1061 −2 + CS0206 −1, runtime 94 → 91).** `unsafe.Pointer(ptr)` emits the pin
  helper `@unsafe.Pointer.FromRef` whose ref target was unconditionally `(expr).val` — right for a genuine
  box, wrong for a DEREF-ALIASED pointer (param/receiver rendered as the value alias `ref var p = ref
  Ꮡp.val`): a `*uintptr` param's alias is a plain nuint → CS1061 (select.go `unsafe.Pointer(pc0)`,
  heapdump.go `unsafe.Pointer(pstk)`), and a `[GoType num]` receiver's `.val` resolves to the generated
  get-only `val` PROPERTY → CS0206 as a ref arg (runtime2.go `guintptr.cas`). Fix (`convCallExpr.go`):
  when `exprIsDerefAliasedPointer(arg)`, take the alias's ref directly — `FromRef(ref p)`; a genuine box
  keeps `(box).val`; inside a lambda the alias renders through the box (`FromRef(ref Ꮡp.val)`, valid).
  **HARDENING (`captureModeOperations.go`): a hung verifier's final probe surfaced a REAL silent-wrong —**
  the helper's receiver arm matched by NAME, so an inner pointer local SHADOWING the receiver mis-took the
  gate (`FromRef(ref rΔ1)` pins the box reference slot: compiles, reads garbage; repro'd — Go 111 vs C#
  garbage). The receiver arm now also requires rendered==raw (the shadow pass Δ-renames every inner
  same-named binding; the param arm was already object-accurate via `identIsParameter`). The CS0206
  cascade is legitimate per the copy-box precedent (`Casuintptr` is a THROWING partial asm stub — loud,
  not silent; the faithful guintptr ж<T>-model remains queued). Hardening is a corpus no-op (all ~107
  bare-form emissions are genuine receivers/params — verifier audit). Test `UnsafePointerParamPin`
  (**output** — param, receiver, SHADOWED-receiver, field-address-control, values vs Go); suite green
  (210), zero churn. Verifier-2 CONFIRMED SAFE (91 exact, buckets exact, cascade mechanism explained);
  verifier-1 hung mid-probe but its dying breath flagged the shadow edge that became the hardening.
- **2026-07-01: cast a wide-integer 3-index slice bound to int (`cc1255754`; CS1503 −2,
  runtime 96 → 94).** A 3-index full slice `s[low:high:max]` lowers to the golib `.slice(nint low, nint high,
  nint max)` method; the bounds were emitted with a bare convExpr, so a wide-integer bound
  (uintptr/uint/uint32/uint64/int64) had no implicit conversion to the nint param → CS1503 (runtime/mprof
  `stk[:b.nstk:b.nstk]`, b.nstk uintptr). Fix (`convSliceExpr.go`): route the 3-index bounds through the
  existing wide-integer narrowing helper (renamed `castElemAddrIndex → castWideIntegerToInt`, now shared by
  element-address indices and slice bounds; caller in convUnaryExpr.go updated). Casts (int) only for
  uint/uint32/uint64/uintptr/int64; int/small-int/untyped left uncast. Go bounds are int so the narrowing
  matches Go; the runtime bound is guarded (`b.nstk ≤ 1024`). Test `Slice3IndexWideBound` (**output**);
  suite green (209), zero churn. **BOTH verifiers CONFIRMED SAFE** (diff exactly 6 lines, no over-fire,
  rename a no-op on the element-address + 2-index paths).
  - **⚠ pallocBits → IArray (5) was investigated and DEFERRED — it is ENTANGLED, not a clean root.**
    `pallocBits` = `[GoType("pageBits")]` (an InheritedTypeTemplate wrapper of `pageBits`, a named `[8]uint64`
    array that DOES implement IArray). The wrapper doesn't forward IArray → `len(b)`/`b[i]` fail (5). BUT the
    non-test runtime also has 7 `(*pageBits)(b)` pointer-reinterprets (mpallocbits.go:344-377) that currently
    COMPILE precisely BECAUSE the `[GoType("pageBits")]` wrapper provides the pallocBits→pageBits conversion.
    Option (a) — emit pallocBits's [GoType] as the array form `[8]uint64` (so it implements IArray) — would
    LOSE the pallocBits→pageBits conversion and break those 7 (net worse). Option (b) — generator: make
    InheritedTypeTemplate forward IArray for an array-inherited type (keeping the wrapper) — is the RIGHT fix
    but needs cross-symbol work (detect pageBits is array-backed) OR delegating IArray to the inner `m_value`.
    Same family: `pMask → @string` copy (4) — pMask=`[GoType("[]uint32")]` (Slice template, implements
    ISlice<uint32> not slice<uint32>), so `copy(slice<uint32>, pMask)` doesn't bind `copy<T>(slice<T>,slice<T>)`
    (fix: generator ISliceTypeTemplate implicit→slice<T>, OR a golib copy(slice, ISlice) overload). Both are
    the InheritedTypeTemplate collection-interface family — a dedicated generator effort.
- **2026-07-01: cast a `make()` length/capacity/hint of a non-int integer type to nint
  (`438d633a0`; CS1503 −5, runtime 101 → 96).** The golib `slice<T>(nint,nint)` / `map<K,V>(nint)` /
  `channel<T>(nint)` ctors take nint; C# has no implicit `uintptr`/`uint`/`uint32`/`uint64`/`int64` → nint
  conversion, so `make([]byte, n/goarch.PtrSize)` (uintptr length, runtime/mbitmap ×4 + a []uint64 variant)
  left `new slice<byte>(n/…)` with no applicable ctor → overload resolution fell onto `slice<T>(T[])`
  (CS1503 `nuint`→`byte[]`). Fix (`convCallExpr.go` make handler): for a slice/map/chan target, cast each
  length/cap/hint arg whose Go type is a non-nint-implicit integer to nint (`makeLenArgs` +
  `makeLenArgNeedsNintCast`); plain int / untyped / widening int8/int16/uint8/uint16 left uncast (no churn).
  The map/chan arms extend the same root the runtime cluster (slices) exposed. Cleared all 5, zero new error
  codes, every other bucket unchanged. Test `MakeSliceUintptrLen` (**output** — slice/map/chan uintptr
  lengths+hints + int/untyped controls, vs Go); suite green (208), zero corpus churn. **Verifier-1 CONFIRMED
  SAFE** (no over-fire, only previously-CS1503 lines change); verifier-2 hung on reconvert mechanics (map/
  chan arms self-verified: compile+run+output match Go).
- **2026-07-01: reinterpret a named-numeric pointer to its underlying basic —
  `(*uint64)(*lfstack)` (`f19153a9e`; CS0030 −13, runtime 114 → 101; second S1-FORK "convert native"
  win).** The runtime's atomic-on-a-named-integer pattern — `atomic.Load64((*uint64)(head))` /
  `atomic.Cas64((*uint64)(head), …)` where head is `*lfstack` (`type lfstack uint64`; also sweepClass
  uint32 / profAtomic uint64 / sysMemStat uint64) — reinterprets `*NamedNumeric` → `*underlying-basic`.
  `ж<lfstack> → ж<uint64>` has no C# conversion (distinct instantiations) → CS0030. Fix (`convCallExpr.go`):
  GENERALIZE the existing `(*Base)(defPtr)` reinterpret block (which boxed a COPY of the [GoType] value
  conversion for Named↔Named, `(*pinnerBits)(*gcBits)`) to also fire when the RESULT elem is a BASIC whose
  underlying equals a NAMED arg elem's (new `namedToBasic` branch) → `Ꮡ((uint64)(head))`. Named↔Named path
  byte-for-byte preserved (pure refactor); reverse `(*Named)(*basic)` and both-basic excluded. Copy-box →
  a READ is faithful (Load64 reads the copy — verified vs Go), a WRITE hits the copy but the runtime's
  Store64/Cas64/Xadd64 on these types are asm-STUB (`partial`) atomics and there's no direct non-atomic
  write, so no faithful write-through is lost. Cleared all 13 named-numeric→ж CS0030, zero new error codes,
  every other bucket unchanged. Test `NamedNumericPointerReinterpret` (**output** comparison — read path
  across uint64/uint32, values vs Go); suite green (207), zero corpus churn. **Verifier-1 CONFIRMED SAFE**
  (old-vs-new diff exactly 3 lines, no over-fire, Named↔Named byte-identical); verifier-2 hung on reconvert
  mechanics (its scope — read-correct / write-hazard / compile-101 — independently confirmed).
- **2026-07-01: route a raw-address pointer reinterpret `(*T)(p)` through uintptr
  (`9e30a1c5b`; CS0030 −23, runtime 137 → 114 — the BIGGEST single-root drop of the campaign; first
  S1-FORK "convert native" win).** A Go pointer-type conversion whose SOURCE is a raw address —
  `(*unsafe.Pointer)(p)` where p is an unsafe.Pointer — reinterprets it as `*T` (`ж<T>`). Because
  unsafe.Pointer is golib `Pointer : ж<uintptr>`, a direct `(ж<T>)p` needs the two chained user-defined
  conversions Pointer→uintptr→ж<T> that C# rejects in one cast (CS0030). The converter ALREADY routed
  through uintptr — `(ж<T>)(uintptr)(p)` — when the deref set `isPointerCast` (`*(*int)(p)`); two shapes
  never set it: a bare call ARGUMENT `atomicwb((*unsafe.Pointer)(ptr), new)` (atomic_pointer.go) and an
  extra-paren deref `*((*unsafe.Pointer)(k))` (map.go indirect key — convStarExpr's deref branch sees a
  ParenExpr, not the CallExpr). Fix (`convCallExpr.go`, new `isRawAddressPointerConversion`): route ANY
  pointer-RESULT conversion whose ARGUMENT is a raw address (Basic UnsafePointer/Uintptr) through uintptr.
  The pointer-to-NAMED-type value conversion `(*Base)(defPtr)` is handled+returned earlier (arg is a
  *types.Pointer), so it's unaffected. Raw-memory code (golib's map<K,V> is what runs) → **Compile+Target**
  test `UnsafePointerReinterpret` (both shapes), values not the contract. Cleared ALL 21
  `unsafe.Pointer→ж<unsafe.Pointer>` + 2 cascade, zero new error codes, every non-CS0030 bucket unchanged;
  suite green (206), zero corpus churn. **Both adversarial verifiers confirmed SAFE** (verifier-1: the gate
  is mutually exclusive with the named-type path — the only line it changes was previously CS0030, no
  over-fire; verifier-2: measured 114 + exact buckets + 0 target-class before its baseline step hung).
- **2026-07-01: cast a constant integer-literal return to the lambda's unsigned result type
  (`0ec8bac1c`; CS8917 −1, runtime 138 → 137 — the LAST CS8917).** A Go closure assigned to a local whose
  result is unsigned/pointer-sized, mixing `return 0` with `return slice[i]` — runtime `select.go`
  `casePC := func(casi int) uintptr { if pcs == nil { return 0 }; return pcs[casi] }` — emits `var casePC =
  (nint casi) => {…}`, whose delegate type C# infers from the return-expression TYPES. `0` is `int`,
  `pcs[casi]` is `nuint`; the best-common-type algorithm uses expression types (not constant
  convertibility) and `int` has no common type with `nuint`/`uint`/`ulong` → CS8917. Fix
  (`visitReturnStmt.go`, new `lambdaConstReturnCastType`): cast the literal to the result type →
  `return (uintptr)(0)`, so both returns share it. Gated tightly: only inside a lambda body
  (`conversionInLambda`; a NAMED func's `return 0` to nuint compiles as a constant conversion), only a bare
  INTEGER literal (the sole int-vs-unsigned inference-gap shape — byte/uint16 widen to int, signed/nint/long
  share a common type with int), only a BASIC uint/uint32/uint64/uintptr result (a NAMED unsigned type is
  left alone — `(gclinkptr)0` could introduce a new error). Provably disjoint from the narrow-arith return
  cast. Test `ClosureMixedReturnUnsigned` (uintptr/uint64/uint32/uint + signed control, vs Go); full suite
  green (205), zero golden churn across the corpus, **both adversarial verifiers CONFIRMED-CORRECT** (tuple
  `return ((uint64)(0), …)`, IIFE, named-defer, expression-body collapse `() => (uint64)(0)`, char-literal
  `(rune)'?'` NOT cast — all compile+match Go). **Residual pre-existing (non-regression, out of scope):**
  same CS8917 class remains for a rune/char literal, a NAMED-unsigned result, and a constant-`BinaryExpr`
  (`return 1+2`) to unsigned inside a lambda — none at the runtime site; a follow-up could extend the helper
  to constant-folded BinaryExpr + named types if they surface upstream.
- **2026-07-01: emit an unreachable trailing `return default!;` after an exhaustive
  fallthrough-default switch (`a99d32f81`; CS0161 −1, runtime 139 → 138).** A Go `switch` lowered to an
  if-chain whose `default:` is reached via `fallthrough` emits the default as a guarded `if (fallthrough
  || !match){…}`; C# can't prove the guard always runs, so a value-returning func ending in it fails
  CS0161 even though the Go `default` is exhaustive (runtime `startpanic_m`). Fix (`visitSwitchStmt.go`):
  emit `return default!;` after the if-chain — GATED to be provably safe: every case is a genuine Go
  TERMINATING statement (new `isTerminatingStmt`/`isTerminatingStmtList`, spec §Terminating, CONSERVATIVE)
  or falls through, none can `break` out, the func returns a value, and NOT namedReturnDefer mode (void
  wrapper → CS8030). **Two adversarial rounds earned their keep: a shallow "last-line-was-return" gate
  false-positived on `if{return}`-without-`else` (falls out → the trailing return SILENTLY returned the
  zero value); fixed with real terminality analysis. A second round found the namedReturnDefer CS8030
  gap.** A whole-stdlib diff confirmed the fix's ONLY effect is the one `return` in `startpanic_m`. Test
  `SwitchFallthroughDefaultReturn` (terminal + break-out + if-without-else + namedReturnDefer shapes vs
  Go); suite green (204), zero churn. **This came from TRIAGING the runtime error tail: the singletons
  hid 7 CONTAINED converter roots (not just the architectural bulk) — CS0161 was #1; 6 remain (see the
  Session-queue triage note).**
- **2026-07-01: emit a `return` against its OWN function literal's results, not the enclosing
  func's (`a59e760b7`; CS8030 −4, runtime 143 → 139).** A bare `return` in a named-results function emits
  `return (n, ok);`. `visitReturnStmt` built this from `currentFuncSignature`, but a NESTED function
  literal kept the ENCLOSING function's signature — so a bare `return` inside a VOID closure got the outer
  named results. Runtime mprof `goroutineProfileWithLabelsSync (n int, ok bool)` passes `forEachGRace(
  func(gp1 *g){ …; return; … })`; the void closure's bare returns emitted `return (n, ok);` into a `void`
  lambda → CS8030 ("void-returning delegate cannot return a value"). Fix (`convFuncLit`/`main.go`/
  `visitFuncDecl`/`visitReturnStmt`): a SEPARATE `currentReturnSignature` field — set to the func signature
  in `visitFuncDecl`, to the literal's own signature (save/restore) in `convFuncLit` — consumed by
  `visitReturnStmt`. `currentFuncSignature` MUST stay the enclosing func's (receiver/param detection needs
  it to resolve a CAPTURED pointer param — an earlier attempt that swapped it wholesale regressed
  captured-param `.val`→`.ValueSlot`, caught by check-no-regression). Test `ClosureBareReturnNamedResults`
  (10 true vs Go); full suite green (203), zero churn, adversarially verified (value/nested/doubly-nested/
  defer-recover/IIFE/sibling closures). **This came from triaging the UNKNOWN class CS8030 — a clean
  contained root. The other unknown, CS0021 (10), is ARCHITECTURAL (malloc `(*[2]uint64)(x)[i]` S1
  unsafe-pointer reinterpret + mgcscavenge/proc/traceback named-over-array indexing `m.scavenged[i]`/
  `mp.cgoCallers[0]`) — SKIP.** *(DISCOVERED pre-existing gap, out of scope: a named-result CLOSURE
  `func() (a, b int){…}` never emits its own `a`/`b` local decls — only `visitFuncDecl` does — so it
  drops/mis-returns results. Verify it's in the runtime bucket before pursuing; a follow-up would emit
  named-result locals for function literals too.)*
- **2026-07-01: qualify a same-package GLOBAL reference shadowed by a same-named LOCAL
  (`99ba29ef0`; CS0841 −1, runtime 144 → 143 — the LAST CS0841; ALL CS0841 now cleared).** Go allows a
  local to shadow a package-level global; a read of the global BEFORE the local's decl refers to the
  global (Go block scoping). C# locals are function-scoped, so the bare global name binds to the
  not-yet-declared local → CS0841 (CS0844 "hides the field" for the plain-global variant — same family).
  Runtime `traceallocfree.traceSnapshotMemory` reads global `trace.minPageHeapAddr` then declares
  `trace := traceAcquire()` (both collision-renamed `Δtrace`). Fix (NOT the declined rename-the-local
  path — that's fragile w/ collision renames × shadow counter): qualify the GLOBAL reference. `convIdent`
  now emits `runtime_package.Δtrace.minPageHeapAddr` when a use resolves to a package-level var of THIS
  package (`ObjectOf(ident).Parent() == v.pkg.Scope()`) AND a same-named function-level local is declared
  (new Visitor field `funcLevelDecls`, set per-function in `performVariableAnalysis`). Gated so an
  ordinary global and the local's OWN uses (resolve to the local, not pkg scope) keep their bare form.
  Test `GlobalShadowedByLocal` (collision + plain global, 49/205/42 100 vs Go); full suite green (202),
  BYTE-IDENTICAL corpus (zero churn), adversarially verified (write-through, cross-package excluded,
  no local-use leak, nested-block-shadow correctly a non-issue). **This was traceallocfree `Δtrace` —
  flagged DECLINED-KIN, but (like malloc `Δp` and mgcsweep `sʗ3`) it had a CLEAN surgical angle
  (qualify the REFERENCE, not rename the local). THREE consecutive declined/subtle-flagged CS0841 all
  yielded clean fixes — the "investigate before assuming undoable" heuristic paid off every time.**
  *(Known pre-existing gap, out of scope: a package-level CONST shadowed by a same-named local still
  fails CS0844 — this fix is `*types.Var`-only; a clean follow-up would mirror it onto `*types.Const`.)*
- **2026-07-01: resolve the lambda-capture rename by OBJECT, not name — closure self-shadow
  (`7baab09cb`; CS0841 −1, runtime 145 → 144).** A closure that captures an outer `s` snapshots
  `var sʗ1 = s;` and rewrites captured uses inside the lambda to `sʗN`. The rewrite map
  (`currentLambdaVars`) was keyed by NAME, so a self-shadowing initializer inside the closure — runtime
  mgcsweep `systemstack(func(){ s := spanOf(uintptr(unsafe.Pointer(s.largeType))); … })` — mapped BOTH
  the captured RHS use and the DISTINCT inner `s` binding to the same `sʗ3`; the inner decl emitted
  `var sʗ3 = …(~sʗ3)…`, its RHS binding to the not-yet-initialized inner var → CS0841. Fix
  (`main.go` + `variableAnalysisOperations.go`): new parallel map `currentLambdaVarObjs` records the
  captured var's `types.Object` per name; in `getIdentName` the capture name is applied ONLY when
  `v.info.ObjectOf(ident) == capturedObj`; a distinct inner binding falls through to its own
  shadow-renamed name (`var sΔ1 = spanOf(…(~sʗ3)…)`). The object check passes for every non-shadowing
  capture, so nothing outside this self-shadow case changes. Test `ClosureSelfShadowCapture` (211 vs Go);
  full suite green (201), **byte-identical corpus (zero churn)**, adversarially verified (8 self-shadow
  shapes — value/pointer/param captures, multi-use inner, write-through, nested/multiple closures — plus
  ordinary-capture no-regression). **This was the mgcsweep `sʗ3` root — the explorer's #2, flagged
  SUBTLE/medium-high-blast-radius, but (like malloc `Δp` before it) it had a CLEAN surgical angle: the
  object-check is inert for all non-shadow captures. Two consecutive "subtle-flagged" CS0841 both yielded
  clean fixes — investigate before declaring undoable.**
- **2026-07-01: qualify a collision-renamed owning type in a box-field accessor
  (`04a5322f7`; CS0841 −1 + CS1061 −2, runtime 148 → 145 — 3 errors, one root).** A box accessor
  `receiver.of(TYPE.Ꮡfield)` was qualified with the package static class only when TYPE equaled the
  `.of()` RECEIVER variable. But a Go local named after its type is renamed to the SAME `Δ`-name, so
  such a local ANYWHERE in the function shadows a bare `Δp.Ꮡfield` (C# locals are function-scoped).
  Runtime malloc `persistentalloc1` does `persistent = &mp.p.ptr().palloc` then declares a local `p`
  (→`Δp`) below; the accessor `(~mp).p.ptr().of(Δp.Ꮡpalloc)` bound its bare `Δp` to that later local
  (CS0841; two mheap `Δp.Ꮡgcw` sites were CS1061 — a local `Δp` of type `unsafe.Pointer`). The receiver
  isn't the colliding local, so the receiver-name check missed it. Fix (`convUnaryExpr.go`
  `boxAccessorType`): qualify whenever the type name is `Δ`-prefixed (a type is never shadow-renamed, so
  a `Δ`-prefixed accessor type is always a collision rename) → `(~mp).p.ptr().of(runtime_package.Δp.Ꮡpalloc)`.
  Value-identical to the bare form when nothing shadows. Extends the `CollisionFieldBoxAccessor` test
  (`localShadowsCollisionType`); full suite green (200), only that test's golden churns (benign
  re-baseline), adversarially verified (write-through, multi-level, no wrong-package/CS0426, all 196
  qualified sites compile). **The malloc `Δp` CS0841 was the cleanest of the 3 remaining CS0841 — the
  explorer's ranked #1, correctly (its `Δp` type-rename alternative had far more blast radius). The
  surgical box-accessor route avoided renaming the core processor `p` type entirely.**
- **2026-07-01: cast a native-int const-ARITHMETIC RHS whose folded value overflows int32
  (`aa0c36b6e`; CS0266 −1, runtime 149 → 148 — the LAST CS0266 cleared).** mbitmap's
  `pattern = 1<<maxBits - 1` (uintptr, maxBits=57) folds `1<<maxBits` to a SIGNED C# `long` literal
  (`144115188075855872L`, > int32), so the whole RHS is `long` — no implicit conversion to the native
  uintptr/nuint/nint target (CS0266); a `UL`/`(nuint)` suffix would not help (ulong→nuint is also
  explicit). Fix (`visitAssignStmt.go` new `nativeIntConstCastType`, wired into the simple-variable `=`
  path as the fallback when the narrow-cast is empty): wrap the whole RHS in the native target's cast.
  **Gated to the provably-64-bit case only** — the target is a PLAIN `*types.Basic` native-width int
  (uintptr/uint/int; a NAMED type is excluded via `.(*types.Basic)` not `.Underlying()`, since a
  `[GoType]` cast rejects a `long` → CS0030), the whole value fits int64 but overflows int32, AND at
  least ONE operand itself folds to a signed `long` (`overflowingConstLiteral != ""`) so the emitted
  arithmetic runs in 64-bit width. Test `NativeIntWideConstAssign` (uintptr/uint/int, values vs Go);
  full suite green (200), goldens byte-identical, **two adversarial verifiers** (the first mis-flagged a
  silent-wrong bare-shift as introduced; HEAD-diff PROVED it pre-existing → tightened the operand-fold
  gate + named exclusion; second verifier CONFIRMED all four claims). ⚠ **DISCOVERED pre-existing latent
  (separate future root, NOT introduced here):** a BARE const shift to a native int — `var p uintptr =
  1 << 40` / `q = 1 << 40` — is emitted as a 32-bit `(uintptr)(1 << (int)(40))` that MASKS the count
  (`40 & 31`) → prints 256 not 1099511627776 (SILENT wrong at HEAD). Fix belongs in the shift-emission
  path (widen the left operand `((nuint)1) << k` for a native/unsigned target, cf. `isWideShiftType`);
  a `NativeIntBareShiftAssign` guard would FAIL today, so don't add it until that path is fixed.
- **2026-07-01: rename a shadowed var used as a method-call receiver in an assignment target
  (`cd86426ce`; CS0841 −1, runtime 150 → 149).** Extends the iteration-5 assignment-target descent: the `=`
  case renames shadowed idents in the LHS base chain (index/key/selector/star/paren), but had NO case for a
  METHOD CALL in the chain — `x.ptr().val.next = …` (runtime stackpoolalloc, loop `x` renamed `xΔ1` because a
  func-body `x` is declared AFTER the loop) buried the `x` inside `x.ptr()`, so the use kept raw `x`, read
  before its later decl → CS0841. Fix (`variableAnalysisOperations.go`): add `case *ast.CallExpr:
  visitNode(cur)` — visits the whole call so receiver + args get the rename (visitNode keys on
  info.Uses→*types.Var, so a method name/global/field of the same name is left alone). Test
  `ShadowedVarMethodCallLHS` (write-through via a pointer-receiver method, C# 30 vs Go); full suite green
  (199), goldens byte-identical, adversarially verified (control: all 8 shapes fail CS0841 without the fix).
  **Remaining CS0841 = 3, all DISTINCT roots:** malloc.cs `Δp` (collision-rename ordering), mgcsweep.cs `sʗ3`
  (closure-capture box name), traceallocfree.cs `Δtrace` (collision-rename ordering — kin to the declined
  proc `Δtrace` CS0136). *(NB: the commit initially failed on a gpg-agent signing TIMEOUT; landed after the
  user unlocked the key — never bypass signing.)*
- **2026-06-30: narrow-int arithmetic cast on the RETURN path (`a351c3cc6`; CS0266 −1, runtime
  151 → 150).** Sibling of the assignment-path fix below: `func lowerASCII(c byte) byte { return c +
  ('a'-'A') }` (runtime env_posix) emits `return c + ((rune)'a' - (rune)'A')` = byte+int = int → CS0266.
  The narrow cast was applied on the assignment/value-spec paths but not the return path. Fix
  (`visitReturnStmt.go`): reuse `narrowArithmeticCastTypeFor` against each result-position type, emitting
  `(type)(expr)` — gated to a binary/unary arith expr whose Go type matches the narrow result type
  (a bare ident / call / already-narrowed / non-narrow return is untouched; the receiver-return branch,
  checked first, is unaffected). Test `NarrowByteArithReturn` (97 122 97 / 145 wrap vs Go); full suite
  green (198), goldens byte-identical, adversarially verified (multi-value, named-return-defer,
  interface/pointer, over-application gate, wrap across all 4 narrow kinds). **CS0266 is now fully
  cleared** (the mbitmap `long→nuint` root landed 2026-07-01, `aa0c36b6e` — see the latest bullet above).
- **2026-06-30: narrow-int arithmetic cast when only the FIRST operand is a conversion
  (`de2e80bd4`; CS0266 −3, runtime 154 → 151).** Go byte arithmetic wraps at byte width; C# promotes to
  `int`, so a narrow-typed assignment needs the result cast back (CS0266). `narrowArithmeticCastTypeFor`'s
  redundant-cast guard skipped the cast whenever the converted RHS merely STARTED with `(byte)(` — but
  `buf[i] = byte(e/100) + '0'` emits `(byte)(e/100) + (rune)'0'`, where that prefix casts only the FIRST
  operand and the binary result is still `int` (runtime print.go exponent-format ×3). Fix
  (`visitAssignStmt.go`): the guard now skips only when the WHOLE RHS is `(byte)(…)` — a paren-balance walk
  (`wholeExprIsCastOfType`) requiring the cast-paren's matching close at the very end, skipping `(`/`)`
  inside char/string literals. Adversarially verified (miscounts are false-NEGATIVE only = harmless
  redundant cast; wrap semantics confirmed vs Go across all 4 narrow kinds). Test
  `NarrowByteArithFirstOperandCast`; full suite green (197), goldens byte-identical. **The narrow cast on a
  RETURN of such arithmetic (env_posix.lowerASCII, CS0266) is a SEPARATE still-open gap** (the return path
  doesn't call narrowArithmeticCastType).
- **⚠ Δtrace CS0136 — INVESTIGATED & DECLINED this session (do not blindly re-attempt).** proc `procresize`
  has three `trace := traceAcquire()` (one func-body, two in nested if/else); `trace` collision-renames to
  `Δtrace` (it's both a package VAR and a method name). The func-body one and one nested one both emit
  `Δtrace` (the OTHER nested one correctly gets `traceΔ1`). The asymmetry — one nested `trace` renames, its
  sibling doesn't — was NOT reproducible in isolation (a plain collision + nested if/else renames BOTH
  siblings correctly) and NOT fully understood; it's a subtle interaction between the collision-rename, the
  shadow-rename counter, and the specific scope nesting (the un-renamed one is a DIRECT statement in the
  outer `else` block, after an inner `if`, not a sibling if/else). Declined rather than gamble a
  poorly-understood fix in the delicate shadow pass. Needs a focused deep-dive on `declareVar`'s
  funcLevelVar-branch needsShadowing logic (why it fires for one nested position but not another). 1 error.
- **2026-06-30: completed shadow-renaming for escaped sibling loop vars + LHS index/key uses
  (`f0c1c946e`; CS0136 −2 + CS0841 −1, runtime 157 → 154).** TWO ENTANGLED fixes the runtime
  `runqputslow` shape needs together (`variableAnalysisOperations.go`): **(A)** an escaping
  function-body `for i := …` loop var is emitted as a func-scope `ref var i = ref heap<…>(out var Ꮡi)`
  decl, so sibling `for i := …` loops reusing the name collide (CS0136) — collect the escaped loop var
  as function-level so the siblings rename `iΔ1`/`iΔ2` (gated to escaped + func-body-level +
  name-not-already-a-real-func-level-decl, preserving the ForVarMasks invariant); **(B)** a shadow-renamed
  var used as an LHS INDEX/MAP KEY (`a[i]=…`, `m[ns]=…`, `(*p)[i]=…`) was never rewritten — the `=` case
  only handled the ROOT ident — a SILENT wrong-value bug (`m[ns]=nsΔ1*100` wrote the wrong key, no compile
  error) and CS0136/CS0165 once the loop var renames. Descend the target's index/selector/deref chain;
  runs even for a PAREN-rooted target `(*p)[i]` (getIdentifier→nil, ~36× in stdlib — a defect the verifier
  caught and I fixed before commit). Entangled: A alone renames loop headers to `iΔ1` but leaves `batch[i]`
  as `i`. Test `EscapedLoopVarSiblingIndex` (C# [10 20 0 30 40]/2002/9 vs Go; array won't compile / map
  returns 30001 / paren OOB without the fixes); full suite green (196), goldens byte-identical,
  adversarially verified. **Remaining CS0136 = 1: proc `Δtrace` (5687)** — a collision-rename
  (`trace`→`Δtrace`) that ALSO shadows an outer `trace`(→`Δtrace`); a rename-INTERACTION (both get the same
  collision name), a DISTINCT root.
- **2026-06-30: block `const` that shadows an enclosing param/var is now shadow-renamed
  (`a09f7826b`; CS0136 −1, runtime 158 → 157).** C# forbids block shadowing (CS0136); the shadow-rename
  pass renamed shadowing *variables* but IGNORED consts (a const's object is `*types.Const`, not the
  `*types.Var` the scope stack tracks), so runtime lock_sema `notetsleep_internal`'s `const ns = 10e6`
  collided with its param `ns`. Fix (converter-only, `variableAnalysisOperations.go`): a `constShadowNames`
  map records a shadowing block const (detected by the same by-name check the var path uses) and renames
  its declaration + every use to `nsΔ1`, leaving the enclosing `ns`; non-shadowing consts are unchanged.
  Test `ConstShadowsParam` (10/14 vs Go); full suite green (195), goldens byte-identical, adversarially
  verified across iota/multi-name/typed/nested/counter-collision/const-shadows-const vectors. **The other 3
  CS0136 in proc are DISTINCT roots** (proc `Δtrace` = collision-rename `trace`→`Δtrace` shadowing;
  proc `i`×2 = a heap-ESCAPED loop var hoisted to func scope `ref var i = ref heap(…)` colliding with two
  sibling `for(var i…)` loops that reuse the name — an emission-hoisting/scope interaction). **⚠ NEW
  PRE-EXISTING BUG discovered by the verifier (silent data corruption, NOT a compile error): a shadowed
  name used as an LHS index / map-key / selector-base in a plain `=` assignment is NOT renamed** — the
  `=` AssignStmt case (`variableAnalysisOperations.go` ~714–734) only processes `getIdentifier(lhs)` (the
  root, e.g. `m` in `m[ns]`) and `visitNode`s the RHS, never descending into LHS sub-expressions, so
  `m[ns] = ns*100` (inner shadow `ns`) emits `m[ns] = nsΔ1*100` — LHS key stays the param, C# returns the
  wrong value with NO compile error. Reproduces with a VAR shadow too (shared with the var path); needs the
  `=` case to walk LHS index/key/selector sub-exprs. Queued as S6b below.
- **2026-06-30: pointer-receiver method promoted through a VALUE embed routes to the embedded
  box (`0abc66e2d`; CS1929 −3, runtime 159 → 158).** `timeTimer` embeds `timer` BY VALUE; a promoted
  `t.modify(…)`/`Ꮡt.stop()`/`Ꮡt.reset(…)` on a `*timeTimer` emitted the whole `ж<timeTimer>` box, but
  the promoted method's ж/[GoRecv]-ref overload binds `ж<timer>` (CS1929) — the TypeGenerator emits NO
  forwarder for this shape (a value-copy forwarder would lose the write). Go auto-takes `&t.timer`, so
  the converter now routes through the embedded field's box exactly as the explicit `t.timer.modify(…)`
  already renders: `t.of(timeTimer.Ꮡtimer).modify(…)` / `Ꮡt.of(timeTimer.Ꮡtimer).stop()`, via
  convUnaryExpr's `&receiver.field` &-machinery. Detection: `Selection.Index() == [embeddedField,
  method]` (single embed hop). GATED to a VALUE embed — a POINTER embed already yields the box as its
  field value and is left to the generated forwarder; taking its address would double-box to `ж<ж<T>>`
  (this gate fixed an initial over-boxing regression in the trace* writers — `traceWriter` embeds
  `traceBufPtr` — caught and corrected before commit). Write-through is genuine (a value-embedded field
  is a SHARED heap box `ж<inner>`, so `.of(…)` aliases the real storage — verified 108/108/108/7/0 vs Go).
  Test `EmbeddedValuePointerMethod`; full suite green (194), goldens byte-identical, adversarially
  verified. Known limitation (NOT a regression, cannot occur in converted code): embedding a hand-written
  baseline type whose pointer methods lack a `[GoRecv]` ж-overload would not bind — the converted stdlib
  always has the RecvGenerator overload. See ConversionStrategies.md "A pointer-receiver method promoted
  through a VALUE embed…". *(Two prior commits this session: `6fd2df8d5` committed InferredForeignTypeNoImport's
  generated .cs, missed in a1d6db87e; `a1d6db87e`/`af541a4e4` generalized the alias fix — below.)*
- **2026-06-30: GENERALIZED cross-package type-reference alias emission (`a1d6db87e`,
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
as items land. As of 2026-07-01 latest (`runtime` = ~63; string-literal spread cleared 1, 64 → 63):
CS0030 9, CS1503 8, CS0029 8, CS0103 7, CS0021 7, CS1929 6, CS0121 6, CS1061 1,
then a SINGLETON tail (CS0128 2, CS0149 2, CS8175/CS8120/CS1593/CS0136/CS0119/CS0118/CS0019 ×1 —
CS0206 + CS0117 GONE).
**CS1061's last entry (the proc.cs double-pointer walk) is RECLASSIFIED entangled** — its writes go
through the &GLOBAL COPY-BOX latent; it rides with the &global model, not a standalone fix. **The
contained converter/generator queue is now EMPTY — what remains: the managed-referent ж<T>-model
(9 CS0030, THE architectural centerpiece — DESIGN DECISION PENDING WITH THE USER: Option A faithful
managed-slot model now vs Option B copy-box compile-milestone precedent now + model at Phase 4), the
S3-S6 buckets (CS1503 8 / CS0029 8 / CS0103 7 / CS0021 7 / CS1929 6 / CS0121 6 — UNTRIAGED for
precedent-class roots), the raw-metal GoManualConversion stub pass (CS0149 + kin), and the entangled
singles.**
**Landed: CS0161 (`a99d32f81`), CS8917 (`0ec8bac1c`), TWO S1-fork convert-native reinterpret wins
(`9e30a1c5b` −23, `f19153a9e` −13), make-len-cast (`438d633a0`, −5), 3-index slice-bound cast (`cc1255754`,
−2), FromRef deref-alias pin (`016ce07ef`, −3), capture-collision qualify (`d133c769b`, −2), wrapper
field-box accessors (`02a610466`, −3, FIRST generator root), pallocBits/pMask IArray-view + ISlice-copy
(`adc8546cc`, −12, generator+golib). ⚠ The characterized contained/approachable roots are now DRY except
Δrtype.TFlag — what remains is dominated by the architectural S-families.**
- **NEXT — the characterized frontier at 51 (determinism DONE `32fd49a45`; slice aliasing DONE
  `86566b9ef`; CS0121 DONE `d0a935138`; embed-receiver CS0103 DONE `308debde7`; benchmarks merged),
  roughly cleanest-first:**
  1. **CS0103 last remnant (1): mprof.cs:1119** — `&b.stk()[0]` emits `Ꮡb.stk().at<uintptr>(0)` where
     `b` is a pointer LOCAL (`var b = head` loop var): b IS the box, the `Ꮡ` prefix is spurious →
     should be `b.stk().at<uintptr>(0)`. The element-address machinery's root-boxing mis-fires on a
     method-CALL-result slice rooted at a pointer local. Small contained gate (convUnaryExpr
     element-address arm).
  2. **CS0030 managed-referent (9, ж<T>-model): gclinkptr(4)/Δguintptr/puintptr/muintptr(3) →
     unsafe.Pointer + 2 singletons (`lfstack → Δhex`, `UntypedInt → unsafe.Pointer`).** MODEL holding
     `ж<T>` directly per the user's model (like `core/sync/atomic` atomic.Pointer<T> / `reflectlite`
     `object? m_target`), NOT a raw round-trip. The architectural S1 centerpiece — a dedicated iteration;
     **the A/B decision is PENDING WITH THE USER** (A faithful managed-slot now / B copy-box compile-
     milestone now + faithful as the first Phase-4 ticket; stated lean B).
  3. **CS0029 box↔value family (8):** linked-list assignment shapes (`x = *y` / `*x = y` box-vs-value
     mismatches) — needs per-site shape triage; may share a gate with the pointer-walk machinery.
  4. **Singles:** double-pointer selector (proc.cs `&(*pprev).alllink` — ENTANGLED with the &GLOBAL
     copy-box latent, rides with that model); CS1929 extension-shadowing (4, mprof UnsafePointer
     Load/StoreNoWB). *(CS0149 (2) = raw-memory delegate → GoManualConversion stub candidate, SKIP for a
     dedicated stub pass.)*
- **DONE this campaign (S1-fork convert-native + the earlier families):** named-numeric reinterpret
  `(*uint64)(*lfstack)` (`f19153a9e`, −13); unsafe.Pointer reinterpret `(*T)(p)` (`9e30a1c5b`, −23); CS8917
  lambda-const-return (`0ec8bac1c`); CS0161 switch-default (`a99d32f81`). (CS8917 residual pre-existing, out
  of scope: rune/char literals, named-unsigned results, constant-`BinaryExpr` returns inside lambdas.)
- **CS0128 (type.cs:414, `i`/`Ꮡi` dup) — ESCAPE-ANALYSIS RABBIT HOLE, not "easiest".** Both sibling
  `for i:=…` loops in `typesEqual`'s `abi.Func` case are escape-hoisted (`ref var i = ref heap<nint>`) → dup.
  A minimal repro of two sibling index loops does NOT escape `i` — the escape is CONTEXT-SPECIFIC to
  `typesEqual` (recursion/unsafe/abi), likely a SPURIOUS over-escape. Needs escape-analysis investigation,
  not a quick sibling-rename. Deprioritize.
- **CS0206 (runtime2.cs:177) — ARCHITECTURAL (S1), explorer MIS-classified.** `atomic.Casuintptr(…ref
  (gp).val…)` where `gp` is `Δguintptr` — `.val` is the managed-referent underlying-value; this is the S1
  guintptr/managed-pointer model. SKIP.
- **CS1593 (metrics.cs:494) — S6 method-VALUE, not delegate-arity.** `d.compute = read.compute` (a bound
  method value) emitted as a 0-arg `() => read.compute()` wrapper; the field wants a 2-arg delegate. Method
  values are S6 (architectural-ish). SKIP unless a clean method-value→delegate emission is found.
- **CS8120 (error.cs:273) — RISKY (compiles-but-maybe-wrong).** `printpanicval`'s type-switch has `case uint`
  and `case uintptr` both → C# `case nuint` (dup). Dedup COMPILES but silently mis-routes if the two bodies
  differ; here both are `print(v)` so a merge is safe, but a general fix is fraught (the Go uint/uintptr
  distinction is lost in the C# type map). Only land with a bodies-identical guard; treat as low priority.
- **CS0118 (tracetime.cs:80) — UNCLEAR.** Error points at the `_` discard of a `(w, _) = w.ensure(…)`
  deconstruction; not the `traceBytesPerNumber` const (that's a plain `const=10`, fine). Needs investigation.
- Architectural (SKIP): CS0119/CS0149 (S6 method-expression), CS0019 (S6 named-numeric bitwise), CS8175 (S5
  ref-local-in-lambda), CS0136 (declined proc-`Δtrace`), CS0103 7 (S5 unsafe.Pointer-param-as-box), plus the
  CS0030/CS1503/CS1061/CS0021/CS0029/CS0121/CS1929 bulk = the architectural wall.
- **⚠ BOTTOM LINE — SUPERSEDED (2026-07-01, user strategy correction).** Iteration 15 concluded "clean
  contained roots exhausted → STOP" because it treated the ~132 CS0030/CS1503/… bulk as an impassable
  "architectural wall." **The user reframed it: that wall is a FORK, not a wall, and the milestone is a
  clean COMPILE (not operational — that's Phase 4).** So the loop no longer stops here; it **SORTS** the
  S1/CS0030 family three ways and keeps going:
  1. **Native-type unsafe/pointer op → CONVERT** faithfully in the converter/`golib` (both are GC langs
     with pinning; native memory ops are identical — the hand-converted `unsafe`/`atomic` proves it).
  2. **Managed-referent (`guintptr`/`muintptr`/… hiding a managed pointer in a `uintptr`) → MODEL** it
     holding `ж<T>`/`object` directly (Volatile/Interlocked + `nilCanon`), like `core/sync/atomic`
     `atomic.Pointer<T>`. Per-site, approachable — CS0206 runtime2.cs `Δguintptr.val` is exactly this.
  3. **Raw-metal on NON-native types (layout math, type-descriptor walking, `*.asm`) → STUB** with
     `[module: GoManualConversion]` (a compiling hand/throwing stub that won't exist in the final build is
     an acceptable milestone solution; file a review note). Copy such stubs BACK into `go-src-converted`.

  Still-tractable pure-converter contained roots (do these too, interleaved): **CS8917** (select.cs:151
  mixed-return closure delegate type, +~6-golden churn) and **CS0118** (tracetime.cs:80, unclear — needs
  investigation). Escalate to the user ONLY a specific site you cannot sort into convert/model/stub, or a
  ж<T> model that needs a design decision. Full rationale:
  [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md) *The corrected end-state* +
  [`Phase3-AutonomousLoop.md`](Phase3-AutonomousLoop.md) *S1 is a FORK to SORT*.

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
  - [x] **`time` `timeTimer.modify/stop/reset` value-embed promotion** *(landed 2026-06-30, `0abc66e2d`;
    CS1929 −3, 159 → 158).* Pointer-receiver method promoted through the VALUE embed `timeTimer.timer`;
    converter routes `t.of(timeTimer.Ꮡtimer).modify(…)` (single-hop, value-embed-gated). Write-through
    verified. Test `EmbeddedValuePointerMethod`. **REMAINING embedding CS1929:** `type` `Δrtype.Uncommon`
    (`Δrtype` embeds CROSS-PACKAGE `abi.Type`) — that is the S3 metadata-only case below, NOT this
    same-package fix.
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

This session: runtime is at 51, EXACT (output is byte-deterministic, `32fd49a45`). Landed since: golib
slice ALIASING (`86566b9ef`); CS0121 ELIMINATED (`d0a935138` — explicit `uintptr → ж<T>` operator);
promoted-embed-call-on-ref-receiver (`308debde7` — explicit field call `sc.scavChunkFlags.setEmpty()`
when the base is the enclosing non-direct-ж receiver, + the rendered==raw hardening in convUnaryExpr's
`&recv.field` arm; pre-cleared the same latent in zip/gcimporter/go-types/image); and the BENCHMARKS
session merged (`8ea5253e5` + `02470cc93` — src/Tests/Performance suite; golib span-path append
reconciled BY HAND with the aliasing rewrite, byte-wise @string compare/hash, the sort.cs
infinite-recursion fix, time.UnixNano; SliceAliasing + full suite green post-merge).

Recommended NEXT root — **the LAST CS0103 (mprof.cs:1119):** `&b.stk()[0]` (Go mprof.go
iterate_memprof) emits `Ꮡb.stk().at<uintptr>(0)` where `b` is a pointer LOCAL (`var b = head` loop
var) — b IS the box, the `Ꮡ` prefix is spurious; correct form `b.stk().at<uintptr>(0)`. The
element-address machinery's root-boxing mis-fires on a method-CALL-result slice rooted at a pointer
local (convUnaryExpr element-address arm — find why the root walk prefixes Ꮡ to a pointer-local root
when the indexed base is a CALL result). Small contained gate. Fallbacks: CS0029 box↔value triage (8 —
linked-list assignment shapes, may share a gate with the pointer-walk machinery), then CS1929
extension-shadowing (4, mprof UnsafePointer Load/StoreNoWB — but verify it isn't the parked
named-over-array entanglement first).

PENDING WITH THE USER: the CS0030 managed-referent ж<T>-model decision (A faithful managed-slot now /
B copy-box compile-milestone now, faithful as first Phase-4 ticket; stated lean B). Re-present when the
cheaper roots run dry.

Standing cautions:
- FORCE `cd src/go2cs && go build -o bin/go2cs.exe .` before any "suite green" claim (stale-exe
  false-green). After any emitted-form change: `run-behavioral.ps1 --update-targets` post fresh build.
- Reconvert with the HARD TIMEOUT pattern (timeout -k 30 600, marker INTO the log, retry once on 124).
  Overlay = bash scratchpad/overlay.sh <recon>/core (it also restores the src/core manual files).
- gpg-agent may TIMEOUT on the signed commit — `gpgconf --launch gpg-agent`; if it still needs a
  passphrase, STOP and ask (never bypass signing). Commits LOCAL only until the milestone workflow.
- NEVER sed package_info.cs (Edit only); paren-unwrap before AST shape-matching; name-gates must respect
  Δ-shadow/ʗ-capture renames; MSBuild project refs are METADATA (cross-package = semantic model).
- mprof indexed-element atomic (CS1929 ×4) is S1/named-over-array ENTANGLED — park it. proc.cs
  double-pointer walk rides the &GLOBAL copy-box model. CS0128 (2) escape-hoist = rabbit hole.
- Logged latents (do not trip over them): bare const shift to native int is SILENT-wrong (`1 << 40`
  masks to 32-bit — fix in shift emission, widen the left operand); zero-valued struct array-field
  backing is NULL (NRE, Phase-4-significant); string-local `&s` escape miss; range sub-slice detach
  (task chip spawned); named-result CLOSURE decls missing; cross-pkg promoted METHOD calls.

First steps:
1. go build fresh; reconvert + overlay + build runtime -clp:ErrorsOnly; re-bucket (expect exactly 59;
   any drift is now REAL signal, not noise).
2. Characterize the 6 CS0121 sites; pick the disambiguation that keeps both the free-call and the
   receiver-call forms compiling; behavioral test with output parity (a two-overload add() shape is easy
   to reproduce); adversarial verify dialed to risk; corpus check; suite; ConversionStrategies.md; one
   signed local commit.

Closing ritual (REQUIRED): update docs/Phase3-Handoff.md — check off the item with a result note, refresh
the runtime count/date — then rewrite this "Next session prompt" block for the next unchecked item.
Commit the doc update. Then stop and hand me that prompt.
```

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` reports **0 errors** across all ~305 packages, with
the green baseline (`src/go2cs.slnx`) and the full behavioral suite still passing. Promote packages
into the baseline (`src/core/<pkg>`) as they go green and stabilize (see CLAUDE.md). Then stop and
stand by — this is THE milestone.
