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
> `$(go2csPath)` to that root. That firms up the reference-resolution story below (§3.4/§3.5). The
> converter side (`-recurse`, phases P1–P5) is the remaining implementation goal.

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

This is the seam the **NuGet stdlib** replaces later: `$(go2csPath)core\<pkg>\<pkg>.csproj`
`ProjectReference` → `go.<pkg>` `PackageReference`. Keeping the reference indirection through
`$(go2csPath)` now means the NuGet switch is a reference-rewrite, not a structural change.

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
- **P4 — solution generation** for the app + third-party + stdlib references.
- **P5 — test harness** (§5).

Each phase is independently reviewable; P1 is a pure refactor with a strong existing regression gate.

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

## 6. Open decisions (for the user)

1. **Flag vs. auto-detect** — start with explicit `-recurse` (recommended), or auto-recurse when a
   third-party import is present?
2. **Module version in the output path** — strip `@<version>` (single-version, simplest) or preserve it
   (supports multiple versions of one module, mirrors the cache)?
3. **Which pre-converted stdlib to reference** — *resolved by the `deploy-core` modes*: `stdlib` (full
   `src/go-src-converted`, compilable) or `stub` (runnable baseline); both stage to
   `%GOPATH%\src\go2cs\core`.
4. **Test app** — `fatih/color` (small DAG, recommended) vs. a zero-dep lib (simplest one-hop).

## 7. Relationship to the NuGet stdlib decision

This design deliberately keeps the stdlib behind the `$(go2csPath)core` reference indirection so that the
future switch to a **NuGet stdlib** (post-Phase-4; see the pros/cons in the project thread /
[`Roadmap.md`](Roadmap.md)) is a `ProjectReference` → `PackageReference` rewrite. The same applies to
third-party libs: because the template already sets `GeneratePackageOnBuild`/`PackageId`, a converted
third-party lib is itself NuGet-publishable — so "convert once, reference everywhere" generalizes beyond
the stdlib.
