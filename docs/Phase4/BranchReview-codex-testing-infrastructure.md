# Branch review: `codex/testing-infrastructure` (Phase-4 test-conversion slice)

> Adversarial review performed 2026-07-16 (worker WP-6, review-only wave). All line numbers cite the
> branch commit `097c94d70` unless marked `master@9844abd0f`. Every claim below was re-verified against
> the actual branch content, current master, and the Go 1.23.1 GOROOT copy at `C:\Program Files\Go\src`
> — nothing is taken from the branch's own docs or from prior session notes on faith. This document is
> the kickoff brief for the dedicated Phase-4 port session (cut from post-wave master).

## 1. Branch facts & verdict

| Fact | Value |
|---|---|
| Branch / commit | `codex/testing-infrastructure`, single commit `097c94d70` |
| Date / title | 2026-06-28, "Testing: add converted Go package test infrastructure" |
| Size | 24 files, +2863/−14 |
| Merge-base | `45636c2f9` — **953 commits behind** master (`9844abd0f` at review time) |
| Never merged; never rebased | Yes — predates Phase-3 completion, `-recurse`, NuGet, sstring, canonicalAlias |

**Verdict: keep-with-rework — port, never merge.** The architecture is right and matches
`docs/TestingInfrastructureRequirements.md` in shape: `packages.Config{Tests:true}` variant separation,
production-source recompilation (no self-`ProjectReference`), a hand-owned `go.testing` runtime with real
`FailNow`/`Cleanup`/`Parallel`/`TestMain` semantics, a digested manifest, capability gating, and a
`go test -json` differential oracle. The C#-side runtime (`TestHost.cs`, `testing.cs`, the MSTest guards,
the `ConvertedTestHarness` fixture) ports nearly verbatim — its golib API surface (`ж<T>`, `PanicException`,
`RuntimeErrorPanic.TryAsPanic`, `@string`) still exists on master. The Go-side converter integration is
**unmergeable as-is**: it duplicates converter package-state machinery that has since grown by ~13 globals
and 6 Visitor/FileEntry fields (nil-map panics guaranteed), and it patches a `visitImportSpec.go` that
master has completely rebuilt. Beyond staleness, this review found several genuine design defects (skip
parity, Examples omission, package-granular capability gating that blocks every real candidate package but
one) that the port must fix before §12.8 can close. Rough salvage estimate: ~60% of lines port verbatim
(runtime + fixtures + unit tests), ~35% re-derive (converter integration), ~5% discard (doc edits).

## 2. Keep / re-derive / discard inventory (all 24 files)

**Keep (port as-is or near-verbatim):**

| File | Disposition |
|---|---|
| `src/core/testing/TestHost.cs` (758 ln) | **Keep.** Registry/reporter/options/runner/execution. Verified golib APIs intact on master. Apply §3 fixes (ownership crash channel, `-count` parallel release, thread-pool starvation). |
| `src/core/testing/testing.cs` (134 ln) | **Keep.** `[GoPackage("testing")]` shim, `struct T/M`, both `ref T` and `ж<T>` overloads (the ж overloads are deliberate — `params Span<object>` is ref-like, see its comment at :104-106). Add `testing.Short()`/`Verbose()` in the port (see §3-F4). |
| `src/core/testing/testing.csproj` | **Keep**, but the `..\fmt\fmt.csproj` (baseline-stub fmt) reference forces the mixed-tree ruling in §3-F17 before any real package converts. |
| `src/Tests/Behavioral/BehavioralTests/TestingRuntimeTests.cs` (7 `[TestMethod]`) | **Keep.** Guards cleanup-LIFO-after-Fatal, parallel barriers, exit codes (error/fatal/panic/runtime-panic/skip), TestMain gating, Setenv/TempDir/fixture isolation, JSON+JUnit emission, hierarchical `-run` filter. |
| `src/Tests/PackageTests/ConvertedTestHarness/*` (value.go, value_test.go, external_test.go, go.mod, testdata/message.txt, .gitignore) | **Keep.** End-to-end fixture: unexported access, external `package p_test`, TestMain, duplicate parallel subtests, cleanup, invalid `Testlower` non-registration. Note: `testdata/message.txt` is never **read by a Go test** — fixture reads are only proven by the MSTest side; add a read to the Go fixture at port time (§4, req §10). |
| `src/Tests/PackageTests/README.md`, `src/Tests/PackageTests/.gitignore` | **Keep** (refresh the command example). |
| `src/go2cs/testConversion_test.go` (9 unit tests) | **Keep.** Ports with minor API drift. Note `TestEligibleTerminalResultsExcludeUnsupportedTestKinds` (:66-83) fabricates a manifest `Kind:"example"` that the discovery code never emits — evidence for finding F2, keep the test but make discovery actually emit it. |

