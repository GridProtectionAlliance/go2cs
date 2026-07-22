# DESIGN — Native reflection bridge (Phase 4 operational)

> **Status (2026-07-22): Phases 1 and 2 are SHIPPED. Phase 3 is the open work** — and it is a
> chip-class arc, per §6.1 of [`Phase4-Autonomous-Loop-Charter.md`](Phase4-Autonomous-Loop-Charter.md).
> Companion to the Phase-4 operational campaign in [`../Roadmap.md`](../Roadmap.md). The `reflect`
> package *compiled* at the project's Phase-3 milestone (see
> [`../Phase3/Reflect-Census.md`](../Phase3/Reflect-Census.md)) but did not *run* — this is the
> operational bridge that makes it work.
>
> ⚠ **"Phase 1/2/3" below are THIS DOCUMENT's scope phases** (§ *Scope*), unrelated to the project's
> Phase-3 / Phase-4 milestones.
>
> **Implemented** — `manualConversionFuncs` in `src/go2cs/manualTypeOperations.go` is the authoritative
> list; derive it fresh rather than trusting this summary: the Kind classifier + type helpers (golib
> `GoReflect`: `KindOf`, `GoTypeName`, `ElementType`, `IsComparable`, `TryAdapterWrappedType`),
> `internal/abi.TypeOf` (`type_impl.cs` synthesizes the descriptor from the managed `System.Type`),
> `reflect.{ValueOf, unpackEface, valueInterface}`, the ~17 `Value` readers + `MapIter.{Next,Key,Value}`,
> `rtype.{String, Name, Elem, Field, NumField}`, **canonical interned `Value.Type`/`toType`**
> (`canonType` — the map-key-ordering fix, § below), `deepValueEqual` (`deepequal_impl.cs`), the
> `internal/reflectlite` mini-bridge (`ValueOf`/`Len`/`Swapper`), and the `synthType.Equal`
> comparability signal (encoding/csv, 2026-07-21).
>
> **NOT implemented — the chip's scope:** everything in *Phase 3* (`Value.Set*` + addressability/
> `CanSet`, `Value.Call`/`MakeFunc` dynamic invocation, `MakeSlice`/`MakeMap`/`New`), the
> `getcallersp` stub (`PartialStubGenerator` `NotImplementedException` in `runtime`; errors TestAs →
> `reflect.mustBeAssignableSlow`), open question 3 (field-name/tag fidelity) beyond what `rtype.Field`
> returns today, and the **adapter-type follow-up**: `GoReflect.KindOf`/`ElementType` still report the
> *adapter class* for `IжAdapter` / ᴠ-adapter types, where `GoTypeName` already unwraps them via
> `TryAdapterWrappedType` (flagged by R10 in
> [`StringsBytes-BlockerMap.md`](StringsBytes-BlockerMap.md)).

## The problem

Go's `reflect` is built on reading an interface's internal two-word layout through `unsafe.Pointer`.
An `any` value is an `eface = { *abi.Type type; unsafe.Pointer data }`; `reflect` reinterprets the
address of the interface as that struct, reads the `type` word (a pointer to the runtime **type
descriptor** `abi.Type`), and reads/writes the value through the `data` word as flat memory at
computed field/element offsets.

None of that exists in the managed world. A go2cs `any` is a `System.Object` **reference** — one
word, no adjacent type descriptor, no flat-memory value the GC will let you address by offset.
Reinterpreting the object reference as `{type,data}` reads garbage and NREs. Concretely, the first
operational hit is:

```
color.New(FgGreen).Println("…")
  → fmt.Sprint(a…) → fmt.doPrint → reflect.TypeOf(arg).Kind()   // spacing decision
  → internal/abi.TypeOf(any a):  ~(ж<EmptyInterface>)(uintptr)(Ꮡ(a))   // reinterpret → NRE
```

(Plain `fmt.Println` sidesteps this: `doPrintln` never calls `reflect.TypeOf`; only `doPrint` — used
by `Print`/`Sprint`/`Sprintf` fallbacks — does. That is why the `fmt.Println("hi")` milestone runs.)

## Key insight — the entire unsafe chain enters at THREE constructors

A full read of the `reflect`/`internal/abi`/`fmt` surface (see the surface map below) shows the whole
model bottoms out on **one** primitive — the eface `{type,data}` reinterpret — reached at exactly
three points:

| # | Function | File | What it does today |
|---|---|---|---|
| 1 | `internal/abi.TypeOf(any a)` | `internal/abi/type.cs:125` | `&a` → `*EmptyInterface` → read `.Type` word |
| 2 | `reflect.unpackEface(any i)` (→ `ValueOf`) | `reflect/value.cs:156` | `&i` → `*EmptyInterface` → read `.Type` + `.Data` |
| 3 | `reflect.toType(*abi.Type)` | `reflect/type.cs:3040` | wraps a descriptor pointer as an `rtype` |

