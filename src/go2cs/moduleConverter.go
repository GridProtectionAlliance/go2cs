// This file drives -recurse: recursive end-user module conversion. ModuleConverter converts a
// downloaded Go application PLUS every third-party dependency package in its transitive closure,
// in dependency order (least dependencies first), while REFERENCING (not converting) the
// pre-converted standard library. It is the module-mode counterpart to StdLibConverter; both
// order conversion through the shared DependencyGraph (dependencyGraph.go).
package main

import (
	"fmt"
	"log"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"time"

	"golang.org/x/tools/go/packages"
)

// packageClass classifies a package in the input module's transitive import closure.
type packageClass int

const (
	// classStdLib — under $GOROOT/src (including GOROOT-vendored golang.org/x/…). Referenced as
	// $(go2csPath)core\… against a pre-converted stdlib; never converted here.
	classStdLib packageClass = iota
	// classApp — a package of the input (main) module. Converted.
	classApp
	// classThirdParty — a package of a non-main dependency module (module cache or a `replace`).
	// Converted.
	classThirdParty
	// classSkip — unsafe/builtin/cgo pseudo-packages, test variants, or anything unclassifiable.
	// Ignored.
	classSkip
)

// ModuleConverter converts an end-user module and its third-party dependency closure in
// dependency order, referencing the pre-converted standard library.
type ModuleConverter struct {
	options           Options
	graph             *DependencyGraph
	startTime         time.Time
	convertedProjects []string          // csproj paths of successfully converted app + third-party packages
	convertedCsproj   map[string]string // import path -> its generated .csproj path (successful conversions)
}

// NewModuleConverter creates a recursive end-user module converter.
func NewModuleConverter(options Options) *ModuleConverter {
	return &ModuleConverter{
		options:         options,
		graph:           NewDependencyGraph(),
		startTime:       time.Now(),
		convertedCsproj: make(map[string]string),
	}
}

// ConvertModule loads the module rooted at moduleDir, partitions its transitive import closure into
// standard library (referenced), third-party, and app packages, then converts the app + third-party
// packages in dependency order (least dependencies first). The standard library is not converted —
// its imports emit $(go2csPath)core references to a pre-converted stdlib (staged by deploy-core).
func (m *ModuleConverter) ConvertModule(moduleDir string) error {
	fmt.Printf("Recursively converting module at %s\n", moduleDir)

	// 1. Load the module and its full dependency closure. This load is used only to DISCOVER and
	//    CLASSIFY the closure (import paths, source dirs, module identity) and to build the
	//    dependency graph — each package is re-loaded with full syntax/types when it is converted,
	//    via processConversion. NeedModule lets us classify by module identity (main vs. dependency).
	closure, err := m.loadClosure(moduleDir)

	if err != nil {
		return err
	}

	// 2. Partition the closure and populate the convert-set (app + third-party). Stdlib packages
	//    are deliberately left OUT of the graph, so edges to them never constrain the conversion
	//    order — they are pre-converted and only referenced.
	m.partition(closure)

	if len(m.graph.packages) == 0 {
		return fmt.Errorf("found no app or third-party packages to convert in module at %s", moduleDir)
	}

	// 3. Build dependency edges among the convert-set and order least-dependencies-first, so each
	//    importer observes its dependency's finished package_info.cs (imported collision-rename
	//    aliases) before it is converted.
	m.buildEdges(closure)
	m.graph.sortAdjacency()
	m.graph.topologicalSort()

	// 4. Convert app + third-party packages in dependency order.
	m.convertAll()

	// 5. Emit a flat .slnx over the converted app + third-party projects at the deploy root, tying
	//    them to the pre-converted stdlib (referenced via $(go2csPath)core) for one dotnet build.
	if err := m.generateSolutionFile(); err != nil {
		fmt.Printf("WARNING: failed to generate solution file: %v\n", err)
	}

	// 6. Emit a per-project .slnx next to each converted .csproj, over that project + its transitive
	//    converted dependencies + golib + the analyzer — so any one package (notably the app) can be
	//    opened/built on its own without the full deploy-root solution.
	m.generatePerProjectSolutions()

	return nil
}

