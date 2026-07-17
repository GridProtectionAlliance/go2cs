# Blocker map: `strings` & `bytes` test suites (Phase-4 packages #3–4 work order)

> Read-only scout, 2026-07-17, against master `1fb3eae1c` (post `/vN` fix; `testing.AllocsPerRun`
> was still in flight). **Verified by construction, not hypothesized**: the scout iteratively
> scratch-patched every blocker until both packages compiled and ran through the real test host —
> end state **strings 57/69 PASS, bytes 74/82 PASS** — so each fix sketch below was demonstrated
> to clear its blocker. The tree was fully restored afterward.

The Step-3 sweep's census reproduces exactly: strings 64/68 tests included (3 × `AllocsPerRun`,
1 × `AllocsPerRun`+`CoverMode`), bytes 81/88 (7 × `AllocsPerRun`). The sweep-era "CS0234
`go.unicode` ×64 vs CS0050 abi" environment-dependence is **gone/superseded** — both symptom sets
were downstream of the same two graph defects (B1 + B2b) under differing `go2csPath` origins. No
abi-accessibility errors exist under the current absolute-path pipeline.

## Build blockers

| # | Blocker | Pkg | First error | Root cause (evidence) | Minimal fix | Size |
|---|---|---|---|---|---|---|
| B1 | `-tests` regenerates the production csproj with raw `core\` refs, clobbering the committed `go-src-converted\` shape | both | CS0246 storm in src/core/errors (600 errors) | main.go:877 runs the full production conversion whose ref writer (main.go:1782) emits `$(go2csPath)core\<pkg>`; the graph reaches it via internal/testenv → internal.testenv.csproj:128 back-ref to strings.csproj. utf8 masked this (its production project refs only golib). Several `core\` targets don't even exist in the stub (unicode, internal/bytealg, internal/stringslite) | Route production-csproj stdlib refs through the same F15 mapping as resolveTestProjectReference when the output root is the go-src-converted tree | S |
| B2 | Name-collision analysis diverges between production emission and test-variant emission | strings | CS0102 `strings_package` already contains `Replacer` + CS0246 `ΔReplacer` | export_test.go adds a method `Replacer` → the test-variant analysis (whole variant universe) renamed the type to `ΔReplacer`, but the production .cs on disk (production-only universe) kept `Replacer`. Two halves of one assembly disagree | Pin production symbol names as immutable in test-variant analyses; collisions resolve by renaming the test-side declarator (method → `ΔReplacer`). Validated by hand-patch | M |
| B2b | In-namespace alias `using io = io_package;` collides (CS0576) when any referenced assembly contributes child namespace `go.io` | strings (bytes precluded) | CS0576 ×~10 + CS0234/CS0535/CS0539 cascade in .g.cs | Transitive project refs flow internal/testenv → io/fs (namespace go.io) into the test compilation. The Δ-alias machinery (cf. bytes' Δunicode) is computed against the production import closure only. Generator fallout: field types become error types, so the generator pastes raw `io.Writer` instead of `global::go.io_package.Writer` | One line in the embedded tests-csproj template: `<DisableTransitiveProjectReferences>true</DisableTransitiveProjectReferences>` — the compile view becomes exactly the direct refs the test converter computed. Validated | XS |
| B3 | package_test_info.cs attrs can't see test-package types | both | CS0246 `errWriter` (strings), `negativeReader`/`panicReader`/`TestReaderCopyNothing_just*` (bytes) | The seeded info file has `using static go.<pkg>_package;` only; GoImplement attrs referencing types nested in `<pkg>_test_package` don't resolve | Add `using static go.<pkg>_test_package;` in the seeding writer (or subsumed by B4/B5's split) | XS |
| B4/B5 | ImplementGenerator anchors all adapters to the FIRST class in the attr-bearing file — test assemblies have two consumer packages | both | CS1929 on errWriter adapter; CS0246 `strings_BuilderжWriter`, `bytes_BufferжWriter`, `strings_ReaderжReader`, `os_FileжWriter`; CS0120/CS0034 in `TestReaderCopyNothing_just*` adapters | ImplementGenerator.cs:126 `GetFirstClassName(compilationUnit)` → `strings_package` (declared first in the info file), so test-introduced casts generate local-named adapters in the wrong class, while the converter emits consumer-perspective names hosted in the test class. Foreign adapters are by design hosted in the consuming package — the merged single info file destroys the two-consumer distinction | Converter-only fix: emit test-introduced GoImplement/ImplicitConv attrs into a SEPARATE compilation unit whose first class is the test package class (production-seeded attrs stay anchored to the production class). Avoids touching go2cs-gen and its full-suite+corpus gate. Validated by hand-hosting the adapters | M |
| B6 | core/testing shim missing compile-surface | both | CS1061 ReportAllocs/SetBytes/ResetTimer/StartTimer/StopTimer/Errorf/Fatal/Fatalf on B; AllocsPerRun; CoverMode | Benchmark bodies and capability-excluded tests still COMPILE (exclusion gates the run list, not emission) | Add no-op B members (+ explicit ж\<B\> overloads for the params ones), `CoverMode() => ""`, and AllocsPerRun (landed separately). All validated | XS–S |
| B7a | Go int constant > int32 emitted as bare `L` literal | both (1 site each) | CS1503 long → nint | `math.MaxInt64/4` in the SplitN tables → `2305843009213693951L` with no cast | Constant renderer wraps int-typed constants exceeding int32 in `(nint)` | S |
| B7b | Func literals lose their declared result type | strings ×3, bytes ×2 | CS1503 `Func<int, UntypedInt>` / CS8917 | `var maxRune = (rune r) => Δunicode.MaxRune;` — body returns an untyped constant (or mixed paths) so C# natural-type inference fails | convFuncLit: emit explicit lambda return type (`var f = rune (rune r) => …`) whenever the Go literal declares a result type | S |
| B8 | Cross-file dynamic-struct resolution failure | bytes | CS1526 + ~170 parser-cascade errors from ONE site | foreach+heap over `compareTests` (anonymous `[]struct{a,b []byte; i int}` declared in compare_test.go) emitted raw Go type text at bytes_test.cs:64 — the synthesized `compareTestsᴛ1` exists in compare_test.cs:15 but isn't found cross-file. Known ToDo class, now with a reproducer. Site is inside an AllocsPerRun-excluded test — **excluded tests still block builds** | Dynamic-struct registry must unify anonymous struct types across files of the (test-)package before emission | M |

## Runtime blockers (found by actually running the hosts)

| # | Blocker | Failing tests | Root cause (evidence) | Fix sketch | Size |
|---|---|---|---|---|---|
| R1 | `[]T(nil)` conversion throws | strings cctor cascade (~30 tests) | `append([]string(nil), …)` → `slice<@string>(default!)` → builtin.cs:1624 `slice<T>(T[])` throws ArgumentNull on null; Go says nil→nil slice | golib: null array → default (validated; cleared the cascade) | XS |
| R2 | internal/godebug not operational | strings cctor (via math/rand) | godebug.cs:170 `Value()` → setting.value atomic pointer only populated by Go-runtime update hooks that never run → nil deref | Hand-owned minimal `Value()` (parse %GODEBUG% or return "" = Go's unset default) | S |
| R3 | runtime_rand linkname stub | strings TestIndexRandom + cctor | rand.cs:375 PartialStubGenerator → NotImplementedException | rand_impl.cs companion supplying a real RNG (validated) | XS |
| R4 | `len(string)` = UTF-16 char count | strings TestIndexAny/TestLastIndexAny/TestLastIndexByte | Tables use `len("a☺b☻")` where the literal stayed a plain C# string (u8 suppressed per-arg via u8StringArgOK, convExprList.go:89) → builtin.cs:1144 returns .Length = 4, Go = 8 | golib: Encoding.UTF8.GetByteCount (validated) + audit other System.String-accepting golib APIs with length/index semantics | XS + M audit |
| R5 | reflect.DeepEqual → converted unsafe.Pointer NRE | strings ×4, bytes ×2 | deepequal.cs:74 → unsafe.cs:261 Pointer.op_Implicit on null managed slot | reflect/unsafe managed-slot null handling | M |
| R6 | MakeNoZero throws .NET OverflowException, not a Go panic | TestRepeatCatchesOverflow (both) | bytealg_impl.cs:9 hand-owned impl; recover() only catches go.PanicException | Validate n and `throw panic("runtime: makeslice: len out of range")` | XS |
| R7 | `string([]rune)` with invalid runes throws | strings TestCaseConsistency | ToUTF8Bytes (builtin.cs:299) rejects surrogates; Go encodes U+FFFD | golib: invalid rune → RuneError bytes | XS |
| R8 | `array<T>` zero-value enumeration NRE | strings TestFinderCreation/Next | `[256]int` default array<T> has null backing; enumerator NREs (array.cs:201 via search.cs:56) | golib array<T> lazy/nil-safe backing | S |
| R9 | `ж<T>.ToString()` pointer-print crash | strings TestClone | PrintPointer → PinnedBuffer[index] IndexOutOfRange | golib PrintPointer bounds handling | S |
| R10 | %T prints adapter class name | strings TestPickAlgorithm | Prints `strings.byteReplacerжreplacer`, Go wants `*strings.byteReplacer` | TestFormat/golib: unwrap IжAdapter for %T | S |
| R11 | Identity-Map copies (unsafe.StringData identity) | strings TestMap | Zero-copy fast-path identity not preserved through @string | Semantics decision — possibly an acceptable-difference disclosure | ? |
| R12 | Nil-receiver method derefs before the nil guard | bytes TestNil | Go's `(*Buffer).String()` checks `b == nil` first; emitted preamble `ref var b = ref Ꮡb.Value` (buffer.cs:70) derefs unconditionally | Converter: receiver preamble must not precede a reachable nil-receiver guard | M |
| R13 | nil-vs-empty slice distinction | bytes TestClone/TestTrim/TestTrimFunc | `TrimRight("a","a")` must return nil, `Clone([])` non-nil empty — golib doesn't preserve the distinction | golib slice nil-identity semantics (subtle; interacts with R1) | M |

## Capability enumeration

- **testing.CoverMode** — exactly one user across both packages: strings_test.go:325 (TestIndexRune):
  `if allocs != 0 && testing.CoverMode() == ""`. A constant-`""` shim member is exactly correct — Go
  returns `""` when coverage is off, and the test then takes the same path as an uncovered `go test` run.
- **t.Parallel** — zero uses in the included sets. All four bytes sites live in boundary_test.go, which
  is `//go:build linux` → platform-excluded by the census on windows/amd64. No host gap for #3–4.
