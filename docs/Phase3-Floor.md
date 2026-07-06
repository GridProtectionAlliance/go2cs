# Phase 3 Floor Assessment — stdlib compile milestone

> Snapshot after the 2026-07-05 converter-fix campaign (~14 roots this session). Measures the full
> `src/go-src-converted` auto-conversion (**303 projects**) built as one solution
> (`go-src-converted.sln`). "Compiling" = the C# compiles; **operational correctness is Phase 4**, not
> this milestone.

## Headline

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

## Recommendation

The stdlib-compile milestone is **substantially met** (~290/303 compile). The remaining work is:

1. **internal/trace** (75 errs) — the one high-value real converter root (anonymous-interface-as-adapter-target lift). **Do this next.**
2. **database/sql** (17) — triage the CS1929/CS1503 mix for real converter fixes.
3. **buildinfo / gosym** (2) — generator fixes.
4. **testing** (3) — the transitive-publicize fixpoint extension.
5. **non-canonical-alias** (clears cryptobyte's 6) — a hot-path but general converter fix.

The rest — `encoding/json` (generic-over-union), `go/ast` (generic-over-interface wall), `nistec`
(asm), `pprof` (runtime-gated), `sha3`/`bidirule` (vendored) — is the **natural Phase-3-compile
floor**: each is either a deep design item, blocked on the runtime conversion, genuine assembly, or a
low-priority vendored copy. These are appropriate to stub (`GoManualConversion`) or defer.

## Campaign delta

- Session start (census2 era): 19 packages / 87 errors, with many masked behind dominant roots.
- This session cleared (to zero or near-zero) `net/mail`, `crypto/md5`, `crypto/x509/pkix`,
  `internal/dag`, `testing/quick`, `crypto/rsa`, `go/constant`, `bidi`, `hpack`, `slog/buffer`,
  `oldtrace`, and landed roots for `database/sql`, the defer-box, and the cryptobyte dup-alias —
  ~14 guarded, regression-clean converter/generator fixes.
