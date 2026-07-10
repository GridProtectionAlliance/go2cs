# CLAUDE.md — go2cs orientation

> Canonical orientation for any Claude/AI task working in this repo. This file is **authoritative**;
> where it disagrees with `docs/README.md` or the `.bat`/`.cmd` build scripts, those are considered **stale** —
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
| **Baseline stdlib** | `src/core/<pkg>` | C# (converted) | Small hand-finished stdlib subset (errors, fmt, io, math, math/rand, sort, strings, sync, **sync/atomic**, time, unsafe, a few internal/*) — **compiles**; the test loop builds against this. Restored from the old stub (`3426298eb`); `sync/atomic` promoted from the full conversion (2026-06-26, first migrated package). |
| **Full auto-conversion (WIP)** | `src/go-src-converted/` | C# (converted) | Whole Go stdlib (~301 projects) auto-output. Does **not** all compile yet. Its own `src/go-src-converted.sln`. |
| **Behavioral tests** | `src/Tests/Behavioral/` (59 test projects + `BehavioralTests` runner) | Go + C# | Per-feature Go↔C# equivalence (arrays, channels, defer, generics, interfaces…). |
| **Performance tests** | `src/Tests/Performance/` (8 `Perf*` benchmarks + `PerformanceRunner`) | Go + C# | Go vs transpiled C# (JIT **and** Native AOT) time/memory comparison — results table in its `README.md`. |
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
  - **Always pass `-comments` when converting the Go stdlib.** It defaults **off**, but the converted C#
    is a derivative work: the per-file `// Copyright … The Go Authors … BSD-style license` header **must be
    preserved** (license requirement), and the Go doc-comments are what make the output readable. Without
    it the header and all comments are stripped. (Behavioral-test goldens were captured *without* comments,
    so don't flip the default — pass the flag on stdlib `-stdlib` runs.)
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
- **`TargetComparisonTests` compares goldens with line endings NORMALIZED** (CRLF→LF; see
  `TargetComparisonTests.FileMatch` / `BehavioralRunner.FilesEqual`, both strip CRs). It was a raw
  byte-for-byte compare until 2026-07-07. Content diffs are still caught exactly; a pure line-ending
  difference is ignored (it can only come from autocrlf, never from the deterministic converter). To
  re-baseline goldens after an *intended* output change, run the **`UpdateTestTargets`** project with
  **`--createTargetFiles`** (re-runs the converter, rewrites all `.cs.target`) — don't hand-edit goldens.
- **autocrlf gotcha (`core.autocrlf=true`) — two SEPARATE concerns:** the converter emits CRLF for C# line
  endings but preserves the Go source's LF inside multi-line string literals, so those `.cs`/`.cs.target`
  contain mixed CRLF/LF, and autocrlf rewrites the in-string LFs to CRLF on checkout.
  (1) **Golden text comparison** — no longer an issue: the comparison is line-ending-insensitive (above),
  so a smudged golden still matches and **no `-text` mark is needed just for the byte compare**.
  (2) **Runtime correctness** — still needs `-text`: if a project's *compiled program* embeds and observes
  a multi-line string literal at runtime (e.g. `Solitaire`'s board, printed via `println`), autocrlf smudges
  that literal's newlines to CRLF in the on-disk `.cs`, and any build that compiles the committed `.cs`
  *without* re-transpiling (VS, CI `dotnet build`, or the runner's up-to-date-skip) bakes the wrong `\r`
  runes into the value → the program misbehaves (Solitaire's board geometry breaks and the solver hangs).
  So `Solitaire`/`SortArrayType`/`StdLibInternalAbi` keep their `.cs` `-text` marks. A NEW multi-line-string
  test only needs `-text` if its program's *behavior/output* depends on the literal's exact bytes; if the
  literal is inert (never printed/measured), no mark is needed and the golden compare stays green regardless.
- **testhost lock gotcha:** a stray `testhost`/`vstest.console` from a prior run can lock
  `BehavioralTests.dll` → next build fails with `MSB3027` ("file locked by testhost"). Kill it (and
  `dotnet build-server shutdown` frees bin/obj locks) before rebuilding — not a real compile error.
  **Root cause + mitigation (2026-06-30):** the MSTest `Exec()` used an unbounded `WaitForExit()`, so a
  hung child (a deadlocked transpiled program, or a build blocked on a lock) hung the suite forever and
  orphaned testhost. `Exec` now has a per-call timeout (180s build/transpile, 30s run) that kills the
  whole child **process tree**, and disables MSBuild node reuse (`MSBUILDDISABLENODEREUSE=1`) so in-test
  builds don't leave lock-holding worker nodes; `AssemblySetup.[AssemblyCleanup]` runs
  `dotnet build-server shutdown`. Prefer **`src/Tests/Behavioral/run-behavioral-tests.ps1`** (clears stale
  hosts *before* the build — the lock manifests at build time — and runs with `--blame-hang`) over a bare
  `dotnet test`.
- **Faster alternative to MSTest — the standalone runner `src/Tests/Behavioral/BehavioralRunner`
  (2026-06-30).** A dependency-free console app that runs the same four phases over all **180** behavioral
  projects (the "59" figures elsewhere in this doc are stale) but is **not** hosted in testhost, so the
  self-lock failure mode above is structurally absent. It collapses the 180 per-project `dotnet build`
  calls into one parallel MSBuild invocation (pre-building the ~10 shared `golib`/analyzer/`core/*` deps
  sequentially first to avoid the parallel-build MSB3026/27 race, then fanning out). **Full cold run ≈2 min
  / warm ≈80s, all 180 green** — at parity with MSTest. Drive it via **`run-behavioral.ps1 [--filter X]
  [--phase transpile,compile,target,output] [--update-targets] [--list]`**. Only output-compared
  (`[GoTestMatchingConsoleOutput]`) projects are `go build`- and stdout-compared, matching MSTest
  (library-style projects like `Constraints` have no `package main`). For a pure converter no-regression
  check with no compile/run at all, use **`check-no-regression.ps1`** (re-transpiles every behavioral dir
  and `git status`es the `.cs`).
- **Run the behavioral suite via the solution, not the project:** `dotnet test src/go2cs.slnx`. Running
  `dotnet test` on `BehavioralTests.csproj` directly breaks because `$(go2csPath)` (→ `$(SolutionDir)`)
  has no solution context, so the `core\golib` ref fails to resolve. The baseline solution is now an
  **`.slnx`** (`src/go2cs.slnx`); `src/go-src-converted.sln` is still classic `.sln`.
- **When iterating on regression work, use FILTERED + `--no-build` tests — don't run the full suite each
  time.** The full `dotnet test go2cs.slnx` rebuilds all ~81 projects first and can take 10+ min or hang
  under Visual Studio lock contention. Instead, from `src/Tests/Behavioral/BehavioralTests`, run
  `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` — that reuses the existing test
  assembly and runs just that project's 4 phases (Transpile/Compile/TargetComparison/OutputComparison) in
  seconds. `--no-build` is valid as long as the `*Tests.cs` files haven't changed (`git status` them).
  Reserve a single full-suite run for final confirmation. Faster still for a pure no-regression check:
  re-transpile every behavioral dir and `git status` the `.cs` — byte-identical generated code ⟹ identical
  compile+output ⟹ identical results, with no compile/run at all.
- **Cap build/test commands at ~3 min (180s).** A clean full suite (`dotnet test --no-build`) runs in
  **~2.3 min / 228 green**; materially longer means the test host has hung under lock contention, not real
  work — stop and clear it rather than waiting 10–20 min. (Raise the ceiling later only if the suite
  legitimately grows.)

### Performance comparison suite (`src/Tests/Performance`, 2026-07-02)
- **Purpose:** answer "how fast is the transpiled C# vs the original Go?" — 8 small `Perf*` benchmark
  projects (Startup, Fib, Sieve, MatMul, String, Map, Sort, Channel), each a behavioral-test-shaped folder,
  measured across **three variants**: Go binary, C# JIT (`Release`), C# **Native AOT** self-contained.
  Drive via **`run-performance.ps1 [--filter X] [--no-aot] [--runs N] [--update-readme]`** (standalone
  `PerformanceRunner`, no testhost; phases Transpile → Build → Verify → Measure; Verify requires identical
  timing-filtered stdout across all three binaries before anything is timed). The results table lives in
  `src/Tests/Performance/README.md` between `PERF-RESULTS` markers (`--update-readme` rewrites it; prior
  toolchain tables accumulate in its *History* section for .NET 9 → 10 comparisons).
- **Mechanics gotchas:** benchmarks self-time via `time.Now().UnixNano()` (added to the baseline
  `core/time` stub for this) and print `elapsed_ns:` lines the runner strips before output comparison; the
  converter **regenerates each benchmark csproj on transpile**, so shared settings live in
  `Directory.Build.props`/`.targets` there (AOT is gated by custom `-p:PerfAot=true` — passing `PublishAot`
  globally breaks the netstandard2.0 `go2cs-gen` analyzer with NETSDK1207); AOT publish needs MSVC
  `link.exe` and the runner prepends the VS Installer dir to PATH for the SDK's `vswhere` probe; AOT trims
  with `TrimMode=partial` because golib `fmt` formatting and sort's `Interface<T>` bind members via
  reflection. Full run ≈3–4 min warm (AOT publishes ≈7 s each); `--no-aot` well under a minute. Keep each
  benchmark ≥50 ms and output deterministic (inline xorshift, no `math/rand`).

### Adding a regression test when a converter defect is fixed
When a meaningful converter bug is fixed, lock it in with a behavioral test so later changes can't silently
reintroduce it. **Prefer extending an existing behavioral project** if one already covers a similar
construct; otherwise add a new one (example: `Tests/Behavioral/GlobalStructFieldPointers`, which guards the
`&cpu.X86.HasADX` cross-file address-of-field fix). To add one:
1. **New folder** `src/Tests/Behavioral/<Name>/` with a Go program that *exercises the specific construct*
   (multiple `.go` files are fine and run as one package — needed to reproduce cross-file bugs). Include a
   `go.mod` (`module go2cs/<Name>`), and copy `go2cs.ico` + a `<Name>.csproj` from a sibling test (adjust
   `AssemblyName`; keep the `golib`/`fmt` refs the program needs). Verify it with `go run .` first.
2. **Make the Go↔C# output match** so `OutputComparisonTests` passes. Mind known runtime limitations — e.g.
   `Ꮡ(value)` (address of a non-boxed value) currently boxes a *copy*, so don't write through a
   `&global.field` pointer and then read the *original* global; read back through the same pointer.
3. **Register in the solution** — add a `<Project Path="Tests/Behavioral/<Name>/<Name>.csproj" />` line under
   the `/tests/behavioral/target-projects/` folder in `src/go2cs.slnx` (alphabetical). **Then verify it
   stuck:** `grep "<Name>/<Name>.csproj" src/go2cs.slnx` and `dotnet restore src/go2cs.slnx`. If Visual Studio
   has the `.slnx` open it can rewrite/reformat the file and silently drop an external edit — re-add and
   re-verify if so. Note the tests pass even when the project is *missing* from the solution (the harness
   builds each `.csproj` by path, not via the solution), so this check is the only thing that catches it.
4. **Transpile once** (`go2cs.exe src/Tests/Behavioral/<Name>`, no `-comments` — behavioral goldens omit
   them) to generate the `.cs` + `package_info.cs`. For output comparison, add `[GoTestMatchingConsoleOutput]`
   to the generated `package_info.cs` class (a hand-added attribute the converter preserves).
5. **Generate tests + goldens:** run the **`UpdateTestTargets`** utility **with `--createTargetFiles`** (from
   its `bin/Debug/net9.0`). It scans every `Tests/Behavioral/*` folder, rewrites the `// <TestMethods>`
   blocks in all four `*Tests.cs` classes (adding `Check<Name>()`), and copies each transpiled `.cs` to a
   `.cs.target` golden. It only emits an `OutputComparison` test for projects whose `package_info.cs` has
   `[GoTestMatchingConsoleOutput]`. Afterward, `git status` should show only your new project + four
   `+3`-line test-class diffs (no other `.target` churn).
6. **Verify (filtered, fast):** preferred — from `src/Tests/Behavioral`, run
   `./run-behavioral.ps1 --filter <Name>` → the 4 phases (Transpile, Compile, TargetComparison,
   OutputComparison) for that project via the standalone runner, in seconds, with no testhost/lock risk.
   Equivalent MSTest path (still valid): from `src/Tests/Behavioral/BehavioralTests`, run
   `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"`. Either way, avoid the full
   `dotnet test go2cs.slnx` while iterating — it rebuilds everything and can hang under VS lock contention
   (see the test-harness notes above). The golden comparison is line-ending-insensitive, so a multi-line
   string literal needs **no** `.gitattributes` handling for the byte compare — mark the `.cs` `-text` **only
   if** the compiled program's behavior/output depends on that literal's exact newlines (autocrlf gotcha above).
7. **Record the conversion decision (keep the strategy doc living).** Any time an *important or non-obvious*
   conversion decision is made — a new emitted form, a runtime/generator behavior, a deliberate trade-off,
   or a changed mapping of a Go construct to C# — add or update the matching section in
   [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) **in the same change** so that living
   document keeps matching reality. Verify every C# snippet against the actual `.cs.target` golden (it is the
   authoritative record of emitted forms — e.g. `u8` format strings, `throw panic(...)`, `ж<T>`/`Ꮡ`). Skip
   only for pure bug-fixes that restore an already-documented behavior. (This rule is not limited to the
   regression-test flow — it applies to *any* commit that lands a notable conversion decision.)

### Phase 3 mechanics — measuring/iterating the full conversion (`src/go-src-converted`)
- **The on-disk `go-src-converted` is stale** (last bulk conversion 2025-05-11); it predates current
  converter fixes. To measure the *current* converter you must reconvert — building the committed tree
  measures old output.
- **Reconvert → overlay → build → bucket (the measurement loop):**
  1. `go2cs.exe -stdlib -comments -parallel 4 -go2cspath <tmp>` → output lands in **`<tmp>/core/<pkg>`**
     (the `core` subdir is hardcoded; `-go2cspath` is the *output* root, unrelated to the MSBuild
     `$(go2csPath)`). Full stdlib ≈ 3–4 min (per-file work is sub-second; the cost is `go/packages`
     loading the whole type graph, so **batch** — don't invoke per package).
  2. Overlay the fresh `.cs` onto `src/go-src-converted/<pkg>` (keep the relocated csprojs).
  3. Build single packages with **`dotnet build <pkg>.csproj -c Debug`** — `src/go-src-converted/Directory.Build.props`
     now pins `$(go2csPath)` to the src root, so `core\golib` + the `go2cs-gen` analyzer resolve to live source
     with **no `-p:go2csPath` flag**; or build the whole `go-src-converted.sln`. (If you ever do pass the flag
     explicitly, use forward slashes — `-p:go2csPath=H:/Projects/go2cs/src/` — a trailing `\` escapes the
     closing quote and mangles the path into phantom golib-not-found errors.)
  4. Bucket: `dotnet build … -clp:ErrorsOnly` then group by `error CS####`. Errors shown are *own-errors*
     of leaf-most failures — dependents of a failed project are skipped, not errored.
- **csproj layout/relocation:** the converter emits inter-package refs as `$(go2csPath)core\<pkg>\…` and
  the golib ref as `$(go2csPath)core\golib\…`. The 2026-06-25 relocation rewrote **`core\` → `go-src-converted\`
  for all stdlib refs *except* `core\golib`** (golib stays shared in `src/core/golib`). A fresh wholesale
  reconvert must re-apply that rewrite to the generated csprojs.
- **Metric:** measure **packages-compiling**, not raw error count. Fixing file-inclusion bugs (e.g. the
  filename build-constraint fix) *raises* the error count because newly-included files surface their own
  latent defects — that's progress, not regression.
- **Don't commit `go-src-converted` regens casually.** It's regenerable; the unit of work is the
  **converter fix**. Keep the tree restorable (overlay into a branch or restore with `git checkout HEAD --`
  + remove untracked) so a converter-fix commit isn't buried under thousands of generated-file changes.

## Current state & known issues

- **Baseline is green:** `src/go2cs.slnx` builds clean and the behavioral suite passes. The
  separation is done — `src/core` (stub baseline + `golib`) vs `src/go-src-converted` (full WIP). The
  converter-improvement loop is restored. **`sync/atomic` was promoted into `core` (2026-06-26)** — the first
  full-conversion package migrated to the baseline (scalar types are converter output; `Pointer[T]` is
  hand-rewritten with a managed slot + `Interlocked`, since `unsafe.Pointer`=`nuint` can't hold a managed
  reference across a GC — see [`docs/Roadmap.md`](docs/Roadmap.md)).
- **Phase 3 in progress:** drive `src/go-src-converted` (the full ~301-package conversion) toward a clean
  C# **compile** (the milestone is compiling, NOT operational — operational is Phase 4 / Go tests). See
  [`docs/Roadmap.md`](docs/Roadmap.md) for the iteration log.
  - **⚠ Strategy correction (2026-07-01):** **do NOT promote `go-src-converted → core` on a clean compile.**
    Promotion is deferred to *post-Go-test* (Phase 4) and may not be needed. `core` stays the bootstrap
    **stub** the behavioral tests build against (`sync/atomic` there is fine). The canonical
    `[module: GoManualConversion]` / `*_impl.cs` files are hand-owned in `core` and must be copied **back
    into `go-src-converted`** (overlay.sh does this) — that overlaid tree is the real final state.
  - **⚠ The S1/CS0030 "architectural wall" is a FORK, not a wall (2026-07-01).** For **native-type** pointer/
    unsafe ops (identical memory semantics in both GC languages) there IS a faithful conversion — do it in
    the converter/`golib`. **Managed-referent** cases (`guintptr`/`muintptr`/… hiding a managed pointer in a
    `uintptr`) hold the `ж<T>`/`object` **directly** (like `core/sync/atomic` `atomic.Pointer<T>`), never a
    `nuint` round-trip. Only genuine **raw-metal on non-native types** (memory-layout math, type-descriptor
    walking, `*.asm`) is the dragon → an immediate **`[module: GoManualConversion]` stub / review** (a stub
    that compiles is an acceptable milestone solution). The loop SORTS these; it no longer stops at S1.
    Full detail: [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md) *The corrected
    end-state*.
  - **Iteration 1 (2026-06-25)** landed 4 converter fixes (variadic type-param, `comparable<T>`, asm/cgo
    throwing stubs, filename build-constraint over-exclusion). Later sessions cleared the whole
    shadow/rename/cast defect family (CS0266/CS0841/CS0136/CS8030/CS0161) — `runtime` 952 → ~138. **Next:**
  address-of-a-field-of-an-anonymous-struct-global mangling (~140 errors from one bug).
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
- Process/gate terminology as used in commit messages and reviews (CNR, A/B footprint, census,
  chip, guard, golden, overlay, banked…): [`docs/Glossary.md`](docs/Glossary.md).
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
