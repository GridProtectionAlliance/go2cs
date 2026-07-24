# DESIGN — Reflection bridge Phase 3: write-back & dynamic call (chip working plan, v2)

> **Status: INCREMENT 1 IMPLEMENTED 2026-07-24 — errors VALIDATES (#34, 61/61 vs `go test -json`,
> zero disclosed/skipped).** The shipped record lives in
> [`DESIGN-reflection-bridge.md`](DESIGN-reflection-bridge.md); this file remains the chip's design
> + adversarial-review ledger. Implementation notes vs the plan: the Set store landed as
> ref-routed writes through `ж<T>.ValueSlot` (no DynamicMethod needed — assignment through the
> ref-returning property); X3 landed as `IInterfaceAdapter`-only transparency (instance-state
> discriminated), satisfying the B2 method-set constraints by construction; two additional
> shared-machinery defects were caught and fixed by the guards during implementation
> (`StructurallyImplements` counting `[GoRecv]` ref-receivers in the VALUE method set; the golib
> core interfaces' plain-`As` conversion hooks invisible to the assert fallback, which forced the
> converted fmt onto `&`-prefixed pointer printing). Chip session `optimistic-bassi-496625`,
> 2026-07-24.
>
> **Blessing record (user rulings):**
> - **Q1 model: blessed**, conditions: (a) the canonical nil-box singleton is **write-protected**
>   — a write through any `(*T)(nil)`-derived box panics exactly like Go's nil deref (structurally
>   enforced by ж's `Value` ref-getter throwing on `IsNull`; verified + guarded by test); (b) the
>   compiled ref-accessor cache is **thread-safe** (concurrent first-`Set` on one closed `ж<T>`).
> - **Q2 scope: chip lands X1–X5** in the same gated change-set; ledger lock extended by the
>   coordinator accordingly. Caveats: the channels track concurrently edits `builtin.cs`'s
>   channel/select region (disjoint; coordinator resolves textual overlap at integration); land on
>   the chip **branch**, not master (three live tracks — coordinator runs final all-ships-rise and
>   ff-merges in order); **one commit per X-fix, each with its own guard**.
> - **Q3: increment 1 first** (errors), increment 2 designed later against testing/quick +
>   encoding/binary differentials.
> - **⚠ Sequencing:** integration-wave3 (Change C reference-model test projects + unicode #33 +
>   CS0050 fix) lands on master imminently; `testConversion.go`'s `processTestConversion` is
>   restructured (X5 targets the NEW shape) and errors — black-box-only — flips to the reference
>   model. **Rebase onto post-landing master and re-measure the errors baseline differential
>   before implementing.** Coordinator flags when landed.
>
> Designed against the **demonstrated consumer**: the `errors` package pipeline
> (`go2cs -tests -test-action all "<GOROOT>/src/errors" src/go-src-converted/errors`).
>
> **v2 changes:** three independent adversarial reviews (lenses: Go-semantics, blast-radius,
> generalization) produced 10 substantive findings; every REWORK-class finding is folded into the
> model below and marked `[R:…]`. The §8 review ledger records them all.

## 1. Ground truth — the errors baseline differential (fresh converter, 2026-07-24)

| Test | Go | C# | Root cause (verified, not assumed) |
|---|---|---|---|
| `TestIs/#09 #10 #15 #27 #28` (poser / `&errorUncomparable`) | pass | **fail** | `err._<is_type>(ᐧ)` **never succeeds for any adapter-wrapped value** — probed at runtime: `assert ok=False` everywhere; the poser `Is` closure never runs. golib `TryTypeAssert`'s structural fallback (`Implements<T>` + `ᴛAs`) probes the **adapter class** (`poserжerror`) instead of the wrapped dynamic value (`ж<poser>`); the adapter has no `Is`. (The registry path *does* unwrap; the fallback doesn't.) |
| `TestAs` (all 18) | pass | **infrastructure-error** | Test preamble `rtarget.Elem().Set(reflect.Zero(...))`: bridge `Elem()` returns a **non-addressable copy** → `Set` → `mustBeAssignable` → `methodName()` → `runtime.Caller` → `callers` → **`getcallersp` `NotImplementedException`**. Behind it: `Set`, `Zero`, addressability, `AssignableTo`, `Implements` are all unimplemented on the bridge. |
| `TestAsValidation/*int(<nil>)` | pass | one-sided | `(*int)(nil)` boxed to `any` is a **null reference** — typed-nil loses its type, `%T` prints `<nil>`. Same loss breaks `errorType = reflectlite.TypeOf((*error)(nil)).Elem()` (wrap.cs:185) → nil Type → `Implements(errorType)` panics for every concrete target. |
| `TestAsValidation/*string(0x…)` | pass | one-sided | Subtest name embeds a pointer address — nondeterministic on *both* sides; exact-name keying can never match. |
| `TestIs` subtests | — | — | C# host names empty-name subtests `#00`, `#00#01`, … ; Go names them `#00`…`#29`. Every row one-sided. |

