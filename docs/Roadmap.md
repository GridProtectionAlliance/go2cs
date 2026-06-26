# go2cs Roadmap

Companion to [`/CLAUDE.md`](../CLAUDE.md). The path from "loop stalled" to "full Go stdlib converts and
compiles." Sequenced **green the loop first**, then drive the full conversion.

> **Status (2026-06-25): Phases 0–2 done — baseline is green.** `go2cs.sln` builds 79/79; behavioral suite
> passes (216 tests). Phase 3 (the full conversion) is the remaining work.

## Phase 0 — Documentation ✅ done

Orientation docs so any task starts informed: [`/CLAUDE.md`](../CLAUDE.md),
[`Architecture.md`](Architecture.md), [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md),
this file; plus a refresh of `README.md`.

## Phase 1 — Restore the separation (repo surgery) ✅ done

Goal: `src/core` = compiling baseline; `src/go-src-converted/` = full WIP; `golib` shared.

- Tagged the full conversion (`full-conversion-2025-05` → `cc14584c7`).
- Relocated the auto-generated stdlib out of `src/core` into `src/go-src-converted/` (2604-file rename);
  rewrote csproj/sln refs; added `.gitignore` rules for the Go `debug`/`log` package name collisions.
- Scoped `src/go2cs.sln` to the baseline; added `src/go-src-converted.sln` for the WIP.
- Fixed `deploy-core.bat` (`gocore`→`core`). `convert-gosrc.*` retarget still pending.

## Phase 2 — Green the loop ✅ done (via stub restore)

**Outcome:** rather than green the full `fmt` closure bottom-up (57 packages incl `runtime`/`syscall`/`os`),
we **restored the old hand-finished stub** (`3426298eb`) into `src/core` — it compiles cleanly against
today's `golib`, so the test loop went green immediately. Plus several converter fixes (below). The
57-package-closure analysis below is retained as context for **Phase 3** (the full conversion).

### How the baseline-restore path worked

The behavioral tests reference `golib` + the `go2cs-gen` analyzer + a few stdlib projects (`fmt` mostly,
plus `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`). The **stub** `fmt` is a minimal proxy with a tiny
closure (errors, io, strings, sync, math, …) — it avoids the runtime substrate, which is why restoring it
is the fast path to green. Restored 14 packages; excluded the stub `testing` (drifted, unused by tests).

### The full-`fmt`-closure path (NOT taken for the baseline; relevant to Phase 3)

The behavioral tests reference `golib` (all 59) + the `go2cs-gen` analyzer (all) + a few stdlib projects:
`fmt` (55 tests), `time` (5), `unsafe` (3), `strings` (1), `sort` (1), `math/rand` (1), `io` (1).

But these pull a **transitive dependency closure**. `fmt` alone references:
`errors, internal/fmtsort, io, math, os, reflect, slices, strconv, sync, unicode/utf8` (and those pull
more). So "green the loop" means **green `fmt`'s closure**, bottom-up.

1. **Compute closure + order.** Reuse `stdLibConverter.go`'s topological `sortedQueue`, restricted to the
   baseline roots, to get a leaf-first build order (`unsafe`, `internal/abi`, `unicode/utf8`, `math/bits`,
   … up to `fmt`).
2. **Green packages bottom-up.** For each package in order: `dotnet build` against current `golib`; fix the
   **converter** (preferred) — or, for genuinely out-of-band pieces like `builtin`, the **runtime** — until
   it compiles. Crib from the `go2cs-stub-ref` worktree when a hand-finished prior approach helps.
3. **Lock it in with a regression test.** Each greened package gets a behavioral/compile test so it stays
   green (mirrors the "raw code vs target file" comparison mode noted in `src/go2cs/ToDo.md`).
4. **Exit criterion.** Baseline `src/core` builds clean **and** all 59 `src/Tests/Behavioral` projects pass.
   The converter-improvement loop is restored.

### Phase 2 findings (measured 2026-06-25)

- **`fmt`'s closure is 57 packages** — the *full* `fmt` pulls in the entire runtime substrate
  (`runtime`, `syscall`, `os`, `reflect`, and a deep `internal/*` tree), unlike the old 36-line stub `fmt`.
- **Baseline build status: 18 / 57 compile clean** out of the box (built in place in `go-src-converted`
  against current `golib`). The pipeline works; failures are concentrated converter defects, not intractability.
- **Prioritized converter-defect roadmap** (own-errors across a probe of failing leaf/mid packages):
  | Count | Code | Meaning | Status |
  |---|---|---|---|
  | 48 | CS0106 | invalid modifier (`static readonly` on a function-local named-type const) | **FIXED** — `visitValueSpec.go`, verified math/bits 16→0 |
  | 20 | CS1002 | `;` expected | open (syntax cluster) |
  | 18 | CS1519 | invalid token in member declaration | open (syntax cluster) |
  | 18 | CS1026 | `)` expected | open (syntax cluster) |
  | 18 | CS1003 | syntax error, X expected | open (syntax cluster) |
  | 18 | CS0051 | inconsistent accessibility (param type less accessible) | open |
  | — | CS0103 | missing package-level lookup tables (e.g. `ntz8tab`/`pop8tab` in math/bits) | open |
