# DESIGN — Recursive end-user conversion (app → third-party libs → stdlib)

> **Status:** proposal for review (no converter code written yet). Scope chosen with the user
> (2026-07-10): **build this as a real go2cs capability** (not a throwaway script), and for the
> first test **reference the already-converted stdlib** rather than reconverting it per app.
> Companion: the *NuGet stdlib* discussion (this thread / to be filed in [`Roadmap.md`](Roadmap.md)) —
> the stdlib reference this design points at is exactly what a NuGet `PackageReference` later replaces.
>
> **Update (2026-07-11):** the *staging* half is built and tested. `deploy-core.ps1` (two modes; see the
> "Deploying the core" section of [`../CLAUDE.md`](../CLAUDE.md)) stages the referenced stdlib + the
> `go2cs-gen` analyzer at `%GOPATH%\src\go2cs` and writes a root `Directory.Build.props` that pins
> `$(go2csPath)` to that root. That firms up the reference-resolution story below (§3.4/§3.5).
>
> **Update (2026-07-11, later):** the converter side is **built** — `-recurse` and phases **P1–P5 are all
> implemented, gated, and committed**. The `-recurse` feature works end to end on a real internet DAG
> (`fatih/color`); the remaining gaps to a *fully compiling* real-world build are per-package **converter**
> defects (Phase-4 territory), catalogued in the §5 *5b results*.

## 1. What we're validating

The normal end-user workflow the project targets (see the *end goal*: "use Go code in my C# project" /
"extend a Go app in C#") is:

1. A developer downloads a Go application from GitHub (`git clone` / `go install`).
2. It imports **one or more third-party libraries** plus the **standard library**.
3. They point `go2cs` at it and get a buildable C# solution.

The goal of this exercise is **process validation** — recursively discovering the reference graph,
converting in dependency order, and producing a solution that *builds* (compile success, not runtime
correctness). It is explicitly OK if not everything compiles yet; Phase 4 (the Go test grind) will
surface the residual per-package defects.

## 2. Current state — what exists, what's missing

There are **two disjoint drivers** today:

| Driver | File | Handles | Dependency graph? |
|---|---|---|---|
| `-stdlib` | `stdLibConverter.go` | The Go standard library only | **Yes** — full graph + topo sort |
| single project | `processConversion` (`main.go:703`) | One input package (dir/file) | **No** — converts only the input |

**What already works in our favor**

- **`processConversion` builds the transitive import closure.** `captureImportDirs` (`main.go:792-805`)
  walks `pkg.Imports` recursively into `importPackageDirs` (import path → `{Dir, Name}`), module-aware
  via `go/packages`. It's used today only for *reference wiring* and *type-alias resolution*, but it is
  exactly the closure a recursive converter needs.
- **`getProjectName` (`importOperations.go:26`) is already module-aware** — it reads the module path
  from `go.mod` and produces a dotted namespace (`github.com/fatih/color` → project
  `github.com.fatih.color`, namespace `go.github.com.fatih.color`).
- **A two-root reference/output convention already exists** in `getImportPackageInfo`
  (`importOperations.go:219-223`):
  ```go
  if isStdLib {
      targetDir = pathReplace(sourceDir, filepath.Join(goRoot, "src"), "$(go2csPath)core")
  } else {
      targetDir = pathReplace(sourceDir, filepath.Join(goPath, "pkg"), "$(go2csPath)pkg")
  }
  ```
  So the intended layout is: **stdlib under `$(go2csPath)core/…`, third-party under
  `$(go2csPath)pkg/…`** — the recursive converter should honor exactly this.
- **`-stdlib` already emits from an app without converting stdlib.** A single `processConversion` run
  emits `$(go2csPath)core\…` project references for the stdlib packages it imports *without* converting
  them — precisely the "reference, don't reconvert" behavior we want for stdlib.
- **The referenced stdlib is now staged by `deploy-core`** (built + tested 2026-07-11). `deploy-core stub`
  (runnable baseline) or `deploy-core stdlib` (compilable full stdlib) copies the runtime + stdlib + the
  `go2cs-gen` analyzer to `%GOPATH%\src\go2cs`, presenting the stdlib uniformly at `core\<pkg>`, and writes
  a **root `Directory.Build.props` pinning `<go2csPath>$(MSBuildThisFileDirectory)</go2csPath>`**. That
  props file is the reference backbone for §3.4/§3.5: every project under the root — the staged stdlib and
  anything `-recurse` later writes there — resolves its `$(go2csPath)core\… / gen\…` references with no
  `-p:go2csPath` flag and no absolute paths.
- **The template already makes every library project NuGet-publishable** (`csproj-template.xml:44-53`:
  `GeneratePackageOnBuild`, `PackageId=go.$(AssemblyName)`) — relevant to the NuGet endgame.

**What's missing**

1. **No recursive convert loop for a non-stdlib module.** `processConversion` iterates only the
   top-level matched packages (`for _, pkg := range pkgs`, `main.go:737`); third-party and app
   sub-packages in the closure are never converted.
2. **The dependency graph / topological sort is hardwired to the stdlib.** `scanStdLib`
   (`stdLibConverter.go:135`) loads the `"std"` pattern with `GO111MODULE=off`; `isStdLib`
   (`stdLibConverter.go:301`) rejects any path containing `.` — so `github.com/...` modules can never
   enter the graph.