**Re-derive against current master (the five converter integration points):**

| # | Branch change | Current master integration target (verified) |
|---|---|---|
| IP-1 | `main.go` +43: `Options{convertTests, testAction, testTimeout, testPackagePath, testPackageName}`, 3 flags, validation, dispatch | `Options` struct now also carries `recurse`/`mainModulePath`/`nugetRefs` (`master main.go` ~:604-620 region); flag registration is `main.go:644-658` (note: the merge-base `-parallel` flag is **gone**, `-recurse` is a custom `flag.Var`); dispatch is `main.go:757-799` — the `-tests` hook must compose with the `options.recurse` branch at `:787` (recommend: `-tests` and `-recurse` mutually exclusive initially, `log.Fatal` on both). The end-of-`processConversion` hook mirrors branch `main.go:1014-1019`. |
| IP-2 | `stdLibConverter.go` +42: per-package status collection, `go2cs_test_packages.json` aggregate, `executeTestAction` in `convertPackage` | `convertAllPackages`/`convertPackage` still exist (`processConversion` call at `stdLibConverter.go:570`) but the function has grown (benchmark file, solutionGenerator, 2026-07-15 hand-owned auto-include). Re-derive the ~4 insertion blocks; decide whether generated `.tests.csproj` entries join the generated `.slnx` (they should NOT initially — the stdlib slnx must stay byte-canonical per the VS-folders rules). |
| IP-3 | `visitImportSpec.go` +9: skip `loadImportedTypeAliases` for the package under test; rewrite `importPath` to `packageNamespace.<name>_package` | Master rebuilt this function entirely (`visitImportSpec.go:107-184`): `canonicalAlias` via `packageUsingAlias` (:132), `canonicalAliasImported`/`importAliasesEmitted`/`importPathAliases` bookkeeping (:151-158, :161-179), blank-import comment handling (:141-149), GOROOT-vendored resolution (:118-120). The self-import override must (a) skip the alias load, (b) rebind the canonical alias target to the local partial class, (c) keep the bookkeeping sets consistent so `collectTypePackages`/`getTypeName` short-forms resolve, and (d) **handle the dot-import form** — `utf8_test.go` does `. "unicode/utf8"` (`using static` path at :139-140), which the branch override happens to cover only because the importPath substitution precedes the branch on `alias == "."`; verify explicitly. |
| IP-4 | `csproj-template.xml`: `*._test.cs` → `*_test.cs;package_test_info.cs;go2cs_test_host.cs` | Master still has the old `*._test.cs` at `csproj-template.xml:107` — the broadening re-applies cleanly and is *required* by req §3. Also port-fix: `writeTestProject` (branch `testConversion.go:732-810`) hardcodes an entire csproj instead of deriving from the template — the template has since grown NuGet/README/`$(CompilerGeneratedFilesOutputPath)` machinery that the hardcoded copy silently misses. Derive the test project from the template (or a sibling template) at port time. Also `$(USERPROFILE)` in the fallback go2csPath is Windows-only. |
| IP-5 | `testConversion.go` Visitor construction (:395-403), `FileEntry` construction (:319, :338), `resetPackageState()` (:438-463, 22 globals) | All three are stale copies of master machinery: master's Visitor literal (`master main.go:1044-1071`) has **4 new required fields** (`referencedForeignPackages`, `canonicalAliasImported`, `importAliasesEmitted`, `importPathAliases`) plus `sstringEligible`/`sstringConvExprs` — the branch literal leaves them nil → **panic on first `.Add`/map write**. Master's `FileEntry` (`master main.go:951-957`) initializes the two sstring maps the branch omits. Master's per-package reset (`master main.go:841-899`) now covers **~35 globals** incl. `constraintProxies`, `adapterClassImplementations`, `conversionPackageUsings`, `packageMovedInitVars/Methods`, `packageImportAliasRenames`, `packageChildNamespaces`, `packageQualifiedNamespaces`, `packageImportLeadingSegments`, `packagePublicizedLiftedTypes`, `packageCaptureModeBoxIdents`, `packageManualTypeNames`, `packageDoc` (`extractPackageDoc`), and the `importPackageDirs` closure walk. **Port rule: extract master's reset/FileEntry/Visitor construction into shared helpers used by BOTH `processConversion` and the test path** — the branch duplicated them and they drifted; the extraction is a pure refactor gated by byte-identical CNR ×385, and it makes this class of drift structurally impossible. Helper signatures verified compatible: `getProjectName(dir, options)` (`importOperations.go:26` — takes a directory despite the param name), `getImportPackageInfo([]string, Options)` (`importOperations.go:194`), `performEscapeAnalysis`/`collectAddressedGlobals`/`collectCaptureModeMethods`/`collectPublicizedTypes`/`performNameCollisionAnalysis`/`performGlobalVariableAnalysis`/`resolveDynamicTypeMarkers`/`containsManualConversionMarker`/`needToWriteFile`/`getSanitizedImport`/`getSanitizedFunctionName` and constants `PackageSuffix`/`PackageInfoFileName`/`TypeAliasDot` all still exist with matching signatures. |