- **Converter-improvement loop (proven end-to-end):** edit `src/go2cs/*.go` → `go build` (Go 1.23.1) →
  re-transpile → `dotnet build`. (For behavioral tests the harness runs this loop itself — see
  [`/CLAUDE.md`](../CLAUDE.md) "Test-harness mechanics".)
- **Retarget detail:** the stdlib converter writes to **`<go2cspath>/core/<pkg>`** (hardcoded `core` subdir).
  To regenerate cleanly into `src/go-src-converted`, point `-go2cspath` accordingly or convert to a temp dir
  and move. Batch several converter fixes, then do one wholesale reconvert + re-measure.

### Converter fixes landed (2026-06-25)

Each verified by rebuild + reconvert + compile:
- `golib/UntypedInt` missing semicolon (CS1002) — blocked the whole runtime.
- `static readonly` on a function-local named-type const → emit a plain local (CS0106) — `visitValueSpec.go`.
- Multi-line type-constraint unions rendered with only the first line commented → collapse to one comment
  line (`visitInterfaceType.go`).
- `~[]E` slice constraint wrongly given `IEqualityOperators` — `*Slice`/`*Array` cases were backwards and an
  empty constraint-type set counted as a subset of every operator set (`constraintOperations.go`).
- Switch fallthrough case with a single pattern value emitted an unbalanced `)` (`visitSwitchStmt.go`).
- Negative typed const (`int8 = -1`) promoted to `GoUntyped` because the range check used `ParseUint`
  (rejects negatives) → also try `ParseInt` (`visitValueSpec.go`).

## Phase 3 — Drive the full conversion to compile (the ultimate goal)

1. **Build-error roadmap.** Convert + `dotnet build` `src/go-src-converted/`, capture `build.log`; bucket
   compile errors by **frequency** and by **Go feature**. This is the prioritized work queue (the README
   already frames this log as the road map).
2. **Fix by error class, bottom-up the full DAG.** Highest-frequency converter defects first (each fix
   clears many packages). Re-convert, re-bucket, repeat. **Measure by packages-compiling and error-count**,
   never by "conversion succeeded."
3. **Promote, don't fork.** When a full package compiles cleanly and matches behavior, promote it toward the
   baseline; `golib`'s hand-written core stays shared and never auto-overwritten. Track promotions here.

### Phase 3 iteration 1 — converter fixes landed (2026-06-25)