3. **Module-cache dependencies would be "converted in place."** When `build.Import` can't resolve a
   module-cache dep (the usual case — the module cache isn't on `GOPATH/src`), resolution falls to
   `getLocalModulePackageInfo` (`importOperations.go:251`), which treats output as **co-located with the
   Go source** (`TargetDir = meta.Dir`, lines 285-292). For a `replace`d / co-located local module that's
   correct, but for a **read-only, versioned module-cache** dep (`$GOPATH/pkg/mod/<m>@<v>/…`) it is wrong:
   the cache is read-only, and refs would point into it. Third-party deps must convert to a **writable**
   location (`$(go2csPath)pkg/…`) and be referenced there.
4. **Solution generation is stdlib-only.** `GenerateSolutionFile` (`solutionGenerator.go:36`) walks the
   `core/` output tree; it needs to also take in the `pkg/` (third-party) output and the app project.

## 3. Proposed design

### 3.1 Invocation

Add a **`-recurse`** flag (default **off**, so existing single-package behavior is unchanged). With it,
for a module/dir input, go2cs converts the input module's packages **and every third-party dependency
package** in the transitive closure, in dependency order. The **stdlib is not converted** (chosen scope);
stdlib imports emit `$(go2csPath)core\…` references to a pre-converted stdlib (see §3.5).

*(Alternative considered: auto-detect and always recurse. Rejected for now — it silently changes the
behavior of today's single-package invocation and can pull in a large graph unexpectedly. A flag is
explicit and reversible; auto-detect can come later.)*

### 3.2 Discover + partition the closure

Reuse the `go/packages` load already in `processConversion`, then partition the transitive closure
(`captureImportDirs`) into three sets by source location:

- **stdlib** — `pkg.Goroot` true (under `$GOROOT/src`) → **reference only** (`$(go2csPath)core`).
- **third-party** — a non-GOROOT module (under the module cache or a `replace`) → **convert** to
  `$(go2csPath)pkg/<import-path>`.
- **app** — the input module's own packages → **convert** to the user's output dir.

Partitioning by `pkg.Goroot` / module identity is more robust than the current `strings.Contains(".")`
heuristic and should replace it in the shared graph code.

### 3.3 Dependency graph + topological order

Generalize the graph/topo-sort out of `StdLibConverter` into a shared component (e.g.
`dependencyGraph.go`) parameterized by *the set of packages to convert* and *the edge predicate*
(an edge counts only when the dependency is itself in the convert-set — the same rule
`buildDependencyGraph` already uses at `stdLibConverter.go:261`). `StdLibConverter` becomes one caller
(convert-set = std); the new **`ModuleConverter`** is the other (convert-set = app + third-party).

Both reuse the existing `topologicalSort` / `visitPackage` (`stdLibConverter.go:313-418`), which already
sorts from deterministic roots and tolerates Go's import cycles with a warning. **"Least dependencies
first" falls straight out of this** — leaf third-party libs convert before the libs and app that import
them, so each importer sees its dependency's finished `package_info.cs` (the source of imported
collision-rename aliases) before it is converted. Conversion stays **sequential** (the converter relies
on package-level global state — the reason the same file just removed `-parallel`).

### 3.4 Output layout and reference resolution

| Set | Output dir | Emitted reference |
|---|---|---|
| stdlib | *(not written — pre-converted)* | `$(go2csPath)core\<dotted>\<name>.csproj` |
| third-party | `$(go2csPath)pkg/<import-path>` | `$(go2csPath)pkg\<dotted>\<name>.csproj` |
| app | user output dir (or in-place) | project(s) under the output dir |

The **core change** is decoupling *source dir* from *converted-output dir* for module-cache deps. Concretely:

- Give `ModuleConverter` an explicit **import-path → output-dir** map (built while partitioning), and
  route reference resolution through it so a third-party dep resolves to `$(go2csPath)pkg\…` — **not**
  in-place at the read-only cache. This is the `getLocalModulePackageInfo` correction from §2/#3; it can
  be a new branch that fires when `meta.Dir` is under `$GOPATH/pkg/mod` (map to `$(go2csPath)pkg`) versus
  a genuinely co-located `replace` module (keep in-place).
- Strip the module cache's `@<version>` segment from the output path (single-version assumption for now;
  see open decisions).

