# Design study: pointer-core type-parameter constraints (`[P *T]`)

**Chip:** pointer-core-typeparam-clone (escalated for design study before prototype)
**Root:** go.types CS0311 ×1 + CS0029 ×2 (3 errors; go.types cannot green without it)
**Status:** design → prototype of the recommended option (this branch); final go/no-go at integration
**Branch:** `claude/silly-morse-354e5a` (base master `58466a0c0`)

---

## 1. The problem

`go/types/predicates.go:569`:

```go
// clone makes a "flat copy" of *p and returns a pointer to the copy.
func clone[P *T, T any](p P) P {
	c := *p
	return &c
}
```

C# generics cannot express "type parameter `P` is exactly `ж<T>`": there is no
constraint form that fixes a type parameter to a *specific constructed type*, and
`ж<T>` implements no interface through which `*p` / `&c` could be expressed
generically without new machinery. The converter currently falls through to the
operator-lifting fallback and emits (fresh recon, master `de6c3d3be`-era):

```csharp
internal static P clone<P, T>(P p)
    where P : /* *T */ IEqualityOperators<P, P, bool>, new()   // constraintOperations.go operator lift
    where T : new()
{
    var c = p;          // CS0029: deref dropped — P is not known to be ж<T>
    return Ꮡ(c);        // CS0029: ж<T> is not P
}
```