Every downstream `Type`/`Value` method then reads *from those words* — `Type.Kind()` masks
`abi.Type.Kind_`; `Value.Int()` does `~(ж<int64>)(uintptr)(v.ptr)`; `Value.Field(i)` does
`add(v.ptr, offset)`; etc.

**So the bridge is: replace those three constructors so they carry a `System.Type` (+ the boxed
`object` for a `Value`) instead of two raw words. The ~30 downstream methods `fmt` needs then become
ordinary managed-reflection wrappers** (`obj.GetType()`, `FieldInfo.GetValue`, array indexing,
`Convert.ToInt64`, `IDictionary` enumeration) with no `unsafe` anywhere.

## Architecture — a native `reflect`/`abi` shim over `System.Type` + `System.Object`

Hand-own the reflection **entry points and the exercised methods** (whole-file
`[module: GoManualConversion]`, the established pattern — cf. native `sync`, `atomic.Value`), so:

- `reflect.TypeOf(x)` returns a `Type` carrying `x.GetType()` (a `System.Type`), **not** a
  `ж<abi.Type>` descriptor pointer.
- `reflect.ValueOf(x)` returns a `Value` carrying `(object box, System.Type)`.
- `internal/abi.TypeOf(x)` returns a compatible handle (or `reflect` stops calling it — TBD, see Q4).
- Each `Type`/`Value` method is reimplemented over managed reflection.

The one genuinely new primitive is the **Kind classifier** — `System.Type → reflect.Kind` — the root
of every method. It reads go2cs's own representations and attributes:

| Go Kind | go2cs C# representation | detect |
|---|---|---|
| Bool / Int* / Uint* / Float* | `bool`,`nint`,`int`,`long`,`byte`,`double`,… | `typeof` |
| Uintptr | `uintptr` (golib struct) | `typeof(uintptr)` |
| Complex128 | `System.Numerics.Complex` | `typeof` |
| String | `@string` | `typeof(@string)` |
| Slice / Array / Map / Chan / Pointer | `slice<T>`/`array<T>`/`map<K,V>`/`channel<T>`/`ж<T>` | open generic typedef |
| Func | `GoFunc` / delegate | `IsSubclassOf(Delegate)` |
| Struct | `[GoType]` value struct (no `num:`) | `[GoType]` + `IsValueType` |
| Interface | Go interface → C# interface | `IsInterface` |
| UnsafePointer | `@unsafe.Pointer` (`: ж<uintptr>`) | `typeof` |
| *named* `type Celsius float64` | `[GoType("num:float64")] struct Celsius` | Kind = underlying; `Name`/`String` from the type |

The metadata the shim recovers Go type info from is **already emitted**: `[GoType(def)]` (struct/
interface marker + `num:<kind>` for named numerics), `[GoTag]` (= `DescriptionAttribute`, the raw Go
struct-field tag), `[GoRecv]` extension methods (the method set), and the golib generic types. C#
`FieldInfo` enumeration returns fields in declared (= Go source) order.

## Scope — three phases, each independently useful

**Phase 1 — `TypeOf().Kind()` / `.String()` (unblocks the color sample + scalar `Print`/`Sprint`).**
`doPrint` calls `reflect.TypeOf(arg).Kind()` for *every* arg; the value itself formats via the fast
path or the `Stringer`/`error`/`Formatter` C# interface assertions in `handleMethods` — which use **no
reflection at all**. So a `Type` shim exposing `Kind()`, `String()`, and `Elem()` (byte-slice check),
plus the Kind classifier, makes `fmt.Print`/`Sprint`/`Sprintf` work for every scalar and every type
with a `String()` method. **~1 shim type, ~4 methods, +1 classifier.** This is the minimal color-sample
unblock.

**Phase 2 — full `printValue` walk (composites without a `String()` method format correctly).**
Add the `Value` shim (~21 methods actually exercised: `Kind, IsValid, CanInterface, Interface, Type,
Bool, Int, Uint, Float, Complex, String, IsNil, NumField, Field, Elem, Index, Len, Bytes, CanAddr,
UnsafePointer, Pointer`), `Type.{Elem, Field(i).Name}`, a `MapIter` (`Next/Key/Value` over
`IDictionaryEnumerator`, for `fmtsort`), and `StructField.{Name,Tag,Type}`. **~2 core shim types +
MapIter + StructField, ~30 methods**, all managed reflection. Makes `%v`/`%+v`/`%#v` of structs,
slices, maps, and pointers correct.

**Phase 3 — write-back & call (`Value.Set*`, `Value.Call`, `MakeFunc`, addressability) — OPEN; the
chip's scope.** Needed by `encoding/binary`, `encoding/gob`·`json`·`xml`, `testing/quick`,
`text/template`, and (transitively) `math/big`; not by `fmt`. Larger, and **best designed against a
concrete consumer** — which is exactly why the charter (§6.1) spawns this chip only once a package's
differential actually lands on this surface, and requires designing WITH the user + adversarial design
review before implementation. Carry the `getcallersp` stub and the adapter-type `Kind`/`Elem` unwrap
in the same chip.

