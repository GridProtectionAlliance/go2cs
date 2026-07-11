# reflect compile-error census — 154 errors, 14 deduplicated families

Merges applied: analysts' families 2+6 (same `getMetadataStructFields` fields-only defect, CS1061+CS0117 views), 4+9 (same `getTypeName` slash-strip), 5+8 (same `@string` InheritedTypeTemplate gap), 12+15 (same `len(sel.Index())==2` gate). Counts sum to 154 and match the recon116 handoff distribution (CS1061 54 = F1+F2; CS0030 ~27; CS1503 16; CS0021 14; CS0117 13).

## 1. Deduplicated families (largest first)

### F1 — Cross-package embed metadata fallback enumerates fields only — **45** (CS1061 ×32 + CS0117 ×13)
reflect's wrappers (`structType`/`ptrType`/`sliceType`/`mapType`/`interfaceType` embedding `abi.XxxType`) resolve through TypeGenerator's metadata fallback `getMetadataStructFields` (`src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs` ~220–254), which yields only public instance `IFieldSymbol`s. It therefore misses (a) the embedded member itself — emitted in abi as a `public partial ref Type Type { get; }` **property** — so no `ᏑType` box accessor exists for `&t.Type` sites (13 × CS0117), and (b) every field promoted transitively through abi.Type's own embed (`Hash`, `TFlag`, `Str`, `Size_`, …) for value reads (32 × CS1061). The code's own comment concedes transitive promotion "is not chased."
**Machinery:** extend the existing metadata fallback (added for runtime's `rtype{*abi.Type}`) to also yield public non-static **ref-returning `IPropertySymbol`s** — exactly the shape of the referenced assembly's generated embed + promoted-field accessors; one enumeration fixes both sub-shapes via the established single-hop `X => ref Embed.X` emission.
**Priority:** highest impact, one generator function. **HARD DEPENDENCY on F2** (the forwarded-to abi accessors must be public first). Watch the parallel cross-package promoted-**method** gap (collectPromotedMethods early-return) as a sibling locus.

### F2 — Promoted-field accessor scope derived from field-TYPE name case — **22** (CS1061)
`PromotedStructDeclarations` (StructTypeTemplate.cs ~128–129, 151–152) computes accessor visibility as `GetScope(GetSimpleName(typeName))` — lowercase-typed fields (`uintptr Size_`, `uint32 Hash`, `ж<byte> GCData`) get **internal** promoted accessors on `abi.ΔArrayType`/`ΔFuncType`/`ChanType`, invisible to reflect cross-assembly. Uppercase-typed fields (`TFlag`, `NameOff Str`) are public — exactly the observed split.
**Machinery:** the identical defect was already fixed in the same file for box-field (`ទ`) accessors — `FieldReferences` (~383–389) derives scope from the **member name's** exportedness. Apply the same rule to both promoted-accessor loops, gated so genuinely-internal field types don't produce CS0053 (whitelist builtins / resolve real accessibility).
**Priority:** prerequisite for F1; tiny, generator-only, zero golden churn. Land F2+F1 as one commit.

### F3 — sparseArrayKey single-cast `(int)key` on unsigned named-numeric keys — **27** (CS0030)
`sparseArrayKey` (`src/go2cs/convKeyValueExpr.go` ~150–154) emits the two-step split `(int)((underlying)key)` only for `types.Uintptr`; `types.Uint` (ΔKind → `nuint`) falls to the single `(int)key`, and C# won't chain the user-defined `ΔKind→nuint` operator into an explicit `nuint→int`. All 27 = the keys of the one `kindNames` literal (type.cs:422–448). Uint32/Uint64 latent too.
**Machinery:** the existing Uintptr split gate (the syscall Errno CS0030×131 fix) — widen it to Uint/Uint32/Uint64/Uintptr.
**Priority:** one-line converter gate, 27 errors, near-zero golden risk. Cheapest big win.

### F4 — getTypeName cross-package slash-strip eats `*` inside composites — **17** (CS1503 ×16 + CS0029 ×1)
`getTypeName` (`src/go2cs/main.go` ~2990–3126) renders `*types.Slice` via `t.String()`; `[]*internal/abi.Type` → peel `[]` → last-slash strip discards the leading `*` → `slice<abi.Type>` (value elems). `typesByString`'s result/locals/type-asserts lose the pointer; all downstream range vars mismatch (`abi.Type → ж<abi.Type>` / `→ uintptr`), plus the append CS0029. Also silently produces **wrong-at-runtime** type asserts that compile (type.cs:2045/2057/2065).
**Machinery:** `getFullTypeName` already fixed this structurally (main.go 3141–3152, comment cites this exact hazard) — port the `[]`/`[N]` + recurse-into-element case into `getTypeName` so pointer elems re-enter the correct `*` arm.
**Priority:** high — fixes 17 plus latent runtime-wrong asserts corpus-wide. Medium care: run check-no-regression; check Map/Chan-of-star strings as a follow-up latent shape.

### F5 — `@string` named-type wrapper lacks index/Range surface + span-literal bridge — **16** (CS0021 ×14 + CS0019 ×2)
`[GoType("@string")]` structs (StructTag) get only `m_value` + implicit `@string` conversions from InheritedTypeTemplate. C# indexing never applies user-defined conversions → every `tag[i]` / `tag[i..]` fails (14); and `tag != ""u8` needs a two-hop conversion C# won't compose (2). Converter emission is already correct — pure generator template gap.
**Machinery:** direct siblings exist for both halves: IArrayViewTypeTemplate (named-over-array indexing, the pallocBits fix) and UintptrBridgeOperators (the "C# never chains two user conversions" precedent). Gate on `TypeName == "@string"`: forward `this[int]`/`this[nint]`/`this[Range]` (Range returns the wrapper) and add `implicit operator {T}(ReadOnlySpan<byte>)`.
**Priority:** high leverage — clears 16 here and latent sites in every named-string wrapper corpus-wide (json.Number, tagOptions, …). Generator-only, zero golden churn; re-run NamedStringConversion behavioral test.

### F6 — Generic-call type args sourced from RESULT-type instantiation — **11** (CS0305)
`convCallExpr.go` ~826–861 builds explicit type args from the return type's `Named.TypeArgs()`; for `rangeNum[int](v.Int())` returning `iter.Seq[Value]` it emits `rangeNum<ΔValue>` (wrong count, wrong identity), and the strip-and-append logic destroys the partially-correct rendering.
**Machinery:** the correct source already exists two blocks down in the same function — `v.info.Instances[funIdent].TypeArgs` (the slices/maps CS0411 fix) — but is preempted. Re-source the first block from Instances for non-`funIsType` callees; keep result-typeargs only for conversions/constructors.
**Priority:** moderate effort, 11 errors, and structurally more-correct for all future generic calls. Gate with check-no-regression (Option[T]-shaped calls render identically).

### F7 — Promoted pointer-receiver method through ≥2 value embeds — **4** (CS1929)
`convSelectorExpr.go:856` gates the embed-hop descent to `len(sel.Index()) == 2`; `interfaceType→abi.InterfaceType→abi.Type` (Uncommon, ×2) and `sliceType→abi.SliceType→abi.Type` (Common, ×2) have Index length 3 and fall through to a bare call the `ж<abi.Type>` extension can't bind.
**Machinery:** the WAVE-17/18 single-hop descent in the same arm, plus TypeGenerator's **transitive** promoted-`Ꮡ` accessors (already proven by `Ꮡt.of(interfaceType.ᏑType)` compiling nearby). Widen the gate to all-value-embed chains, synthesizing the selector with the **terminal** embed field.
**Priority:** small-medium; grep runtime for siblings — likely pays beyond reflect.

### F8 — Append element is a *T→interface adapter ctor — **3** (CS0121)
`builtin.append(@in, new rtypeᴵΔType(Ꮡt))` — adapter class as bare append element makes both golib append overloads applicable, neither better.
**Machinery:** the `castArgToType` untyped-numeric gate in convCallExpr.go 784–808 — extend it to cast adapter-ctor elements to the slice's interface element type (`(ΔType)new …`, free reference conversion).
**Priority:** small, will recur corpus-wide wherever adapters land in appends.

### F9 — TypeGenerator promoted-method shim hardcodes `this ref` receiver — **2** (CS1510)
Every promotion shim is emitted `this ref` even when the source method is by-value, so `v.Elem().kind()` (rvalue receiver) can't bind.
**Machinery:** the generator already captures receiver ref-ness (`IsExtensionMethodForStruct` distinguishes `flag` vs `ref flag`); StructTypeTemplate ~330 just ignores it — preserve it.
**Priority:** small generator fix; bundle into the F2+F1 StructTypeTemplate commit. Matches Go copy semantics exactly.

### F10 — Constant C# switch over an UntypedInt-rendered tag — **2** (CS9135)
`switch (goarch.PtrSize)` — cross-package untyped const renders as golib `UntypedInt` static readonly; int case labels can't constant-convert to the governing struct type. Label-side gating exists; the **tag** was never gated.
**Machinery:** visitSwitchStmt.go's own label gates (`isCSharpConstantExpr` — whose comment literally cites `goarch.PtrSize` — and the line-185 uintptr-struct gate). Apply the same check to the tag → force the if-else form (option a).
**Priority:** one-conditional fix.

### F11 — Heap-box name mismatch on reserved-renamed locals — **2** (CS0103)
Box declared `Ꮡslice` (raw ident) but field-address arm composes `Ꮡ + sanitized alias` → `ᏑΔslice`.
**Machinery:** `boxBaseName` (convUnaryExpr.go:111–132) exists for exactly this and is already used at two sibling arms — apply it at the heap-boxed-local arm (:372–374); audit the whole-value `&local` path.
**Priority:** trivial.

### F12 — Lifted numeric constraints omit IIncrement/IDecrementOperators — **1** (CS0023)
`i++` on a type param constrained by the Go integer-union lift.
**Machinery:** `getLiftedConstraints` (constraintOperations.go ~281), already extended once this way (IShiftOperators fix). Add both operators.
**Priority:** one-line, but the only tail fix with **golden churn** (Constraints where-clauses grow) — requires UpdateTestTargets re-baseline; sequence accordingly.

### F13 — Double-deref of reinterpret cast: naked `.Value` re-binds into cast operand — **1** (CS0029)
`**(**mapType)(unsafe.Pointer(&imap))` — outer StarExpr falls to the naked-`.Value` default; postfix binds tighter than the casts/`~`.
**Machinery:** convStarExpr.go 132–140 paren-wrap (the runtime panic.go fix) — widen the gate to nested StarExpr operands.
**Priority:** trivial gate widening in the file that owns this defect class.

### F14 — Natural-typed lambda with heterogeneous unsafe.Pointer/uintptr return branches — **1** (CS8917)
convFuncLit discards the declared result type; the deliberate `(uintptr)` unsafe.Pointer call routing makes the two return branches diverge with no best common type.
**Machinery:** convFuncLit.go already computes the result type and throws it away (line 135). Emit an explicit C#-10 lambda return type, gated narrowly to `unsafe.Pointer`-returning literals (the only systematically-divergent shape). Do NOT touch the `(uintptr)` routing.
**Priority:** narrow, low-risk.

## 2. Raw-metal vs fixable — the call

**ZERO families are GoManualConversion stub territory.** Every one is a mechanical emission/template defect with named existing machinery to extend — none involves memory-layout math, type-descriptor byte-walking, or asm. F1's own evidence settles the closest call: `&t.Type` is a plain managed field address, fully convertible via the established box-accessor form.

**One Phase-4 flag (not a compile blocker, not a stub):** the F4 sites route managed `ж<abi.Type>` pointers through `(uintptr)(new @unsafe.Pointer(…))` nuint round-trips (e.g. type.cs:1408). Post-fix this **compiles**, but per the S1-fork doctrine it's a managed-referent hazard across GC at *runtime* — record it on the Phase-4 audit list (same class as `guintptr`); the compile-milestone answer is the emission fix, nothing more.

## 3. Attack order

Note: the handoff shows a **staged, GPG-blocked generator pair** (CS0057/CS0102/0111) awaiting commit — land that first so Wave A doesn't entangle with it.

1. **Wave A — StructTypeTemplate.cs triple (F2 → F1 → F9): −69.** F2 (scope) is the hard prerequisite for F1 (metadata fallback ref-properties); F9 rides in the same file. Generator-only ⇒ no golden churn; rebuild `internal/abi`, then reflect. Verify with full behavioral suite (analyzer output changes affect every project).
2. **F3 sparseArrayKey gate widen: −27.** One-line converter change; check-no-regression.
3. **F5 @string template surface (indexers + Range + span bridge): −16.** Generator-only; corpus-wide payoff; run NamedStringConversion filtered test + full suite.
4. **F4 getTypeName structural composite recursion: −17.** Highest-care converter change (also fixes silent wrong-type asserts); check-no-regression + reflect reconvert; note Map/Chan-of-star as a follow-up check.
5. **F6 generic type args from Instances: −11.** check-no-regression gate.
6. **Tail sweep, one commit each or batched: F7 (−4), F8 (−3), F10 (−2), F11 (−2), F13 (−1), F14 (−1): −13.** All trivial-to-small gate widenings in files that already own their defect class.
7. **F12 last: −1.** Only tail item with behavioral-golden churn (Constraints re-baseline via UpdateTestTargets --createTargetFiles) — isolate it so golden diffs stay attributable.

Expected trajectory: 154 → ~85 (Wave A) → ~58 → ~42 → ~25 → ~14 → ~1 → 0. Since fmt's 178 is largely a reflect leak (per the 2026-07-03 handoff), remeasure fmt/os after Wave A — the census there should collapse to its own ~28 residue.