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

This is the **"go2cs iteration 2"** generation of the project: the converter is now **written in Go** using the
official `go/ast` + `go/types` toolchain. The earlier converter (C# + ANTLR4 grammar) is fully retired —
the last build scripts that referenced it (`convert-gosrc.*`) were removed 2026-07-11.

> **General working principles** (think before coding, simplicity first, surgical changes, goal-driven
> execution) live in the user-global `~/.claude/CLAUDE.md` so they apply across all projects. This file adds
> the go2cs-specific discipline: root-cause against the real emitted `.cs`/`.cs.target` (the golden is the
> authoritative record), keep the A/B footprint minimal, change *only* the goldens a fix must, and prove no
> corpus drift with `check-no-regression` — **compiling is not correctness** (that is the Phase-3 → Phase-4
> distinction). And **prefer the durable path over the shortcut**: when a task could be solved
> quickly-but-throwaway or correctly-but-harder, take the harder, general fix — a converter change over a
> one-off hand-patch, a real root cause over a workaround, the reproducible-from-repo result over a deploy-only
> hack. go2cs is a long-horizon project; work that advances the long-term vision is worth the extra effort, and
> throwaway code that has to be redone later is a net loss even when it ships faster today (the
> *nothing-throwaway* principle). This does not license speculative machinery — it is still the *minimal*
> solution, just the one that generalizes and lasts rather than the one that merely unblocks today.

## Architecture map

| Component | Location | Language | Role |
|---|---|---|---|
| **Converter** | `src/go2cs/*.go` (~67 files) | Go | Parses Go with `go/ast`/`go/types`, emits C#. |
| **Runtime library (`golib`)** | `src/core/golib/` | C# | Hand-written Go semantics: `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `array<T>`, `builtin` (`append`/`len`/`make`/`panic`/`recover`…), `ж<T>` heap box, `nil`, type aliases. **Shared by everything; never auto-overwritten.** |
| **Source generators** | `src/gen/go2cs-gen/` | C# (Roslyn) | Compile-time Go semantics: `ImplementGenerator` (interface impl), `RecvGenerator` (pointer-receiver overloads), `ImplicitConvGenerator` (type-alias conversions), `TypeGenerator` (struct embedding/promotion). Referenced as an **analyzer** by every converted project. |
| **Baseline stdlib** | `src/core/<pkg>` | C# (converted) | Small hand-finished stdlib subset (errors, fmt, io, math, math/rand, sort, strings, sync, **sync/atomic**, time, unsafe, a few internal/*) — **compiles**; the test loop builds against this. Restored from the old stub (`3426298eb`); `sync/atomic` promoted from the full conversion (2026-06-26, first migrated package). |
| **Full auto-conversion** | `src/go-src-converted/` | C# (converted) | Whole Go stdlib (302 packages, Go 1.23.1) auto-output. **Compiles clean as of 2026-07-10** (the Phase-3 milestone; operational validation is Phase 4). Its own `src/go-src-converted.slnx`. |
| **Behavioral tests** | `src/Tests/Behavioral/` (457 transpiled test projects, 468 registered `.csproj` incl. sub-libraries, + `BehavioralTests` runner) | Go + C# | Per-feature Go↔C# equivalence (arrays, channels, defer, generics, interfaces…). |
| **Performance tests** | `src/Tests/Performance/` (8 `Perf*` benchmarks + `PerformanceRunner`) | Go + C# | Go vs transpiled C# (JIT **and** Native AOT) time/memory comparison — results table in its `README.md`. |
| **Examples** | `src/Examples/` | Go + C# | Hand-converted Tour-of-Go / go101 / misc samples. |

**Two solutions:** `src/go2cs.slnx` = converter-dev workspace (golib + `go2cs-gen` + the baseline subset +
all tests/examples/utilities) — **builds green**. `src/go-src-converted.slnx` = the full conversion
(302 packages, **compiling clean as of 2026-07-10**), kept separate so its `namespace go` classes don't
collide with the baseline's. (A third, classic-format `src/go2cs-examples.sln` covers the samples — the two
primary solutions are `.slnx`; the old hand-maintained classic `.sln` files are retired.)

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
2. **Full conversion** (`src/go-src-converted/`) — the entire Go stdlib auto-converted; **all 302
   packages compile as of 2026-07-10** (the Phase-3 milestone). Phase 4 (operational — Go tests) is next.
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
  - `-recurse` — recursively convert an end-user module + its third-party deps (references the pre-converted
    stdlib via local `$(go2csPath)` project refs). `-recurse=nuget` instead emits NuGet PackageReferences
    (`go.<pkg>`/`go.lib`/`go.gen`, versioned `$(GoStdLibVersion)`) for the go2cs stdlib/runtime/analyzer so a
    converted app restores from nuget.org with no `deploy-core` staging; the app's own converted packages
    stay project refs, and the converter emits an output-root `Directory.Build.props` pinning `$(go2csPath)`
    + a floating `GoStdLibVersion` default.
  - `-tests` — also convert the package's eligible `_test.go` suite + emit a runnable test-host project
    (default off; mutually exclusive with `-recurse` — `log.Fatal` on both). Forces `-comments` on (test
    conversions are derivative works), resolves the output path absolute, and self-locates `$(go2csPath)` by
    walking the output dir up to the first root containing `core/golib` — so the canonical two-argument form
    `go2cs -tests -test-action all <goroot-pkg-dir> <converted-pkg-dir>` needs no flags or env from a clone.
  - `-test-action convert|build|run|compare|all` (default `convert`) — `convert`/`all` convert-and-hook
    (production sources then tests); `build`/`run`/`compare` act on EXISTING digest-validated artifacts
    without reconverting; `compare` (and `all`) diffs the C# host's terminal results vs `go test -json -count=1`.
  - `-test-timeout <dur>` — per converted-test child process (build/run/compare); Go duration syntax,
    default `2m`, must be > 0.
  - `-go2cspath <dir>` — output root for converted code (default `~/go2cs`; env `GO2CSPATH`).
  - `-goroot` / `-gopath`, `-platforms os/arch`, `-indent 4`, `-var` (default on),
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
- **FALSE-GREEN route #2 — stale OUTPUT (fixed 2026-07-20).** Distinct from the stale-`go2cs.exe` trap
  (route #1, where an un-rebuilt binary runs old logic): here the exe IS current but the runners *skip
  transpiling* and validate the **previous** converter's `.cs`. All three of `BehavioralRunner.UpToDate`,
  `PerformanceRunner.UpToDate`, and MSTest `BehavioralTestBase.TranspileProject` short-circuited on a
  `.cs`-newer-than-`.go` check alone. Converter work is exactly the case where the `.go` files *don't*
  change, so every project stayed "up to date", transpile was skipped for all of them, and Target/Output
  then compared the old converter's output against goldens that same converter had generated — everything
  matched and the suite printed **PASS**. A guard test "validated" that way guards nothing. All three now
  also require the `.cs` to be newer than **`go2cs.exe`**, so any converter rebuild invalidates the whole
  corpus. Verified by neutering a real converter fix (`lhsReusedInLaterRhs`) and rebuilding: the old
  runner reported PASS, the fixed runner reports `FAIL [Target,Output]` with no manual touch.
- **`check-no-regression.ps1` re-transpiles UNCONDITIONALLY** (it has no `UpToDate` equivalent), which is
  why CNR was immune to both false-green routes and remains the authoritative drift instrument for
  converter changes. Preserve that asymmetry: never add an up-to-date skip to CNR.
- **`TargetComparisonTests` compares goldens with line endings NORMALIZED** (CRLF→LF; see
  `TargetComparisonTests.FileMatch` / `BehavioralRunner.FilesEqual`, both strip CRs). It was a raw
  byte-for-byte compare until 2026-07-07. Content diffs are still caught exactly; a pure line-ending
  difference is ignored (it can only come from autocrlf, never from the deterministic converter). To
  re-baseline goldens after an *intended* output change, run the **`UpdateTestTargets`** project with
  **`--createTargetFiles`** — it copies each project's current on-disk transpiled `.cs` over its
  `.cs.target` (it does **NOT** re-run the converter — re-transpile first, e.g. via
  `check-no-regression.ps1` or a runner pass, or the copy silently re-baselines stale output) —
  don't hand-edit goldens.
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
  (2026-06-30).** A dependency-free console app that runs the same four phases over every behavioral
  project but is **not** hosted in testhost, so the
  self-lock failure mode above is structurally absent. It collapses the per-project `dotnet build`
  calls into one parallel MSBuild invocation (pre-building the ~31 shared `golib`/analyzer/`core/*` deps
  sequentially first to avoid the parallel-build MSB3026/27 race, then fanning out). **All green**, at
  parity with MSTest — the parallel MSBuild invocation keeps wall-time from
  scaling linearly with project count. Drive it via **`run-behavioral.ps1 [--filter X]
  [--phase transpile,compile,target,output] [--update-targets] [--list]`**. Only output-compared
  (`[GoTestMatchingConsoleOutput]`) projects are `go build`- and stdout-compared, matching MSTest
  (library-style projects like `Constraints` have no `package main`). For a pure converter no-regression
  check with no compile/run at all, use **`check-no-regression.ps1`** (re-transpiles every behavioral dir
  and `git status`es the `.cs`).
- **Run the behavioral suite via the solution, not the project:** `dotnet test src/go2cs.slnx`. Running
  `dotnet test` on `BehavioralTests.csproj` directly breaks because `$(go2csPath)` (→ `$(SolutionDir)`)
  has no solution context, so the `core\golib` ref fails to resolve. The baseline solution is now an
  **`.slnx`** (`src/go2cs.slnx`); `src/go-src-converted.slnx` is ALSO `.slnx` as of 2026-07-10 — auto-generated by the converter's `-stdlib` run (solutionGenerator.go) with solution folders mirroring the Go package namespaces; adopt a fresh one by copying it from the output root and rewriting project paths `core/` → `go-src-converted/` (golib excepted). The old hand-maintained classic `.sln` is retired.
- **When iterating on regression work, use FILTERED + `--no-build` tests — don't run the full suite each
  time.** The full `dotnet test go2cs.slnx` rebuilds all **502** registered projects first and can take
  10+ min or hang under Visual Studio lock contention. Instead, from `src/Tests/Behavioral/BehavioralTests`, run
  `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` — that reuses the existing test
  assembly and runs just that project's 4 phases (Transpile/Compile/TargetComparison/OutputComparison) in
  seconds. `--no-build` is valid as long as the `*Tests.cs` files haven't changed (`git status` them).
  Reserve a single full-suite run for final confirmation. Faster still for a pure no-regression check:
  re-transpile every behavioral dir and `git status` the `.cs` — byte-identical generated code ⟹ identical
  compile+output ⟹ identical results, with no compile/run at all.
- **Budget each command against its MEASURED baseline — the old flat "~3 min" cap is no longer right for
  the full runs (re-measured 2026-07-23, corpus at 457 transpiled / 468 registered `.csproj`).** The corpus
  has grown ~25% since those figures were written (371 → 457 projects), and both full instruments now
  legitimately exceed three minutes. Timeouts must clear the real number or a healthy run gets killed
  mid-flight (a 600s ceiling killed a *passing* full suite once):

  | Command | Measured (warm) | Set timeout | Notes |
  |---|---|---|---|
  | `run-behavioral.ps1` (full, 4 phases) | **~385–475s (6.5–8 min)** | 600–900s | 457/457 Transpile+Compile+Target; 427 Output-compared, 30 skipped (no `package main`) |
  | `check-no-regression.ps1` (full) | **~285–320s (5–5.5 min)** | 480–600s | transpile-only, no compile/run; re-transpiles unconditionally |
  | `run-behavioral.ps1 --filter <Name>` | **~10–20s** (8 projects) | default | the iteration loop — use this, not the full suite |
  | `go2cs -stdlib -comments` (full reconvert) | **~195s (3m 14s)** | 600s | 304 projects; per-file work is sub-second, the cost is `go/packages` |
  | single `go-src-converted` pkg build | **~6s** (log/slog) – **~60s** cold (go/types) | 180–400s | cold includes the dependency chain |

  Materially *past* these means the test host has hung under lock contention, not real work — stop and
  clear it rather than waiting 10–20 min. **Re-measure and update this table when the corpus grows again**;
  a stale baseline is what makes a healthy run look hung (and vice versa). The spreads above are real
  run-to-run variance on the same corpus (machine load), so budget from the TOP of the range, not the
  midpoint. A converter rebuild invalidates every project's up-to-date check, so the *next* full run
  after one always pays full price.
  ⚠ **Piping a long run through `Select-Object -Last N` buffers ALL output until it completes** — a
  backgrounded suite will look stuck at its first line for its entire duration. Check liveness with
  `Get-Process BehavioralRunner,dotnet`, not the output file.

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
   the `/tests/behavioral/target-projects/` folder in `src/go2cs.slnx` (alphabetical). **If the test pulls in
   a sibling library sub-project via `<ProjectReference>`** (e.g. `GoNamespaceShadow` → `nsshadowlib/go.nsshadow.csproj`),
   register **that** too, on the line right after its parent (the pattern used by `IoLike`→`IoLike/FsLike`,
   `NamedSliceChildPkg`→`.../netlike`). **Then verify it stuck** — run **`./check-solution-integrity.ps1`**
   (from `src/Tests/Behavioral`): it asserts every behavioral `.csproj` on disk is registered in `go2cs.slnx`
   and flags any dangling entry, exit-1 on violation. (Also runs automatically as the preflight of
   `check-no-regression.ps1`.) This matters because the harness builds each `.csproj` **by path**, not via the
   solution, so a missing registration still passes the whole suite — it only breaks the `go2cs.slnx` build in
   Visual Studio (the unregistered project loses the Debug/`$(go2csPath)` context and its `core\*`/`gen\*` refs
   fail: CS0246/CS0234). That is exactly how `nsshadow` slipped through (added in `96eff53cd`, unregistered
   until `53dd2497e`). If Visual Studio has the `.slnx` open it can rewrite/reformat the file and silently drop
   an external edit — re-add and re-verify if so.
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
7. **Record the conversion decision (keep the strategy docs living).** The conversion strategy lives in
   **two** documents, and a notable decision updates the right one (often both):
   - [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) — the exhaustive
     **technical reference**. Nearly every conversion decision lands here: add or update the `###` subsection
     under the matching `##` topic with the emitted form, the edge case, the reasoning, and the guarding
     behavioral test. This is where the deep detail and history accumulate.
   - [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — the high-level **summary** (one section
     per topic, tight prose + a couple of real Go→C# examples, each linking into the reference). Update it
     only when the decision changes the *headline* mapping of a construct or warrants a better/clearer
     example — not for every edge-case fix. Keep it short and readable; push the detail to the reference.

   Do this **in the same change** so both docs keep matching reality. Verify every C# snippet against the
   actual `.cs.target` golden (it is the authoritative record of emitted forms — e.g. `u8` format strings,
   `throw panic(...)`, `ж<T>`/`Ꮡ`); the summary's examples should prefer real snippets pulled from the
   `go-src-converted` stdlib (Go source ↔ converted C#). Skip only for pure bug-fixes that restore an
   already-documented behavior. (This rule is not limited to the regression-test flow — it applies to *any*
   commit that lands a notable conversion decision.)

### Phase 3 mechanics — measuring/iterating the full conversion (`src/go-src-converted`)
- **The on-disk `go-src-converted` is stale** (last bulk conversion 2025-05-11); it predates current
  converter fixes. To measure the *current* converter you must reconvert — building the committed tree
  measures old output.
- **Reconvert → overlay → build → bucket (the measurement loop):**
  1. `go2cs.exe -stdlib -comments -go2cspath <tmp>` → output lands in **`<tmp>/core/<pkg>`**
     (the `core` subdir is hardcoded; `-go2cspath` is the *output* root, unrelated to the MSBuild
     `$(go2csPath)`). Full stdlib ≈ 3–4 min (per-file work is sub-second; the cost is `go/packages`
     loading the whole type graph, so **batch** — don't invoke per package).
  2. Overlay the fresh `.cs` onto `src/go-src-converted/<pkg>` (keep the relocated csprojs).
  3. Build single packages with **`dotnet build <pkg>.csproj -c Debug`** — `src/go-src-converted/Directory.Build.props`
     now pins `$(go2csPath)` to the src root, so `core\golib` + the `go2cs-gen` analyzer resolve to live source
     with **no `-p:go2csPath` flag**; or build the whole `go-src-converted.slnx`. (If you ever do pass the flag
     explicitly, use forward slashes — `-p:go2csPath=H:/Projects/go2cs/src/` — a trailing `\` escapes the
     closing quote and mangles the path into phantom golib-not-found errors.)
  4. Bucket: `dotnet build … -clp:ErrorsOnly` then group by `error CS####`. Errors shown are *own-errors*
     of leaf-most failures — dependents of a failed project are skipped, not errored.
- **csproj layout/relocation:** the converter emits inter-package refs as `$(go2csPath)core\<pkg>\…` and
  the golib ref as `$(go2csPath)core\golib\…`. The 2026-06-25 relocation rewrote **`core\` → `go-src-converted\`
  for all stdlib refs *except* `core\golib`** (golib stays shared in `src/core/golib`). A fresh wholesale
  reconvert must re-apply that rewrite to the generated csprojs. **⚠ There is a SECOND exception (2026-07-20):
  `core\testing`.** `internal/testenv` deliberately references `$(go2csPath)core\testing\testing.csproj` per the
  F15b one-testing-package ruling (`98642bca1`) — the Phase-4 test host must bind ONE testing package, not a
  per-tree copy. A blanket `core\` rewrite clobbers that reference and silently breaks `internal/testenv` and
  everything downstream of it, so an overlay must except **both** `core\golib` and `core\testing`.
- **Metric:** measure **packages-compiling**, not raw error count. Fixing file-inclusion bugs (e.g. the
  filename build-constraint fix) *raises* the error count because newly-included files surface their own
  latent defects — that's progress, not regression.
- **Don't commit `go-src-converted` regens casually.** It's regenerable; the unit of work is the
  **converter fix**. Keep the tree restorable (overlay into a branch or restore with `git checkout HEAD --`
  + remove untracked) so a converter-fix commit isn't buried under thousands of generated-file changes.

## Current state & known issues

- **Baseline is green:** `src/go2cs.slnx` builds clean and the behavioral suite passes. The
  separation is done — `src/core` (stub baseline + `golib`) vs `src/go-src-converted` (full conversion). The
  converter-improvement loop is restored. **`sync/atomic` was promoted into `core` (2026-06-26)** — the first
  full-conversion package migrated to the baseline (scalar types are converter output; `Pointer[T]` is
  hand-rewritten with a managed slot + `Interlocked`, since `unsafe.Pointer`=`nuint` can't hold a managed
  reference across a GC — see [`docs/Roadmap.md`](docs/Roadmap.md)).
- **Phase 3 complete (2026-07-10 — commit `51ba5d9cf`, tag `stdlib-green-2026-07-10`):** all **302**
  packages of the full `src/go-src-converted` conversion (Go 1.23.1) compile clean — zero errors, zero
  exclusions (`runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`, … all included).
  **Compiling is the milestone, NOT operational** — operational validation is Phase 4 (running Go's own
  package tests). Campaign detail: [`docs/Roadmap.md`](docs/Roadmap.md) (Phase 3 iteration log) and the
  [`docs/README.md`](docs/README.md) NEWS section.
  - **⚠ Promotion is still deferred (2026-07-01 ruling — the milestone did NOT change it):** do **not**
    promote `go-src-converted → core` on a clean compile. Promotion waits for post-Go-test (Phase 4) and may
    not be needed at all. `core` stays the bootstrap **stub** the behavioral tests build against (`sync/atomic`
    there is fine). The canonical `[module: GoManualConversion]` / `*_impl.cs` files are hand-owned in `core`
    and must be copied **back into `go-src-converted`** (overlay.sh does this) — that overlaid tree is the real
    final state.
  - **⚠ Phase-4 operational: two hand-owned patterns, and a WHOLE-FILE rewrite MUST carry the marker.** Making a
    package *run* (not just compile) often needs a native reimplementation where the literal conversion compiles
    but cannot work — e.g. `sync`'s Mutex/RWMutex/WaitGroup (2026-07-11), whose Go runtime sleeping semaphore
    cannot be emulated, are hand-rewritten on `SemaphoreSlim`/monitors. A `<name>_impl.cs` companion
    *supplements* some declarations (bodyless `partial` + a comment placeholder the converter emits); a
    **whole-file** hand rewrite *replaces* the converted `<name>.cs` and **must carry `[module:
    go.GoManualConversion]`** — else a `-stdlib` reconvert regenerates the Go version over it (`main.go`'s
    `containsManualConversionMarker` drops marked files from the convert set; place it after the `using`s,
    before the file-scoped namespace). Detail:
    [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md) *Hand-owning a package to make it
    OPERATIONAL*.
  - **⚠ The S1/CS0030 "architectural wall" was a FORK, not a wall (2026-07-01) — and the fork held to 302/302.**
    **Native-type** pointer/unsafe ops (identical memory semantics in both GC languages) get a faithful
    conversion in the converter/`golib`. **Managed-referent** cases (`guintptr`/`muintptr`/… hiding a managed
    pointer in a `uintptr`) hold the `ж<T>`/`object` **directly** (like `core/sync/atomic` `atomic.Pointer<T>`),
    never a `nuint` round-trip. Genuine **raw-metal on non-native types** (memory-layout math, type-descriptor
    walking, `*.asm`) is stubbed with `[module: GoManualConversion]` (a stub that compiles is an acceptable
    milestone solution). Full detail:
    [`docs/Baseline-vs-FullConversion.md`](docs/Baseline-vs-FullConversion.md) *The corrected end-state*.
  - **Next — Phase 4 (operational):** convert and run Go's own `_test.go` suites against the compiling
    packages; design in [`docs/TestingInfrastructureRequirements.md`](docs/TestingInfrastructureRequirements.md)
    and Phase 4 of [`docs/Roadmap.md`](docs/Roadmap.md). The `-tests` pipeline is live (`go2cs -tests
    -test-action all <goroot-pkg> <go-src-converted-pkg>`): converts `_test.go` variants, builds a
    hand-owned `go.testing` host (`src/core/testing`), runs it isolated, and diffs terminal results
    against `go test -json`. `-tests` **always forces `-comments`** (test conversions are derivative
    works — the per-file Go copyright header must survive) and **self-locates `$(go2csPath)`** by walking
    the output dir up to the tree root (so the two-arg command works from a bare clone, no env). First
    validated package: `unicode/utf8` (2026-07-17, tag `utf8-tests-green-2026-07-17`).
  - **⚠ Validated-package commit policy (2026-07-17 user ruling):** when a package's Go test suite
    **validates** through the pipeline, COMMIT its converted C# test sources into
    `src/go-src-converted/<pkg>` beside the production code — `*_test.cs`, `package_test_info.cs`,
    `go2cs_test_host.cs`, `<pkg>.tests.csproj` — so the passing suite is **visible and reviewable on
    GitHub**, and reproducible via the [README "Try it yourself"](docs/README.md#try-it-yourself--validate-a-converted-test-suite)
    instructions. The pipeline's regenerated inputs/outputs are **git-ignored** by
    `src/go-src-converted/.gitignore` (the staged `*.go` source copies + `go2cs_test_manifest.json`
    [machine-specific exe-hash digest] + `go2cs_test_comparison/results.json`/`.xml`). The production
    `<pkg>.csproj` also updates on this run (the IP-4 test-artifact `<Compile Remove>` exclusion) — that
    change is intended, not drift. Refresh the committed test sources at each milestone rebank alongside
    the production tree.
- Open converter items: `src/go2cs/ToDo.md` (e.g. `visitMapType` completion, remaining dynamic-struct
  implicit-cast checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).

### Deploying the core to the GOPATH root
`src/deploy-core.ps1` (cmd launcher `deploy-core.bat`) stages the runtime + standard library at
`%GOPATH%\src\go2cs` so converted projects — and, later, recursively converted end-user apps that target
that same root — resolve their `$(go2csPath)core\<pkg>` / `gen\go2cs-gen` references relatively. Two modes:
`deploy-core stub` (baseline `src/core`, **runnable**) and `deploy-core stdlib` (full `src/go-src-converted`,
**compilable**; rewrites `go-src-converted\`→`core\` refs so both modes present the stdlib at `core\<pkg>`).
Both also deploy golib + the `go2cs-gen` analyzer, write a root `Directory.Build.props` that pins
`$(go2csPath)` to the deploy root (so no `-p:go2csPath` is needed), generate `go2cs-core.slnx`, and build to
verify. The other src PowerShell utilities `clean-bin.ps1` (remove bin/obj/Generated) and `set-version.ps1`
each also have a `.bat` launcher.

### Known staleness (do not trust blindly)
- `docs/README.md` is partly historical (ANTLR4-era prose) but now carries a banner and corrected references.
- The retired `net6.0` C# converter scripts (`src/convert-gosrc.cmd` / `convert-gosrc.bat`) were **removed**
  2026-07-11; the current converter is the Go build with the flags listed above.

## Conventions

- C# style: see [`docs/coding-style.md`](docs/coding-style.md) (Allman braces, 4 spaces, `m_`/`s_`/`t_`
  field prefixes, explicit types over `var`, language keywords over BCL types, `\uXXXX` for non-ASCII).
- Conversion strategy: [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — a high-level,
  example-driven **summary** of how each Go construct maps to C#; each section links into the exhaustive
  [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) for the full detail.
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
| `3c8b3a848` | 2026-06-25 | Separation + stub-baseline restore + converter fixes → green baseline. |
| `05a53e8c0` | 2026-06-26 | First full-conversion package promoted — `sync/atomic` into `core`. |
| `914d4bd72` | 2026-06-27 | `math` compiles clean (tag `math-green-2026-06-27`). |
| `51ba5d9cf` | 2026-07-10 | **First clean full-standard-library compile** — all 302 `src/go-src-converted` packages (tag `stdlib-green-2026-07-10`); Phase-3 milestone. |
| `337a928df` | 2026-07-17 | **First real Go test suite validated in C#** — `unicode/utf8` 14/14 vs `go test -json` through the Phase-4 `-tests` pipeline (tag `utf8-tests-green-2026-07-17`); §12.8 opened. |
| `f999c8f78` | 2026-07-18 | **Second validated package** — `sort` 63/63 vs `go test` (tag `sort-tests-green-2026-07-18`); first with real algorithmic depth (interface-driven sort, `sort.Slice` reflection, NaN ordering). |
| `40f39d2be` | 2026-07-18 | **Packages #3 and #4 validate** — `bytes` 81, `strings` 68 (tag `bytes-strings-tests-green-2026-07-18`), via the hand-owned signature-pinned **disclosed-divergence manifest** (`go2cs_test_disclosures.json`) for the alloc-count asserts the managed CLR provably cannot satisfy. |