// loadClosure loads the module at moduleDir and returns its transitive import closure keyed by
// import path (roots + every reachable import, including the standard library, which is classified
// out later).
func (m *ModuleConverter) loadClosure(moduleDir string) (map[string]*packages.Package, error) {
	cfg := &packages.Config{
		Mode: packages.LoadAllSyntax | packages.NeedModule,
		Dir:  moduleDir,
	}

	targetParts := strings.Split(m.options.targetPlatform, "/")

	if len(targetParts) == 2 {
		cfg.Env = append(os.Environ(), "GOOS="+targetParts[0], "GOARCH="+targetParts[1])
	}

	pkgs, err := packages.Load(cfg, "./...")

	if err != nil {
		return nil, fmt.Errorf("failed to load module at %q: %w", moduleDir, err)
	}

	if len(pkgs) == 0 {
		return nil, fmt.Errorf("no packages found in module at %q", moduleDir)
	}

	// Report load errors on the root (app) packages as warnings — non-fatal, since a package with
	// errors may still convert usefully (success criterion is a buildable solution; partial compile
	// is acceptable at this stage).
	for _, pkg := range pkgs {
		for _, e := range pkg.Errors {
			log.Printf("WARNING: load error in %s: %v", pkg.PkgPath, e)
		}
	}

	// Collect the transitive closure (roots + all reachable imports), deduped by import path.
	closure := make(map[string]*packages.Package)

	var walk func(p *packages.Package)

	walk = func(p *packages.Package) {
		if p == nil {
			return
		}

		if _, seen := closure[p.PkgPath]; seen {
			return
		}

		closure[p.PkgPath] = p

		for _, imp := range p.Imports {
			walk(imp)
		}
	}

	for _, p := range pkgs {
		walk(p)
	}

	return closure, nil
}

// classify buckets a package by source location and module identity — more robust than a
// dotted-domain heuristic: stdlib is recognized by its location under $GOROOT/src (covering
// GOROOT-vendored packages too), the app by main-module membership, and third-party by
// dependency-module membership.
func (m *ModuleConverter) classify(pkg *packages.Package) packageClass {
	pkgPath := pkg.PkgPath

	// Pseudo-packages (unsafe/builtin have no real source to convert; C is cgo) and test variants.
	if pkgPath == "unsafe" || pkgPath == "builtin" || pkgPath == "C" || strings.HasSuffix(pkgPath, "_test") {
		return classSkip
	}

	if pkg.Dir == "" {
		return classSkip
	}

	// Standard library (including GOROOT-vendored golang.org/x/… under $GOROOT/src/vendor).
	if isPathUnder(pkg.Dir, filepath.Join(m.options.goRoot, "src")) {
		return classStdLib
	}

	// The input module's own packages.
	if pkg.Module != nil && pkg.Module.Main {
		return classApp
	}

	// A dependency module (module cache or a `replace`).
	if pkg.Module != nil {
		return classThirdParty
	}

	// No module and not under GOROOT — unexpected in module mode; leave it referenced-only.
	return classSkip
}

// partition classifies every closure package and adds the app + third-party packages to the graph
// as the convert-set, printing a one-line census.
func (m *ModuleConverter) partition(closure map[string]*packages.Package) {
	var appCount, thirdPartyCount, stdlibCount, skipCount int

	for pkgPath, pkg := range closure {
		switch m.classify(pkg) {
		case classApp:
			m.graph.AddPackage(pkgPath, pkg.Dir)
			// Record the app's module path so outputDirFor / reference emission can route the app's
			// own packages to src\ and every dependency to pkg\ (all app packages share this module).
			if pkg.Module != nil {
				m.options.mainModulePath = pkg.Module.Path
			}
			appCount++
		case classThirdParty:
			m.graph.AddPackage(pkgPath, pkg.Dir)
			thirdPartyCount++
		case classStdLib:
			stdlibCount++
		case classSkip:
			skipCount++
		}
	}

	fmt.Printf("Closure: %d packages discovered — converting %d app + %d third-party, referencing %d stdlib (%d skipped)\n",
		len(closure), appCount, thirdPartyCount, stdlibCount, skipCount)
}