Measurement workflow used (no wholesale commit of `go-src-converted`; it stays regenerable):
reconvert the stdlib to a temp dir (`go2cs -stdlib -comments -go2cspath <tmp>` — always `-comments` so the
Go authors' BSD license header survives; output lands in `<tmp>/core/<pkg>`),
overlay the fresh `.cs` onto `src/go-src-converted/<pkg>` (keeping the relocated csprojs, or regenerating
them and rewriting `$(go2csPath)core\` → `$(go2csPath)go-src-converted\` except `core\golib`), then
`dotnet build src/go-src-converted.sln` and bucket. Single packages build with
`-p:go2csPath=<repo>\src\` so the `$(go2csPath)` golib ref resolves outside the solution.

Four converter defects fixed (each verified by reconvert + compile of an affected package; **behavioral
suite stays green: 216/216**):

| Defect | Symptom (top error class) | Fix |
|---|---|---|
| Variadic of a **type parameter** (`func Or[T any](v ...T)`) emitted a namespace-level `using ꓸꓸꓸT = Span<T>` alias — `T` out of scope | CS0246 `'T' not found` | `visitFuncDecl.go`: when the variadic element is a `*types.TypeParam`, emit `params Span<T>` inline (C# 13 params-collections) instead of the alias. |
| Go built-in **`comparable`** constraint emitted as bare `comparable` (golib's type is generic `comparable<T>`) | CS0305 `requires 1 type argument` | `main.go` `getGenericDefinition`: special-case `comparable` → `comparable<T>`. |
| **Bodyless** (assembly/cgo) funcs emitted as accessibility-modified `partial` methods with no implementing half | CS8795 (49) — biggest cluster | `visitFuncDecl.go`: emit a non-partial throwing stub (`=> throw new NotImplementedException(...)`) until an asm/cgo backend exists. |
| **Filename build-constraint over-exclusion**: `isFileNameCompatible` treated *any* unknown `_word` suffix (e.g. `bits_tables.go`, `bits_errors.go`) as a failing platform tag, silently dropping the file → missing symbols | CS0103 (`pop8tab`/`ntz8tab`/`divideError`…) | `directiveOperations.go`: only a trailing recognized `_GOOS`, `_GOARCH`, or `_GOOS_GOARCH` constrains the build (full GOOS/GOARCH name tables added); descriptive suffixes impose no constraint. |

The filename fix is the highest-impact: a full reconvert went from **1472 → 1660 emitted `.cs` files** (~188
previously-dropped stdlib source files now converted). That raises the raw error count (144 → 224) because
newly-included files surface their own latent defects — so **track packages-compiling, not error count**,
this phase.

### Phase 3 iteration 2 — `internal/cpu` address-of-field (2026-06-25)

`internal/cpu/cpu_x86.cs` owned ~140 of the syntax errors, all from `&cpu.X86.HasADX` (address of a field
of the anonymous-struct package global `X86`). Two stacked bugs, both fixed; **behavioral suite stays green
216/216**:

| Bug | Symptom | Fix |
|---|---|---|
| The anonymous struct type of `X86` is lifted to `X86ᴛ1` while visiting `cpu.go`, but `liftedTypeMap` is **per-file** (each file gets its own concurrent `Visitor`), so `cpu_x86.go` couldn't resolve it — `getExprTypeName` fell back to the raw struct text, mangled to `cpu.CacheLinePad}` | syntax errors (`)`/`;` expected …) | New **package-level shared registry** (`packageDynamicTypeNames`, signature→C# name) populated by `visitStructType` for package-level lifts; `convUnaryExpr` emits a marker for unresolved anonymous structs, resolved after the file-visit barrier (`dynamicTypeOperations.go`, `main.go`). Race-free: resolution runs post-`Wait()`. |
| With the name fixed, `&X86.HasADX` emitted `ᏑX86.of(…)` (identifier form), assuming a heap-boxed pointer companion — but a package-global value var has none | CS0103 `ᏑX86 does not exist` | `convUnaryExpr`: choose the address-of form by escape state — `isHeapBoxedExpr` (mirrors the existing escape check) → `Ꮡvar.of(…)` for escaping locals, else the constructor form `Ꮡ(value).of(…)` (consistent with the existing whole-value `&global` path). |

Result: `internal/cpu` went **~140 → 8 errors**. Caveat: `Ꮡ(value)` heap-allocates a *copy* (golib
`Ꮡ<T>(in T)`), so `&global.field` currently points into a copy — a pre-existing whole-category limitation
(`&global` already did this on line 124); the proper fix is **boxed companions for package-global vars whose
address is taken** (future work).

### Phase 3 iteration 3 — `internal/cpu` compiles clean (2026-06-26)

The remaining 8 `internal/cpu` errors are fixed — **`internal/cpu` is the first full-conversion stdlib
package to compile clean** (was the ~140-error blocker). Three general fixes, all behavioral-green (228/228);
re-transpiling all 61 behavioral projects left every golden byte-identical (no converter-output regression),
and the whole solution rebuilds clean against the changed golib:

| Defect | Symptom | Fix |
|---|---|---|
| Large untyped constant typed by value-range as `(nint)…L` even in an unsigned context (`cpuid(0x80000000, 0)`) | CS1503 `nint → uint` (×6) | `convBasicLit`: in the `> int32` branch, if the literal's contextual type is unsigned (`isUnsignedType` via `info.Types`), emit an unsigned C# literal (`2147483648U` / `…UL`). |
| Slicing an `@string` returned `slice<byte>`, so `field[:4] != "cpu."` was `slice<byte> != string` | CS0019 | **golib**: `@string this[Range]` now returns `@string` (Go string slicing yields a string). Runtime-only — no `.cs` change. |
| Empty-string literal in a tuple assignment emitted `""u8` (a `ReadOnlySpan<byte>` ref struct) — illegal as a ValueTuple element | CS9244 | `visitAssignStmt`: suppress the u8 form for string literals in a multi-value (tuple) RHS (`field, env = env, ""`). |

Guarded by the `StringSliceAndUnsignedConst` behavioral test.

### Phase 3 iteration 4 — address-of-global correctness (2026-06-26)

`Ꮡ(value)` heap-allocates a **copy**, so `&global` / `&global.field` pointed into a copy — mutations never
reached the global (e.g. `internal/cpu.doinit` set feature flags on a throwaway copy of `X86`). Fixed by
backing **address-taken** package-global vars with a heap box, so the pointer references the original:

- New `globalAddressOperations.go`: a synchronous **pre-pass** (`collectAddressedGlobals`) scans all files for
  `&g` / `&g.field` / `&g[i]` rooted at a package-level var → `packageAddressedGlobals` (cross-file, since the
  global may be declared in one file and addressed in another).
- `visitValueSpec`: an addressed global is emitted as a box + ref-property —
  `static ж<T> ᏑG = new(default(T)); static ref T G => ref ᏑG.val;` — instead of `static T G;`. Reads/writes of
  `G` are unchanged (the ref-property forwards to the box). Only address-taken globals are boxed; everything
  else keeps the plain field, so the blast radius is tiny (only `GlobalStructFieldPointers` re-transpiled).
- `convUnaryExpr` / `isHeapBoxedExpr`: for an addressed global, emit the identifier form `ᏑG` (the box) rather
  than `Ꮡ(G)` (a copy). `&X86.HasADX` → `ᏑX86.of(X86ᴛ1.ᏑHasADX)`.

`internal/cpu` still compiles clean and now mutates the real `X86`. Behavioral green; `GlobalStructFieldPointers`
strengthened to assert the global itself is mutated (would print `false/0` before the fix).

**Known limitation:** cross-package `&otherPkg.ExportedGlobal` isn't boxed (only globals addressed within
their own package are detected).

### Phase 3 iteration 5 — anonymous-struct global declarations (2026-06-26)

A package-global var whose type is inferred from an anonymous-struct composite literal
(`var S = struct{…}{…}`) emitted the raw `struct{…}` text as its C# declaration type
(`public static struct{A int; B int} S = new Δtype(…);`) — invalid C#. The value was lifted to a named type
but the declaration wasn't (the lifting happened inside the composite literal, *after* the declaration type
name was resolved). Fix in `visitValueSpec`: for a package global with an inferred anonymous-struct type, lift
the struct with the var name **before** resolving the declaration type (mirroring the explicit-type path), so
both the declaration and the value share one lifted name (`Sᴛ1`). This also unblocks **boxing** such globals,
so addressed anonymous-struct globals (`&S.field`) now work too. Behavioral green; zero existing goldens
changed (no behavioral test had an anonymous-struct global). Guarded by an extension to the `AnonymousStructs`
test (a package-global anonymous-struct var, read and mutated through a field pointer).

### Phase 3 iteration 6 — TypeGenerator CS0051 (unexported embedded marker) (2026-06-26)

A public struct embedding an unexported marker type as a blank field (`_ noCopy`, the
`sync/atomic.Bool` pattern) made the **`TypeGenerator`** (Roslyn) emit `public Bool(noCopy _)` —
a public constructor whose parameter type `noCopy` is `internal` → CS0051. Root cause:
`GetScope("_")` returns `"public"` (the `firstChar == '_'` rule), so the blank embedded field
was classified as a public member and drove the public ctor. Fix in `StructTypeTemplate.PublicStructMembers`:
exclude blank/underscore-prefixed fields (never exported in Go) from the public constructor. All CS0051
in `sync/atomic` cleared; behavioral green (232/232 + the new test). Guarded by `UnexportedEmbeddedMarker`.

### Phase 3 iteration 7 — asm-function companion source generator (2026-06-26)

Resolves the iteration-6 follow-up. Bodyless (asm/cgo) Go functions are once again emitted by the converter
as `partial` *declarations* (reverting iteration-1's non-partial throwing stub). A new **`PartialStubGenerator`**
(`go2cs-gen`) emits a throwing `partial` *implementation* for every bodyless `partial` method that has **no**
other implementing part in the compilation (`IMethodSymbol.PartialImplementationPart is null`). So:
- packages that ship a hand-written companion (`sync/atomic`'s `doc_impl.cs`, real `Interlocked` bodies) use
  those bodies — the generator detects the impl and skips them; and
- companion-less packages (`crypto/internal/boring/sig`, `crypto/subtle`, …) get a generated throwing stub,
  so they compile instead of CS8795/CS0111.

**`sync/atomic` now compiles clean** — the second full-conversion stdlib package to go green (after
`internal/cpu`). `sig` compiles too. Behavioral suite stays green (the generator is a no-op for the tests —
none contain asm functions; zero behavioral `.cs` changed). Not behaviorally testable (Go rejects a bodyless
function without an `.s` file), so verified via the full-conversion packages compiling.

### Phase 3 — promotion gate finding: `sync/atomic` typed API is broken (2026-06-26)

A behavioral validation test (atomic ops, Go-vs-C# output, referencing `go-src-converted/sync/atomic`)
was written as the gate before promoting `sync/atomic` to the baseline. **It failed, and that is the
point — "compiles" ≠ "correct".** The package-level functions (`atomic.AddInt32(&n, 3)`) work, but the
**typed atomic types are broken**: `var i atomic.Int32; i.Store(10); i.Add(5); i.Load()` yields `0` in C#
instead of `15` (Go).

Root cause: a method like `func (x *Int32) Store(v) { StoreInt32(&x.v, v) }` converts `&x.v` (address of a
field of the **pointer receiver**) to `Ꮡ(x.v)`, which **boxes a copy** (the `ж(in T)` ctor copies), so the
atomic op never touches the real field. Attempting the fix uncovered a deep stack of issues in the
**receiver-capture mechanism**, which is the only way to get a non-copying pointer to a receiver field:
1. `&recv.field` must use the captured receiver box (`Ꮡx.of(Type.ᏑField)`), not `Ꮡ(x.field)` — and the
   detection has to run *before* the struct-field gate (a pointer receiver's selector type is a pointer).
2. The capture field name (`<Method>ꓸᏑx`) **collides** across overloaded same-named methods on different
   receiver types (`Int32.Add`, `Int64.Add`, …) — needs the receiver type in the name (converter + generator).
3. The capture field is a **static `ThreadLocal`** on the (non-generic) package class, so it **cannot hold a
   generic receiver's `T`** (`atomic.Pointer[T]`).
4. Even fixed, the `ThreadLocal` is only initialized when the method is called via the `ж` (pointer) overload;
   a value-receiver-style call (`i.Store(10)`) routes through the `ref` overload and the capture is **never
   initialized** (runtime "Receiver target … is not initialized").

**Conclusion:** `sync/atomic` is **not promotable** — its primary API doesn't work, and the receiver-field
address / capture machinery needs a substantial rework (likely replacing the static-`ThreadLocal` capture
with something that works for value calls and generic receivers). `internal/cpu` likewise isn't promotable
(asm `cpuid` is a stub). Promotion stays **pull/validation-driven**; this gate correctly blocked it.

### Receiver-field address / capture rework — staged (2026-06-26)

Making `&recv.field` reference the real receiver field (the `sync/atomic` typed-type unlock). Split into two
stages:

- **Stage A — DONE (commit `36363ef16`, behavioral green 236/236).** `convUnaryExpr` emits the captured-box
  field-ref form (`<capturedName>.of(Type.ᏑField)`) for `&recv.field` on a non-generic pointer receiver (the
  detection must run *before* the struct-field gate, since the receiver's selector type is a pointer). The
  captured-receiver field name is made unique per receiver type (`<Method>_<RecvType>ꓸᏑx`) so it doesn't
  collide across overloaded same-named methods (`Int32.Add`, `Int64.Add`, …) — coordinated in the converter
  (`getCapturedReceiverName`) and the generator (`ReceiverMethodTemplate`).
- **Stage B — DONE (commit `9aeaf29e2`, behavioral green 236/236).** Value-receiver calls of capture-mode
  methods now route through the `ж` overload so the captured form references the real field:
  - `captureModeOperations.go` — a pre-pass (`collectCaptureModeMethods`) scans the package **and its
    transitive imports** (`LoadAllSyntax` provides dep ASTs) for non-generic pointer-receiver methods taking
    `&recv.field`, keyed by the interned `*types.Func` so cross-package call sites match;
  - `escapeAnalysisOperations.go` — a value var on which a capture-mode method is called is marked escaping
    (heap-boxed), so its `Ꮡname` companion exists;
  - `convSelectorExpr.go` — the call is routed through the `ж` overload (`Ꮡi.Store(10)`).
  Also fixed an **inverted `CompareAndSwap`** in the hand-written `sync/atomic` companion (`doc_impl.cs`):
  `Interlocked.CompareExchange` returns the original, so a swap succeeded iff `== old` (was `!= old`) — found
  by the validation test. **Result: `sync/atomic`'s scalar typed types (`Int32/64`, `Uint32/64/ptr`, `Bool`)
  now work end to end** — `var i atomic.Int32; i.Store(10); i.Add(5); i.Load()` → 15, CAS/Swap/etc. all match
  Go. Guarded by the self-contained `ReceiverFieldAddress` behavioral test (no `go-src-converted` dependency).
- **Generic receivers (direct-`ж`) — DONE.** Generic capture-mode receivers (`atomic.Pointer[T]`) are now
  emitted with the heap box **as the receiver** (`this ж<Box<T>> Ꮡx` + `ref var x = ref Ꮡx.val;`), and
  `&x.field` field-refs through the box parameter (`Ꮡx.of(Box<T>.ᏑField)`) — `T` stays in scope, no static
  field needed. `[GoRecv]` is suppressed automatically (the signature is `this ж<…>`, not `this ref …`, so
  `RecvGenerator` skips it and there is no duplicate overload). Value calls heap-box and route through the ж
  overload (the Stage-B escape/routing already handles this once the generic methods are included). Converter
  changes: `captureModeOperations.go` (new `packageDirectBoxReceiverMethods`, keyed by `*types.Func.Origin()`
  so instantiated call sites match), `visitFuncDecl.go` (un-skip the receiver deref + emit `ж<T>` receiver),
  `convUnaryExpr.go` (generic branch → `Ꮡx.of(...)`), and `main.go` `getFullTypeName` (append type args for
  instantiated **cross-package** generics, else a boxed `atomic.Pointer[Config]` emits `new …Pointer()` with
  no `<Config>`). Validated by the **`GenericReceiverFieldAddress`** behavioral test (a generic `Box[T]`
  with `Set`/`Get` taking `&b.v`, exercised for `int` and `string`); behavioral suite green, zero churn.
- **Direct-`ж` extended to ALL field-address capture methods — fixes a concurrency bug.** The non-generic
  capture path used a static `ThreadLocal<ж<T>>` field reassigned per call (`new ThreadLocal<…>(() => Ꮡx)`),
  which is a **shared static** — concurrent calls on *distinct* receivers race on that field, so e.g.
  `u0.Load()` can return `u6`'s value (proven by an 8-thread stress test: each thread Loads its own
  `atomic.Uint32`, sees another's). Broken precisely for the concurrent types atomics exist for, and invisible
  to the single-threaded behavioral suite. Fix: `captureModeOperations.go` now marks *every* `&recv.field`
  capture method (generic **and** non-generic) direct-`ж`, so the box is the receiver parameter — no shared
  state, and alloc-free. `convUnaryExpr.go` always emits `Ꮡx.of(...)`; `visitReturnStmt.go` returns `Ꮡrecv`
  for a direct-box method (keeps a method that both takes `&recv.field` and returns the receiver consistent).
  Re-ran the stress test → "no race observed". `core/sync/atomic/type.cs` scalars re-emitted to direct-`ж`
  (the file is `GoManualConversion`, so re-emitted by hand from a reconvert, keeping the managed `Pointer<T>`).
  Guarded by the `AtomicValues` test; only the `ReceiverFieldAddress` golden changed (intended, still correct).
- **Return-receiver capture converted too, and the ThreadLocal mechanism DELETED.** The other capture trigger
  — `func (t *T) Common() *T { return t }` (`internal/abi`) — now also emits direct-`ж` (`captureModeOperations.go`
  `bodyReturnsReceiver` marks it direct-box; `visitReturnStmt.go` returns `Ꮡrecv`). With **both** triggers on
  direct-`ж`, nothing emits `[GoRecv("capture")]` anymore, so the whole racy mechanism is dead and was removed:
  converter (`captureReceiver` field, `getCapturedReceiverName`, the `[GoRecv("capture")]` emission) and
  generator (`ReceiverMethodTemplate`'s `CapturePointer`/`CaptureName`/`CaptureDeclarations`/`CaptureOperation`
  + the `ThreadLocal` `using`, and `RecvGenerator`'s now-unused `Options` arg) — ~55 lines gone, generated `ж`
  overloads are now a clean deref+delegate. Baseline `core/internal/abi/type.cs` `Common` surgically updated to
  direct-`ж` (promotion overloads regenerate correctly); `StdLibInternalAbi` golden updated. Full solution green;
  32/32 capture-affected behavioral phases pass; stress test still "no race observed". There is now **one** way
  a pointer-receiver method captures its box (the `ж<T>` parameter) — no thread-local, no shared state.
- **`sync/atomic` PROMOTED to `core` — first stdlib package migrated.** `src/core/sync/atomic` is now in the
  green baseline (`go2cs.slnx`, `/core/` folder), referencing `core/unsafe` + `core/golib`. Scalar typed
  types are the converter output as-is; `Pointer[T]` is **hand-rewritten** in the promoted (hand-owned) copy.
  - **The `unsafe.Pointer` finding (why the rewrite, not a global fix).** `unsafe.Pointer` is a type **alias
    to `nuint`** (`[assembly:GoTypeAlias("@unsafeꓸPointer","nuint")]`) — a raw number — used across **171
    files / ~1522 `ж↔uintptr` sites** in the full conversion, almost all *legitimately* numeric (pointer
    arithmetic, syscall, reflect). In .NET a managed reference **cannot** be stored as a number and survive a
    GC move (CLR rule), so golib's `ж↔uintptr` operators (`ж.cs:449-458`) can't round-trip a managed `ж<T>`:
    the zero case NREs (`*(T*)0`) and a real case dangles (a `fixed`-pin address read after the block closes).
    A *global* `unsafe.Pointer` redesign would be reckless (huge blast radius, breaks the correct numeric
    uses), so the contained fix lives in the one type that stores a managed pointer.
  - **The managed `Pointer<T>` (`core/sync/atomic/type.cs`).** Field `v` is a managed `ж<T>` (not
    `@unsafe.Pointer`); `Load`/`Store` use `Volatile.Read/Write`, `Swap`/`CompareAndSwap` use
    `Interlocked.Exchange`/`CompareExchange<ж<T>>`. A `nilCanon` helper collapses an explicit nil-`ж` to
    `null` so reference-based CAS treats every nil pointer as equal (matching Go's `nil == nil`). No
    `unsafe.Pointer`, GC-safe, nil-safe.
  - **Validated** end to end: `atomic.Pointer[int]`/`[Config]` Load/Store/Swap/CAS + the scalar API all match
    Go. Guarded by the **`AtomicValues`** behavioral test (scalar `Int32` + generic `Pointer[int]`, both
    paths). Solution builds green; behavioral suite green (zero churn).
  - **csproj gotcha:** `core/sync/sync.csproj` had no subfolder exclusion (it never had one); adding
    `core/sync/atomic` made it swallow atomic's generated files (`CS0579`). Fixed by mirroring `math.csproj`'s
    `<Compile Remove="rand\**" />` pattern → `<Compile Remove="atomic\**" />`.
- ~~Promote `internal/cpu` and `sync/atomic`~~ — atomic gated on the `unsafe.Pointer`/`ж` representation
  above (scalars work, `Pointer[T]` runtime-broken); cpu gated on asm `cpuid`.
- Confirm `internal/cpu` / `sync/atomic` build within the full
  `go-src-converted.sln` alongside their dependents.
- Re-bucket after a fresh full reconvert to find the next highest-frequency converter defect.

### Phase 3 iteration 8 — re-bucket + 4 converter/generator fixes (2026-06-26)

Fresh full reconvert (305 pkgs, 1659 `.cs`) + full `go-src-converted.sln` build, re-bucketed by `CS####`
frequency. **Measurement gotcha caught:** the reconvert-overlay must NOT blanket-delete `.cs` first — ~15
hand-written companion/pseudo-package files (`unsafe/unsafe.cs`, `*_impl.cs`, generator companions) are not
regenerated by the converter, and deleting them produces a phantom ~120-error `unsafe_package` CS0246 bucket.
Overlay by copying generated `.cs` over; `git checkout --` any deletions.

