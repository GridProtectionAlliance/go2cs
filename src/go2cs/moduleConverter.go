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
	options   Options
	graph     *DependencyGraph
	startTime time.Time
}

// NewModuleConverter creates a recursive end-user module converter.
func NewModuleConverter(options Options) *ModuleConverter {
	return &ModuleConverter{
		options:   options,
		graph:     NewDependencyGraph(),
		startTime: time.Now(),
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

// convertAll converts every convert-set package in the sorted (least-dependencies-first) order,
// surviving a panic in any single package so the rest still convert (partial compile acceptable).
func (m *ModuleConverter) convertAll() {
	total := len(m.graph.sortedQueue)
	fmt.Printf("Converting %d packages in dependency order...\n", total)

	var failed []string

	for i, pkgPath := range m.graph.sortedQueue {
		pkg := m.graph.packages[pkgPath]

		fmt.Printf("[%d/%d] Converting %s\n", i+1, total, pkgPath)

		// Convert in place (P2): output co-located with the Go source. This is correct for the app
		// and for `replace`d/co-located third-party modules; P3 routes read-only module-cache
		// dependencies to a writable $(go2csPath)pkg output instead.
		err := func() (err error) {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("panic converting %s: %v", pkgPath, r)
				}
			}()

			processConversion(pkg.Dir, true, pkg.Dir, m.options)
			return nil
		}()

		if err != nil {
			log.Printf("WARNING: %v", err)
			failed = append(failed, pkgPath)
		}
	}

	fmt.Printf("\nRecursive conversion complete in %s: %d/%d packages converted",
		formatDuration(time.Since(m.startTime)), total-len(failed), total)

	if len(failed) > 0 {
		fmt.Printf(" (%d failed: %s)", len(failed), strings.Join(failed, ", "))
	}

	fmt.Println()
}