// buildEdges records intra-convert-set dependency edges for every convert-set package. Imports to
// the standard library (not in the convert-set) are filtered out by the graph, so they never
// constrain the order.
func (m *ModuleConverter) buildEdges(closure map[string]*packages.Package) {
	for pkgPath, pkg := range closure {
		if !m.graph.Contains(pkgPath) {
			continue
		}

		imports := make([]string, 0, len(pkg.Imports))

		for importPath := range pkg.Imports {
			imports = append(imports, importPath)
		}

		m.graph.addImportEdges(pkgPath, imports)
	}
}

// outputDirFor returns where a convert-set package's C# output is written, in a parallel tree under
// the deploy root that keeps the original Go source pure (nothing is written in place). The app's own
// packages (the main module) go to $(go2csPath)src\<import-path>; every dependency — module-cache or
// a co-located `replace` — goes to $(go2csPath)pkg\<import-path>. The path is built from the
// version-free import path, so a module-cache dep's @version segment is dropped, and it matches the
// reference getRecurseDependencyInfo emits, so reference and output agree.
func (m *ModuleConverter) outputDirFor(pkgPath string) string {
	root := "pkg"

	if isMainModulePackage(pkgPath, m.options.mainModulePath) {
		root = "src"
	}

	return filepath.Join(m.options.go2csPath, root, filepath.FromSlash(pkgPath))
}

// convertAll converts every convert-set package in the sorted (least-dependencies-first) order,
// surviving a panic in any single package so the rest still convert (partial compile acceptable).
func (m *ModuleConverter) convertAll() {
	total := len(m.graph.sortedQueue)
	fmt.Printf("Converting %d packages in dependency order...\n", total)

	var failed []string

	for i, pkgPath := range m.graph.sortedQueue {
		pkg := m.graph.packages[pkgPath]
		outputDir := m.outputDirFor(pkgPath)

		fmt.Printf("[%d/%d] Converting %s\n", i+1, total, pkgPath)

		err := func() (err error) {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("panic converting %s: %v", pkgPath, r)
				}
			}()

			processConversion(pkg.Dir, true, outputDir, m.options)
			return nil
		}()

		if err != nil {
			log.Printf("WARNING: %v", err)
			failed = append(failed, pkgPath)
			continue
		}

		// Record the generated project so the solution can list it. processConversion names the
		// csproj after getProjectName(pkg.Dir) (the dotted module/import path) in the output dir.
		projectName, _ := getProjectName(pkg.Dir, m.options)
		csproj := filepath.Join(outputDir, projectName+".csproj")
		m.convertedProjects = append(m.convertedProjects, csproj)
		m.convertedCsproj[pkgPath] = csproj
	}

	fmt.Printf("\nRecursive conversion complete in %s: %d/%d packages converted",
		formatDuration(time.Since(m.startTime)), total-len(failed), total)

	if len(failed) > 0 {
		fmt.Printf(" (%d failed: %s)", len(failed), strings.Join(failed, ", "))
	}

	fmt.Println()
}

// recurseSolutionFileName is the flat .slnx ModuleConverter writes at the deploy root over the
// converted app + third-party projects.
const recurseSolutionFileName = "go2cs-recurse.slnx"

// generateSolutionFile writes a flat .slnx at the deploy root ($(go2csPath)) listing the converted
// app + third-party projects. It is placed at the deploy root so that building it makes
// $(SolutionDir) resolve to that root — the pre-converted stdlib (referenced via $(go2csPath)core),
// the third-party libs (under pkg\...), golib, and the analyzer then all resolve and build
// transitively. Project paths are emitted relative to the root (forward-slash), sorted for
// deterministic output.
func (m *ModuleConverter) generateSolutionFile() error {
	if len(m.convertedProjects) == 0 {
		return nil
	}

	root := m.options.go2csPath

	rels := make([]string, 0, len(m.convertedProjects))

	for _, csproj := range m.convertedProjects {
		rel, err := filepath.Rel(root, csproj)

		if err != nil {
			rel = csproj
		}

		rels = append(rels, filepath.ToSlash(rel))
	}

	sort.Strings(rels)

	contents := buildFlatSolutionXML(rels)
	solutionFile := filepath.Join(root, recurseSolutionFileName)

	// The deploy root may not exist yet — e.g. an all-in-place conversion (a co-located `replace`
	// module with no module-cache deps) writes nothing else under it. Create it before writing.
	if err := os.MkdirAll(root, 0o755); err != nil {
		return fmt.Errorf("failed to create deploy root %q: %w", root, err)
	}

	if needToWriteFile(solutionFile, []byte(contents)) {
		if err := os.WriteFile(solutionFile, []byte(contents), 0644); err != nil {
			return fmt.Errorf("failed to write recurse solution file %q: %w", solutionFile, err)
		}
	}

	fmt.Printf("Solution file generated: %s (%d projects)\n", solutionFile, len(rels))

	return nil
}