After clearing the phantom bucket, the real own-error defects bucketed as: reflectlite (24, CS1537 dup
`global using`), container/list (6→25 post-fix) + container/ring (12) sharing one `ж<T>` defect, unicode (4),
internal/types/errors (4), plus runtime/unsafeheader-missing-ref cascades. Four fixes landed (behavioral
suite green throughout; zero existing goldens changed by any):

| Defect | Symptom | Fix |
|---|---|---|
| **Generator dup `global using`** — `GetFullyQualifiedUsingStatements` (all 5 generators) copied `global using` alias directives from the source file as file-local `using`, colliding with the in-scope global one | CS1537 (×24 in reflectlite, via `PartialStubGenerator` asm stubs) | `go2cs-gen/Common.cs`: skip `global using` directives (already in scope everywhere, generated files included). reflectlite 24→0. Not behaviorally testable (asm-stub-only path); verified via the full conversion. Commit `9c9431b3f`. |
| **Mixed-type for-init** — `for i, e := s.Len(), s.Front()` (int + pointer) emitted two `;`-separated decls inside the C# for-init clause (invalid: the `;` ends the clause); the combined `var (a,b)` form is blocked by the int special-casing | CS1002/CS1003 (container/list + ~20 files) | `visitAssignStmt.go` (+ `forInit` flag in `FormattingContext`/`visitForStmt`): emit a tuple-deconstruction declaration with per-element types — `(nint i, var e) = (...)`. Gated on all-new, non-heap-boxed LHS. Guarded by `ForInitMixedTypes`. Commit `6d339a3d0`. |
| **Variadic-of-pointer param** — `func In(r rune, ...*RangeTable)` emitted an invalid using alias `ꓸꓸꓸж<RangeTable> = Span<…>` (alias identifier can't contain `<`/`>`) | CS1002/CS1022 (unicode) | `visitFuncDecl.go`: emit `params Span<T>` inline when the element type is generic/pointer (`Contains "<"`), extending iteration-1's type-parameter special case. |
| **Empty/spread variadic-of-pointer call** — `In(r)` (no trailing args) panicked the converter (`Args[i]` indexed past end); `f(slice...)` emitted `Ꮡslice` (element address-of applied to the spread slice) | converter panic → dropped file; CS0103 | `convCallExpr.go`: guard the element-pointer arg treatment with `paramHasArg` (empty call) and `!(hasSpreadOperator && last param)` (spread). |

The last two ship together with the `VariadicPointerParam` behavioral test (args/empty/single/spread calls).

### Phase 3 iteration 9 — blank-identifier collision (CS0102) (2026-06-26)

A package declaring blank `_` constants (skipping `iota` values) **and** a blank `func _()` (the stringer
compile-time-assertion idiom — e.g. `internal/types/errors`) emitted multiple `internal static readonly … Δ_`
fields that collided: CS0102 "already contains a definition for 'Δ_'". Root cause: `performNameCollisionAnalysis`
recorded `_` in both the named-element set (the blank consts) and the method-name set (`func _()`), flagged it
as a const↔method collision, and `getSanitizedIdentifier` Δ-prefixed every `_` to the same `Δ_` — defeating
the value-spec visitor's per-blank unique naming (`_ᴛNʗ`). Fix (`nameCollisionAnalysisOperations.go`): exclude
the blank identifier from collision analysis (it is a discard, never referenced, and already gets unique names).
internal/types/errors CS0102 4→0 (remaining: a `strconv` project-ref, separate). Guarded by `BlankIdentifierCollision`;
behavioral suite green, zero existing goldens changed. **Found-but-deferred:** a bare discard `_ = expr` *inside*
a `func _()` emits `_ = …` which binds to the method group → CS1656 ("cannot assign to '_'") — a separate edge
case (real stringer asserts use `_ = x[C-C]`); not hit by internal/types/errors' actual body.

### Phase 3 iteration 10 — accurate re-bucket + pointer-copy fix (2026-06-26)

**Measurement-methodology finding (important):** the committed `go-src-converted` csprojs are stale and lack
inter-package `ProjectReference`s that the *current* converter emits correctly. Overlaying fresh `.cs` onto those
stale csprojs inflated the bucket with phantom CS0246 "package not found". Regenerating the csprojs from the fresh
conversion (with the documented `core\`→`go-src-converted\` rewrite, golib excepted) dropped the total **95→79**
and CS0246 **23→5**. **So the measurement loop must regenerate csprojs, not keep the committed ones** — there is no
converter csproj-emission defect. Reusable overlay script: `scratchpad/overlay.sh`.

True own-defect leaders after the rewrite: container/list (25) + ring (12) = the `ж<T>` model; internal/chacha8rand
(7, mostly the same `ж<T>` pattern — `State.Init64`/`Refill` on a value needing the box); math/bits (4, unsigned-
arithmetic/shift-count coercions); a handful of 1–2-error leaves.

**The `ж<T>` model split into two sub-problems** (converter derefs a pointer param/receiver to a value alias
`ref var x = ref Ꮡx.val`, losing pointer identity when Go uses it *as* a pointer):
- **Sub-problem B — DONE (this iteration).** `r := p` / `r = p` (pointer copy of a *T parameter) emitted `var r = p`
  (a copy of the pointed-to value); the converter already treats the walked target as a pointer (`r.val`/`~r`), so
  it miscompiled. Fix (`visitAssignStmt.go`): a plain pointer-typed identifier on an assignment RHS now gets the
  pointer (box) form — `var r = Ꮡp` — via a new `rhsPointerCopyContext`/`appendRhsPtrContext` helper applied in both
  the declare/reassign branch and the per-variable (escaping-var) branch. A pointer *local* already holds the pointer
  directly, so it is unchanged → **zero existing behavioral goldens changed**; suite green 260/260. Guarded by the
  `PointerCopyWalk` behavioral test (copy-then-walk, for-init copy, explicit local seed).
- **Sub-problem A — TODO (the harder half).** Using the *receiver* as a pointer value — `func (n *node) selfLink() {
  n.next = n }` — still emits `n.next = n` (a value-ref receiver `this ref node n` has no box). Needs the receiver
  direct-`ж` mechanism (extend `captureModeOperations` so "receiver used as a bare pointer value" — assigned to a
  pointer field, stored, compared — triggers a `this ж<T> Ꮡn` receiver, like the existing `&recv.field`/`return recv`
  triggers). container/ring & list need **both** A and B to fully compile; B lands alone as a correctness fix.

**Other queued leaf defects** (from the 79-error bucket): plugin CS0553 (ImplicitConvGenerator emits illegal
`object↔T` conversions), runtime/internal/math CS0133/0266 (`const MaxUintptr` ulong→nuint not const), unicode
CS0051/52 (exported field of an unexported type), internal/runtime/atomic CS1526 (`new` without `()`), and the
deferred CS1656 (bare `_ = expr` discard inside a `func _()`).

### Phase 3 — earlier note: the `ж<T>` self-referential-pointer-struct confusion in container/ring & container/list —
`r.next = r` (assign receiver to a pointer field → needs the box `Ꮡr`) and `r = r.prev` (reassign the Go
pointer variable, but the C# `ref var r = ref Ꮡr.val` deref aliases the value). The deeper one (CS0019/CS1061/
CS0029/CS1929 cluster), blocking both container packages.

### The ultimate correctness gate — convert and run the stdlib's own tests (TODO)
The behavioral suite proves *our* hand-written programs convert correctly; it does not prove the
*converted stdlib behaves like Go*. The strongest possible gate is to **convert each stdlib package's
own `_test.go` files and run them** (Go `go test` output vs the converted C# test output). Go's standard
library ships an enormous, adversarial, edge-case-rich test suite — if a converted package passes its own
upstream tests, its runtime semantics are validated against the hardest available spec. The thesis: once
the **stdlib converts, compiles, and passes its own tests**, almost *any* Go program will convert and
work, because nearly everything bottoms out on stdlib + the runtime substrate. Sketch of the work:
- Teach the converter/test-harness to also emit the package's `*_test.go` as a runnable C# test target
  (today behavioral tests deliberately exclude `*._test.cs`; this would add an opt-in stdlib-test mode).
- Map Go's `testing.T`/`testing.B` onto a thin C# shim (or convert `testing` itself).
- Drive per-package: convert pkg + its tests → `go test ./pkg` vs the converted run → diff. Promote a
  package to "validated" only when its own tests pass, not merely when it compiles.
- Start with leaf packages that already compile (e.g. `strconv`, `math/bits`, `unicode/utf8`) where the
  test dependency closure is small, then work up.

## Progress tracking

| Metric | Source | Status |
|---|---|---|
| Baseline + tests build clean | `dotnet build src/go2cs.sln` | ✅ 79 / 79 |
| Behavioral suite passing | `BehavioralTests` (MSTest) | ✅ 216 tests |
| Full packages compiling | `src/go-src-converted.sln` | ◻ Phase 3 — iters 1–2: 5 converter fixes; `internal/cpu` ~140→8 errors |
| Full-conversion error count | build-error buckets | ◻ Phase 3 — next: address-of-global correctness; re-bucket after reconvert |

## Reference: open converter items (`src/go2cs/ToDo.md`)

`visitMapType` completion; remaining dynamic-struct implicit-cast checks across `AssignStmt`/`CompositeLit`/
`IndexExpr`/`BinaryExpr`/`UnaryExpr`/`SelectorExpr`/`TypeSwitchStmt`/`ValueSpec`; optional recursive
dependent-package conversion; map/channel `GoType` generator support (`IMap`/`IChannel`); comment
conversion; cgo + Go-assembler (`.s`) targets.