## Open design questions (for review)

> **Resolved by what shipped (2026-07-22):** **Q1** — the native shim was confirmed and built; the
> descriptor-synthesis alternative was not pursued. **Q2** — both Phase 1 *and* Phase 2 were built.
> **Q4** — the answer was *both*, deliberately: the entry points and exercised methods are whole-file
> hand-owned `*_impl.cs`, while the reusable managed logic (Kind classification, Go type naming,
> element types, comparability, adapter unwrap) lives in golib `GoReflect` so `reflect`,
> `internal/abi`, `internal/reflectlite`, and golib's own `builtin` formatting all share one
> implementation. **Q3 is still open** and belongs to the Phase-3 chip. Q1–Q4 are kept below as the
> record of the decision.

1. **Approach — confirm the native shim.** Replace the 3 constructors + reimplement the exercised
   methods over `System.Type`/`System.Object` (recommended). The alternative — synthesize faithful
   `abi.Type` *descriptors* from `System.Type` and keep Go's converted `reflect` reading them via
   `unsafe` offsets — is judged infeasible (the offset reads themselves don't work in managed memory;
   it is strictly more surface than the shim).

2. **Scope to build now — Phase 1 only, or Phase 1 + 2?** Phase 1 unblocks the color sample and all
   scalar/Stringer formatting for the least work; Phase 2 makes composite formatting correct. Phase 3
   is deferred regardless.

3. **Field-name / tag fidelity.** `fmt`'s `%+v`/`%#v` (and later `json`) need the **exact Go** field
   name + tag. C# field names are the Go names except where escaped (`@string`) or Δ-collision-renamed;
   `[GoTag]` already carries the tag. Options: reverse-map the escapes at the shim, or have the
   converter emit a per-struct Go-name table (a small attribute) the shim reads. (Phase 2 concern.)

4. **Where the shim lives.** Hand-own `reflect` + `internal/abi.TypeOf` as whole-file
   `[module: GoManualConversion]` (consistent with native `sync`/`atomic.Value`), **or** put the
   managed logic in a golib `GoReflect` helper that the converted `reflect` delegates into (smaller
   hand-owned footprint, but a converted↔golib seam through the shim). Recommend whole-file hand-own of
   the entry points, since the value/type model is pervasively unsafe and not worth converting.

## Surface map (reference)

`abi.Type` fields: `Size_`, `Kind_` (the field everything keys off), `TFlag`, `Str`/`PtrToThis`
(offset-encoded), `PtrBytes`/`Hash`/`Equal`/`GCData` (unused by fmt). Kind enum (values 0–26) is
defined identically in `internal/abi/type.cs:40` and `reflect/type.cs:245`. `fmt` calls only
`Type.{String, Kind, Elem, Field(i).Name}` and the ~21 `Value` methods tabulated in Phase 2.
`handleMethods` (`fmt/print.cs:795`) formats `Stringer`/`error`/`GoStringer`/`Formatter` via C#
interface assertions — no reflection. Fast-path `printArg` types (no reflection):
`bool, int/8/16/32/64, uint/8/16/32/64, uintptr, float32/64, complex64/128, string, []byte,
reflect.Value`.

## Fix — reflect.Type must be canonical (map-key ordering)

Go's `reflect.Type` is a canonical interned descriptor: `TypeOf(x) == TypeOf(y)` exactly when `x`
and `y` share a dynamic type, so `aType == bType` is a pointer compare. `internal/fmtsort.compare`
(the map-key ordering used by `fmt`'s `%v`) relies on it: `if aType != bType { return -1 }`.

The bridge minted a **fresh** wrapper per access — `abi.TypeOf` allocates a new `abi.Type` box, and
both `Value.Type()` and `toType` then build a fresh `rtypeжΔType` (an `IжAdapter` compared by box
identity via `golib` `AreEqual`). So two Types describing the *same* type never compared equal →
`compare` returned `-1` for every pair → the stable sort **reversed** the keys
(`map[b:2 a:1]` instead of `map[a:1 b:2]`).

**Fix:** hand-own `Value.Type` and `toType` in `reflect/value_impl.cs` (registered in
`go2cs/manualTypeOperations.go` `manualConversionFuncs["reflect"]`) so both route through `canonType`,
which **interns** the `ΔType` wrapper in a `ConcurrentDictionary<System.Type, ΔType>` keyed on the
`abi.Type.sysType` the Phase-1 `synthType` stamped. Identity-equality then matches Go. The cache is
process-lifetime (type descriptors are permanent, like Go's). Interning by `System.Type` preserves
Go's named-type distinctness for free: a `type Celsius float64` is a distinct `[GoType("num:float64")]`
struct whose `System.Type` differs from `float64`, so `TypeOf(Celsius) != TypeOf(float64)`. `typeSlow`
(method-value Types) stays auto — not exercised by `fmt`.
