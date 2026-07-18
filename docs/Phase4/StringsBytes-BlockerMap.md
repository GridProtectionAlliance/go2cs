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
- **B8: FIXED** (worktree branch `claude/eloquent-mestorf-c5e017`) — the general type-name
  renderers (`getTypeName`/`getFullTypeName`) now resolve a non-empty anonymous struct/interface
  through the shared dynamic-type registry / deferred marker (`deferredDynamicTypeName`) instead
  of falling through to raw `t.String()` Go text; the `«DYNTYPE:…»` marker payload is now the
  HEX-ENCODED signature so it survives the string-transform passes (`convertToCSTypeName`
  rewrites `[`/`]` → `<`/`>`, which corrupted a raw-signature marker before post-barrier
  resolution). Guard: `AnonStructCrossFile` (three files, both visit-order directions, heap-boxed
  range var; CS1526 without the fix). CNR byte-identical ×399. **Bytes probe after the fix:** the
  ~170-error parser cascade is gone — `bytes.tests.csproj` is down to exactly **2 × CS0246** in
  ONE generated adapter (`negativeReader` → io.Reader ptr impl in `bytes_package`'s .g.cs), i.e.
  the B4/B5 anchoring class. Bytes' next wall = B4/B5 (+ B7a/B7b sites at their lines), then the
  runtime rows.
- **B7a + B7b: FIXED** (2026-07-17, worktree branch `claude/modest-ramanujan-87ebb2` — coordinator
  gates the merge). B7a: the typed-`int` signed constant fold (`overflowingConstLiteral`) now
  carries its own `(nint)(…L)` cast, parenthesized so the assignment path's
  `nativeIntConstCastType` recognizes it and does not re-wrap — zero corpus drift
  (`NativeIntWideConstAssign` byte-identical). B7b: `convFuncLit` states an explicit lambda
  return type when a single-result BASIC-numeric literal in assignment position has a named
  untyped-const return arm (targeted predicate, not the uniform fallback — no golden churn;
  argument-position and literal-only-arm literals keep the plain form). Guards:
  `NativeIntWideConstElement` + `FuncLitUntypedConstReturn`, both discriminating — reverted-fix
  runs fail with exactly the mapped errors (CS1503 long→nint; CS1503 `Func<int, UntypedInt>`).
- **B4/B5 + B2c: FIXED** (worktree branch `claude/trusting-engelbart-161588`) — converter-only,
  as ruled. (a) The EXTERNAL variant's GoImplement/GoImplicitConv records now split across TWO
  anchors: test-anchored records (bare test-local impls, every non-production ж pointer adapter,
  adapter-class-marked ᴠ pairs) land in a NEW compilation unit `package_info_test.cs` whose
  first — and only — class is the test package class (bare partial, no `[GoPackage]` — that
  stays on `package_test_info.cs`'s appended block, CS0579), so the generators host their
  output where test-file cast sites resolve it; production-qualified records keep the
  production anchor. The `_test.cs` suffix free-rides the committed `*_test.cs` production
  exclusions — no shared-csproj-template edit (which would churn every behavioral csproj).
  The unit is only written when the variant records test-anchored attrs — utf8's committed
  shape is byte-identical (re-validated 14/14, git clean). (b) Same-assembly naming coherence:
  the production-under-test package's pairs are pre-loaded from its seeded package_info.cs, so
  production-type pointer casts reference the seeded adapter through the aliased qualifier
  (`sort.XжIface`, not the never-generated `sort_XжIface`), value casts fall through to the
  plain emission the partial-struct route implements (`sort_IntSliceᴠInterface` ᴠ-adapters are
  never generated for same-assembly types), and interface-source adapters compose unprefixed —
  in every case matching what ImplementGenerator (foreign = containing-ASSEMBLY) actually
  emits. (c) B2c: `using`-alias lines in the final test metadata are scanned for `go.`-rooted
  `_package` namespace tokens, reverse-mapped through the transitive import closure (the same
  `/vN`-collapsing renderer that emitted them), and any target not directly referenced gets a
  direct F15-mapped project reference (sort: `internal/abi`); manifest dependencies stay
  import-derived. Guards: `TestExternalVariantRecordPartitionAnchors`,
  `TestWriteExternalVariantMetadataSplitsAnchors`,
  `TestAliasReferenceImportsAddsTransitiveAliasTargets`. CNR byte-identical ×399.
- **Sort's wall after B4/B5 + B2c (wave 2 — method-body errors the declaration errors had
  MASKED; Roslyn skips method-body binding while declaration errors exist, so these were
  invisible to every earlier probe):** 23 errors, sort still does NOT build. New rows:
  - **B9 (new, M?):** 14 × CS1501 — sort_test.go dot-imports sort AND example_keys_test.go
    declares a METHOD `Sort` on its `By` type; Go keeps those namespaces separate, but the
    converted method becomes a static `Sort(this By, …)` member of `sort_test_package`, and
    C# member lookup prefers the enclosing class's method group over `using static` imports —
    every dot-imported `Sort(x)` call binds the wrong group. Name-collision family (B2's
    cousin: production symbols must also be pinned against test-package METHOD names under
    dot-import).
  - **B10 (new, S-M):** 6 × CS1503 — delegate-typed argument mismatches: named func type `By`
    passed where the emitted parameter is the raw `Func<ж<Planet>, ж<Planet>, bool>`
    (example_keys_test.cs:29), and method groups passed where `Action<sort_package.Interface>`
    is expected (5 sites, sort_test.cs 569/784/821/829 + one more in the same family).
  - **B7a-family:** 2 × CS1503 numeric-constant typing (`long`→`nint` sort_test.cs:769 = B7a
    exactly; `double`→`nint` search_test.cs:49 is a float-typed sibling).
  - **B6-gap (XS):** 1 × CS1929 — the shim's compile-only `B` surface lacks `Skip`
    (sort_test.cs:791 `b.Skip(…)`); add the no-op + ж\<B\> overload beside the existing eight.
  Sort's dir was fully restored after measurement (validated-package policy: only a validating
  package commits its test sources).