**Discard (do not port):**

| File | Why |
|---|---|
| `CLAUDE.md` edits | Describe the 2026-06 world (59 behavioral tests, `.sln`, Phase 3 in progress). Write fresh orientation text post-port. |
| `docs/Architecture.md` edits | Salvage the two-paragraph pipeline description as prose *input*, rewrite against the ported code. |
| `docs/Roadmap.md` edits | Marks Phase 4 "in progress" against a pre-Phase-3 roadmap; the current roadmap has the full Phase-3 iteration log. Rewrite. |
| `docs/TestingInfrastructureRequirements.md` Status edit | Claims "implementation in progress" for a branch that never merged. Update Status only when the port lands. |
| `src/go2cs.slnx` +2 lines, `BehavioralTests.csproj` +1 line | Trivially re-derived; the current `.slnx` format/content churned (and the stdlib slnx generation is byte-canonical — hand-edit carefully or via the Edit tool, never `sed`/`awk`, per the CRLF rule). |

**Documented for WP-3 (do not port here):** the branch fixes the GOOS/GOARCH loader-env bug — merge-base
(and **current master**, verified at `master main.go:816`) appends ONE garbage env string
`fmt.Sprintf(`\`"GOOS=%s", "GOARCH=%s"\``, …)` (literal quotes + comma inside a single entry), so the
loader never receives real GOOS/GOARCH; the branch splits it into two proper entries (branch diff,
`main.go:506-509`). The newer `-recurse` path already does it correctly (`moduleConverter.go:122`), which
is independent confirmation of the correct form. `testConversion.go:113-120` contains the same correct
pattern. This fix is a **precondition** for `-tests` (build-constraint selection of `_test.go` variants
depends on the loader env) — the port session must confirm WP-3's extraction landed before anything else.

## 3. Adversarial design attacks (verified against branch code)

**F1 — Oracle: skip parity is treated as failure.** `compareGoAndConvertedTests` flags an error when
`goStatus != "pass"` even if both sides agree (`testConversion.go:1223`): a test that legitimately
`t.Skip`s in both Go and C# makes the package permanently `failing`. Real stdlib suites skip routinely
(platform gates, `-short` interactions). Fix: match on `goStatus == csStatus` for terminal states, with
skip==skip counted as matched (optionally require the skip to be disclosed in the comparison record).

**F2 — Oracle: Example functions are undiscovered AND undisclosed.** `discoverTestDeclarations`'s
signature filter requires exactly one parameter (`testConversion.go:523`), and there is no Example branch
(:531-545) — `func ExampleX()` never enters the manifest in any status. `go test` *runs* Examples with
`Output:` comparison; the eligibility filter (:1268-1284) then silently censors them from BOTH sides of
the comparison. A package can be labeled **validated** with dozens of executed-in-Go, never-run-in-C#
Examples and zero disclosure — a direct violation of req §2.7 ("never silently count as passing"), §4.4
("shall be reported as unsupported until their stage"), and §7's definition of validated. The branch's own
unit test (:66-83) papers over this by hand-fabricating a `Kind:"example"` manifest entry the converter
cannot produce. Scale: `strings` has 52 Examples, `bytes` 64, `sort` 17, `unicode` 14, `unicode/utf8` 16
(counted from GOROOT). Fix: discover `Example*` (zero-param) declarations, emit `kind:"example",
status:"unsupported"` until Phase 4D.

**F3 — Oracle: fuzz seed-corpus runs are disclosed but invisibly dropped.** Fuzz targets are declared
unsupported (:542-544 — disclosed, good), but `go test` executes each fuzz target's seed corpus as
ordinary tests; the eligibility filter removes them from the Go results (:1268-1284), so a seed-corpus
regression is invisible. Acceptable under §4.4 staging *because it is disclosed*, but the port should note
it in the comparison record ("N disclosed-unsupported declarations excluded") rather than filtering
silently. None of the first-wave candidate packages has fuzz targets (verified), so this is not blocking.

**F4 — Capability gating is package-granular and blocks every candidate package except one.**
`discoverTestingCapabilities` (:585-631) records `T.`/`M.`-receiver members and package-level `testing.X`
*functions*; any requirement outside the 20-entry supported list (:575-583) marks the whole package
`infrastructure-blocked` (:1085-1094). Empirically (GOROOT 1.23.1, `_test.go` scans): `strings` requires
`testing.Short`+`testing.AllocsPerRun`+`testing.CoverMode`, `sort` requires `testing.Short`+
`testing.Verbose`, `bytes` requires `testing.Short`+`testing.AllocsPerRun`, `unicode` requires
`testing.Benchmark` — **all four prompt candidates are blocked as converted**. `unicode/utf8` is the only
candidate whose test bodies use nothing outside the supported list (sole T-method: `t.Errorf`). Fixes, in
order of value: (a) implement `testing.Short()`/`Verbose()` (trivial: host flag, default false = `go test`
default); (b) attribute capabilities **per test declaration** (walk each test's body/callees) so one
`AllocsPerRun` test blocks itself, not its 80 siblings — package-level blocking contradicts §11's staged
design; (c) `testing.AllocsPerRun`/`CoverMode`/`Benchmark` stay unsupported until Phase 4D.

**F5 — Capability gating counter-attack: B/F receiver usage is invisible — by luck, not design.** The
receiver filter admits only `T`/`M` (:614), so `b.N`, `b.ReportAllocs`, `f.Add`, `f.Fuzz` inside
benchmark/fuzz bodies add nothing to the required set. Today that is what saves benchmark-heavy files from
self-blocking (benchmarks are excluded from execution anyway), but it means a *supported-kind* test that
calls a helper taking `*testing.B` would sail through ungated. When per-test attribution (F4b) lands,
scope the scan to included tests' call graphs and drop the receiver filter.

**F6 — Silent-pass channel: the comparison is filtered by the manifest on BOTH sides.**
`eligibleTerminalTestResults` (:1268-1284) removes from the Go results every test the manifest didn't
declare eligible. Discovery and comparison therefore share a single point of failure: a discovery bug
self-censors (exactly how F2 stays invisible). The port should add an independent census gate — compare
manifest declarations against `go test -list '.*'` output (or against all `Test`-kind entries in the go
-json stream *before* filtering) and fail on any name the manifest cannot account for.

**F7 — Manifest digest/staleness gaps.** What invalidates (verified in `testInputDigest`, :941-979): any
`*.go` content/size change in the package dir; recorded fixture content (incl. `testdata/`); `*_impl.cs`
companions in the output dir; targetPlatform; Go toolchain version; converter revision. What SHOULD
invalidate but does not: (a) **a newly added `testdata` file** — `validateTestManifest` (:1135-1152)
recomputes over `manifest.Fixtures` (the OLD list), and only `*.go` files are globbed fresh, so a new
fixture leaves the digest matching; (b) **converter options other than `-platforms`** — `-uco`, `-var`,
`-indent`, `-comments` all change output but not the digest (:977); (c) the hand-owned **testing runtime
and golib themselves**. Also: `converterRevision()` (:1007-1027) prefers hashing the *on-disk converter
source directory* captured by `runtime.Caller(0)` — on a dev machine this reflects current sources, not
the running binary (a stale `go2cs.exe` reports a fresh revision — the exact stale-binary false-green
failure mode the project has already been burned by). And `writeNoTestsManifest` swallows the digest error
(:991). Port fixes: glob testdata fresh at validate time, fold the options struct and a golib/testing-
runtime digest into the hash, and prefer the executable-hash path over the source-dir path.

**F8 — TestHost thread-ownership: a Go-tolerated misuse becomes a process crash.** `EnsureOwner`
(`TestHost.cs:744-748`) throws `InvalidOperationException` when `FailNow`/`SkipNow`/`Run`/`Parallel`/
`Setenv` is called off the test's thread. Go documents the same restriction but *tolerates* violations in
practice (cross-goroutine `t.Fatal` is a common stdlib-adjacent pattern); in the C# host the throw
surfaces inside a converted-goroutine thread where — unless golib's goroutine wrapper catches it — an
unhandled exception kills the process before results are written. Req §6.1 demands "a clear infrastructure
failure rather than silently terminating unrelated work". Port: verify golib's `GoFunc` catch translation
for non-panic exceptions, and route ownership violations to `InfrastructureFailed` on the owning execution
instead of throwing into foreign code. (`Fail`/`Error`/`Log` correctly have no owner check — Go allows
them cross-goroutine.)

**F9 — Cleanup-LIFO edge cases.** Verified correct: LIFO order, runs after Fatal/panic/skip, cleanups
registered during cleanup run (the pop-loop re-checks, :695-728), cleanup panic → infrastructure-fail with
log. Divergences: `t.FailNow` *inside* a cleanup is swallowed (`TestAbortException` catch at :711-713 —
Go records the failure); `Cleanup()` after completion is not guarded (Log is, :575); a cleanup
`Directory.Delete` failure from `TempDir` maps to `infrastructure-error` where Go reports plain `fail`
(status-string mismatch in the oracle for that edge).

**F10 — Parallel semantics.** Top-level parallel tests are released only after the **entire `-count`
loop** (`TestHost.cs:418-438`) — Go releases per iteration, so `-count N` interleaves iterations. Real
risk at stdlib scale: `WaitForSerialBoundary` blocks synchronously and every parallel test parks a
thread-pool thread on `m_parallelGate` — with dozens-to-hundreds of parallel subtests, pool injection
(~1 thread/s) can push a suite past the 10-min host timeout (and past the converter's 2-min default child
timeout, :1101-1110 — note the two defaults disagree). Go's `-parallel` cap (GOMAXPROCS) is also
unimplemented — C# runs ALL parallel tests concurrently. Port: dedicated threads or a release-per-
iteration + capped-concurrency scheme. Subtest naming (`NextSubtestName`, :630-640) matches Go's
space→`_` and `#%02d` dedup; control chars diverge (U+FFFD vs Go's `\xNN` hex escape) — cosmetic until a
real suite hits it.