- **testdata/fixtures** — neither package has a testdata/ dir.
- **internal/testenv surface** — only `testenv.Builder()` (strings TestCompareStrings — passed) and
  `testenv.SkipIfOptimizationOff(t)` (bytes TestNewBufferShallow, AllocsPerRun-excluded anyway). Both
  work through the converted testenv; no TB-surface gap for these two packages.

## Sequencing recommendation

1. **B1 + B2b + B3 + B6** (all XS/S) get both packages to the interesting errors immediately; B2b is one
   template line and precludes the whole CS0576 class for every future package whose test deps drag
   nested stdlib packages.
2. **B4/B5** is the structural decision (converter-side two-anchor split — avoids the go2cs-gen
   full-suite+corpus gate).
3. **B7a/B7b/B8** are self-contained converter emission fixes; each deserves a behavioral guard test.
4. **Runtime:** R1+R2+R3+R4 alone took strings from 23→57 PASS in the scout run — highest leverage.
   R5 (DeepEqual) clears 6 tests across both packages. The tail (R7–R13) is per-test polish.

## Status updates (2026-07-17 evening)

- **B1 + B2b + B3: FIXED** (master `3ef721665`, chip commit `9400f3680`) —
  `resolveProductionProjectReference` routes `-tests` production-csproj refs through the F15
  mapping (pass-through otherwise, CNR byte-identical ×399); the tests-csproj template sets
  `DisableTransitiveProjectReferences`; `appendExternalTestPackageClass` widens the info file's
  `using static` scope to the external test class. Three converter guards. utf8 re-validates
  14/14 through the changed pipeline; sort's production csproj survives a `-tests` run with only
  the intended IP-4 exclusion diff.
