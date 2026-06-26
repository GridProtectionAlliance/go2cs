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

## Progress tracking

| Metric | Source | Status |
|---|---|---|
| Baseline + tests build clean | `dotnet build src/go2cs.sln` | ✅ 79 / 79 |
| Behavioral suite passing | `BehavioralTests` (MSTest) | ✅ 216 tests |
| Full packages compiling | `src/go-src-converted.sln` | ◻ Phase 3 — trending → 301 |
| Full-conversion error count | build-error buckets | ◻ Phase 3 — trending → 0 |

## Reference: open converter items (`src/go2cs/ToDo.md`)

`visitMapType` completion; remaining dynamic-struct implicit-cast checks across `AssignStmt`/`CompositeLit`/
`IndexExpr`/`BinaryExpr`/`UnaryExpr`/`SelectorExpr`/`TypeSwitchStmt`/`ValueSpec`; optional recursive
dependent-package conversion; map/channel `GoType` generator support (`IMap`/`IChannel`); comment
conversion; cgo + Go-assembler (`.s`) targets.