**F11 — Panic-in-test vs panic-in-cleanup vs TestMain exit codes.** Panic in test body: `PanicException`
and runtime-error panics → log + fail (:656-665) — matches Go's fail-with-panic-output (Go also aborts the
whole run on panic; C# continues — acceptable divergence, terminal statuses still compare). Panic in
cleanup: infrastructure-fail (F9). TestMain: `os.Exit(m.Run())` in converted code will call golib's exit →
`Environment.Exit` → **`--result`/`--junit` files are never written** (stdout JSON events already flushed,
so the comparison itself survives; the artifacts are lost). TestMain-returns-without-Run → exit 0, and
returns-after-Run → `runner.ExitCode` (:293-301) — both match Go 1.15+ semantics. No fixture covers
TestMain nonzero/os.Exit (req §10 gap).

**F12 — Environment asymmetry between the two oracle sides.** The C# host pins InvariantCulture and
TZ=UTC (`TestHost.cs:245-247`); the Go baseline runs with the machine's inherited locale/TZ
(`runCommandWithTimeout` adds only GOOS/GOARCH/go2csPath, :1286-1304). A locale/TZ-sensitive test can
legitimately diverge. Also the GOOS/GOARCH passed to `go test` means a non-native `-platforms` value
produces a Go binary that cannot run — cross-target comparison is silently impossible (fine for now;
document it). Correction to the prior findings: the **run-results file** `go2cs_test_results.json` DOES
record dotnetRuntime/culture/timezone/shuffleSeed (:320-339); it is the *manifest* that lacks them, and
the Go side that goes uncontrolled.