- **B2 + B9: FIXED** (worktree branch `claude/friendly-archimedes-833505` — coordinator gates the
  merge). One shared mechanism, as ruled: production symbol names are IMMUTABLE in a test-variant
  analysis; a collision a test file introduces Δ-renames the TEST-side method declarator.
  `performNameCollisionAnalysis` now tracks per-name declaration origin — (B2) an element-vs-method
  collision whose methods are all test-declared over a production element pins the element (no
  nameCollisions entry, no exported alias) and renames the method; (B9) a test-declared method
  matching a dot-imported foreign function the variant references UNQUALIFIED (Sel-excluded AST
  scan over the whole universe — qualified refs never trigger) renames the same way. The registry
  (`testMethodRenames`) is OBJECT-keyed (same-named production symbols keep their plain emission)
  and SESSION-scoped (one per -tests run, not per variant — both variants share one load, so the
  external variant's `tc.r.ΔReplacer()` resolves the internal pass's rename by object identity).
  Declaration renames in visitFuncDecl; every reference follows via convIdent's isMethod arm; the
  RecvGenerator ж-overloads follow from the emitted name. Guards:
  `TestTestVariantPinsProductionTypeAgainstTestMethodCollision` +
  `TestTestVariantRenamesTestMethodShadowingDotImportedFunction`, both discriminating (neutered-fix
  runs fail on the mapped symptoms). CNR byte-identical ×402; utf8 re-validates 14/14, git-clean.
- **Sort's wall after B2/B9 (probe 2026-07-17):** ALL 14 CS1501 GONE — and so are the 5
  method-group CS1503 sites wave 2 attributed to B10: they were B9-DOWNSTREAM (the wrong method
  group failing delegate conversion; another Roslyn-masking layer — B10's real count was measured
  with B9 present). Remaining: exactly **3 × CS1503** — sort_test.cs:769 `long`→`nint` (B7a
  exactly) + search_test.cs:49 `double`→`nint` (B7a float sibling) + example_keys_test.cs:29 `By`
  → raw `Func<ж<Planet>, ж<Planet>, bool>` (the one true B10 site). Runtime rows unreached; dir
  fully restored after measurement.