Everything else (TestUnwrap, TestJoin*, TestNewEqual, remaining TestIs) already passes on the
Phase-1/2 bridge.

## 2. The write-back model — one new primitive

Phase 1/2: carry `System.Type` + the boxed object instead of `{type,data}` words. Phase 3 adds
one primitive: **an addressable Value carries the `ж<T>` box it aliases**; every write goes
through that box. No `unsafe` — the box *is* Go's address, and golib boxes already alias struct
fields and array elements, so the same slot extends to `Field(i)`/`Index(i)` addressability
without a model change.

### 2.1 Value companion + the structural-nil predicate `[R:C1]`

```csharp
partial struct ΔValue {
    internal object? boxed;    // Phase 2 (existing): the boxed Go value this Value represents
    internal object? addrBox;  // Phase 3: the ж<T> box this Value ALIASES when addressable
}
```

- `ValueOf(ptrBox).Elem()` → `{ addrBox = ptrBox, typ_ = synthType(pointee), flag = kind | flagAddr | flagIndir }`.
- **Readers read through the box lazily** (`currentValue` accessor: `addrBox` set → live
  `box.Value` read; else `boxed`). Hard requirement: TestAs's `poser.As` writes through the *same*
  heap box directly, then the test reads `rtarget.Elem().Interface()` — a snapshot returns stale
  data.
- **`[R:C1]` The nil predicate is STRUCTURAL, never value-peeking.** `ж<T>.IsNull` returns true
  for a heap box whose *held reference-typed value* is null (`m_val is null` arm) — but such a box
  (`ᏑerrP`, a real `ж<ж<PathError>>` address) is a **non-nil pointer holding a nil value**. Probing
  `IsNull` makes `As` panic "non-nil pointer" on 8 of 18 TestAs targets and `Elem()` return the
  invalid Value at case #0. golib gains a structural predicate on `ж<T>` (true iff `m_isNull` — the
  canonical-nil/X2 form — with a null *reference* also structurally nil); the bridge's
  `IsNil`/`Elem`/nil-guards use **only** that. The existing value-peeking `IsNull` probes in the
  Phase-2 readers (`IsNil`, `Elem`, `reflectPointerToken`) are corrected in the same change.
- `Elem()` of a **structurally nil** pointer → invalid zero Value (Go). `Elem()` of an
  interface-kind Value re-derives from the dynamic value (existing).
- `CanSet`/`CanAddr`/`Kind`/`IsValid` keep working from auto flag code — entry points now set real
  `flagAddr|flagIndir`.

### 2.2 `Value.Set(x)` (hand-owned, both packages) `[R:C2,C4,G-F1]`

1. `v.mustBeAssignable()` — keep the auto flag-based check; its panic path is fixed by §2.5.
2. **Assignability decided FIRST, Go-style (`assignTo`), for every source including nil**
   `[R:C2]`: identity (`srcType == dstType`) or interface-implements (§2.3); a typed-nil src of
   the wrong pointer type panics exactly like Go. Marshalling per dst slot:

| dst slot | src | store |
|---|---|---|
| concrete struct `T` | boxed `T` (identity) | the struct value |
| pointer `ж<T>` | `IжAdapter` wrapping `ж<T>` | the unwrapped **box** (Go: interface holds the `*T`) |
| pointer `ж<T>` | raw `ж<T>` / canonical nil box (identity) | the box / `null` slot value for structural nil |
| interface `I` | implements-check passed | an `I` instance via the golib assert machinery (non-generic `TryTypeAssert(object, Type, out object)`, added by X1); **a typed-nil pointer src stores the canonical nil box wrapped for `I` — a NON-nil interface holding `(*T)(nil)`, Go's `packEface` result** `[R:C2-B]` |
| mismatch | — | Go panic `"reflectlite.Set: value of type X is not assignable to type Y"` |