**F13 — Stdout interleaving in `--json` mode.** Converted code writing to stdout (Go tests do this)
interleaves raw text with the host's JSON event lines. `terminalTestResults` (:1253-1266) skips unparseable
lines, so it degrades gracefully — but any printed line that happens to parse as JSON with `test`/`action`
keys is accepted as an event. go test wraps all test output in Output events; the C# host does not capture
`Console` writes at all (t.Log output is only attached to the terminal event). Low probability, cheap
fix (redirect Console.Out around test execution), worth doing before big suites.

**F14 — csproj exclusion breadth: SAFE, verified.** Go cannot have a *production* source ending
`_test.go` (the go tool defines that suffix as test-only), so no legitimate production `.cs` matches
`*_test.cs`; `package_test_info.cs`/`go2cs_test_host.cs` are exact names. Hand-owned `*_impl.cs` never
matches. The change is affirmatively required by req §3 (the old `*._test.cs` pattern matches nothing the
converter emits today). One adjacent trap found: `productionCSFiles` (:834-851) excludes `*.g.cs` from the
test project's production set — if any package carries an on-disk generated companion (the
`timeLocation.g.cs` pattern), the test assembly loses it; verify against the overlaid tree at port time.
Second adjacent trap: dependencies whose `PackageInfo.Err != nil` are **silently dropped** from the test
project's references (:741) — later CS0246s with no causal diagnostic; fail loudly instead.

