# CLAUDE.md — go2cs orientation

> Canonical orientation for any Claude/AI task working in this repo. This file is **authoritative**;
> where it disagrees with `docs/README.md` or the `.bat`/`.cmd` build scripts, those are **stale** —
> trust this file and the source. See companion docs: [`docs/Architecture.md`](docs/Architecture.md),
> [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md), [`docs/Roadmap.md`](docs/Roadmap.md).

## What this is

`go2cs` is a **transpiler that converts Go source code into C#** that is both *behaviorally* and
*visually* similar to the original Go — the goal is that a Go developer can read the generated C# and
follow it easily. Go's compiler-provided conveniences (slices, maps, channels, multiple returns,
defer/panic/recover, goroutines, struct embedding, interface duck-typing) are emulated either by a
hand-written runtime library or by Roslyn source generators, so the visible converted code stays close
to the Go original.

This is the **"go2cs2"** generation of the project: the converter is now **written in Go** using the
official `go/ast` + `go/types` toolchain. The earlier converter (C# + ANTLR4 grammar) is retired — but
the README and some build scripts still reference it (see *Known staleness* below).

## Architecture map

| Component | Location | Language | Role |
|---|---|---|---|
| **Converter** | `src/go2cs/*.go` (~67 files) | Go | Parses Go with `go/ast`/`go/types`, emits C#. |
| **Runtime library (`golib`)** | `src/core/golib/` | C# | Hand-written Go semantics: `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `array<T>`, `builtin` (`append`/`len`/`make`/`panic`/`recover`…), `ж<T>` heap box, `nil`, type aliases. **Shared by everything; never auto-overwritten.** |
| **Source generators** | `src/gen/go2cs-gen/` | C# (Roslyn) | Compile-time Go semantics: `ImplementGenerator` (interface impl), `RecvGenerator` (pointer-receiver overloads), `ImplicitConvGenerator` (type-alias conversions), `TypeGenerator` (struct embedding/promotion). Referenced as an **analyzer** by every converted project. |
| **Baseline stdlib** | `src/core/<pkg>` | C# (converted) | Curated, **compiling** subset of the Go stdlib that the test loop builds against. |
| **Full auto-conversion (WIP)** | `src/go-src-converted/` *(target; see separation doc)* | C# (converted) | Whole Go stdlib (~305 packages) auto-output. Does **not** all compile yet. |
| **Behavioral tests** | `src/Tests/Behavioral/` (59 projects) | Go + C# | Per-feature Go↔C# equivalence (arrays, channels, defer, generics, interfaces…). |
| **Examples** | `src/Examples/` | Go + C# | Hand-converted Tour-of-Go / go101 / misc samples. |

Converter internals (full taxonomy in [`docs/Architecture.md`](docs/Architecture.md)):
- Entry: `src/go2cs/main.go`. Stdlib driver: `src/go2cs/stdLibConverter.go` (builds the package
  dependency graph + topological `sortedQueue`).
- `visit*.go` — walk AST nodes → C# declarations/statements (e.g. `visitFuncDecl.go`, `visitRangeStmt.go`,
  `visitDeferStmt.go`, `visitSelectStmt.go`).
- `conv*.go` — convert expressions/types (e.g. `convCallExpr.go`, `convSliceExpr.go`, `convStarExpr.go`).
- Analysis passes: `escapeAnalysisOperations.go`, `variableAnalysisOperations.go` (shadowing),
  `nameCollisionAnalysisOperations.go`, `constraintOperations.go` (generics), `importOperations.go`.

## Two libraries, one runtime (read this before touching `src/core`)

There are **two** notions of "the Go standard library" plus **one** shared runtime:

1. **Baseline** (`src/core/<pkg>`) — small, hand-finished, **compiles**. The converter-improvement /
   behavioral-test loop builds against this.
2. **Full conversion** (`src/go-src-converted/`) — the entire Go stdlib auto-converted; the *ultimate*
   goal; **work in progress, does not all compile**.
3. **`golib`** (`src/core/golib/`) — hand-written runtime, **shared by both**, the never-overwritten core.

Baseline and full **cannot both be referenced by one C# project**: both emit into `namespace go` with
`<pkg>_package` static partial classes, so they collide. Keep them physically and referentially separate.

