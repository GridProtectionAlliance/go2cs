# go2cs Roadmap

Companion to [`/CLAUDE.md`](../CLAUDE.md). The path from "loop stalled" to "full Go stdlib converts and
compiles." Sequenced **green the loop first**, then drive the full conversion.

## Phase 0 — Documentation (done / in progress)

Orientation docs so any task starts informed: [`/CLAUDE.md`](../CLAUDE.md),
[`Architecture.md`](Architecture.md), [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md),
this file; plus a refresh of the stale `README.md`.

## Phase 1 — Restore the separation (repo surgery)

Goal: `src/core` = curated/compiling baseline; `src/go-src-converted/` = full WIP; `golib` shared.

1. **Tag the full-conversion work** so it is never lost: `git tag full-conversion-2025-05 cc14584c7`.
2. **Relocate** the auto-generated stdlib package dirs out of `src/core` into `src/go-src-converted/`
   (`git mv`); keep `src/core/golib` in place.
3. **Retarget** all `-stdlib` runs to `-go2cspath …/src/go-src-converted`; update `convert-gosrc.*` and
   fix `deploy-core.bat` (`gocore`→`core`).
4. **Reference worktree** for the old baseline: `git worktree add ../go2cs-stub-ref 3426298eb`.

## Phase 2 — Green the loop (establish the minimal compiling baseline)

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
  `go2cs -stdlib -go2cspath <out> <pkg>` (needs `GOROOT` stdlib source, present) → `dotnet build`.
- **Retarget detail:** the stdlib converter writes to **`<go2cspath>/core/<pkg>`** (hardcoded `core` subdir).
  To regenerate cleanly into `src/go-src-converted` we must either make that subdir name configurable or
  convert to a temp dir and move. Batch several converter fixes, then do one wholesale reconvert + re-measure.

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

| Metric | Source | Target |
|---|---|---|
| Baseline builds clean | `dotnet build src/core` | ✅ green |
| Behavioral tests passing | `src/Tests/Behavioral/*` | 59 / 59 |
| Full packages compiling | `src/go-src-converted/build.log` | trending → 305 |
| Full-conversion error count | `build.log` buckets | trending → 0 |

*(Fill in counts as phases progress.)*

## Reference: open converter items (`src/go2cs/ToDo.md`)

`visitMapType` completion; remaining dynamic-struct implicit-cast checks across `AssignStmt`/`CompositeLit`/
`IndexExpr`/`BinaryExpr`/`UnaryExpr`/`SelectorExpr`/`TypeSwitchStmt`/`ValueSpec`; optional recursive
dependent-package conversion; map/channel `GoType` generator support (`IMap`/`IChannel`); comment
conversion; cgo + Go-assembler (`.s`) targets.