**F15 — Reference-tree mixing (the biggest unproven architecture risk).** Three compounding hazards, none
exercised by the synthetic fixture: (a) `resolveTestProjectReference` (:812-832) resolves each dependency
to `core\<pkg>` if present else `go-src-converted\<pkg>` — but go-src-converted projects internally
reference go-src-converted siblings while `testing.csproj` references baseline `core\fmt` → one test build
can transitively contain BOTH trees' `namespace go` partial classes (exactly the collision CLAUDE.md
forbids). (b) Test-only deps that themselves import `testing` (`internal/testenv` — used by strings, sort,
bytes suites) reference the **auto-converted** `go-src-converted/testing` (exists on master, verified),
colliding with the hand-owned shim's `[GoPackage("testing")]`. (c) The test assembly recompiles the
production sources while dep assemblies reference the production package's *assembly* (utf8 ← strings,
bytes) → CS0436 (source wins) plus two runtime copies of package state — benign for stateless packages,
wrong for stateful ones. Port ruling needed up front: resolve ALL stdlib deps from the overlaid
go-src-converted tree only (golib + the testing shim being the sole `core` references), rebuild the shim's
fmt dependency accordingly (or make the shim fmt-free by inlining its two Sprint helpers), and decide the
self-duplication policy before the first stateful package.

## 4. Requirements coverage matrix (`docs/TestingInfrastructureRequirements.md`, master copy)

| Section | Status | Note |
|---|---|---|
| §1 Purpose | met | Design targets the success statement verbatim. |
| §2.1-2.6 principles | met | Recompile-production / colocated / clean production artifacts / go-packages metadata / thin runtime / process isolation all implemented. |
| §2.7 honest exclusions | **partial** | Benchmarks/fuzz disclosed; **Examples entirely undisclosed** (F2); capability blocking package-granular (F4). |
| §2.8 determinism | met (likely) | `needToWriteFile` everywhere, sorted collections throughout; no determinism golden exists (§10 gap). |
| §3 layout | met | Normative names all match; template exclusion broadened as §3 explicitly requires; explicit-include `.tests.csproj` with `EnableDefaultCompileItems=false`. |
| §4.1 opt-in mode | met | `-tests`/`-test-action`/`-test-timeout`; stdlib filtering via `-stdlib` composition; convert-only and run-without-reconvert paths work. |
| §4.2 variants | met | Production/internal/external separation (:290-307); metadata consolidation (:633-692); self-import binding (IP-3). |
| §4.3 file eligibility | partial | Loader-driven `CompiledGoFiles` ✓, platform-excluded recorded with reason ✓; no per-file `failed-to-convert` status (a convert panic aborts the whole package instead). |
| §4.4 discovery | **partial** | Typed-AST signatures ✓, invalid names rejected ✓ (fixture-proven), benchmarks/fuzz reported unsupported ✓; **Examples not reported** (F2). |
| §5.1 host + orchestrator | partial | Host/registry/compat assembly ✓, no assertion rewriting ✓, `dotnet run -- --json` ✓; repo-level orchestration is only the `-stdlib` aggregate (`go2cs_test_packages.json`) — no CI sharding, no cross-package JUnit/TRX rollup (Phase 4C by design, but §5.1 asks for the orchestrator shape earlier). |
| §5.2 isolation & env | partial | Process + temp working dir + fixtures ✓, GOROOT never written ✓, timeouts kill children ✓; env recorded in the results file but not the manifest; **Go baseline env uncontrolled** (F12). |
| §6.1 first-tier API | met | Full listed surface implemented incl. Parallel barriers, LIFO cleanup, defers-unwind-first; goroutine-misuse handling is a crash channel not an infra-failure (F8). |
| §6.2 capability reporting | partial | Versioned capability list ✓, pre-compile gating ✓, never throwing-stubs ✓; B/F usage invisible (F5), package-granular blocking (F4). |
| §7 differential oracle | **partial** | Normalized events with all required fields ✓, five statuses all implemented ✓, no byte-comparison of logs/timing ✓; **skip-parity defect** (F1), Examples omission (F2), manifest-filtered census (F6). |
| §8 manifest | partial | All listed fields present incl. digest + revisions; staleness detection real but gapped (F7). |
| §9 Phase-5 interaction | deferred-by-design | Correctly out of scope for the slice. |
| §10 self-validation fixtures | **partial** | Covered: pass/nonfatal/fatal/panic/runtime-panic/skip, cleanup order, parallel barriers, duplicate names, unexported access, external test, deps, TestMain (return path), TempDir/Setenv/fixture isolation, filtering. **Missing:** timeout fixture, TestMain nonzero/os.Exit, build-tag + GOOS/GOARCH filename selection, deterministic-manifest golden, end-to-end infrastructure-blocked fixture (unit-test only), Go-side testdata read. |
| §11 4A | partial | All 4A machinery built; **exit criterion unmet** — zero stdlib packages validated. |
| §11 4B | partial | Subtests/parallel/isolation/comparison implemented; exit criterion (representative leaf packages) unmet. |
| §11 4C / 4D | not started | By design. |
| §12.1 single command | met | `go2cs -tests -test-action all …` (fixture-proven per branch docs). |
| §12.2 coexisting projects | met | Template exclusion + explicit test project. |
| §12.3 same-pkg + external + self-import | met | Fixture-proven on the synthetic package only. |
| §12.4 semantics fixtures | partial | Timeout + TestMain-exit fixtures missing. |
| §12.5 test-only deps resolve | partial | Mechanism exists; unproven against real deps; mixed-tree hazard (F15). |
| §12.6 comparable results, exclusions exposed | partial | F1/F2/F6. |
| §12.7 determinism + staleness | partial | F7. |
| §12.8 ≥3 validated stdlib packages | **unmet** | Zero. This is the port session's first proof. |
| §12.9 CI aggregation | partial | Per-package JSON/JUnit + stdlib aggregate; no CI wiring. |
| §12.10 roadmap gate | unmet | Branch's roadmap edit is discarded; re-record post-port. |
| §13 non-goals | respected | No assertion rewriting, no monolithic runner, no auto-converted-testing bootstrap. |

