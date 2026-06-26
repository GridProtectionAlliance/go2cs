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
official `go/ast` + `go/types` toolchain. The earlier converter (C# + ANTLR4 grammar) is retired — a few
build scripts still reference it (see *Known staleness* below).

## Architecture map

| Component | Location | Language | Role |
|---|---|---|---|
| **Converter** | `src/go2cs/*.go` (~67 files) | Go | Parses Go with `go/ast`/`go/types`, emits C#. |
| **Runtime library (`golib`)** | `src/core/golib/` | C# | Hand-written Go semantics: `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `array<T>`, `builtin` (`append`/`len`/`make`/`panic`/`recover`…), `ж<T>` heap box, `nil`, type aliases. **Shared by everything; never auto-overwritten.** |
| **Source generators** | `src/gen/go2cs-gen/` | C# (Roslyn) | Compile-time Go semantics: `ImplementGenerator` (interface impl), `RecvGenerator` (pointer-receiver overloads), `ImplicitConvGenerator` (type-alias conversions), `TypeGenerator` (struct embedding/promotion). Referenced as an **analyzer** by every converted project. |
| **Baseline stdlib** | `src/core/<pkg>` | C# (converted) | Small hand-finished stdlib subset (errors, fmt, io, math, math/rand, sort, strings, sync, time, unsafe, a few internal/*) — **compiles**; the test loop builds against this. Restored from the old stub (`3426298eb`). |
| **Full auto-conversion (WIP)** | `src/go-src-converted/` | C# (converted) | Whole Go stdlib (~301 projects) auto-output. Does **not** all compile yet. Its own `src/go-src-converted.sln`. |
| **Behavioral tests** | `src/Tests/Behavioral/` (59 test projects + `BehavioralTests` runner) | Go + C# | Per-feature Go↔C# equivalence (arrays, channels, defer, generics, interfaces…). |
| **Examples** | `src/Examples/` | Go + C# | Hand-converted Tour-of-Go / go101 / misc samples. |

**Two solutions:** `src/go2cs.sln` = converter-dev workspace (golib + `go2cs-gen` + the baseline subset +
all tests/examples/utilities) — **builds green**. `src/go-src-converted.sln` = the WIP full conversion
(301 projects), kept separate so it doesn't break the green solution.

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

**How this was resolved (2026-06-25):** the baseline had lived at `src/gocore/` (2020–2025), was renamed
to `src/core/` on 2025-03-08 (`ba6fef6c9`), then **overwritten in place** by the first full-stdlib
conversion on 2025-05-05 (`6ca1c45b7`, +508k lines) — which is what stalled the loop ("conversion
succeeded" there meant the transpiler didn't crash, not that the C# compiles). The fix: relocate that full
conversion to `src/go-src-converted/`, and **restore the old hand-finished stub from `3426298eb` into
`src/core`** — it still compiles cleanly against today's `golib`, giving a green baseline immediately.
Full details: [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md).

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
- **Behavioral tests** (`src/Tests/Behavioral/`): each test references `golib` + the `go2cs-gen` analyzer;
  most also reference `core/fmt` (a few reference `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`).
  The `BehavioralTests` MSTest runner has these phases: `TranspileTests`, `CompileTests`,
  `OutputComparisonTests` (runs Go vs C#, compares stdout), `TargetComparisonTests` (byte-compares the
  transpiled `.cs` against a `.cs.target` golden).

### Test-harness mechanics (important when changing the converter)
- **`dotnet build` does NOT run the converter** — it only compiles committed C#. A clean build leaves the
  tree clean. **Running the tests re-runs the converter:** `BehavioralTestBase` rebuilds `go2cs.exe` via
  `go build` whenever any converter `*.go` is newer than the binary, then re-transpiles. So after a
  converter change, running the suite regenerates the behavioral `.cs` from current source (and may show
  them as modified in git — that's expected).
- **`TargetComparisonTests` is byte-for-byte.** To re-baseline goldens after an *intended* output change,
  run the **`UpdateTestTargets`** project with **`--createTargetFiles`** (re-runs the converter, rewrites
  all `.cs.target`) — don't hand-edit goldens.
- **autocrlf gotcha:** `core.autocrlf=true`. The converter preserves the Go source's LF inside multi-line
  string literals, so those `.cs`/`.cs.target` are mixed CRLF/LF and must be marked `-text` in
  `.gitattributes` (done for Solitaire, SortArrayType, StdLibInternalAbi) or autocrlf re-breaks the byte
  comparison on a fresh checkout. New tests with multi-line string literals need the same.
- **testhost lock gotcha:** a stray `testhost`/`vstest.console` from a prior run can lock
  `BehavioralTests.dll` → next build fails with `MSB3027` ("file locked by testhost"). Kill it (and
  `dotnet build-server shutdown` frees bin/obj locks) before rebuilding — not a real compile error.

## Current state & known issues

- **Baseline is green:** `src/go2cs.sln` builds 79/79 and the behavioral suite passes (216 tests). The
  separation is done — `src/core` (stub baseline + `golib`) vs `src/go-src-converted` (full WIP). The
  converter-improvement loop is restored.
- **Next (Phase 3):** drive `src/go-src-converted` (the full ~301-package conversion) toward compiling,
  bottom-up by converter-defect frequency; promote packages into the baseline as they go green. See
  [`docs/Roadmap.md`](docs/Roadmap.md).
- Open converter items: `src/go2cs/ToDo.md` (e.g. `visitMapType` completion, remaining dynamic-struct
  implicit-cast checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).

### Known staleness (do not trust blindly)
- `src/convert-gosrc.cmd` / `convert-gosrc.bat` — still invoke a retired `net6.0` C# `go2cs.exe` with old
  flags (`-s -r -e -g`); the current converter is the Go build with the flags listed above. (Not yet fixed.)
- `docs/README.md` is partly historical (ANTLR4-era prose) but now carries a banner and corrected
  references; `src/deploy-core.bat` was fixed (`gocore`→`core`).

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
| `3426298eb` | 2025-05-05 01:51 | Last clean stub baseline — **restored into `src/core`** on 2026-06-25. |
| `6ca1c45b7` | 2025-05-05 01:59 | First full stdlib conversion — overwrote the baseline. |
| `cc14584c7` | 2025-05-11 | Full-conversion work; tagged `full-conversion-2025-05`. |
| (2026-06-25) | 2026-06-25 | Separation + stub-baseline restore + converter fixes → green baseline. |