3. **Store mechanism `[R:C4,G-F1]`:** `ж<T>.Value` is a **get-only ref-returning property** —
   `PropertyInfo.SetValue` cannot write it, and reflection-invoking a ref getter yields an
   unwritable copy. The store is a **cached compiled ref-accessor write** (per closed `ж<T>`: a
   generic helper closed via `MakeGenericMethod` that assigns `((ж<T>)box).Value = (T)v` through
   the `ref`, or equivalent DynamicMethod IL). This same shape is what `Field(i)`/`Index(i)`
   addressability needs in increment 2 (ж's typed `of(FieldRefFunc)`/array-index alias ctors route
   through `Value` — `FieldRef.Create`'s `m_val`-hardcoded IL is **wrong** for element/nested
   parents), so the contract is landed ref-based from day one rather than reworked later.
   **Blessing conditions (Q1):** the accessor cache is a `ConcurrentDictionary` keyed by the
   closed `ж<T>` type (`GetOrAdd`; a benign double-compile race is acceptable, a torn entry is
   not); and a store through a **structurally nil** box must panic Go-style — ж's `Value`
   ref-getter already throws `NilPointerDereference` on `IsNull` before the ref exists, which
   protects the shared canonical singleton structurally; the Set path re-checks and converts this
   to the Go panic, and a behavioral guard pins it (write through a `(*T)(nil)`-derived Value
   panics; the singleton is observably unmodified after).

### 2.3 `Type.Implements` / `Type.AssignableTo` (hand-owned, reflectlite now)

Bridged over the **existing** golib structural machinery — one method-set rule everywhere (§8
charter), never a second implementation:

- `directlyAssignable(T, V)` → `T.sysType == V.sysType` (identity; named-type distinctness free).
  The Go unnamed↔named underlying rule is deferred with a named consumer (encoding/binary) and a
  TODO in the impl.