## 5. Port plan for the Phase-4 session (cut from post-wave master)

**Step 0 — preconditions.** Confirm WP-3's GOOS/GOARCH loader-env fix is on master (`main.go:816` must
match the two-entry form of `moduleConverter.go:122`); it is load-bearing for `-tests`. Branch from
post-wave master; keep `-tests` default-off so CNR stays byte-identical ×385 throughout.

**Step 1 — runtime first (no converter coupling).** Port `src/core/testing/` (3 files),
`TestingRuntimeTests.cs`, the `BehavioralTests.csproj` reference, and register `core/testing/testing.csproj`
in `src/go2cs.slnx`. Gate: solution builds; the 7 MSTest guards green. This proves the golib API fit in
isolation and gives the converter work a stable target.

**Step 2 — conflict-defusal order for the converter (IP-5 → IP-3 → IP-1 → IP-4 → IP-2):**
1. **IP-5 first:** extract master's per-package reset (`main.go:841-899`), FileEntry ctor
   (`main.go:951-957`), and Visitor ctor (`main.go:1044-1071`) into shared helpers consumed by
   `processConversion`. Pure refactor; gate = CNR byte-identical ×385 + full behavioral suite. This
   defuses the largest drift bomb before any test code exists.
2. Land `testConversion.go` on those helpers (delete its private `resetPackageState`), together with the
   defect fixes that change emitted artifacts: skip-parity (F1), Example declaration (F2), per-test
   capability attribution + `testing.Short`/`Verbose` (F4), digest additions (F7), loud dependency-
   resolution failure (F14b). Port the 9 unit tests; add ones for F1/F2/F4.
3. **IP-3:** re-derive the self-import binding inside master's canonicalAlias machinery, including the
   dot-import form (`. "unicode/utf8"` — required by the first-proof package). Guard with a harness
   fixture variant that dot-imports the package under test.
4. **IP-1 + IP-4:** flags/validation/dispatch (mutually exclusive with `-recurse` initially); template
   exclusion broadening; `writeTestProject` re-derived from the template.
5. **IP-2 last:** stdlib aggregation (only needed for batch runs; keep generated `.tests.csproj` out of
   the canonical stdlib `.slnx` initially).