**Anchor the whole conversion at the deploy root.** `-recurse` should default `-go2cspath` to
`%GOPATH%\src\go2cs` — the same root `deploy-core` stages to — and write the app + third-party output
*under* it (third-party at `pkg\…`; the app at, e.g., `app\<module>\`). The root `Directory.Build.props`
that `deploy-core` wrote then supplies `$(go2csPath)` to every generated project automatically, so the
app, the third-party libs, the staged stdlib, and the analyzer all resolve their `$(go2csPath)…`
references with no per-project property and no machine-specific absolute paths.

### 3.5 Referencing the pre-converted stdlib

Per the chosen scope, stdlib imports emit `$(go2csPath)core\…` references and are **not** converted. The
staging step (`deploy-core stub`|`stdlib`) has already placed a pre-converted stdlib at
`%GOPATH%\src\go2cs\core\<pkg>` and written a root `Directory.Build.props` pinning `$(go2csPath)` to that
root — so `$(go2csPath)core\<pkg>\<pkg>.csproj` resolves deterministically for every project beneath it,
independent of the template's Debug/`$(SolutionDir)` vs. Release/`$(USERPROFILE)` fallback (the props value
wins, because the template's `go2csPath` block is `Condition="'$(go2csPath)'==''"`). `deploy-core stdlib`
stages the full 302-package stdlib (compilable); `deploy-core stub` stages the runnable baseline subset —
pick per what the app imports. `deploy-core` also stages the `go2cs-gen` analyzer at `gen\go2cs-gen`, which
every converted csproj references at `$(go2csPath)gen\go2cs-gen`.

This is the seam the **NuGet stdlib** replaces — now **implemented** as `-recurse=nuget` (2026-07-14):
`$(go2csPath)core\<pkg>\<pkg>.csproj` `ProjectReference` → `go.<pkg>` `PackageReference`, and likewise golib
→ `go.lib` and the `go2cs-gen` analyzer → `go.gen` (`PrivateAssets="all"`), all versioned
`$(GoStdLibVersion)`. Keeping the reference indirection through `$(go2csPath)` made the switch a
reference-rewrite, not a structural change: the app's own converted packages (`src\` + `pkg\`) stay
`ProjectReference`s, and the converter emits an output-root `Directory.Build.props` in this mode that pins
`$(go2csPath)` for them and defaults `GoStdLibVersion` to the converter's Go release (floating, e.g.
`1.23.1.*`), so a converted app restores from nuget.org with **no `deploy-core` staging**.

### 3.6 Solution generation

Generalize `GenerateSolutionFile` to accept the app project + the `pkg/` (third-party) output roots in
addition to (or instead of) `core/`, grouping into solution folders by module namespace (the folder-ID
hashing in `solutionGenerator.go:246` already handles duplicate leaf names). The stdlib projects are
included as references via `$(go2csPath)core` (either listed for build, or assumed pre-built).
`deploy-core` already emits a flat `go2cs-core.slnx` over the staged core + analyzer; the recurse solution
follows the same shape, adding the app + `pkg\…` third-party projects.

## 4. Phased implementation plan

- **P0 — stage the referenced stdlib (done).** `deploy-core stub`|`stdlib` puts the stdlib + `go2cs-gen`
  analyzer + a root `Directory.Build.props` at `%GOPATH%\src\go2cs`; the phases below build on that root.
  Already implemented + tested (2026-07-11).
- **P1 — extract the shared dependency graph (done).** Graph build + topological sort now live in a
  reusable `DependencyGraph` (`src/go2cs/dependencyGraph.go`) parameterized by the convert-set: the added
  nodes ARE the edge predicate (`addImportEdges` records an edge only to a dependency that is itself a
  node, so a dependency outside the set — e.g. the stdlib, when converting a user module — never
  constrains the order). `StdLibConverter` became one caller (discovers the `std` set + each package's
  imports, delegates ordering); `ModuleConverter` (P2) will be the other. Pure refactor: the moved
  algorithm is verbatim, unit-tested for the exact topo order / edge predicate / GOROOT-vendored-key
  resolution / cycle tolerance (`dependencyGraph_test.go`), and gated green — `check-no-regression`
  byte-identical across 371 behavioral projects, and a filtered `-stdlib fmt strconv` run scans + sorts
  the full 305-package std graph and converts strconv→fmt in dependency order. Implemented + tested
  (2026-07-11). *(A drive-by fix updated the stale `solutionGenerator_test.go` folder assertions, which
  predated the `Id="…"` folder attribute, so `go test ./...` is a reliable gate for the phases below.)*
- **P2 — `ModuleConverter` + `-recurse` (done).** Added the `-recurse` flag (default off — single-package
  behavior unchanged) and `ModuleConverter` (`src/go2cs/moduleConverter.go`): it loads the input module +
  full dependency closure once (`LoadAllSyntax | NeedModule`), partitions every closure package by
  `classify` — **stdlib** = under `$GOROOT/src` (covers GOROOT-vendored), **app** = `pkg.Module.Main`,
  **third-party** = any other dependency module (`pkg.Module != nil`) — adds the app + third-party set to the
  shared `DependencyGraph`, topo-sorts, and converts each in dependency order via `processConversion` (which
  re-loads each package with full syntax). Stdlib is referenced (`$(go2csPath)core\…`), never converted.
  Conversion stays sequential (package-level global state). Validated on a synthetic `app → lib(replace) →
  stdlib` fixture: closure discovered + partitioned, converted **lib before app**, and the app csproj wired
  `..\lib\example.com.lib.csproj` (relative `ProjectReference`) + `$(go2csPath)core\fmt` while the lib wired
  `$(go2csPath)core\strings` — the cross-package `lib.Greeting` call resolved. Gate: `check-no-regression`
  byte-identical across 371 behavioral projects (the default path is untouched); `classify` +
  convert-set-order unit-tested (`moduleConverter_test.go`). **P2 converts in place** (output co-located with
  source) — correct for the app and `replace`d/co-located modules; read-only module-cache deps + the
  `$(go2csPath)pkg` output routing are P3. Implemented + tested (2026-07-11).
- **P3 — output/reference decoupling (done).** Read-only, versioned module-cache dependencies
  (`$GOPATH/pkg/mod/<m>@<v>/…`) now convert to a **writable `$(go2csPath)pkg\<import-path>`** location and are
  referenced there, never in place at the read-only cache. Three changes: (a) a new module-cache branch in
  `getLocalModulePackageInfo` (`importOperations.go`) emits `$(go2csPath)pkg\<import-path>\…` references
  (used as-is, like the stdlib `$(go2csPath)core` refs — `writeProjectFile` only relativizes `IsAbs`
  references) with the `@version` **stripped** by deriving the path from the version-free import path;
  (b) `ModuleConverter.outputDirFor` routes a module-cache package's OUTPUT to the matching
  `$(go2csPath)pkg\<import-path>` dir so reference and output agree (the app + co-located `replace` modules
  stay in place); (c) `processConversion` loads only the single target package under `-recurse` (never
  `./...`, which would re-convert sibling sub-packages). Validated live against a real cache dep
  (`github.com/google/uuid@v1.6.0`): it converted to `…\pkg\github.com\google\uuid\` (version stripped), the
  read-only cache was untouched, and the app csproj referenced
  `$(go2csPath)pkg\github.com\google\uuid\github.com.google.uuid.csproj`. Gate: `outputDirFor` unit-tested;
  `check-no-regression` byte-identical across 371 behavioral projects (all changes are recurse-guarded).
  Implemented + tested (2026-07-11).
- **P4 — solution generation (done).** `ModuleConverter` now emits a flat `go2cs-recurse.slnx` at the
  deploy root ($(go2csPath)) listing every converted app + third-party project (relative forward-slash
  paths — the app in place via a `..\` path, third-party under `pkg\…`). It mirrors the flat shape
  deploy-core.ps1 emits for `go2cs-core.slnx` (new `buildFlatSolutionXML`, no namespace-folder grouping —
  distinct from the stdlib's folder-grouped `buildSolutionXML`). Placing it at the deploy root makes
  `$(SolutionDir)` resolve there on build, so the pre-converted stdlib ($(go2csPath)core), golib, and the
  analyzer resolve and build transitively via the ProjectReferences — the stdlib is referenced, not
  listed. Validated: a `go2cs -recurse` run over the uuid cache-test emitted a two-project solution
  (`..\cache-test\example.com.cachetest.csproj` + `pkg\github.com\google\uuid\…csproj`). Gate:
  `buildFlatSolutionXML` unit-tested; `check-no-regression` byte-identical across 371 behavioral projects.
  Implemented + tested (2026-07-11).
- **P5 — test harness (done).** *5a synthetic:* `TestRecurseSyntheticModule` (`moduleConverter_integration_test.go`)
  runs the real `-recurse` conversion over an in-process two-module fixture (`app → lib(replace) → stdlib`)
  and asserts the loop end to end — topo order (lib before app), in-place output, the cross-package
  `lib.Greeting` call, the lib + `$(go2csPath)core\fmt` references, and the flat solution — deterministic,
  network-free, CI-able via `go test`. It caught a `generateSolutionFile` edge case (an all-in-place
  conversion never created the deploy root), now fixed. *5b real internet:* see §5 results — `-recurse`
  handled the real `fatih/color` DAG (74-package closure, 4 third-party) flawlessly at the process level;
  the dotnet build is partial, surfacing per-package **converter** defects (Phase-4 territory), not recurse
  defects. Implemented + tested (2026-07-11).

Each phase is independently reviewable; P1 is a pure refactor with a strong existing regression gate.
**All five phases (P1–P5) are implemented, gated, and committed (2026-07-11).**

## 5. Test plan

**5a. Synthetic local test (deterministic, CI-able).** A two-module fixture: an `app` `main` package
that imports a co-located `lib` module (via `replace`), which imports stdlib. Confirms the recurse loop,
topo order, and in-place vs. `pkg` output rules without network. Fits the existing `crosspkglib` test
pattern.

**5b. Real internet test (the requested exercise).**

1. **Fetch** a small real app into a scratch module — proposed: a CLI using
   **`github.com/fatih/color`**, which pulls `github.com/mattn/go-colorable` +
   `github.com/mattn/go-isatty`. That's a genuine small DAG
   (`app → color → {colorable, isatty} → stdlib`) that actually exercises "least-dependencies-first."
   *(Simplest alternative: a zero-dep lib like `github.com/spf13/pflag` or `github.com/google/uuid` for a
   one-hop `app → lib → stdlib` graph.)*
2. **Baseline with Go** — `go build ./...` to confirm it compiles as Go first.
3. **Stage the stdlib** — `deploy-core stdlib` once, putting the compilable stdlib + analyzer + root
   `Directory.Build.props` at `%GOPATH%\src\go2cs`.
4. **Convert** — `go2cs -recurse <module-dir> -go2cspath %GOPATH%\src\go2cs`, converting the app +
   third-party libs under that root (stdlib referenced, not converted).
5. **Build** — `dotnet build` the generated solution.
6. **Report** — packages discovered / converted / compiled, and CS-error buckets for the rest. Success
   criterion is **process completion + solution builds** (partial compile is acceptable at this stage).

**5b results (2026-07-11).** Ran end to end against `github.com/fatih/color` (v1.16.0) — a genuine DAG
`colordemo → color → {go-colorable, go-isatty, golang.org/x/sys/windows} → stdlib`. `go build ./...`
baselined; `deploy-core stdlib -NoBuild` staged the 302-package compilable stdlib + analyzer + golib;
`go2cs -recurse` then `dotnet build`:

- **Recurse process: 100%.** 74-package closure discovered → 1 app + 4 third-party converted, 68 stdlib
  referenced; **topological order correct** (`go-isatty` & `x/sys/windows` leaves → `go-colorable` →
  `color` → `colordemo`); all four third-party routed to `pkg\<import-path>`; flat solution emitted.
- **dotnet build: partial (as expected — partial compile is the accepted criterion).** The full staged
  stdlib subset + golib + analyzer compiled (~1315 DLLs); `github.com/mattn/go-isatty` (pure-stdlib deps)
  built; and a pure-Go control lib `github.com/google/uuid` built. The rest failed on **three per-package
  CONVERTER defects** (Phase-4 territory — not recurse-orchestration defects):
  1. `uintptr`→C# `nuint` cannot be `const` (`x/sys/windows`, ~180 × CS0283/CS0133) — the syscall
     "raw-metal on non-native types" case flagged in [`Baseline-vs-FullConversion.md`].
  2. A hyphen in a package's import-path segment is emitted into C# identifiers unsanitized
     (`go-colorable` → `go-colorable_package`, ~8 × CS1002/CS0116) — invalid C# identifier.
  3. A non-stdlib imported type-alias is namespace-qualified from the bare package name, not the dotted
     import path (`getLocalModulePackageInfo` sets `PackageName = meta.Name`), so `uuid`'s app got
     `go.uuid_package.…` instead of `go.github.com.google.uuid_package.…` (4 × CS0234). The lib built;
     only the importer's `package_info.cs` aliases misqualified.

  All three are converter identifier/qualification/`const` gaps in specific Go constructs, surfaced for the
  first time by converting real third-party code — precisely the residual per-package defects Phase 4
  addresses. The `-recurse` feature itself (discovery, partition, dependency order, output/reference
  routing, solution generation) is validated correct on this real DAG.

**Converter fixes landed (2026-07-11).** All three defects above were fixed as focused, separately-gated
commits (`check-no-regression` byte-identical across 371 behavioral projects for each — none of these Go
constructs appears in the corpus):

- **#3** (`729ce52df`) — `getLocalModulePackageInfo` now sets `PackageName` to the dotted namespace path
  (`packageQualifiedName`), using the Go package name (not the import-path segment) as the last part.
  **`github.com/google/uuid` now compiles fully end to end** (app + lib, 0 errors) — a working pure-Go
  real-world example.
- **#1** (`2d3424085`) — `convertImportPathToNamespace` uses the Go package name (from the module-aware
  import graph, always a valid identifier) for a non-stdlib import's class segment, clearing the hyphen
  parse errors.
- **#2** (`238c6db36`) — `visitValueSpec` unaliases a const's type before the `*types.Named` test, so a
  const typed through an alias to a named type (`type Errno = syscall.Errno`) emits `static readonly`.
  **`golang.org/x/sys/windows` now compiles** — the syscall/Windows-API "raw-metal" package.

These took the `fatih/color` build from **188 → 2 errors**. The remaining 2 were a **fourth** defect, this
one in **go2cs-gen** (the Roslyn generator, not the converter): a Go defined type named after a C# keyword
(`type short int16` in go-colorable) is declared `partial struct @short`, but `ImplicitConvGenerator` built
the conversion operator's host/return from the raw symbol name `short`, emitting `partial struct short {
implicit operator short(dword) => new short(…) }` — which parses the operator into the enclosing static
class (CS0715/CS0057) and, once the names were escaped, cast `dword.Value` (`uint`) straight to `@short`
(CS0030) because the numeric-conversion body was skipped (`GetStructDeclaration` matched `Identifier.Text`
`"@short"` against the symbol name `"short"`). **Fixed** (keyword-escape the type names via `EscapeCsKeyword`;
match `GetStructDeclaration` on the `@`-stripped `ValueText`), yielding `partial struct @short { implicit
operator @short(dword src) => new @short((short)src.Value); }`. **`fatih/color` now builds fully — 0 errors,
all five projects** (app + color + colorable + isatty + x/sys/windows). Gate: full behavioral suite green
(the generator change is byte-identical for the corpus — no keyword-named `GoImplicitConv` structs there).

**Namespace-sanitization fix + adversarial review (2026-07-11).** An adversarial review of the five fixes
above (14 subagents, review + verify) confirmed the fixes were correct for exactly what `fatih/color` hit but
**incomplete for other real-world modules** — the byte-identical corpus gate cannot catch out-of-corpus
identifier cases. The highest-impact class was fixed:

- **Hyphen/tilde in an import-path segment** — the converter never mapped a hyphen (or tilde) in an
  import-path segment to a legal C# identifier, so `github.com/google/go-cmp/cmp` produced the illegal
  namespace `…google.go-cmp` (the hyphen is in a NON-leaf segment, so the earlier `#1` fix — which only
  substitutes the leaf segment's package name — did not reach it). **Fixed**: `replaceInvalidIdentifierChars`
  (hyphen/tilde → `_`) applied in both `getSanitizedImport` and `getCoreSanitizedIdentifier`, so both the
  dependency's own namespace (`getProjectName`) and every importer's reference (`convertImportPathToNamespace`)
  render `…google.go_cmp` consistently. Unit-tested (`sanitization_test.go`); `check-no-regression`
  byte-identical (Go identifiers never contain hyphens, so only import-path-derived names change and the
  corpus has none). This covers the ubiquitous `go-*`/`*-go` module families (go-cmp, go-isatty, go-sql-driver,
  golang-jwt, …). *(A first attempt also routed `convertImportPathToNamespace`'s non-leaf parts through
  `getCoreSanitizedIdentifier` to escape the embedded keyword in `gopkg.in` — but that Δ-prefixed the
  `_package` class suffix and regressed 11 corpus goldens, so it was reverted; the `gopkg.in` case is backlog
  item 8.)

### Output-layout redesign + flag/reference fixes (2026-07-11)

First real end-user run of the walkthrough surfaced two blocking defects and drove a layout redesign. All
four changes below are recurse-only (or transparent to flags-first callers); `check-no-regression` stayed
byte-identical across all 371 behavioral projects at every step.

- **R1 — flag ordering (`main.go`, all modes).** Go's `flag` package stops parsing at the first non-flag
  token, so `go2cs -recurse . -go2cspath <dir>` silently DROPPED `-go2cspath` and wrote to the default
  `%USERPROFILE%\go2cs` — the converted deps and solution appeared "missing" at the expected path.
  `parseArgsInterspersed` now peels off one positional at a time and re-parses the remainder, so flags before
  OR after the module path are honored. Unit-tested (`argParse_test.go`).
- **R2 — parallel output tree + version-free references.** Nothing is written in place any more (the original
  Go source stays pure): the app's own packages (the main module) convert to `$(go2csPath)src\<import-path>`
  and every dependency — module-cache or a co-located `replace` — to `$(go2csPath)pkg\<import-path>`. Because
  the app now lives UNDER the deploy root, it inherits the deploy-root `Directory.Build.props`, so a bare
  `dotnet build` in the app folder (or an IDE open) resolves `$(go2csPath)` with no solution needed. This
  also fixed a broken app→dependency reference: the app csproj had referenced a module-cache dep as
  `$(go2csPath)pkg\mod\<module>@<version>\…` (the `build.Import` success path), which did not match where the
  dep converts (`$(go2csPath)pkg\<import-path>`), yielding `CS0246`. App/third-party references now route
  through the module-aware go/packages metadata (`getRecurseDependencyInfo`) to the version-free src\/pkg\
  paths; stdlib still resolves to `$(go2csPath)core`. Added `Options.mainModulePath` to classify app vs.
  dependency consistently in both output routing and reference emission.
- **R3 — per-project solutions.** A `.slnx` is written next to every converted `.csproj`, over that project
  plus its **transitive** converted dependencies + golib + the analyzer (no stdlib), with the solution's own
  anchor project marked the VS default startup (`DefaultStartup="true"` — a `.slnx` capability the old `.sln`
  lacked). Building the app's per-project solution builds the app and its whole dependency closure in one
  shot, without the ~300-project stdlib solution. *(The separate flat `go2cs-recurse.slnx` at the deploy root
  was removed as redundant — the app's own per-project solution already lists its whole converted dependency
  closure, so it IS the build-everything solution for the app.)* Projects are grouped into three top-level
  solution folders that mirror the `%GOPATH%` layout — **`src`** for the project(s) being converted (the
  app's own main-module packages), **`pkg`** for their converted dependency packages, and **`core`** for the
  go2cs runtime/generator projects (`golib`, `go2cs-gen`) — emitted in that **enforced `src → pkg → core`
  order** (deliberately not alphabetic). Classification is by import path (`isMainModulePackage`, the same
  rule that routed each package's output to `src\`/`pkg\`), so the folders agree with the on-disk tree. An
  empty folder is omitted (a dependency's own per-project solution has no `src` package), mirroring how the
  stdlib solution drops its `/tests/` folder when empty. The three folder names are unique leaves, so —
  unlike the namespace-nested stdlib solution — no folder `Id` is needed. (`buildRecurseSolutionXML`,
  `solutionGenerator.go`; guarded by `TestBuildRecurseSolutionXML` + `TestBuildRecurseSolutionXMLSkipsEmptyFolders`
  and the folder-order assertions in `TestRecurseSyntheticModule`.)
- **R4 — acceptance.** With a **current** deploy root, the `fatih/color` example (app + `color` +
  `go-colorable` + `go-isatty` + `x/sys/windows` + golib + analyzer) **compiles clean — 0 errors, 0 MSB
  reference errors**. Two operational caveats learned here:
  - *Stale deploy.* The deploy root is a snapshot; building against an analyzer older than a converter fix
    surfaces spurious errors (here `CS0715`/`CS0057` on `go-colorable`'s keyword-named `short`/`dword` types,
    from a deploy predating the `EscapeCsKeyword` fix). Re-running `deploy-core` cleared them.
  - *Toolchain skew.* `fatih/color` v1.19 requires go 1.25 while go2cs's `go.mod` pins go 1.23.1, so
    `go/packages` loads the dependency closure with `package requires newer Go version` and degraded type
    info. Bumping go2cs's toolchain is deferred — a different type-checker could shift the byte-identical
    behavioral corpus and needs its own re-baseline. **Running** the compiled example currently throws from a
    converted third-party `init` (a Phase-4 operational defect, plausibly downstream of the degraded x/sys
    conversion), but the milestone — a buildable, compiling solution — is met.

### Residual real-world-module hardening (Phase-4 backlog)

The review + the go-cmp/gopkg.in verification surfaced these **known residual limitations** — real, but each
out-of-corpus and narrower than the namespace fix. Left for Phase-4 hardening (none affects the stdlib,
`fatih/color`, `uuid`, or the behavioral corpus, all of which build):

1. *(medium, go2cs-gen)* A C#-keyword-named Go type nested as a **generic type argument** (the `ж<short>`
   pointer-box of an `Indirect`/self-box conversion) is emitted unescaped — `GetFullTypeName` rebuilds
   generic names from the raw `ITypeSymbol.Name`, and the single top-level `EscapeCsKeyword` cannot reach
   nested arguments. Needs recursive keyword-escaping of generic arguments.
2. *(medium, converter)* A `const` typed through an **alias to `uintptr`** (`type Handle = uintptr; const X
   Handle = 42`) still emits `const` — the `#2` unalias reached the `*types.Named` gate but the downstream
   `csTypeName == "uintptr"` guard reads the alias name (`Handle`), not the unaliased underlying.
3. *(medium, go2cs-gen)* `EscapeCsKeyword` misses C# **contextual** keywords (`file`) that the converter DOES
   escape (`os`-style `type file …` → `@file`), so a keyword-named-type conversion for such a type mismatches.
4. *(low, converter)* A dependency whose Go **package name is a go2cs-reserved word** (`package slice`/`array`)
   or ends in `_package`: the imported-type-alias qualification (`#3`) Δ-prefixes the class segment
   (`Δslice_package`) while the producer emits it un-prefixed (`slice_package`) — dangling reference.
5. *(low, converter)* An **alias-to-alias-to-named** chain (`type A = Named; type B = A`) emits
   `global using B = go.<pkg>.A;`, referencing the intermediate alias `A` as if a class member (CS0426).
6. *(robustness, recurse)* `processConversion`'s `log.Fatalf` on a package **load failure** (e.g. a dependency
   whose transitive test-dep has a missing `go.sum` entry — `gopkg.in/yaml.v3` → `gopkg.in/check.v1`) aborts
   the ENTIRE `-recurse` run rather than logging + skipping that one package. `ModuleConverter.convertAll`
   already recovers panics; the load path should return an error instead of exiting.
7. *(robustness, recurse)* A **deeply-nested module-cache subpackage** (`github.com/google/go-cmp/cmp/internal/
   flags`) converted with a bare `namespace go;` instead of the full dotted namespace — `getProjectName`'s
   walk-up-to-`go.mod` needs to handle the `@version`-segmented cache path at depth.
8. *(low, converter)* A C# **keyword embedded after a dot** in a single import-path segment (the `in` of
   `gopkg.in`) is escaped by `getProjectName` (the dependency's own namespace → `gopkg.@in`) but NOT by
   `convertImportPathToNamespace` (an importer's reference → bare `gopkg.in`), so a `gopkg.in/*` dependency's
   own package compiles while its importers mis-reference it. Needs a dot-splitting sanitizer in
   `convertImportPathToNamespace` that escapes the embedded keyword WITHOUT Δ-prefixing the `_package` class
   suffix (the naive `getCoreSanitizedIdentifier` swap does the latter — see the reverted attempt above).

### Operational stdlib — native sync primitives (2026-07-11, Phase-4 start)

Running (not just compiling) the `fatih/color` sample exposed the Phase-3 → Phase-4 boundary: the full
conversion **compiles** but is not **operational**. The first blocker is `sync`: its `//go:linkname`
runtime concurrency primitives (`Semacquire`/`Semrelease`, `notifyList`, `procPin`, …) are emitted as
throwing stubs, so `sync.init` — reached by `os`/`syscall` and nearly every program — crashes at startup.

Emulating Go's runtime *sleeping semaphore* on a .NET primitive is a **dead end**: it is co-designed with
the mutex state machine (starvation-mode ownership is handed to one specific waiter via an exact ticket),
and both a global-gate and a faithful per-address FIFO+handoff emulation deterministically trip
`sync: inconsistent mutex state` / `unlock of unlocked mutex` at sustained contention (2 goroutines × 10k
locks). So the runtime-dependent **types are reimplemented natively** (hand-owned files replacing the
converted output), on proven .NET primitives: `Mutex` → binary `SemaphoreSlim`; `WaitGroup` → guarded
counter + latch; `RWMutex` → writer-preferring monitor lock (keeps the `RLocker`/`rlocker` witness and the
`syscall_hasWaitingReaders` linkname); `throw`/`fatal` → native fatal hooks. `Once`/`Map`/`Cond`/`Pool`
ride on these unchanged (`Cond` still uses `runtime_impl.cs`'s notify-list; `Pool` sharding is best-effort).
Validated by sync-only converted stress tests (no `os`/`fmt`, so unaffected by the syscall gap):
Mutex+WaitGroup+Once at 100 goroutines × 1000 → 12/12 clean; RWMutex + Cond correct. Commit `e36dea3aa`.

These hand-owned files survive a reconvert via the existing **`[module: GoManualConversion]`** marker —
the converter's `containsManualConversionMarker` scans each output `.cs` and skips re-converting the
matching `.go` when present (verified: `-stdlib sync` into a seeded dir leaves `mutex.cs` byte-identical,
still the native `SemaphoreSlim` impl). Remaining polish: `RWMutex` uses `Monitor.PulseAll` (correct but
O(n)/op under pathological contention).
**Remaining to run `color` end-to-end:** the **package-level var initialization order** crash is FIXED
(2026-07-11) — a general converter fix, not a syscall patch: an initializer whose Go dependency order C#'s
static-field-initializer order cannot reproduce (cross-file, same-file forward reference, or transitively
through package function bodies — syscall's `Stdin = getStdHandle(…)` reading zsyscall's
`procGetStdHandle`) is emitted as a bare field plus an `initᴛ<name>()` method in its home file, called in
`types.Info.InitOrder` by a generated `package_init.cs` static constructor. See
[ConversionStrategies-Reference → Package-Level Variable Initialization Order](../ConversionStrategies-Reference.md#package-level-variable-initialization-order);
guarded by the `PackageVarInitOrder` behavioral test.

### Operational stdlib — syscall FFI + runtime bootstrap (2026-07-11, groundwork)

With init order fixed, the converted-program startup crash chain was cleared one layer at a time. Each
fix below is committed groundwork; the chain now reaches `fmt.newPrinter`, blocked on a distinct
go2cs-gen issue (see the end). All fixes live in `src/go-src-converted` unless noted, deployed via
`deploy-core stdlib`:

1. **`internal/godebug`** runtime `//go:linkname` hooks (`setUpdate`/`registerMetric`/`setNewIncNonDefault`)
   were throwing `PartialStubGenerator` stubs → **`godebug_impl.cs`** companion (no marker needed —
   supplying a bodyless partial's implementing part auto-suppresses the stub). One-shot `update()` notify.
2. **Windows syscall FFI** (`loadlibrary`/`getprocaddress`/`Syscall…`/`SyscallN`) → native
   **`syscall/dll_windows.cs`** (whole-file `GoManualConversion`): `LoadLibraryExW`/`GetProcAddress`
   P/Invoke, plus an unmanaged function-pointer `calli` dispatch (a switch on argument count — Windows
   x64 has a single calling convention). `LazyDLL.Load`/`LazyProc.Find` use a plain double-checked lock
   (CLR reference writes are atomic — Go's `atomic.LoadPointer((*unsafe.Pointer)(&d.dll))` managed-
   referent round-trip cannot be emulated).
3. **`runtime`'s own `init` functions** divided by zero / hit stubs at class-initialization (arena
   sizing checks against a zero `physPageSize`, etc.). In Go these run only after the assembly bootstrap
   (`osinit`/`schedinit`) populates such globals; converted code has no bootstrap — **.NET is the
   runtime**. **Converter fix** (`visitFuncDecl.go`): for `pkg.Path()=="runtime"`, emit `init` funcs as
   commented-out `/* [GoInit] runtime bootstrap init - not run */` plain methods. Behavioral-neutral
   (CNR byte-identical — scoped to `runtime`, which no behavioral test is).
4. **`efaceOf`** reinterpret panicked at class-init (several `_type` field initializers walk it) →
   returns an inert `eface` in `runtime/runtime2.cs` (descriptor walking is vestigial — go2cs replaces
   Go's itab dispatch with C# interfaces + generators). Same file seeds `gomaxprocs`/`ncpu` from
   `Environment.ProcessorCount` (the bootstrap would; left 0, `sync.Pool` sizes a zero-length shard
   array and indexes out of range on the first `Println`).
5. **`sync.Pool`** per-P sharding presumes Go's scheduler → native **`sync/pool.cs`** (whole-file
   `GoManualConversion`): one `ConcurrentBag` per Pool, lazily created through the Pool's heap box (the
   next member of the native-`sync` family).
6. **`runtime.SetFinalizer`** walks type descriptors (`os.newFile` sets a finalizer on every opened
   file) → **`runtime/mfinal.cs`** frozen whole-file `GoManualConversion`, its body replaced with a
   `ConditionalWeakTable` + sentinel-finalizer bridge (the rest is inert converted GC-queue machinery
   kept for compilation).

**⚠ Open soundness constraint** (documented in `dll_windows.cs`): golib's `ж<T>→uintptr` conversion
returns an address captured inside a `fixed` block that exits before the return, so every `uintptr` the
converted `zsyscall` wrappers pass to the trampoline is a **transient** (GC-moveable) address. The
window is short and allocation-free in practice; the sound fix is to pin at the capture seam — a golib
decision to make with the project owner.

**The final blocker for `fmt.Println`** is a **go2cs-gen** gap, not FFI: `fmt.newPrinter` does
`@new<pp>()`, and `pp` has a plain field `fmt fmt` whose type carries a promoted embed (`fmtFlags`, a
ctor-initialized `ж` box). `@new<T>()` runs `pp`'s parameterless constructor, which leaves the `fmt`
field `default` — so its embed box (and its array-field backing) is null and `clearflags` NREs.
`StructTypeTemplate.AppendPromotedBoxInitializers` constructs a struct's OWN embed boxes but not a
struct-typed FIELD whose type needs construction. The fix — construct such fields as `new FieldType(nil)`
in the zero-value constructors — is a general go2cs-gen change of comparable scope to the init-order fix
(large blast radius: every struct with a struct-typed field), so it is deferred to its own validated pass.

## 6. Open decisions (RESOLVED)

1. **Flag vs. auto-detect** — **explicit `-recurse`** (default off; single-package behavior unchanged).
2. **Module version in the output path** — **strip `@<version>`** (single-version assumption; the
   `$(go2csPath)pkg\<import-path>` path is derived from the version-free import path).
3. **Which pre-converted stdlib to reference** — the `deploy-core` modes: `stdlib` (full
   `src/go-src-converted`, compilable) or `stub` (runnable baseline); both stage to
   `%GOPATH%\src\go2cs\core`. 5b used `stdlib`.
4. **Test app** — **`fatih/color`** (the small DAG), with `github.com/google/uuid` as a pure-Go control.

## 7. Relationship to the NuGet stdlib decision

This design deliberately keeps the stdlib behind the `$(go2csPath)core` reference indirection so that the
switch to a **NuGet stdlib** is a `ProjectReference` → `PackageReference` rewrite. That switch is now
**implemented** as **`-recurse=nuget`** (2026-07-14): the go2cs stdlib (`go.<pkg>`), runtime (`go.lib`) and
analyzer (`go.gen`) become NuGet `PackageReference`s versioned `$(GoStdLibVersion)`, while the app's own
converted packages stay `ProjectReference`s and the converter emits an output-root `Directory.Build.props`
supplying the `$(go2csPath)` pin + a floating `GoStdLibVersion` default — so a converted app restores from
nuget.org with no `deploy-core` step. The same applies to third-party libs: because the template already
sets `GeneratePackageOnBuild`/`PackageId`, a converted third-party lib is itself NuGet-publishable — so
"convert once, reference everywhere" generalizes beyond the stdlib.