- `implements(T, V)` → `T.sysType.IsInterface &&` (nominal `IsAssignableFrom` **or** golib
  `StructurallyImplements(V', T)`), `V'` = `ж<X>`→`X` receiver-element resolution (already inside
  `StructurallyImplements`, which also already enforces Go's value-vs-pointer method-set rule).
- `Comparable()` — already correct (Phase 2).

### 2.4 `reflect.Zero(t)` + ONE nil encoding `[R:C3,G-F2,C5]`

Hand-owned `Zero`:

- value kinds → `Activator.CreateInstance(sysType)` boxed zero; String → empty `@string`.
- pointer kind → a **valid Value whose `boxed` IS the canonical per-type nil box** (the same X2
  singleton — see below). `[R:G-F2]` There is exactly **one** typed-nil encoding in the whole
  system; `boxed == null` never means "typed nil", only "nil interface / invalid".
- interface kind → valid Value, `boxed = null` (Go's nil interface genuinely has no type).
- slice kind → `default(slice<T>)` (the nil slice). map/chan/func kinds: deferred with named
  consumers (gob/binary/quick), impl throws a scoped `NotImplementedException` naming them.

`Interface()` / `valueInterface` on a valid typed-nil pointer Value returns the canonical nil box
(a **non-nil** `any` holding `(*T)(nil)`, Go-correct `%T`/`!= nil`) `[R:C3]` — never null.
`reflect.New` is deferred to the gob/binary increment (cheap once Zero exists) rather than landed
untested.

### 2.5 The `getcallersp` path

`getcallersp` is unimplementable; the semantic boundary with a managed answer is `methodName()` —
the only operational consumer on this path. Hand-own `methodName` in `reflect` + `reflectlite`
over `System.Diagnostics.StackTrace` (best-effort Go-shaped `pkg.Method`, `"unknown method"`
fallback). Misuse of `Set` then panics like Go instead of dying in a stub. (Review confirmed no
errors test observes the message text.) The `getcallersp` stub stays — its other callers are
non-operational runtime paths.

### 2.6 Adapter-type unwrap at the descriptor (R10) `[R:B-R10]`

`abi.TypeOf`/`synthType`, `GoReflect.KindOf`, `GoReflect.ElementType` gain the unwrap
`GoTypeName` already does: pointer-sourced `IжAdapter` ⇒ descriptor for `*T`
(`sysType = typeof(ж<T>)`, Kind Pointer); ᴠ-adapter ⇒ the wrapped struct type. `Value.boxed`
keeps the original interface value (dispatch/`Interface()` untouched); only type identity
unwraps. Load-bearing for `AssignableTo`; also heals the latent Phase-2 hole where adapter-held
and raw-box values interned to different canonical Types. Review confirmed `%T`, `IsComparable`,
and the csv DeepEqual paths are stable under the flip.

`[R:B-R10]` In the same change, the **Value readers become adapter-aware** where kind now reports
Pointer: `Elem()`/`reflectPointerToken`/`UnsafePointer` unwrap `IжAdapter.Box` before probing —
review showed `Elem()` on an adapter currently returns the invalid Value and is masked only by
the equal-vs-equal DeepEqual shape; a guard test pins the fixed behavior (`%v` of a *methodless*
`*struct` in an interface prints Go's `&{…}`).

## 3. Registry additions (`manualConversionFuncs`)

- `reflect`: `Value.Set`, `Zero`, `methodName`.
- `internal/reflectlite`: `Value.Elem`, `Value.IsNil`, `Value.Set`, `rtype.Elem`,
  `rtype.Implements`, `rtype.AssignableTo`, `methodName`.

(reflectlite's auto `Elem`/`IsNil` read the never-populated `v.ptr` — `IsNil` currently answers
true for every pointer; both must be bridged for `errors.As` to reach its loop at all.)

## 4. Adjacent shared-machinery fixes errors REQUIRES that are NOT the reflect surface

Root-caused during this chip's survey; errors cannot validate without them; they live outside the
chip's declared file lock. **Ownership decision requested (§6 Q2).**

- **X1 — golib `TryTypeAssert` structural-fallback unwrap** *(the "multiError residual")*: the
  fallback (`Implements<T>` probe + `ᴛAs`) must probe the unwrapped dynamic value. **Guarded
  pattern REQUIRED `[R:B2]`: match `IжAdapter { Box: not null }`** (the null-guarded form
  `builtin.cs` and `error.cs` already use) — a bare `IжAdapter` match would null out value-backed
  Δ-wrappers (X3) and break their asserts. Also adds the non-generic
  `TryTypeAssert(object, Type, out object)` §2.2 needs. Fixes TestIs #09/#10/#15/#27/#28 and
  TestAs's poser `As` route.
- **X2 — typed-nil pointer boxing (converter + golib)**: a nil pointer crossing into interface
  space becomes the **canonical per-type nil box** (per closed *named* pointer type — `intRef`,
  not just `ж<int64>` `[R:B1]`), restoring Go's `any((*T)(nil)) != nil`, `%T`, and the pervasive
  `reflect.TypeOf((*T)(nil)).Elem()` idiom. Scope, per review:
  1. Conversion-expression sites (`(*T)(nil)` → the singleton) **and the comparison-operand
     emission symmetrically** `[R:B1]` — `NamedPointerReinterpret` compares `v == ((intRef)default!)`
     via *reference equality on `object`*, so both sides must yield the same singleton instance or
     its output flips `nilref`→`other`. That test is the X2 gate; its golden re-baselines.
  2. **Adapter construction sites** `[R:C5]`: a null pointer entering a generated adapter ctor
     wraps the canonical nil box (`Box` never null) — otherwise `errors.Is((*A)(nil)-err, (*B)(nil)-err)`
     compares two null Boxes equal (Go: false), typed-nil equality across the `any`/`error`
     boundary breaks, and `case *T:` on a nil-holding interface can't match.
  3. **ж equality unification** (chip-found): a canonical nil box vs a plain null `ж<T>`
     reference must compare equal (`p == q`, one from `(*T)(nil)`, one never assigned) — `Equals`
     and both operator forms treat structural-nil ↔ null-reference as equal.
- **X3 — Δ-dyn wrapper adapter transparency (go2cs-gen TypeGenerator template)**: the duck-typing
  wrappers join the unwrap protocol so a reflection-`Set` interface store compares equal to the
  original value in `AreEqual` (TestAs `&timeout` want-compare). **Shape constrained by review
  `[R:B2]`:** `IжAdapter.Box` returns `m_target_ptr` **only when ptr-backed** (null otherwise);
  the value-backed form exposes its value via `IInterfaceAdapter.Value` — the discriminator is a
  runtime field, and an unconditional `Box` would grant a value-sourced wrapper the pointer
  method set through X1's unwrap (a Go method-set violation). New behavioral guard: value-sourced
  dyn-interface value with a pointer-receiver-only method must NOT assert to the wider interface.
- **X4 — test-host empty-name subtest numbering** (`core/testing`): Go names repeated `t.Run("")`
  children `#00`…`#NN`; the host emits `#00`, `#00#01`, …. **Branch only the `name == ""` case**
  `[R:B-X4]` — the shared dedup dictionary also serves duplicate *non-empty* names
  (`sort.TestFind`'s `ab#01/#02`), which must keep their keys byte-identical.
- **X5 — oracle address normalization** (`testConversion.go`, compare-only): **two-phase
  matching** `[R:B-X5]` — exact names first; only the leftovers are re-keyed with
  `0x[0-9a-f]+ → 0x…` and paired **only when unambiguous 1:1**. Deterministic hex-literal names
  (text/scanner-style tables) match exactly in phase 1 and are never collapsed; collisions
  simply stay unmatched (fail loud, never mask).

## 5. Explicitly deferred Phase-3 surface (named next consumers — same chip, next increment)

| Item | Next consumer (demonstrated, not predicted) |
|---|---|
| `Value.Call` (delegate `DynamicInvoke`, multi-return tuple destructure into `NumOut`/`Out(i)`) | **testing/quick** (review: buildable on the triple; no `flagMethod` needed) |
| `MakeSlice`/`MakeMap`/`New`/`SetMapIndex`/`Set{Int,Uint,…}`; `Field(i)`/`Index(i)` addressability via ж field-ref/element-ref alias boxes + a runtime ref-accessor builder | **testing/quick** (`R`-struct round-trip), **encoding/binary** (Read/Write recursion), then **gob** |
| Go unnamed↔named `directlyAssignable` refinement | encoding/binary named-slice cases |
| reflect-side `Implements`/`AssignableTo` mirrors | first reflect consumer that calls them |
| map/chan/func `Zero` kinds | quick/gob |
| **Known limitation `[R:G-F3]`:** named func types collapse to their structural `Func<>`/`Action<>` under `canonType` System.Type interning — `TypeOf(namedFunc) == TypeOf(plainFunc)` wrongly true. Recorded now; needs carried named-func identity if a consumer (gob's type registry) lands on it. | gob |

## 6. Decisions requested (§10)

1. **Bless the v2 write-back model** (§2): box-carried addressable Values with the *structural*
   nil predicate, assignability-first Set with compiled ref-accessor stores, the single
   canonical-nil-box encoding shared with X2, methodName as the getcallersp boundary, R10 unwrap
   with adapter-aware readers.
2. **Scope ruling on X1–X5**: recommendation — this chip lands them **in the same gated
   change-set** (errors cannot validate without them; the chip already owes every §5 gate these
   layers require; splitting to the coordinator serializes the same gates twice and leaves the
   chip's consumer undemonstrable). The chip's ledger lock extends to: golib
   (`builtin.cs` fallback, `ж.cs` equality/structural-nil, `GoReflect.cs`), the TypeGenerator
   `InterfaceTypeTemplate`, the converter's nil-conversion/comparison emission sites,
   `core/testing` host naming, and `testConversion.go`'s oracle matching.
3. **Increment split**: land the errors-validating increment 1 first, then design increment 2
   (`Call`, `MakeSlice`/`MakeMap`, Field/Index addressability) against testing/quick +
   encoding/binary differentials. The §2 contracts (ref-accessor store, single nil encoding) were
   *specifically* hardened by review so increment 2 extends rather than reworks them.

## 7. Gates (§5) for increment 1

golib + gen + converter all touched → **every** gate:

- Full behavioral suite; CNR byte-identical **except** the X2 emission sites (each individually
  justified; `NamedPointerReinterpret` golden re-baseline expected and its **Output** must stay
  `nilref` — the X2 correctness gate); 302-corpus reconvert + build; operational re-validation of
  all 32 validated packages (isolation-reconvert-diff to skip byte-identical).
- **⚠ Deploy-root hazard (chip-found):** behavioral tests that reference `core\reflect`
  (`DeepEqual`) resolve `$(go2csPath)` to the **deployed** `%GOPATH%\src\go2cs\` tree, not the
  repo — refresh the deployed `reflect`/`golib` (deploy-core) before trusting those results.
- **⚠ Baseline-mirror drift (chip-found):** `src/core/internal/reflectlite` is a **pre-bridge**
  copy (no `makeReflectValue`/`ValueOf` bridge, no `swapper_impl.cs`) and `src/core/internal/abi`
  has **no `type_impl.cs`** — yet the baseline stub `errors` is the modern reflectlite-using
  emission, so its `Is`/`As` would NRE at first real call in a behavioral context (nothing calls
  it today, which is why the suite is green). **Guard placement therefore:** golib-machinery
  guards (X1 assert-through-adapter, X2 typed-nil `%T`/equality, X3 method-set negative) exercise
  golib directly in the behavioral suite — no baseline reflect needed; reflect-surface coverage
  (Zero/Elem/Set round-trip, Q1a write-protection) rides the **errors pipeline suite** (18 TestAs
  cases exercise the preamble shape every re-validation). Whether to sync the baseline
  reflectlite/abi mirrors (a partial-promotion question under the 2026-07-01 no-promotion ruling)
  is **explicitly out of this chip's scope** — reported to the coordinator instead, alongside the
  pre-existing `DeepEqual` deploy-binding fragility.
- New behavioral guards: typed-nil-interface semantics (`%T`, `!= nil`,
  `TypeOf((*T)(nil)).Elem()`, cross-type/nil-box equality); dyn-assert-through-adapter (poser
  shape); value-vs-pointer method-set negative (§4 X3); reflect Zero/Elem/Set round-trip (TestAs
  preamble shape); methodless-pointer `%v` (§2.6); `sort.TestFind` subtest-key stability (X4).
- New guard for blessing condition Q1(a): a `Set` through a typed-nil-derived Value panics
  Go-style and the canonical singleton is unmodified after.
- **Commit shape (Q2 ruling):** one commit per X-fix, each carrying its own guard; the reflect
  §2 surface as its own commit(s); all on the chip branch `claude/optimistic-bassi-496625` —
  the coordinator ff-merges after the final all-ships-rise.
- Docs in the same change: `ConversionStrategies(-Reference).md` (X2 typed-nil boxing is a
  headline strategy change), `DESIGN-reflection-bridge.md` Phase-3 increment-1 record.

## 8. Adversarial-review ledger (§7, 2026-07-24)

Three independent reviewers (Go-semantics / blast-radius / generalization lenses). Findings and
disposition — all folded above:

| # | Finding | Disposition |
|---|---|---|
| C1 | `IsNull` misclassifies `ж<ж<T>>`-holding-null as nil → TestAs dies at case #0 | §2.1 structural-nil predicate (**critical**) |
| C2 | Set's nil row skipped assignability; typed-nil→interface dst must store non-nil eface | §2.2 rule 2 + interface row |
| C3 | valid-nil `Interface()` returned null → type loss one call from X2's fix | §2.4 single encoding |
| C4/G-F1 | `ж<T>.Value` is get-only ref — reflection SetValue store unimplementable; field/element parents need ref-routed accessors | §2.2 step 3 compiled ref-accessor contract |
| C5 | X2 empty-interface-only scope left adapter `Box=null` typed-nils: `errors.Is((*A)(nil),(*B)(nil))` wrongly true | §4 X2.2 adapter-ctor seeding |
| G-F2 | Dual nil encoding (Zero's null vs X2 singleton) split-brains gob/json round-trips | §2.4 single encoding |
| G-F3 | Named func types collapse under System.Type interning | §5 recorded limitation |
| B1 | X2 asymmetry flips `NamedPointerReinterpret` output (`nilref`→`other`); comparison operands are reference-equality on `object` | §4 X2.1 symmetric emission + named-type singletons + gate |
| B2 | X1×X3 interaction can violate Go's value-vs-pointer method set (both directions) | §4 X1 `{Box: not null}` + X3 conditional exposure + new negative guard |
| B-R10 | Adapter-kind flip exposes `Elem()`-invalid latent fault, currently masked | §2.6 adapter-aware readers + `%v` guard |
| B-X4/X5 | Host renumbering entangled with duplicate-name dedup; hex normalization can collapse deterministic names | §4 X4 branch-only-empty; §4 X5 two-phase matching |
| chip | nil-box vs null-reference `ж` equality; DeepEqual behavioral test binds the *deployed* reflect | §4 X2.3; §7 deploy-root hazard |

Reviewer verdicts on the surviving core: the 18-case TestAs walk closes under §2+X1+X3 (correct
by case analysis, not hope); X1-alone and X3-alone could not be refuted against the 457-test
corpus; R10's `%T`/`IsComparable`/csv-DeepEqual consumers verified stable; Call confirmed
buildable on the `boxed/addrBox/typ_/flag` representation without `flagMethod`.