- **B6: FIXED** (master `21dd3da1c`) — compile-only `B` surface (`ReportAllocs`/`SetBytes`/
  `ResetTimer`/`StartTimer`/`StopTimer`/`Errorf`/`Fatal`/`Fatalf`) + `CoverMode() => ""`;
  discriminating guard `BenchmarkCompileSurfaceIsNoOpAndCoverModeReportsCoverageOff`.
  `testing.CoverMode` census inclusion followed as a coordinator commit, so strings censuses
  68/68 included once it builds.
- **NEW — B2c (exposed by B2b's fix; 1 × CS0234 in sort):** the seeded global alias
  `reflectliteꓸKind = go.@internal.abi_package.ΔKind` (package_test_info.cs) targets
  `internal/abi`, which sort reaches only transitively (sort → reflectlite → abi) — now hidden
  from the test compile view by `DisableTransitiveProjectReferences`. **Ruling:** the converter
  must emit direct F15-mapped project references for every assembly a seeded alias targets (it
  knows the alias set it seeds); NOT subsumed by B4/B5's anchoring split — the alias survives in
  whichever compilation unit hosts the production-seeded attributes. Folded into the B4/B5 chip.
- **Sort's wall after these fixes:** 1 × CS0234 (B2c) + 10 × CS0246 adapter anchoring (B4/B5) —
  exactly the documented next tier; runtime rows R1-R13 still unreached.

## Cross-cutting lessons

- **Capability-excluded tests still compile** — exclusion gates the run registry, not emission; a broken
  emission inside an AllocsPerRun-excluded test blocked all of bytes.
- The sweep's environment-dependent error sets were both shadows of B1/B2b, not an abi problem.
- Minor: the tests-csproj template's later `<OutDir>` override defeats its own
  `BaseOutputPath=bin\tests\` (exe lands in bin/Debug/net9.0/) — cosmetic; align when touching the template.