- **Strings' wall after B2/B9 (probe 2026-07-17):** B2's CS0102/CS0246 GONE — strings now BUILDS
  and runs through the test host end-to-end (first time). Wall moved to the runtime rows as
  mapped: C# host runs 72 → **23 pass / 7 fail / 42 infrastructure-error**, the infra bucket
  dominated by the R1 cctor cascade (`slice array reference is null` via `NewReplacer` in the
  `strings_test_package` cctor — the `[]T(nil)` signature). R1–R4 remain the next chip's scope;
  dir fully restored after measurement.
- **B10 + B7-family remainder: FIXED / RE-BUCKETED (2026-07-17 late, worktree branch
  `claude/pensive-torvalds-58cda0` — coordinator gates the merge; base master `b642642a5`).**
  Four defects fixed, one re-bucketed:
  - **B10b FIXED** — the composite-literal walk gains the MIRROR arm the call-site rule already
    had: a STRUCTURAL func-type field receiving a value that renders as a named delegate wraps in
    the synthesized structural delegate (`by: new Func<ж<Planet>, ж<Planet>, bool>(by)`,
    example_keys_test's planetSorter). Guard: `NamedFuncTypeStructuralField` (method-group field
    control stays bare).
  - **B7a-family `long`→`nint` (sort_test.cs:769) FIXED** — a NEW shape, not B7a exactly: the
    whole value of `maxswap: 1<<31 - 1` fits int32 (no whole-expression fold), but the untyped
    inner shift folds to a bare `2147483648L`, widening the rendering to `long`; neither the
    argument path nor `nativeIntConstCastType` (whole-value-out-of-range gate) narrows it.
    `convBinaryExpr` now wraps the emission itself: `(nint)(2147483648L - 1)`, shape-restricted
    to folded OPERATOR operands (`maxInt - maxInt` wrapper refs stay unwrapped). Guard:
    `NativeIntWideConstElement` extension.
  - **Float-const `double`→`nint` (search_test.cs:49) FIXED** — integer contexts now propagate
    through `markUntypedConstContexts` (division excluded: exact-rational vs truncating `/`), and
    a float literal under an integer context renders its exact integer form: `1e9 - 7` →
    `1000000000 - 7`. Guard: `NativeIntWideConstElement` extension.
  - **B7b-GAP (bytes_test.cs:1160) FIXED** — `returnArmKeepsUntypedWrapper` widens the B7b
    predicate to constant paren/unary/binary arms containing a named untyped-const ref (bytes
    TestMap's `return utf8.MaxRune + 1`), unless a constant fold rewrites the arm to a plain
    literal. Guard: `FuncLitUntypedConstReturn` extension (both must-stay-plain controls green).
  - **B10a RE-BUCKETED into B9** — root-caused against the real emission: all five method-group
    CS1503 sites pass `Sort`, and `sort_test_package` declares the converted `By.Sort` as
    `public static void Sort(this By, slice<Planet>)`, so C# simple-name lookup binds the
    enclosing class's method group and never reaches the `using static` production `Sort` —
    `Stable` ×6 and `Heapsort` ×1 at the SAME `func(Interface)` helper parameters convert
    cleanly, proving the method-group machinery itself is sound. These are the argument-position
    manifestation of B9's dot-import shadowing (CS1503 instead of CS1501); the B9 pinning fix
    clears all 19. No separate converter defect exists.
  - **Post-fix probe walls:** sort = exactly 19 errors, all `Sort` shadowing (14 CS1501 + 5
    CS1503 = B9); **bytes BUILDS CLEAN (0 errors)** and its differential RAN — the C# host dies
    in the `go.bytes_test_package` type initializer ("The type initializer for
    'go.bytes_test_package' threw an exception", exit 2), so every included test reports
    `Go="pass" C#=""` — the expected R-row wall (R1 `[]T(nil)` et al.); runtime rows remain
    untouched per the chip split.

## Sort's first full differential (2026-07-18, master `9c620008b`)

Sort BUILDS clean and runs the complete differential: **53/63 included tests agree with
`go test`**. Every divergence is root-caused and owned:

| Bucket | Tests | Root cause | Owner |
|---|---|---|---|
| runtime_rand stub (R3) | 7 (CountSortOps, CountStableOps, HeapsortBM, SortBM, SortLarge_Random, Stability, StableBM) | math/rand/v2 `runtime_rand` PartialStub NotImplemented | R1-R4 chip |
| Embed-override dispatch | 2 (ReverseSortIntSlice, Float64s) | dispatch through `reverse{Interface}` does not call the overriding `Less` — reverse sorts ascend; NaN order breaks | dispatch chip |
| reflectlite Swapper NRE | 1 (TestSlice) | nil `ж` deref at `abi.Kind` via reflectlite (R5 family) | dispatch chip |

**R14 (FIXED, master `9c620008b`): `--json` now implies `Verbose()`** — `go test -json` implies
`-v` (cmd/go passes `-test.v`), so the Go side of every differential runs verbose; the host
mirrored `false`, making every Verbose-gated test (sort's countOps pair) a guaranteed skip-vs-pass
mismatch. Post-fix those tests RUN and surface their true wall (R3) — the honest progression.
utf8 unaffected (still 0-skipped both sides).
- **R1 + R2 + R3 + R4: FIXED** (2026-07-17, worktree branch `claude/youthful-newton-4156e6` —
  coordinator gates the merge; no converter change, so CNR is trivially byte-identical). R1:
  golib `slice<T>` T[]-taking ctors map a null source to the nil slice (`this = default`) per Go
  `[]T(nil)`; the bounded forms allow it only while every index stays at zero (Go's legal
  `nil[0:0]`). R4: golib `builtin.len(string)` returns `Encoding.UTF8.GetByteCount` — Go counts
  UTF-8 bytes, `.Length` counted UTF-16 chars; audit of every other raw-`string`-accepting golib
  API found no second instance (`@string`/`sstring` transcode at construction; remaining `string`
  params are format/message/parse-ASCII only). Guards: `NilSliceConversion` +
  `StringLenUtf8Bytes`, both discriminating (reverted-fix runs fail Output: exit-2 crash / UTF-16
  counts). R2: whole-file hand-owned `internal/godebug/godebug.cs` (`[module: GoManualConversion]`,
  old output preserved as `godebug.cs.auto`, `godebug_impl.cs` subsumed/removed) — parses $GODEBUG
  once on first use, "" unset default, unlisted-name panic kept; the scout's "atomic pointer" nil
  deref is actually the generated promoted-field box of the embedded `*setting` treating its held
  nil pointer as a nil DEREFERENCE even for the populating assignment (`Setting.g.cs`
  `setting => ref Ꮡʗsetting.Value` → PanicException) — a latent TypeGenerator/ж defect for ALL
  post-construction embedded-pointer assignment, worth its own gen-gated chip. R3: `rand_impl.cs`
  companions for math/rand AND math/rand/v2 (the failing site was v1's rand.cs:375; both carry the
  same linkname) — `runtime.rand` on `Random.Shared`, correctly non-deterministic. Probe evidence
  (small driver against the built assemblies): `godebug.Value()` = "" unset / "0" under
  `GODEBUG=randautoseed=0`, and under that setting v1 `Int63()` returns 5577006791947779410 —
  Go's canonical first Seed(1) draw, proving the GODEBUG-routed deterministic path end to end;
  unset, v1/v2 draw random values through the new hook. R5–R13 remain.

## 🏁 SORT VALIDATED (2026-07-18) — package #2

With R1-R4 merged (`32638f729`), sort's full differential went green:
**`Validated 63 tests against go test (1 skipped identically on both sides, 46
disclosed-unsupported declarations excluded).`** — every included test agrees, skip-parity holds
(TestSearchWrappersDontAlloc), and the converted test sources are committed beside the production
code per the validated-package policy. The path consumed, in order: B1, B2b, B2c, B3, B4/B5, B6
(+Skip), B7a (+fold-widening +float-context), B7b (+const-expr arms), B8, B9, B10, AllocsPerRun,
CoverMode census, /vN imports, --json-implies-Verbose (R14), array-copy cloning, IEEE float
equality, the reflectlite mini-bridge, the gen nil-embed fix, and R1-R4. Bytes/strings same-day
attempts ran deep and reported their R5-R13 tails honestly (bytes: DeepEqual/MakeNoZero/nil-empty
classes; strings: Builder-allocs/Map/Finder classes) — next wave's work order.

- **R5: FIXED (2026-07-18, worktree branch `claude/adoring-kirch-cd66c2` — coordinator gates the
  merge; base master `f999c8f78`).** The reflect bridge gains DeepEqual: the converter skips ONLY
  `deepValueEqual` (`manualConversionFuncs["reflect"]` — `DeepEqual` stays auto, its body only touches
  the bridged ValueOf/Type/AreEqual), and `reflect/deepequal_impl.cs` re-implements the recursion
  arm-for-arm over the bridge's BOXED values: elementwise arrays/slices (+`[]byte` span fast path),
  nil-vs-empty slice from the REAL backing (`m_array` via cached reflection — public `Source` is a
  detached copy), struct fields via goStructFields, maps key-by-key through the backing Dictionary
  (same-map identity short-circuit), pointer identity = ж-box reference equality, NaN ≠ NaN, funcs
  never equal unless both nil; cycle detection mirrors Go's hard()/visited on managed-identity pairs
  (self-referential structures terminate). The committed `deepequal.cs` was regenerated from a FULL
  -stdlib reconvert (filtered reconverts derive wrong cross-package names) — the only reflect drift
  was the placeholder swap. Guard: `DeepEqual` behavioral test (30 cases output-compared vs `go run`;
  discriminating — the pre-fix reflect crashes it at the first slice compare, exit 2 NRE). Its
  `Directory.Build.targets` redirects the emitted `core\reflect` ref to `go-src-converted\reflect`
  (baseline has no reflect; the targets file survives csproj regeneration — the Performance-suite
  pattern). golib print/println now render bools gc-style (`true`/`false`) — surfaced by the guard's
  output compare. **Differentials: bytes 14 → 12 mismatches (TestSplit + TestSplitAfter flip to
  agreement), strings 15 → 13 (same two flip)** — the four R5 tests exactly; remainder untouched.
- **Shared infrastructure-error diagnosis (the six-test class, from the real stacks):** strings
  TestSplit/TestSplitAfter + bytes TestSplit/TestSplitAfter are R5 (identical stacks:
  deepequal.cs:74 → unsafe.cs:261 `Pointer.op_Implicit` on the never-populated `v.ptr`). Strings
  **TestReplacer + TestWriteStringError are R8, not R5**: TestReplacer NREs at replace.cs:83
  (`r.replacements[o]` — the `[256][]byte` field left zero-value by the composite literal, so the
  default `array<T>`'s null backing faults at array.cs:134 — the same class as
  TestFinderCreation/Next), and TestWriteStringError is R8-DOWNSTREAM: build's panic runs under
  `sync.Once` whose deferred done-store marks the shared package-level Replacer built, so the next
  `Do` no-ops and `r.r` is still nil at replace.cs:109. Fixing R8's zero-value array backing clears
  both (plus the Finder pair).
- **Differential-run note:** strings' C# host needs `-test-timeout` headroom — TestCompareStrings
  legitimately takes minutes through the golib @string paths (the 2m default killed the host mid-run,
  reporting everything after TestCompareIdenticalString as `C#=""`); 8m completes. Slow ≠ hung.

## Cross-cutting lessons

- **Capability-excluded tests still compile** — exclusion gates the run registry, not emission; a broken
  emission inside an AllocsPerRun-excluded test blocked all of bytes.
- The sweep's environment-dependent error sets were both shadows of B1/B2b, not an abi problem.
- Minor: the tests-csproj template's later `<OutDir>` override defeats its own
  `BaseOutputPath=bin\tests\` (exe lands in bin/Debug/net9.0/) — cosmetic; align when touching the template.