**History of the collision (why the loop is currently stalled):** the baseline lived at `src/gocore/`
(2020–2025), was renamed to `src/core/` on 2025-03-08 (`ba6fef6c9`), then **overwritten in place** by the
first full-stdlib conversion on 2025-05-05 (`6ca1c45b7`, "Initial standard library conversion",
2,359 files / +508k lines). The last clean baseline state is **`3426298eb`** (8 min before the overwrite).
"Conversion succeeded" there meant *the transpiler didn't crash* — **not** that the C# compiles. Full
details and the recovery contract: [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md).

## Build / test workflow

- **Converter (Go):** built with the Go toolchain from `src/go2cs/`. Usage:
  `go2cs [options] <input_dir> [output_dir]`. Key flags (from `main.go`, authoritative):
  - `-stdlib` — convert the Go stdlib. `-stdlib fmt strings io` — convert only those packages (+filter).
  - `-go2cspath <dir>` — output root for converted code (default `~/go2cs`; env `GO2CSPATH`).
  - `-goroot` / `-gopath`, `-parallel 1..4`, `-platforms os/arch`, `-indent 4`, `-var` (default on),
    `-uco` (channel operators, default on), `-comments`, `-cgo`, `-tree`, `-csproj <tmpl>`, `-debug`.
  - Single project/file: `go2cs package_dir` or `go2cs example.go [out.cs]`.
- **Converted C# projects:** standard `dotnet build` (target **net9.0**, C# latest). Each converted
  `.csproj` references `golib`, the `go2cs-gen` analyzer, and the stdlib packages it imports. The
  `$(go2csPath)` MSBuild property resolves to `$(SolutionDir)` in Debug builds (so refs point at
  `src/core/...`); it is **distinct** from the converter's `-go2cspath` output flag.
- **Behavioral tests** (`src/Tests/Behavioral/`): convert a test's `.go` → `.cs`, build both, compare
  program output. All 59 reference `golib` + the `go2cs-gen` analyzer; most also reference `core/fmt`
  (and a few reference `time`, `unsafe`, `strings`, `sort`, `math/rand`, `io`).

## Current state & known issues

- The behavioral-test loop is **stalled** because the May 2025 full conversion overwrote the compiling
  baseline `src/core/<pkg>` with large machine-generated versions that don't all compile.
- **Active plan:** (A) write these docs, (B) restore baseline↔full separation
  (`src/core` = curated/compiling; `src/go-src-converted` = full WIP; `golib` shared),
  (C) re-green the loop bottom-up by dependency closure, then drive the full conversion's compile-error
  count down. See [`docs/Roadmap.md`](docs/Roadmap.md).
- Open converter items: `src/go2cs/ToDo.md` (e.g. `visitMapType` completion, remaining dynamic-struct
  implicit-cast checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).

### Known staleness (do not trust blindly)
- `docs/README.md` — describes the **old** ANTLR4/C# converter and the retired `src/gocore` +
  `src/go-src-converted` layout.
- `src/deploy-core.bat` — still XCOPYs `gocore\*.*` (should be `core`).
- `src/convert-gosrc.cmd` — invokes a `net6.0` C# `go2cs.exe` with old flags (`-s -r -e -g`); the current
  converter is the Go build with the flags listed above.

## Conventions

- C# style: see [`docs/coding-style.md`](docs/coding-style.md) (Allman braces, 4 spaces, `m_`/`s_`/`t_`
  field prefixes, explicit types over `var`, language keywords over BCL types, `\uXXXX` for non-ASCII).
- Conversion strategy reference: [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md)
  (how each Go construct maps to C#).
- Generated C# intentionally targets Go-like *behavior first* (no implicit async), and Go-like *appearance*
  second (extra machinery hidden in partial classes / generated files).

## Git anchors

| Commit | Date | Meaning |
|---|---|---|
| `9792eeea2` | 2020-07-09 | Original hand-converted stub created (`src/gocore`). |
| `ba6fef6c9` | 2025-03-08 | Renamed `src/gocore` → `src/core`. |
| `3426298eb` | 2025-05-05 01:51 | **Last clean baseline** (use as reference/fallback). |
| `6ca1c45b7` | 2025-05-05 01:59 | First full stdlib conversion — overwrote the baseline. |
| `cc14584c7` | 2025-05-11 | Current `master` tip of the full-conversion work. |
