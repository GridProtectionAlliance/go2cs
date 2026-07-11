# Phase 3 Floor Assessment — stdlib compile milestone

> ✅ **PHASE 3 COMPLETE (2026-07-10, commit `51ba5d9cf`, tag `stdlib-green-2026-07-10`) — 302/302 packages
> compile.** The assessment below is a mid-campaign snapshot (2026-07-05, 239/302); it is **retained as
> historical record**. Next is Phase 4 (operational — running Go's own tests); see [`Roadmap.md`](Roadmap.md).

> Snapshot after the 2026-07-05 converter-fix campaign (~14 roots this session). Measures the full
> `src/go-src-converted` auto-conversion (**303 projects**) built as one solution
> (`go-src-converted.sln`). "Compiling" = the C# compiles; **operational correctness is Phase 4**, not
> this milestone.

## Headline

**DLL-measured reality (2026-07-05, HEAD `128a69f52`): 239/302 packages genuinely compile (own DLL emitted); 12 fail with own-errors; 51 are SKIPPED only because they sit behind those 12.** The census "291/303" counts by *own-error absence*, which over-counts: a package skipped behind a failed dependency reports zero errors yet emits no DLL. The honest number is **239/302 (79%) genuinely compiling** — but the entire 63-package gap reduces to **just the 12 roots** (fix them and the 51 fall out). **`runtime` now compiles** (DLL emitted), along with `reflect`, `os`, `net`, `fmt` — the historical "runtime = singular ~237-package gate" is **CLEARED**.

**No true design walls remain (adversarial pro/skeptic panel, 2026-07-05).** The two candidates were both refuted as walls:
- **`go/ast` generic-over-interface is NOT an adapter-model wall — it is a converter constraint-emission BUG.** `main.go:3107` (getGenericDefinition) stamps a phantom CRTP self-type `Node<N>` + `, new()` for a method-bearing interface constraint, but `Node` is arity-0 (`ast.cs:32`) so `Node<N>` doesn't exist → CS0308. Fix in the converter: stop emitting the CRTP+`new()` form; lower `walkList`-shaped bodies to a plain interface-typed loop (`slice<Node>`), reusing the box→adapter widening that ALREADY fires at scalar call sites (`walk.cs` emits `new IdentжNode(...)`). No `ж<T>` fork, no generator change. Effort = medium (constraint-emission change is corpus-wide → check-no-regression + re-measure gated). **Highest single lever: unblocks the entire `go/*` toolchain (~11 pkgs) with no co-fix.**
- **`encoding/json` generic-over-union is NOT a wall — a one-member golib gap.** The `[]byte|string` union is already modeled by hand-written `core/golib/IByteSeq.cs`; only the spread `.ꓸꓸꓸ` is missing from the interface (both `@string` and `slice<T>` already implement it concretely). Add `Span<T> ꓸꓸꓸ { get; }` to `IByteSeq<T>` — additive, plausibly clears all 8 at once. Effort = small.
- **`runtime/pprof` is NOT runtime-gated anymore** (runtime compiles); its CS0411 is a genuine tractable generic-method-group-arg fix (stamp explicit type args, cf. `66be4f914`).

**Reachability verdict: full stdlib compile IS reachable.** Ceiling = **302/302 (100%) with exactly one hand-stubbed package** (`crypto/internal/nistec` p256 asm, via the ratified `[module: GoManualConversion]` rule), or **~301/302** if pure-auto-conversion with zero manual stubs is required. No architectural fork needed. Leverage-ranked cheapest-first order: `cryptobyte` (trivial, opens the crypto cluster) → `go/ast` (11 solo) → `sha3`+`bidirule`+`nistec`-stub (completes the net/http tower) → `encoding/json` → small converter tail (`template/parse`, `testing`, `internal/trace`, `pprof`) → `database/sql`+`gosym` (leaves).

### Superseded census5 headline

**census5 (2026-07-05, HEAD `40f4fac2a`): 12 packages fail with own-errors; 291/303 (96%) compile.**
(census4 at HEAD `443ec9a91` had 13 — `debug/buildinfo` has since been cleared by the promoted-adapter
generator fix, `175cba3d0`. Note census4's buildinfo attribution was partly a stale-incremental artifact,
which is why census5 is the authoritative count. All other 12 packages are unchanged from census4.)

The remaining 12 split into **deep-but-real converter roots** — `internal/trace` (75, anonymous-interface
cross-file lift), `database/sql` (17, five mixed roots), `vendor/cryptobyte` (6, non-canonical import alias
in type signatures), `testing` (3, lifted-anonymous-struct-alias publicize) — one **generator** root
(`debug/gosym`, 1), and the **true floor** (`encoding/json` generic-over-union 8, `crypto/internal/nistec`
asm 3, `go/ast` generic-over-interface wall 1, `runtime/pprof` runtime-gated 1, vendored `sha3` 2 +
`bidirule` 1, `text/template/parse` 1). The clean and medium-clean single-root phase is **exhausted**; each
remaining converter root is a substantial multi-step change (a lift pre-pass, a hot-path `getTypeName`
alias map, a lifted-type-publicize mechanism, or a five-way triage), and the floor is genuinely
deep-design / assembly / runtime-gated / vendored.

### Original census4 headline (superseded)

**census4 (2026-07-05, HEAD `443ec9a91`): 13 packages fail with own-errors; ~290/303 compile.**

The compile milestone is **substantially met**: the overwhelming majority of the standard library
transpiles to C# that compiles. What remains is a small, well-characterized tail — one dominant real
converter root, a couple of generator fixes, and a residue of deep-design / runtime-gated / assembly /
vendored cases that are the *natural floor* of a source-to-source compile without the Go runtime.

Note the incremental solution build reports **own-errors of leaf-most failures**; dependents of a
failing package are *skipped* (not counted). So each fix below can unmask latent defects in its
dependents (progress, not regression) — exactly what happened this session when the `oldtrace` fix
un-skipped its parent `internal/trace` and surfaced its 75 own-errors.

## The 13 failing packages, categorized

### (a) Real converter fix still available — highest value

| Pkg | Errs | Root |
|---|---|---|
| **internal/trace** | 75 | **Anonymous interface as a `GoImplement`/adapter target** — a concrete type asserted to an inline `interface{io.Reader; io.ByteReader}` is emitted as the **raw Go literal** into `package_info.cs` (`[assembly: GoImplement<bufio_package.Reader, interface{io.Reader; io.ByteReader}>]`) — invalid C# whose `}` breaks the parse and cascades CS1730 across every following assembly attribute — and into the adapter class name (`bufio_ReaderжByteReader}`, a stray `}`). The inline interface must be **lifted to a named type** (as other anonymous interfaces already are) and that name used in the attribute + adapter. This ONE root drives most of the 75 syntax errors (CS1730/CS1022/CS1519/CS1031/CS8124…). **HIGH value — the top remaining real root.** (A few residual body-level emission errors in `gc.cs`/`generation.cs` may remain after.) |
| **database/sql** | 17 | Mixed medium: 7×CS1929 (extension-method receiver mismatch), 6×CS1503 (arg conversion), 2×CS0029, 2×CS8175 (tuple element name). Some are real converter fixes; needs per-error triage. The earlier CS0246 (cross-package `using driver`) was fixed this session (`091762bf9`). |
| **testing** | 3 | 2×CS0051 + 1×CS0050 — the **transitive-publicize** follow-up: a publicized unexported interface's METHOD-signature types (`testDeps.CoordinateFuzzing(… corpusEntry …)`) are not cascaded because `collectPublicizedTypes` walks method *declarations*, not interface method *signatures*. Extend the fixpoint. (The direct `testDeps` publicize was fixed this session, `849395ca9`.) |

### (b) Generator (go2cs-gen) fix

| Pkg | Errs | Root |
|---|---|---|
| **debug/buildinfo** | 1 | CS1501 — `ImplementGenerator`'s foreign-struct arm registers a package-static forward for a **promoted** interface method (`io.ReaderAt` embedded in `xcoff.Section`), emitting a 3-arg `ReadAt` call to a non-existent static. Gate `forwardStaticCalls` on a real static method existing; else let the embedded-interface-field arm bind. |
| **debug/gosym** | 1 | CS0542 — a generated promoted accessor whose member name equals its enclosing type name (`Func`). Rename with `Δ` in the TypeGenerator template. HEAVY gate (full behavioral + corpus). |

### (c) Deep / design-level

| Pkg | Errs | Root |
|---|---|---|
| **encoding/json** | 8 | Byte-sequence generic spread — `appendString[Bytes []byte\|string]` generic over an `IByteSeq` union, `src[i:j]...` spread of a generic type param. Generic-over-union modeling. |
| **vendor/…/cryptobyte** | 6 | **Non-canonical import alias in type signatures** — `getTypeName` renders a foreign type via its *canonical* alias (`asn1.ObjectIdentifier`) instead of the file's actual import alias (`encoding_asn1.ObjectIdentifier`); `types.Type` doesn't carry the source alias, so a per-file path→alias map must be threaded into `getTypeName` (hot path). General (affects any non-canonical alias). The dup-`using` half was fixed this session (`443ec9a91`). |
| **go/ast** | 1 | CS0308 — the **generic-over-interface wall**: `func walkList[N Node](list []N)` where `N` instantiates to boxed `ж<Ident>` etc., but `ж<X>` does not directly implement the interface (the adapter does), so `where N : Node` cannot be satisfied. A fundamental adapter-model limitation (drop the constraint + route through the interface, or make `ж<X>` implement `X`'s interfaces). |
| **text/template/parse** | 1 | CS7036 — `newRange` tuple-argument expansion. |

### (d) Runtime-gated — not independently fixable

| Pkg | Errs | Root |
|---|---|---|
| **runtime/pprof** | 1 | CS0411 (generic method-group arg `slices.Compare`) — but pprof imports `runtime`, the **singular ~237-package gate**: a full green build is blocked on the runtime conversion regardless. The CS0411 is a genuine own-error worth fixing (it recurs elsewhere), but the package will not compile until runtime does. |

### (e) Assembly — GoManualConversion-stub territory

| Pkg | Errs | Root |
|---|---|---|
| **crypto/internal/nistec** | 3 | CS8130/CS1579 — `p256_asm.go` deconstruction/foreach over asm-backed types. Genuine raw-metal (`*.s` companions); a `[module: GoManualConversion]` stub that compiles is the acceptable milestone solution. |

### (f) Vendored, low priority

| Pkg | Errs | Root |
|---|---|---|
| **vendor/…/sha3** | 2 | CS1929 extension-receiver (vendored x/crypto). |
| **vendor/…/bidirule** | 1 | CS0234 namespace (vendored x/text). |

## Recommendation (census5, 291/303)

The stdlib-compile milestone is **substantially met** — **291/303 packages (96%) compile**. `buildinfo`
was cleared after census4 (generator fix `175cba3d0`), so it no longer appears below. The remaining work
splits cleanly into *deep-but-real converter roots* and the *true floor* — and, importantly, **the clean
and medium-clean single-root phase is exhausted**: every remaining converter root is now a substantial
multi-step change, not a one-liner.

**Deep-but-real converter roots** (each a genuine, non-trivial converter fix):
1. **internal/trace** (75 errs) — anonymous-interface-as-adapter-target must be *lifted to a named type*
   (cross-file lift pre-pass; the per-file `liftedTypeMap` needs to become package-level). Highest raw
   count but one root.
2. **database/sql** (17) — a five-way mix (named-delegate→`Action` bridge, `pingDC` CS1929,
   ref-local-in-lambda, GEN `Lock`/`Unlock`, tuple-name CS8175). Needs per-error triage.
3. **cryptobyte** (6) — non-canonical import alias in type signatures: `getTypeName` must honour the
   file's actual import alias, not the canonical one (a per-file path→alias map threaded into a hot path).
4. **testing** (3) — lifted-anonymous-struct-**alias** publicize: `type corpusEntry = struct{…}` is an
   *alias*, so the referenced type is the lifted `corpusEntryᴛ1`, which the named-type publicize cascade
   (extended this session, `40f4fac2a`) does not reach. Needs a `packagePublicizedLiftedTypes` keyed by
   the anon-struct `types.Type`, consulted at lift emission.
5. **gosym** (1) — generator CS0542: a promoted accessor whose member name equals its enclosing type
   name (`Func`); rename with `Δ` in the TypeGenerator template. Small but **heavy gate** (full behavioral
   suite + corpus).

**True floor** — deep-design / runtime-gated / assembly / vendored; appropriate to stub
(`GoManualConversion`) or defer:
- `encoding/json` (8) — generic over a `[]byte|string` union, `src[i:j]...` spread of a type param.
- `go/ast` (1) — the generic-over-interface **wall**: `where N : Node` unsatisfiable because `ж<X>` does
  not itself implement `X`'s interfaces (the adapter does). A fundamental adapter-model limitation.
- `crypto/internal/nistec` (3) — `p256_asm.go` over asm-backed types; genuine raw-metal.
- `runtime/pprof` (1) — a real CS0411, but the package is blocked on the runtime conversion regardless
  (the singular ~237-package gate).
- vendored `sha3` (2) + `bidirule` (1), `text/template/parse` (1) — low priority.

## Campaign delta

- Session start (census2 era): 19 packages / 87 errors, with many masked behind dominant roots.
- End of session (census5, HEAD `40f4fac2a`): **12 packages / 119 own-errors; 291/303 compile.**
- This session cleared (to zero or near-zero) `net/mail`, `crypto/md5`, `crypto/x509/pkix`,
  `internal/dag`, `testing/quick`, `crypto/rsa`, `go/constant`, `bidi`, `hpack`, `slog/buffer`,
  `oldtrace`, `buildinfo`, and landed roots for `database/sql` (CS0246), the defer-box, the cryptobyte
  dup-alias, the interface-publicize family (direct + method-signature cascade), and the variadic-closure
  prologue — ~16 guarded, regression-clean converter/generator fixes.
- The frontier is now the **hard tail**: 4 deep-but-real converter roots, 1 heavy-gate generator root,
  and a ~7-package true floor. Recommend a deliberate, user-steered pass on the deep roots rather than
  continued 60s-cadence autonomous churn.