Inherited validation gates for every converter step: `check-no-regression.ps1` byte-identical (tests off),
full behavioral suite (371), `go build` + converter unit tests, `ConvertedTestHarness` end-to-end
`-test-action all` green, and force-rebuild `go2cs.exe` before every measurement (stale-binary
false-green rule).

**Step 3 — FIRST PROOF (opens §12.8): `unicode/utf8`.** Convert `C:\Program Files\Go\src\unicode\utf8`
(Go 1.23.1 — matches the repo pin) end-to-end vs `go test -json -count=1`. Rationale, all verified against
GOROOT this review:
- **It is the only candidate not capability-blocked as converted** (F4): its 14 top-level tests use
  exactly one T-method (`t.Errorf`) and no package-level `testing.*` functions. strings/sort/bytes/unicode
  all require unimplemented `testing.Short`/`AllocsPerRun`/`Verbose`/`CoverMode`/`Benchmark`.
- Smallest surface: 2 test files, 827 lines, 21 benchmarks (excluded-disclosed), 16 Examples (exercises
  the F2 disclosure fix immediately), no TestMain, no skips, no subtests, no testenv/os/syscall/runtime/
  flag test deps.
- External `package utf8_test` with a **dot-import** of the package under test — exercises §12.3's
  self-import handling in its hardest form.
- Test deps (`bytes`, `strings`, `unicode`, `fmt`, `testing`) force the F15 mixed-tree ruling and the
  benign (stateless) version of the self-duplication diamond — the architectural questions §12.8 needs
  answered anyway, on the least dangerous package. It is also the requirements doc's own normative
  layout example (§3).

Then toward §12.8's three: **sort** or **strings** second (after `testing.Short`/`Verbose` + per-test
capability attribution; both have 1 `t.Run` subtest usage; both need an `internal/testenv` ruling — a
hand-owned mini-shim or per-test gating), **bytes** third for parallel coverage (4 `t.Parallel` tests;
also needs os/syscall operational). `unicode` is deprioritized (needs `testing.Benchmark` + `flag` at
test runtime + the stateful-diamond via sort/strings).

**Step 4 — close the silent-omission channel.** Add the `go test -list`-based census gate (F6) before
declaring any package validated; wire the repo-level aggregate + CI exit semantics (§5.1/§12.9) once ≥1
package validates.

## 6. Risks & unknowns

**Corrections to the prior findings this review was handed (verify-yourself results):**
- "Manifest env fields partial — missing culture/timezone/seed/.NET runtime": **half right.** The
  run-results file records all four (`TestHost.cs:320-339`); the *manifest* lacks them, and the deeper gap
  is that the Go baseline's env is uncontrolled (F12).
- "resets ~25 package globals": actually **22** (:438-463) vs ~**35** now required — the direction of the
  prior finding was right, the magnitude worse.
- The prior findings did not surface the four decisive new results of this review: (1) every prompt
  candidate package except `unicode/utf8` is **infrastructure-blocked** by the shipped capability list;
  (2) skip==skip reads as package failure; (3) Examples are undiscovered *and* undisclosed (silent-pass
  vector); (4) master's Visitor/FileEntry growth makes the branch's converter path **panic**, not merely
  drift. All prior file-level facts (counts, mechanisms, flag surface, ~1333/758 line sizes, 9 unit tests,
  7 MSTest tests) verified accurate.

**Open unknowns for the port session:**
- Does golib's goroutine wrapper translate a non-panic exception (the F8 ownership throw) into a contained
  failure or a process crash? Determines the urgency of the F8 fix.
- Do any overlaid go-src-converted packages carry on-disk `.g.cs` production companions that
  `productionCSFiles` would drop (F14)?
- CS0436 source-wins behavior for the utf8 diamond: expected benign for a stateless package, but the
  go2cs-gen analyzer's assembly-level attribute handling (GoImplement/GoTypeAlias interning) across the
  duplicate has never been exercised — related to the known cross-assembly analyzer gap from the
  `-recurse=nuget` work.
- `go test -json` inside `C:\Program Files\Go\src\...`: expected clean (writes only GOCACHE), but the
  harness account's ACLs there are unverified.
- Whether the solution generator and NuGet packaging paths need explicit guards to ignore
  `*.tests.csproj`/test artifacts when `-tests` output lands inside a tree they later scan.
- Determinism of the manifest across machines: paths are relativized and collections sorted, but
  `gitRevision(inputPath)` is empty for GOROOT inputs (fine) and machine-dependent for repo-local
  fixtures — decide whether `sourceRevision` belongs in the digest-relevant set or is informational only.