// transitiveConvertedDeps returns the import paths of pkgPath's transitive dependencies within the
// convert-set (the app + third-party graph), in deterministic (sorted) order. Standard-library
// imports are not graph nodes, so they are naturally excluded.
func (m *ModuleConverter) transitiveConvertedDeps(pkgPath string) []string {
	seen := make(map[string]bool)

	var visit func(p string)

	visit = func(p string) {
		pkg, ok := m.graph.packages[p]

		if !ok {
			return
		}

		for _, dep := range pkg.Dependencies {
			if !seen[dep] {
				seen[dep] = true
				visit(dep)
			}
		}
	}

	visit(pkgPath)

	deps := make([]string, 0, len(seen))

	for dep := range seen {
		deps = append(deps, dep)
	}

	sort.Strings(deps)

	return deps
}

// relSolutionPath renders target as a forward-slash path relative to baseDir (the solution's own
// directory), falling back to the absolute path if no relative one can be formed (e.g. a different
// drive on Windows).
func relSolutionPath(baseDir, target string) string {
	if rel, err := filepath.Rel(baseDir, target); err == nil {
		return filepath.ToSlash(rel)
	}

	return filepath.ToSlash(target)
}

// generatePerProjectSolutions writes, next to each converted project's .csproj, a sibling .slnx over
// that project plus its transitive converted dependencies and the shared golib runtime and go2cs-gen
// analyzer. The standard library is referenced (via $(go2csPath)core), not listed. Building that one
// solution builds the project and everything it needs from the convert-set — so the app (or any
// single package) can be opened/built on its own, without the full deploy-root go2cs-recurse.slnx.
// Project paths are relative to the .slnx's own directory; the runtime/analyzer live under the
// deploy root ($(go2csPath)core\golib, $(go2csPath)gen\go2cs-gen).
func (m *ModuleConverter) generatePerProjectSolutions() {
	golibCsproj := filepath.Join(m.options.go2csPath, "core", "golib", "golib.csproj")
	genCsproj := filepath.Join(m.options.go2csPath, "gen", "go2cs-gen", "go2cs-gen.csproj")

	written := 0

	for _, pkgPath := range m.graph.sortedQueue {
		csproj, ok := m.convertedCsproj[pkgPath]

		if !ok {
			continue // this package failed to convert — no project to anchor a solution on
		}

		projectDir := filepath.Dir(csproj)

		// The project itself + every transitive converted dependency that also converted, then the
		// shared runtime + analyzer.
		members := []string{csproj}

		for _, dep := range m.transitiveConvertedDeps(pkgPath) {
			if depCsproj, ok := m.convertedCsproj[dep]; ok {
				members = append(members, depCsproj)
			}
		}

		members = append(members, golibCsproj, genCsproj)

		rels := make([]string, 0, len(members))

		for _, member := range members {
			rels = append(rels, relSolutionPath(projectDir, member))
		}

		sort.Strings(rels)

		contents := buildFlatSolutionXML(rels)
		slnxFile := filepath.Join(projectDir, strings.TrimSuffix(filepath.Base(csproj), ".csproj")+".slnx")

		if needToWriteFile(slnxFile, []byte(contents)) {
			if err := os.WriteFile(slnxFile, []byte(contents), 0644); err != nil {
				fmt.Printf("WARNING: failed to write per-project solution %q: %v\n", slnxFile, err)
				continue
			}
		}

		written++
	}

	if written > 0 {
		fmt.Printf("Per-project solutions generated: %d (one .slnx next to each converted .csproj)\n", written)
	}
}