and the single call site (`call.go:577` `asig = clone(asig)`) synthesizes explicit
type args (`calleeHasConstraintOnlyTypeParam` — T appears only in P's constraint):

```csharp
asig = clone<ж<ΔSignature>, ΔSignature>(asig);   // CS0311: ж<> implements no IEqualityOperators
```

Error accounting: exactly the 3 diagnosed go.types errors (CS0311 at call.cs:647,
CS0029 ×2 at predicates.cs:694–695).

---

## 2. Exact Go semantics required (verified against the spec and GOROOT)

Three spec facts drive the whole design (Go spec, "Interface types" and "Address
operators"; behavior confirmed against go1.23.1 sources):

1. **A non-tilde type term's type set is a singleton.** The type set of the
   constraint element `*T` is exactly `{*T}` — not "types assignable to",
   not "types whose underlying type is". Therefore **`P`'s only permissible type
   argument is `*T` itself: `P ≡ *T` definitionally**, at every instantiation.
   (With `~*T` the set widens to all types whose *underlying* type is `*T`, i.e.
   named pointer types too — see §5, gating.)

2. **Pointer operations on a type parameter require a pointer *core type*.**
   `*p` where `p` is of type-param type is only legal when the constraint's core
   type is a pointer (`go/types` enforces this; `types.CoreType()` is the API).
   The type of `*p` is the core pointer's element. So any Go code that derefs or
   writes through a `P` has, by construction, a pointer core — the recognition
   predicate is aligned with Go's own rule, not a go/types special case.

3. **`clone`'s contract is a flat copy**: allocate a new `T`, copy `*p` into it,
   return its address. In go2cs terms: a fresh `ж<T>` box holding a copy —
   exactly what the existing escape-analysis emission (`ref var c = ref heap(...,
   out var Ꮡc); … return Ꮡc;`) produces once `P` is known to be `ж<T>`.

### 2.1 Stdlib census — every occurrence (exhaustive)

Methodology: five sweeps over `GOROOT/src` (go1.23.1) — direct `[P *T]`-style
lists, `~*` approximations, inline `interface{ *T }`, **multiline** interface
bodies containing a pointer term (pointer terms are only legal in constraint
context, so interface-scoped matching is complete), and pointer-union terms —
plus `GOROOT/src/vendor`.

| Occurrence | In conversion set? | Notes |
|---|---|---|
| `go/types/predicates.go:569` `clone[P *T, T any]` | **YES — the only one** | single call site: `call.go:577` |
| `cmd/compile/internal/types2/predicates.go:566` (twin) | No | `cmd/*` excluded by `stdLibConverter.go:181` |
| `internal/types/testdata/**` (~25 files) | No | test *data*, never compiled |
| `GOROOT/src/vendor/**` | — | zero matches |

So the corpus-wide blast radius of any decl-level lowering is **one function and
one call site** today. The fix must still be *general* (all-ships-rise: user code
and future stdlib — e.g. the typescript-go fork validation target — can use the
pattern), but the A/B gate has a sharp expected footprint: `go/types/predicates.cs`
+ `go/types/call.cs` and nothing else.

---

## 3. Where the converter stands today (mechanics that shape the fix)

Verified by reading the current sources on this branch:

- **Constraint emission** (`main.go getGenericDefinition` ~3187–3388): `*T` matches
  no composite branch (slice/map/chan/array-core/func/struct), so it falls to the
  operator lift (`getLiftedConstraints`, constraintOperations.go): `Pointer` is in
  the comparable operator set → the spurious `IEqualityOperators<P, P, bool>`.
- **Body deref** (`convStarExpr.go:35–61`): `*p` on a parameter ident already
  returns the bare deref-aliased local `p` — the emission is *already correct
  under the pointer-parameter convention*; what's missing is the classification
  that emits the convention (the `ref var p = ref Ꮡp.Value;` preamble and the
  `Ꮡp` box parameter name).
- **Pointer-parameter classification** (`visitFuncDecl.go:406` preamble, `:569`
  box naming): keyed on `param.Type().(*types.Pointer)` — a `*types.TypeParam`
  never matches (its `Underlying()` is the constraint interface).
- **Call-site type-arg synthesis** (`convCallExpr.go:1273–1286`, and
  `convSelectorExpr.go:1403–1408` for non-call uses): emits ALL of
  `info.Instances[funIdent].TypeArgs` when some type param is constraint-only.
- **Prior art for "see through a type param to a pointer"**: the *instantiation*
  side already exists — `instantiatedParamIsPointer` (convCallExpr.go:2593)
  resolves a call's instantiated param type and applies pointer argument-passing
  conventions (internal/weak's `abi.Escape(ptr)` with `T = *T`). This chip is the
  *declaration-side dual*: pointerness known from the constraint, at the decl.
- **`heap<T>` needs no `new()`** (golib builtin.cs:1289 uses `Ꮡ<T>(default!)`),
  so `T any` stays unconstrained after the erasure.
- **Named pointer types** (`type intRef *int64`) emit as `[GoType("ж<int64>")]
  partial class intRef` — a wrapper class, *not* identical to `ж<int64>`. This is
  what gates `~*T` (see §5).

---

## 4. Candidate lowerings

### Option 1 — Constraint-driven substitution ("erase P: render it as `ж<T>`") — RECOMMENDED

When a type parameter's constraint type-set is a **single, non-tilde pointer
term** `*E`, drop the parameter from the emitted C# generic parameter list and
render every occurrence of it as the pointer type it definitionally is:

```csharp
// go: func clone[P *T, T any](p P) P { c := *p; return &c }
internal static ж<T> clone<T>(ж<T> Ꮡp) {
    ref var p = ref Ꮡp.Value;
    ref var c = ref heap(p, out var Ꮡc);
    return Ꮡc;
}
// call.go:577 `asig = clone(asig)`:
asig = clone<ΔSignature>(asig);
```

This is not an approximation: by spec fact §2.1, `P ≡ *T` at every possible
instantiation, so erasing `P` loses **zero** information. The emitted function is
exactly what the Go author could equivalently have written as
`func clone[T any](p *T) *T` — and every existing pointer emission path (deref
alias, escape-analysis heap box, `Ꮡ` conventions, argument passing) then applies
unchanged.

- **Fidelity:** exact for `*T` terms (identity, not encoding). Deref, write-through,
  address-of, assignment through `P` all route through the normal `ж<T>` machinery,
  which is the project's faithful pointer model.
- **Blast radius:** only decls/calls matching the predicate — today exactly
  `predicates.cs` + `call.cs`. The recognition predicate never fires on any other
  constraint shape (single non-tilde pointer term, no methods), so zero churn
  elsewhere is *expected and gateable*.
- **C#-consumer ergonomics (library use case):** best of all options. The consumer
  calls `clone(p)` with a `ж<Signature>` in hand — C# infers `T` because it now
  appears in a real parameter position (`ж<T> p`); no phantom `P` to spell. The
  signature reads naturally to a C# developer.
- **Phase-4 durability:** durable — it's the semantic identity, with no runtime
  shim to later remove and no reflection/encoding to make operational. Runtime
  behavior is the ordinary flat copy through a fresh box.
- **Cost:** ~6 surgical converter sites (§6), three of which are in
  merge-wave-owned files (small additive edits; sequencing note in §6.1).
- **Visual parity cost (the one trade):** the Go reader sees `P` eliminated from
  the emitted signature. Mitigation: keep the converter's existing
  original-constraint breadcrumb convention — emit `/* [P *T] erased: P ≡ ж<T> */`
  (exact form decided at prototype) on the declaration.

### Option 2 — General core-type substitution pass (`types.CoreType`-keyed)

Same mechanism, but fire on *any* pointer core type: `~*T`, and unions collapsing
to a pointer core. Superset of Option 1.

- **Fidelity risk:** under `~*T`, `P` may be instantiated with a *named* pointer
  type, which emits as a `[GoType("ж<E>")]` wrapper class — **not identical** to
  `ж<E>`. Substitution would change the static type flowing through the body.
  Behaviorally this is *nearly* sound (Go forbids methods on named pointer types,
  so no dispatch divergence; wrappers carry implicit conversions), but it is
  conversion-dependent and unproven.
- **Need:** zero occurrences in the entire conversion set (census §2.1).
- **Verdict:** don't buy unproven generality with no customer. Structure Option 1's
  predicate so this is a *gated relaxation later* (the helper already sees the
  tilde bit; today it declines and warns).

### Option 3 — golib constraint shim (static-abstract interface over `ж<T>`)

Add `IPointerBox<TSelf, T>` to golib (deref member + static `From(ж<T>)`),
implement it on `ж<T>`, emit `where P : IPointerBox<P, T>`, and reroute type-param
deref/address emission through interface members (`p.Deref`, `P.From(Ꮡc)`).

- **Fidelity:** sound, and keeps `P` visible in the emitted signature (visual
  parity preserved).
- **Why rejected:** (a) *worse C# ergonomics* — C# cannot infer a type parameter
  through a constraint, so every consumer must spell **both** args:
  `clone<ж<Signature>, Signature>(p)`, forever; (b) the body no longer reads like
  Go (`p.Deref` / `P.From(...)` instead of the established pointer forms) —
  visual parity is lost in the *body* instead of the signature, plus a new golib
  surface and a parallel emission dialect for type-param pointers that every
  future pointer feature must also support (`**P`? `p.field`? each needs a shim
  form). Heavier forever, for zero fidelity gain over Option 1 — the
  managed-referent Option-C ruling prefers the faithful direct model (`ж<T>` *is*
  the model; the shim is an indirection around it).

### Option 4 — Per-instantiation monomorphization

Emit a concrete non-generic `clone` overload per instantiation, rewriting call
sites to the specialized copies.

- **Why rejected:** breaks the one-Go-decl→one-C#-decl visual correspondence;
  cross-package instantiations of an exported generic would require emitting
  code into *consumer* packages (architecturally new and wrong); unbounded
  duplication. Strictly dominated by Option 1, which achieves specialization
  *within* a single still-generic declaration.

### Option 5 — `[module: GoManualConversion]` stub

- **Why rejected:** the FORK ruling reserves stubs for genuine raw-metal
  (layout math, type-descriptor walking, asm). A faithful conversion exists here;
  stubbing it would also leave the general `[P *T]` capability missing for user
  code (end-goal use cases) and violate no-shortcuts-to-compile.

### Option table

| | Fidelity | Blast radius | C# ergonomics | Phase-4 durability | Cost |
|---|---|---|---|---|---|
| **1. Erase P → `ж<T>`** | **Exact (identity)** | 2 files (gated) | **Best — inference works** | **Durable, no shim** | ~6 sites, 3 in wave files |
| 2. General core-type pass | Exact for `*T`; unproven for `~*T` | same today | same as 1 | durable | 1 + tilde/union proofs |
| 3. golib shim | Sound | golib + gen + emission dialect | Poor — both args always | durable but heavy | High |
| 4. Monomorphize | Sound per copy | per-instantiation copies | odd (concrete only) | rewrite magnet | Medium-high |
| 5. Manual stub | None (bypass) | 1 file | n/a | thrown away | Low |

**Recommendation: Option 1**, with the recognition predicate written so Option 2
is a later gated relaxation, and explicit warnings (never silent mis-emission)
for the shapes it declines (`~*T`, pointer unions, pointer-core params on generic
*type* declarations).

---

## 5. Gating rules of the recognition predicate

`pointerCoreConstraint(tp *types.TypeParam) (*types.Pointer, bool)` (new, in
`constraintOperations.go`) returns the pointer only when ALL hold:

1. the constraint interface has **no methods** and its type set is a **single term**;
2. the term is **non-tilde** (`P ≡ *E` identity — §2.1);
3. the term's type is `*types.Pointer`.

Declined shapes (all zero-occurrence in the conversion set) keep today's emission
**plus a `showWarning`** so a future occurrence surfaces instead of silently
mis-compiling: `~*E` (named-pointer identity gap, §4 Option 2), unions containing
pointer terms, method-carrying pointer constraints (`interface{ *T; M() }`).
Generic **type** declarations (`type X[P *T, T any] …`) share `getGenericDefinition`;
the same erasure applies mechanically, but with no stdlib instance to prove it the
prototype keeps it gated-with-warning unless the guard test proves it cheaply.

---

## 6. Implementation plan (Option 1)

The invariant that makes this correct: **the renderer and every pointer-
classification predicate must flip together.** `P` renders as `ж<T>` *and* is
classified as a pointer everywhere the emission asks — a partial flip is worse
than none. The surgical site list (all verified against current sources):

| # | Site | Change |
|---|---|---|
| 1 | `constraintOperations.go` (new) | `pointerCoreConstraint(tp)` predicate (§5) |
| 2 | `main.go getTypeName` (~3818, beside the `*types.Pointer` arm) | `*types.TypeParam` arm: if pointer-core, render `"*" + getTypeName(elem)` — reuses the entire existing pointer rendering chain (`convertToCSTypeName` → `ж<…>`, `getRefParamTypeName`, etc.). Check `getFullTypeName` needs the same arm (A/B will show). |
| 3 | `main.go getGenericDefinition` (~3214–3388) | skip erased params from the `<…>` list and `where` clauses; emit the breadcrumb comment |
| 4 | `visitFuncDecl.go:406` + `:569` | classification unwrap: treat a pointer-core type param like `*types.Pointer` (deref-alias preamble; `Ꮡ` box naming). **Wave file (goformat chip).** |
| 5 | `convCallExpr.go:1279` loop; `convSelectorExpr.go:1408` loop | when synthesizing explicit type args, **skip erased positions** (resolve the callee's type-param list; drop args whose param is pointer-core). **Wave files (functype / capture chips).** |
| 6 | — | `showWarning` for declined shapes (§5) |

Explicitly **no changes needed** (verified by reading the paths): `convStarExpr`
(the parameter-deref shortcut already emits the alias form — convStarExpr.go:49–60),
`convUnaryExpr`/escape analysis (`&c` on a local is type-agnostic; `heap<T>` has
no `new()` bound), `visitReturnStmt` (return type now matches), argument passing
(`instantiatedParamIsPointer` already resolves instantiated pointer params).

Call-site strategy note: an alternative is to update `calleeHasConstraintOnlyTypeParam`
/ `typeUsesTypeParam` so erased params count as "using" their element param — then
no synthesis fires at all and the call emits bare `clone(asig)` (C# infers `T`).
Chosen instead: keep synthesis and skip erased positions (`clone<ΔSignature>(asig)`)
— deterministic under all shapes (e.g. a callee with *another* genuinely
constraint-only param still synthesizes correctly), and doesn't perturb a shared
predicate. The bare-call polish can come later if wanted.

### 6.1 Sequencing / file ownership (BINDING for this chip)

`visitFuncDecl.go`, `convCallExpr.go`, `convSelectorExpr.go` belong to concurrent
chips in the merge wave that had not landed on master (`58466a0c0`) when this
study was written. Per the coordination rule, the prototype must **rebase onto
post-wave master before touching them**; if the wave still hasn't landed when the
prototype is ready, those three edits stay staged-but-uncommitted or the branch
waits — they are small and additive (one `if` unwrap at two visitFuncDecl sites;
a per-position `continue` in two synthesis loops), so conflict risk after rebase
is minimal. `constraintOperations.go` and `main.go`'s type-name/generic-definition
arms (NOT the func-type textual arm ~4520–4595, which the functype chip owns) are
this chip's own.

### 6.2 Gates (Phase 2)

1. `check-no-regression.ps1` — expect **zero** behavioral-corpus churn (no existing
   test uses pointer-core constraints).
2. Full-stdlib A/B reconvert (HEAD-exe vs fixed-exe, stash dance) — expected diff:
   **exactly `go/types/predicates.cs` + `go/types/call.cs`**; inspect every changed
   file; any third file means the predicate over-fires — stop and re-derive.
3. Build `go.types` (overlay) — the 3 errors clear; own-error count otherwise moves
   only by those 3 (sibling chips own the rest).
4. **New behavioral guard** `PointerCoreConstraints`: generic funcs over `[P *T, T any]`
   exercising deref-read (`c := *p`), deref-write (`*p = v`), address-of-local
   return (`&c`), P-typed param/return round-trip, flat-copy independence
   (mutate original after clone; both orders of type-param declaration), against
   two element types (struct + basic). Output-compared vs Go; registered in
   `src/go2cs.slnx` + grep-verified + `dotnet restore` check.
5. `docs/ConversionStrategies.md` — new subsection under **Generic Constraints**
   (§ line ~1007): "A single-term pointer constraint `[P *T]` erases P to `ж<T>`".

---

## 7. Questions for the integration ruling

1. **Accept the visual-parity trade?** The emitted signature drops `P`
   (`clone<T>(ж<T> Ꮡp)` + breadcrumb comment) rather than keeping a two-param
   generic that C# cannot constrain. This is the crux of Option 1 vs Option 3;
   everything else follows. (Recommendation: yes — signature-level erasure with a
   breadcrumb beats body-level dialect divergence and permanently worse C# calls.)
2. **Warn-only gating for `~*T`, pointer unions, and generic type decls** until a
   real occurrence exists? (Recommendation: yes — zero occurrences; never silent.)
3. **Sequencing**: confirm the prototype's wave-file edits (visitFuncDecl,
   convCallExpr, convSelectorExpr) land only after a rebase onto post-wave master.
4. Call-site form: explicit skip-erased type args (`clone<ΔSignature>(asig)`,
   chosen) vs bare-call inference polish — cosmetic; either compiles.
