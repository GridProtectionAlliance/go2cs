package main

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"go/ast"
	"go/types"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"runtime/debug"
	"sort"
	"strings"
	"time"
	"unicode"
	"unicode/utf8"

	"golang.org/x/tools/go/packages"
)

// Phase-4 test conversion: converts a package's _test.go variants (in-package and external
// package_test) into a runnable, self-registering C# test project driven by the hand-owned
// go.testing runtime (src/core/testing), plus a machine-readable manifest and a `go test -json`
// differential oracle. Ported from the codex/testing-infrastructure branch (097c94d70) onto the
// shared per-package helpers in packageStateOperations.go and the shared writePackageInfoFile —
// the branch's private copies of that machinery are gone by design (they drifted; see the port
// review in docs/Phase4/BranchReview-codex-testing-infrastructure.md).

const (
	testPackageInfoFileName = "package_test_info.cs"
	testHostFileName        = "go2cs_test_host.cs"
	testManifestFileName    = "go2cs_test_manifest.json"
)

// Markers substituted into test-csproj-template.xml by writeTestProject (embedded-resource
// template, following the csproj-template.xml precedent — never a hardcoded csproj string).
const (
	TestRootNamespaceMarker     = ">>MARKER:TEST_ROOT_NAMESPACE<<"
	TestAssemblyNameMarker      = ">>MARKER:TEST_ASSEMBLY_NAME<<"
	TestGo2CSRelativePathMarker = ">>MARKER:TEST_GO2CS_RELATIVE_PATH<<"
	TestCompileItemsMarker      = ">>MARKER:TEST_COMPILE_ITEMS<<"
	TestFixtureItemsMarker      = ">>MARKER:TEST_FIXTURE_ITEMS<<"
	TestProjectReferencesMarker = ">>MARKER:TEST_PROJECT_REFERENCES<<"
)

const unsupportedCapabilityReasonPrefix = "requires unsupported testing capabilities: "

type testDeclaration struct {
	Name                 string   `json:"name"`
	Kind                 string   `json:"kind"`
	PackageName          string   `json:"packageName"`
	Source               string   `json:"source"`
	Line                 int      `json:"line"`
	Status               string   `json:"status"`
	Reason               string   `json:"reason,omitempty"`
	RequiredCapabilities []string `json:"requiredCapabilities,omitempty"`
}

type testSource struct {
	Path   string `json:"path"`
	Kind   string `json:"kind"`
	Status string `json:"status"`
	Reason string `json:"reason,omitempty"`
}

type testManifest struct {
	SchemaVersion           int               `json:"schemaVersion"`
	CapabilitiesVersion     int               `json:"capabilitiesVersion"`
	PackageImportPath       string            `json:"packageImportPath"`
	ProjectName             string            `json:"projectName"`
	TestProject             string            `json:"testProject"`
	GoVersion               string            `json:"goVersion"`
	TargetGOOS              string            `json:"targetGOOS"`
	TargetGOARCH            string            `json:"targetGOARCH"`
	SourceRevision          string            `json:"sourceRevision,omitempty"`
	ConverterRevision       string            `json:"converterRevision"`
	InputDigest             string            `json:"inputDigest"`
	ProductionFiles         []string          `json:"productionFiles"`
	TestSources             []testSource      `json:"testSources"`
	Fixtures                []string          `json:"fixtures"`
	Tests                   []testDeclaration `json:"tests"`
	TestMain                *testDeclaration  `json:"testMain,omitempty"`
	Dependencies            []string          `json:"dependencies"`
	Capabilities            []string          `json:"capabilities"`
	RequiredCapabilities    []string          `json:"requiredCapabilities"`
	UnsupportedCapabilities []string          `json:"unsupportedCapabilities"`
}

func processTestConversion(inputPath, outputPath string, options Options) error {
	inputPath, err := filepath.Abs(inputPath)
	if err != nil {
		return err
	}

	outputPath, err = filepath.Abs(outputPath)
	if err != nil {
		return err
	}

	targetParts := strings.Split(options.targetPlatform, "/")
	if len(targetParts) != 2 {
		return fmt.Errorf("invalid target platform format %q", options.targetPlatform)
	}

	cfg := &packages.Config{
		Mode:  packages.LoadAllSyntax,
		Dir:   inputPath,
		Tests: true,
		Env: append(os.Environ(),
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}

	loaded, err := packages.Load(cfg, ".")
	if err != nil {
		return fmt.Errorf("load test package variants: %w", err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		return fmt.Errorf("go/packages did not return a production package for %q", inputPath)
	}

	if len(production.Errors) > 0 {
		return fmt.Errorf("production package load failed: %v", production.Errors)
	}

	// External package tests import the package under test. Bind that import to the
	// production partial class compiled into the same test assembly, never to a
	// project reference back to the production DLL.
	options.testPackagePath = production.PkgPath
	options.testPackageName = production.Name

	internal, external := findTestVariants(loaded, production)
	if internal == nil && external == nil {
		return writeNoTestsManifest(production, inputPath, outputPath, targetParts, options)
	}

	// Seed package_test_info.cs from the production package_info.cs so the production
	// assembly-level metadata carries over verbatim; each converted variant's ADDITIONS are then
	// merged in by the shared writePackageInfoFile (identical emission semantics to production —
	// pointer-form unwrapping, dedup, pruning — because it IS the production writer).
	productionInfoPath := filepath.Join(outputPath, PackageInfoFileName)
	productionInfo, err := os.ReadFile(productionInfoPath)
	if err != nil {
		return fmt.Errorf("read production package metadata (convert the package itself before its tests): %w", err)
	}

	testInfoPath := filepath.Join(outputPath, testPackageInfoFileName)
	if err := os.WriteFile(testInfoPath, productionInfo, 0644); err != nil {
		return fmt.Errorf("seed test package metadata: %w", err)
	}

	projectName, projectNamespace := getProjectName(inputPath, options)
	supported := NewHashSet(supportedTestCapabilities())
	allImports := HashSet[string]{}
	for importPath := range production.Imports {
		allImports.Add(importPath)
	}
	requiredCapabilities := HashSet[string]{}
	outputFiles := make([]string, 0)
	declarations := make([]testDeclaration, 0)
	includedSources := HashSet[string]{}
	var testMain *testDeclaration

	for _, variant := range []*packages.Package{internal, external} {
		if variant == nil {
			continue
		}

		if len(variant.Errors) > 0 {
			return fmt.Errorf("test package variant %q failed to load: %v", variant.ID, variant.Errors)
		}

		entries := testFileEntries(variant)
		if len(entries) == 0 {
			continue
		}

		for _, entry := range entries {
			includedSources.Add(filepath.Clean(entry.filePath))
		}

		capabilities := analyzeTestingCapabilities(variant)
		found, foundMain := discoverTestDeclarations(variant, entries, inputPath, capabilities, supported)
		declarations = append(declarations, found...)

		// Package-level capability reporting aggregates over RUNNABLE declaration kinds only
		// (tests + TestMain) — benchmark/fuzz/example requirements must not block the package,
		// they are excluded-disclosed by their own status (F4: attribution is per-test).
		for _, declaration := range found {
			if declaration.Kind == "test" {
				requiredCapabilities.UnionWith(declaration.RequiredCapabilities)
			}
		}

		if foundMain != nil {
			if testMain != nil {
				return fmt.Errorf("multiple valid TestMain declarations: %s and %s", testMain.Source, foundMain.Source)
			}
			testMain = foundMain
			requiredCapabilities.UnionWith(foundMain.RequiredCapabilities)
		}

		variantOutputs, imports, err := convertTestVariant(variant, entries, outputPath, projectNamespace, options)
		if err != nil {
			return err
		}

		// Merge this variant's collected metadata globals into package_test_info.cs while they
		// are still live (the next variant's conversion resets them).
		writePackageInfoFile(testInfoPath, true)

		outputFiles = append(outputFiles, variantOutputs...)
		allImports.UnionWithSet(imports)
	}

	if err := appendExternalTestPackageClass(testInfoPath, external); err != nil {
		return err
	}

	sort.Slice(declarations, func(i, j int) bool {
		if declarations[i].Name == declarations[j].Name {
			return declarations[i].PackageName < declarations[j].PackageName
		}
		return declarations[i].Name < declarations[j].Name
	})
	sort.Strings(outputFiles)

	fixtures, err := copyTestFixtures(inputPath, outputPath)
	if err != nil {
		return err
	}

	productionFiles, err := productionCSFiles(outputPath)
	if err != nil {
		return err
	}

	if err := writeTestHost(outputPath, projectNamespace, production.PkgPath, declarations, testMain, fixtures); err != nil {
		return err
	}

	dependencies := allImports.Keys()
	dependencies = removeString(dependencies, production.PkgPath)
	dependencies = removeString(dependencies, "testing")
	sort.Strings(dependencies)

	testProjectName := projectName + ".tests.csproj"
	if err := writeTestProject(filepath.Join(outputPath, testProjectName), projectName, projectNamespace, productionFiles, outputFiles, fixtures, dependencies, options); err != nil {
		return err
	}

	sources, err := classifyTestSources(inputPath, includedSources, external)
	if err != nil {
		return err
	}

	capabilities := supportedTestCapabilities()
	required := requiredCapabilities.Keys()
	sort.Strings(required)
	unsupported := NewHashSet(required)
	unsupported.ExceptWith(capabilities)
	unsupportedList := unsupported.Keys()
	sort.Strings(unsupportedList)

	manifest := testManifest{
		SchemaVersion:           1,
		CapabilitiesVersion:     1,
		PackageImportPath:       production.PkgPath,
		ProjectName:             projectName,
		TestProject:             testProjectName,
		GoVersion:               runtime.Version(),
		TargetGOOS:              targetParts[0],
		TargetGOARCH:            targetParts[1],
		SourceRevision:          gitRevision(inputPath),
		ConverterRevision:       converterRevision(),
		ProductionFiles:         productionFiles,
		TestSources:             sources,
		Fixtures:                fixtures,
		Tests:                   declarations,
		TestMain:                testMain,
		Dependencies:            dependencies,
		Capabilities:            capabilities,
		RequiredCapabilities:    required,
		UnsupportedCapabilities: unsupportedList,
	}

	manifest.InputDigest, err = testInputDigest(inputPath, outputPath, options, manifest.ConverterRevision)
	if err != nil {
		return err
	}

	return writeJSONFile(filepath.Join(outputPath, testManifestFileName), manifest)
}

func findProductionPackage(pkgs []*packages.Package, inputPath string) *packages.Package {
	for _, pkg := range pkgs {
		if pkg.Name == "main" || strings.Contains(pkg.ID, "[") {
			continue
		}

		if samePath(pkg.Dir, inputPath) {
			return pkg
		}
	}

	return nil
}

func findTestVariants(pkgs []*packages.Package, production *packages.Package) (internal, external *packages.Package) {
	testID := production.PkgPath + ".test]"

	for _, pkg := range pkgs {
		if !strings.HasSuffix(pkg.ID, testID) || !strings.Contains(pkg.ID, "[") {
			continue
		}

		switch {
		case pkg.PkgPath == production.PkgPath && pkg.Name == production.Name:
			internal = pkg
		case pkg.Name == production.Name+"_test":
			external = pkg
		}
	}

	return internal, external
}

func testFileEntries(pkg *packages.Package) []FileEntry {
	entries := make([]FileEntry, 0)

	for i, file := range pkg.Syntax {
		if i >= len(pkg.CompiledGoFiles) {
			break
		}

		path := pkg.CompiledGoFiles[i]
		if strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
			entries = append(entries, newFileEntry(file, path, false))
		}
	}

	return entries
}

// convertTestVariant converts one test package variant's _test.go files into C# in outputPath.
// The whole variant (production + test files) feeds the package-wide analyses so the test files
// convert with complete state, but only the test files are EMITTED — the production .cs already
// exist from the normal conversion and are recompiled into the test assembly as-is.
//
// Files convert SEQUENTIALLY in pkg.Syntax order for byte-reproducible output, mirroring
// processConversion (the per-file visitors share package-level state claimed at visit time; the
// branch's concurrent goroutines reproduced exactly the nondeterminism master removed).
func convertTestVariant(pkg *packages.Package, testEntries []FileEntry, outputPath, projectNamespace string, options Options) ([]string, HashSet[string], error) {
	resetPackageState(pkg)
	packageNamespace = projectNamespace

	allEntries := make([]FileEntry, 0, len(pkg.Syntax))
	entryByPath := make(map[string]*FileEntry, len(pkg.Syntax))

	for i, file := range pkg.Syntax {
		if i >= len(pkg.CompiledGoFiles) {
			break
		}

		allEntries = append(allEntries, newFileEntry(file, pkg.CompiledGoFiles[i], false))
		entryByPath[filepath.Clean(pkg.CompiledGoFiles[i])] = &allEntries[len(allEntries)-1]
	}

	selected := make([]FileEntry, 0, len(testEntries))
	for _, requested := range testEntries {
		entry := entryByPath[filepath.Clean(requested.filePath)]
		if entry == nil {
			continue
		}

		outputFile := filepath.Join(outputPath, strings.TrimSuffix(filepath.Base(entry.filePath), ".go")+".cs")
		manual, err := containsManualConversionMarker(outputFile)
		if err != nil {
			return nil, nil, err
		}

		// A hand-owned (GoManualConversion-marked) test `.cs` is never overwritten, but its
		// source stays in the convert set — its visit feeds package-wide emission state that
		// sibling files depend on; only its EMISSION redirects, to the non-compiled `.cs.auto`
		// review sibling. Same semantics as processConversion's marked-file flow — dropping the
		// visit corrupts sibling emission.
		// The copy shares the analysis maps with the allEntries element (maps are references).
		selectedEntry := *entry
		selectedEntry.manualConversion = manual
		selected = append(selected, selectedEntry)
	}

	if len(selected) == 0 {
		return nil, HashSet[string]{}, nil
	}

	globalIdentNames := make(map[*ast.Ident]string)
	globalScope := map[string]*types.Var{}

	// Mirror processConversion's package-wide analysis sequence — a test file is an ordinary Go
	// file and needs the same emission inputs. collectMovedInitVars is deliberately NOT run: the
	// test project has no package_init.cs emission path yet, and marking vars as relocated
	// without the emitter would drop their initializers (test-file package vars keep their
	// inline-initializer emission; revisit if a real suite surfaces a cross-file init-order
	// dependency in its _test.go files).
	performNameCollisionAnalysis(pkg)

	for _, entry := range allEntries {
		performGlobalVariableAnalysis(entry.file.Decls, pkg.TypesInfo, globalIdentNames, globalScope)
	}

	collectCaptureModeMethods(pkg)
	collectTypeSpecRHS(pkg)
	performEscapeAnalysis(allEntries, pkg.Fset, pkg.Types, pkg.TypesInfo)
	collectAddressedGlobals(allEntries, pkg.Types, pkg.TypesInfo)
	computeImportAliasRenames(allEntries, pkg.Types, packageNamespace)
	collectPublicizedTypes(pkg.Types)
	preloadImportedTypeAliases(allEntries, options)

	var compileNames []string // emitted test .cs basenames — the csproj's compile items
	var resolveNames []string // every emission (incl. .cs.auto review siblings) for marker resolution

	convert := func(entry FileEntry) (err error) {
		if !options.debugMode {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("convert test file %q: %v", entry.filePath, r)
				}
			}()
		}

		visitor := newFileVisitor(pkg.Fset, pkg.Types, pkg.TypesInfo, options, globalIdentNames, globalScope, entry)
		visitor.visitFile(entry.file)

		baseName := strings.TrimSuffix(filepath.Base(entry.filePath), ".go")

		if entry.manualConversion {
			// Hand-owned destination: the visit above already fed this file's package-wide state;
			// emit the auto conversion to the `.cs.auto` review sibling, leaving the marked `.cs`
			// untouched. The HAND-OWNED `.cs` is the compile item; the `.cs.auto` sibling never is.
			outputName := filepath.Join(outputPath, baseName+".cs.auto")
			if writeErr := writeAutoConversionSibling(outputName, baseName, visitor.targetFile.String()); writeErr != nil {
				showWarning("%s", writeErr)
			}

			projectImports.UnionWithSet(visitor.importQueue)
			compileNames = append(compileNames, baseName+".cs")
			resolveNames = append(resolveNames, outputName)
			return nil
		}

		outputName := filepath.Join(outputPath, baseName+".cs")
		if writeErr := visitor.writeOutputFile(outputName); writeErr != nil {
			return writeErr
		}

		projectImports.UnionWithSet(visitor.importQueue)
		compileNames = append(compileNames, filepath.Base(outputName))
		resolveNames = append(resolveNames, outputName)
		return nil
	}

	for _, entry := range selected {
		if err := convert(entry); err != nil {
			return nil, nil, err
		}
	}

	resolveDynamicTypeMarkers(resolveNames)

	return compileNames, NewHashSet(projectImports.Keys()), nil
}

// appendExternalTestPackageClass appends the external test package's [GoPackage] partial class
// declaration to package_test_info.cs — converted external-test files declare partial pieces of
// <name>_test_package, and this block is the attribute-bearing anchor the production
// package_info.cs provides for the production class.
func appendExternalTestPackageClass(testInfoPath string, external *packages.Package) error {
	if external == nil {
		return nil
	}

	data, err := os.ReadFile(testInfoPath)
	if err != nil {
		return fmt.Errorf("read test package metadata: %w", err)
	}

	className := getSanitizedImport(external.Name + PackageSuffix)
	block := fmt.Sprintf("\r\n[GoPackage(\"%s\")]\r\npublic static partial class %s\r\n{\r\n}\r\n", external.Name, className)

	if strings.Contains(string(data), block) {
		return nil
	}

	return os.WriteFile(testInfoPath, append(data, []byte(block)...), 0644)
}

// discoverTestDeclarations finds every go-test-shaped top-level declaration in the variant's
// selected test files and classifies it. Disclosure is total by design (req §2.7): every
// discovered Test*/Benchmark*/Fuzz*/Example*/TestMain declaration lands in the manifest with an
// explicit status — nothing is silently absent. Capability gating is PER TEST (F4): a test whose
// transitive call closure requires capabilities outside the supported list blocks itself
// (status "unsupported" + reason), not its package.
func discoverTestDeclarations(pkg *packages.Package, entries []FileEntry, inputPath string, capabilities testCapabilityAnalysis, supported HashSet[string]) ([]testDeclaration, *testDeclaration) {
	selected := make(map[*ast.File]string, len(entries))
	for _, entry := range entries {
		selected[entry.file] = entry.filePath
	}

	result := make([]testDeclaration, 0)
	var testMain *testDeclaration

	for _, file := range pkg.Syntax {
		path, ok := selected[file]
		if !ok {
			continue
		}

		relPath, _ := filepath.Rel(inputPath, path)
		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Recv != nil || fn.Name == nil {
				continue
			}

			obj, ok := pkg.TypesInfo.Defs[fn.Name].(*types.Func)
			if !ok {
				continue
			}

			sig, ok := obj.Type().(*types.Signature)
			if !ok || sig.TypeParams().Len() != 0 || sig.Results().Len() != 0 {
				continue
			}

			name := fn.Name.Name
			position := pkg.Fset.Position(fn.Pos())
			entry := testDeclaration{Name: name, PackageName: file.Name.Name, Source: filepath.ToSlash(relPath), Line: position.Line}

			requirements := capabilities.requiredFor(obj)
			required := requirements.Keys()
			sort.Strings(required)
			entry.RequiredCapabilities = required

			// Example functions take no parameters (F2): `go test` runs them with Output:
			// comparison, so they MUST appear in the manifest — disclosed-unsupported until
			// Phase 4D — or the differential oracle would silently under-compare.
			if sig.Params().Len() == 0 {
				if isGoTestName(name, "Example") {
					entry.Kind, entry.Status, entry.Reason = "example", "unsupported", "example execution is deferred to Phase 4D"
					result = append(result, entry)
				}
				continue
			}

			if sig.Params().Len() != 1 {
				continue
			}

			switch {
			case name == "TestMain" && isTestingPointer(sig.Params().At(0).Type(), "M"):
				entry.Kind = "test-main"
				entry.Status = "included"
				applyCapabilityGate(&entry, requirements, supported)
				declarationCopy := entry
				testMain = &declarationCopy
			case isGoTestName(name, "Test") && isTestingPointer(sig.Params().At(0).Type(), "T"):
				entry.Kind = "test"
				entry.Status = "included"
				applyCapabilityGate(&entry, requirements, supported)
				result = append(result, entry)
			case isGoTestName(name, "Benchmark") && isTestingPointer(sig.Params().At(0).Type(), "B"):
				entry.Kind, entry.Status, entry.Reason = "benchmark", "unsupported", "benchmark execution is deferred to Phase 4D"
				result = append(result, entry)
			case isGoTestName(name, "Fuzz") && isTestingPointer(sig.Params().At(0).Type(), "F"):
				entry.Kind, entry.Status, entry.Reason = "fuzz", "unsupported", "fuzz execution is deferred to Phase 4D"
				result = append(result, entry)
			}
		}
	}

	return result, testMain
}

// applyCapabilityGate downgrades an included declaration to disclosed-unsupported when its
// transitive capability requirements exceed the supported list.
func applyCapabilityGate(entry *testDeclaration, requirements HashSet[string], supported HashSet[string]) {
	unsupported := NewHashSet(requirements.Keys())
	unsupported.ExceptWith(supported.Keys())

	if unsupported.IsEmpty() {
		return
	}

	blocked := unsupported.Keys()
	sort.Strings(blocked)
	entry.Status = "unsupported"
	entry.Reason = unsupportedCapabilityReasonPrefix + strings.Join(blocked, ", ")
}

func isTestingPointer(t types.Type, typeName string) bool {
	pointer, ok := t.(*types.Pointer)
	if !ok {
		return false
	}
	named, ok := pointer.Elem().(*types.Named)
	if !ok || named.Obj() == nil || named.Obj().Pkg() == nil {
		return false
	}
	return named.Obj().Pkg().Path() == "testing" && named.Obj().Name() == typeName
}

func isGoTestName(name, prefix string) bool {
	if !strings.HasPrefix(name, prefix) {
		return false
	}
	if len(name) == len(prefix) {
		return true
	}
	r, _ := utf8.DecodeRuneInString(name[len(prefix):])
	return !unicode.IsLower(r)
}

func supportedTestCapabilities() []string {
	capabilities := []string{
		"T.Cleanup", "T.Error", "T.Errorf", "T.Fail", "T.FailNow", "T.Failed",
		"T.Fatal", "T.Fatalf", "T.Helper", "T.Log", "T.Logf", "T.Name", "T.Parallel",
		"T.Run", "T.Setenv", "T.Skip", "T.SkipNow", "T.Skipf", "T.Skipped", "T.TempDir", "M.Run",
		"testing.Short", "testing.Verbose",
	}
	sort.Strings(capabilities)
	return capabilities
}

// testCapabilityAnalysis is the per-variant capability attribution input (F4): the testing.*
// members each function uses DIRECTLY, and the static same-package reference graph used to close
// over helpers. References are collected conservatively (any use of a same-package function, not
// just direct calls), so a capability reached through a stored function value still gates the
// test that stores it; cross-package helpers (e.g. internal/testenv) are outside the graph and
// gate through their own package's conversion instead.
type testCapabilityAnalysis struct {
	direct   map[*types.Func]HashSet[string]
	referees map[*types.Func]map[*types.Func]bool
}

// analyzeTestingCapabilities walks every function declaration in the variant (production and
// test files alike — helpers can live in either) recording direct testing.* usage and the
// same-package reference graph. The receiver filter is deliberately absent (F5): a helper taking
// *testing.B contributes B.* requirements, so a supported-kind test calling it is gated instead
// of sailing through.
func analyzeTestingCapabilities(pkg *packages.Package) testCapabilityAnalysis {
	analysis := testCapabilityAnalysis{
		direct:   make(map[*types.Func]HashSet[string]),
		referees: make(map[*types.Func]map[*types.Func]bool),
	}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Name == nil {
				continue
			}

			obj, ok := pkg.TypesInfo.Defs[fn.Name].(*types.Func)
			if !ok {
				continue
			}

			direct := HashSet[string]{}
			referees := make(map[*types.Func]bool)

			if fn.Body != nil {
				ast.Inspect(fn.Body, func(node ast.Node) bool {
					switch expr := node.(type) {
					case *ast.SelectorExpr:
						if selection := pkg.TypesInfo.Selections[expr]; selection != nil {
							member := selection.Obj()
							if member == nil || member.Pkg() == nil || member.Pkg().Path() != "testing" {
								return true
							}

							receiver := selection.Recv()
							if pointer, ok := receiver.(*types.Pointer); ok {
								receiver = pointer.Elem()
							}
							if named, ok := receiver.(*types.Named); ok && named.Obj().Pkg() != nil && named.Obj().Pkg().Path() == "testing" {
								direct.Add(named.Obj().Name() + "." + member.Name())
							}
						} else if member := pkg.TypesInfo.Uses[expr.Sel]; member != nil && member.Pkg() != nil && member.Pkg().Path() == "testing" {
							if _, ok := member.(*types.Func); ok {
								direct.Add("testing." + member.Name())
							}
						}
					case *ast.Ident:
						if referee, ok := pkg.TypesInfo.Uses[expr].(*types.Func); ok && referee.Pkg() == pkg.Types {
							referees[referee] = true
						}
					}
					return true
				})
			}

			analysis.direct[obj] = direct
			analysis.referees[obj] = referees
		}
	}

	return analysis
}

// requiredFor returns the transitive testing.* capability requirements of fn — its own direct
// usage plus that of every same-package function reachable through the reference graph.
func (a testCapabilityAnalysis) requiredFor(fn *types.Func) HashSet[string] {
	required := HashSet[string]{}
	visited := make(map[*types.Func]bool)

	var walk func(current *types.Func)
	walk = func(current *types.Func) {
		if visited[current] {
			return
		}
		visited[current] = true

		if direct, ok := a.direct[current]; ok {
			required.UnionWithSet(direct)
		}
		for referee := range a.referees[current] {
			walk(referee)
		}
	}

	walk(fn)
	return required
}

func writeTestHost(outputPath, namespace, importPath string, declarations []testDeclaration, testMain *testDeclaration, fixtures []string) error {
	var b strings.Builder
	b.WriteString("// Code generated by go2cs test conversion. DO NOT EDIT.\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n\r\n", namespace))
	b.WriteString("using go.testing_runtime;\r\n\r\n")
	b.WriteString("internal static class Go2CsTestHost\r\n{\r\n")
	b.WriteString("    public static int Main(string[] args)\r\n    {\r\n")
	b.WriteString(fmt.Sprintf("        TestRegistry registry = new(\"%s\", new string[]\r\n        {\r\n", escapeCSharp(importPath)))
	for _, fixture := range fixtures {
		b.WriteString(fmt.Sprintf("            \"%s\",\r\n", escapeCSharp(filepath.ToSlash(fixture))))
	}
	b.WriteString("        });\r\n")

	for _, test := range declarations {
		if test.Kind != "test" || test.Status != "included" {
			continue
		}
		className := getSanitizedImport(test.PackageName + PackageSuffix)
		methodName := getSanitizedFunctionName(test.Name)
		b.WriteString(fmt.Sprintf("        registry.Add(\"%s\", %s.%s, \"%s\", %d);\r\n", escapeCSharp(test.Name), className, methodName, escapeCSharp(test.Source), test.Line))
	}

	if testMain != nil && testMain.Status == "included" {
		className := getSanitizedImport(testMain.PackageName + PackageSuffix)
		b.WriteString(fmt.Sprintf("        registry.SetTestMain(%s.%s);\r\n", className, getSanitizedFunctionName(testMain.Name)))
	}

	b.WriteString("        return TestHost.Run(registry, args);\r\n")
	b.WriteString("    }\r\n}\r\n")

	contents := []byte(b.String())
	fileName := filepath.Join(outputPath, testHostFileName)
	if needToWriteFile(fileName, contents) {
		return os.WriteFile(fileName, contents, 0644)
	}
	return nil
}

// writeTestProject emits the test project from the embedded test-csproj-template.xml (following
// the csproj-template.xml precedent). The template carries the static machinery (explicit
// compile items via EnableDefaultCompileItems=false, generated-files exposure, the go2csPath
// fallback chain with the $(HOME) non-Windows fallback, the Go type-alias usings); the markers
// carry the per-project values.
func writeTestProject(projectFile, projectName, namespace string, productionFiles, testFiles, fixtures, dependencies []string, options Options) error {
	references := HashSet[string]{
		`$(go2csPath)core\golib\golib.csproj`:     {},
		`$(go2csPath)core\testing\testing.csproj`: {},
	}

	for _, dependency := range dependencies {
		for _, info := range getImportPackageInfo([]string{dependency}, options) {
			// A dependency that fails to resolve must fail the conversion NAMING the dependency
			// (F14b) — silently dropping the reference would surface later as an uncaused CS0246.
			if info.Err != nil {
				return fmt.Errorf("resolve test project dependency %q: %w", dependency, info.Err)
			}

			reference := resolveTestProjectReference(info)
			if reference != "" && !strings.HasSuffix(strings.ToLower(reference), strings.ToLower(projectName+".csproj")) {
				references.Add(reference)
			}
		}
	}

	relativeGo2CSPath, relErr := filepath.Rel(filepath.Dir(projectFile), options.go2csPath)
	if relErr != nil {
		relativeGo2CSPath = options.go2csPath
	}
	relativeGo2CSPath = strings.TrimRight(filepath.ToSlash(relativeGo2CSPath), "/") + "/"

	var compileItems strings.Builder
	compileFiles := append(append([]string{}, productionFiles...), testFiles...)
	compileFiles = append(compileFiles, testPackageInfoFileName, testHostFileName)
	sort.Strings(compileFiles)
	for _, file := range compileFiles {
		compileItems.WriteString(fmt.Sprintf("\r\n    <Compile Include=\"%s\" />", xmlEscape(filepath.ToSlash(file))))
	}

	var fixtureItems strings.Builder
	for _, fixture := range fixtures {
		fixtureItems.WriteString(fmt.Sprintf("\r\n    <None Include=\"%s\" CopyToOutputDirectory=\"PreserveNewest\" />", xmlEscape(filepath.ToSlash(fixture))))
	}

	var referenceItems strings.Builder
	refs := references.Keys()
	sort.Strings(refs)
	for _, reference := range refs {
		referenceItems.WriteString(fmt.Sprintf("\r\n    <ProjectReference Include=\"%s\" />", xmlEscape(reference)))
	}

	contents := []byte(strings.NewReplacer(
		TestRootNamespaceMarker, namespace,
		TestAssemblyNameMarker, projectName+".tests",
		TestGo2CSRelativePathMarker, xmlEscape(relativeGo2CSPath),
		TestCompileItemsMarker, compileItems.String(),
		TestFixtureItemsMarker, fixtureItems.String(),
		TestProjectReferencesMarker, referenceItems.String(),
	).Replace(string(testCsprojTemplate)))

	if needToWriteFile(projectFile, contents) {
		return os.WriteFile(projectFile, contents, 0644)
	}
	return nil
}

// resolveTestProjectReference maps a test dependency's project reference per the mixed-tree
// ruling (F15): ALL standard-library dependencies of a test project resolve from the overlaid
// go-src-converted tree — never the baseline core stub — because one build containing both
// trees' "namespace go" partial classes is the collision CLAUDE.md forbids. golib (and the
// hand-owned core/testing shim, added as fixed references by the template) are the only core
// references. The mapping is deterministic — no filesystem probing — so a missing converted
// project surfaces as a loud MSBuild resolve error naming the exact expected path.
//
// KNOWN ITEM (F15b, deferred): a TEST-ONLY dependency that itself imports "testing" (e.g.
// internal/testenv, used by the strings/sort/bytes suites) resolves here to the auto-converted
// go-src-converted/testing — colliding with the hand-owned shim's [GoPackage("testing")] classes
// in the same build. Needs a ruling before such a package converts: exclude go-src-converted/
// testing in favor of the shim, or hand-own a testenv mini-shim (see the first-proof plan).
func resolveTestProjectReference(info PackageInfo) string {
	if info.Err != nil || info.ProjectReference == "" || !info.IsStdLib {
		return info.ProjectReference
	}

	// F15b ruling: `testing` ALWAYS resolves to the hand-owned shim — go-src-converted/testing
	// is excluded from test graphs (ONE testing package, period). Two testing assemblies in one
	// build make every `testing_package` type ambiguous (CS0433, sort via internal/testenv).
	// The direct dependency is normally stripped upstream (the template carries the fixed shim
	// reference), so this is the backstop for any resolver path that still sees it.
	if info.PackageName == "testing" {
		return `$(go2csPath)core\testing\testing.csproj`
	}

	normalized := strings.ReplaceAll(info.ProjectReference, "/", `\`)

	relative, ok := strings.CutPrefix(normalized, `$(go2csPath)core\`)
	if !ok || strings.HasPrefix(relative, `golib\`) {
		return info.ProjectReference
	}

	return `$(go2csPath)go-src-converted\` + relative
}

func productionCSFiles(outputPath string) ([]string, error) {
	entries, err := os.ReadDir(outputPath)
	if err != nil {
		return nil, err
	}
	result := make([]string, 0)
	for _, entry := range entries {
		name := entry.Name()
		lower := strings.ToLower(name)
		if entry.IsDir() || !strings.HasSuffix(lower, ".cs") || strings.HasSuffix(lower, "_test.cs") ||
			lower == strings.ToLower(PackageInfoFileName) || lower == testPackageInfoFileName || lower == testHostFileName || strings.HasSuffix(lower, ".g.cs") {
			continue
		}
		result = append(result, name)
	}
	sort.Strings(result)
	return result, nil
}

// testFixturePaths enumerates the package's test fixture inputs — every top-level *.go source
// plus the full testdata/ tree — as sorted slash-relative paths. Shared by copyTestFixtures and
// testInputDigest so staleness detection always sees the CURRENT fixture set (a newly added
// testdata file changes the digest; the manifest's recorded list plays no part — F7).
func testFixturePaths(inputPath string) ([]string, error) {
	paths := make([]string, 0)

	goSources, err := filepath.Glob(filepath.Join(inputPath, "*.go"))
	if err != nil {
		return nil, err
	}
	for _, sourceFile := range goSources {
		paths = append(paths, filepath.Base(sourceFile))
	}

	testdata := filepath.Join(inputPath, "testdata")
	if info, err := os.Stat(testdata); err == nil && info.IsDir() {
		err = filepath.WalkDir(testdata, func(path string, entry fs.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}
			if entry.IsDir() {
				return nil
			}
			rel, err := filepath.Rel(inputPath, path)
			if err != nil {
				return err
			}
			paths = append(paths, filepath.ToSlash(rel))
			return nil
		})
		if err != nil {
			return nil, err
		}
	}

	sort.Strings(paths)
	return paths, nil
}

func copyTestFixtures(inputPath, outputPath string) ([]string, error) {
	fixtures, err := testFixturePaths(inputPath)
	if err != nil {
		return nil, err
	}

	if samePath(inputPath, outputPath) {
		return fixtures, nil
	}

	for _, fixture := range fixtures {
		data, err := os.ReadFile(filepath.Join(inputPath, filepath.FromSlash(fixture)))
		if err != nil {
			return nil, err
		}

		target := filepath.Join(outputPath, filepath.FromSlash(fixture))
		if err := os.MkdirAll(filepath.Dir(target), 0755); err != nil {
			return nil, err
		}
		if needToWriteFile(target, data) {
			if err := os.WriteFile(target, data, 0644); err != nil {
				return nil, err
			}
		}
	}

	return fixtures, nil
}

func classifyTestSources(inputPath string, included HashSet[string], external *packages.Package) ([]testSource, error) {
	matches, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))
	if err != nil {
		return nil, err
	}
	result := make([]testSource, 0, len(matches))
	for _, path := range matches {
		kind := "internal-test"
		if external != nil {
			for _, file := range external.CompiledGoFiles {
				if samePath(file, path) {
					kind = "external-test"
					break
				}
			}
		}
		status, reason := "included", ""
		if !included.Contains(filepath.Clean(path)) {
			status, reason = "platform-excluded", "not selected by go/packages for the requested GOOS/GOARCH and build constraints"
		}
		result = append(result, testSource{Path: filepath.ToSlash(filepath.Base(path)), Kind: kind, Status: status, Reason: reason})
	}
	sort.Slice(result, func(i, j int) bool { return result[i].Path < result[j].Path })
	return result, nil
}

// conversionOptionsDigest canonicalizes the OUTPUT-AFFECTING conversion options for the input
// digest (F7): any option that changes emitted C# invalidates the manifest. Machine-specific
// paths (goRoot/goPath/go2csPath) are deliberately excluded so digests stay machine-portable.
func conversionOptionsDigest(options Options) string {
	return fmt.Sprintf("uco=%t;var=%t;indent=%d;comments=%t;cgo=%t",
		options.useChannelOperators, options.preferVarDecl, options.indentSpaces,
		options.includeComments, options.parseCgoTargets)
}

// runtimeSourcesDigest hashes the hand-owned runtime the converted tests build against (golib +
// the go.testing shim) as staged under the converter's go2csPath output root (F7): a runtime
// behavior change invalidates prior comparisons. KNOWN ITEM (review #5, accepted): best-effort
// by design — in a dev tree the runtime is resolved by MSBuild from $(SolutionDir), not the
// converter's output root, so the sources may not be present and a runtime edit then does NOT
// invalidate the manifest ("runtime-unavailable" keeps the digest deterministic either way);
// deployed (deploy-core) and -go2cspath-staged layouts get full invalidation.
func runtimeSourcesDigest(options Options) string {
	var files []string

	for _, dir := range []string{
		filepath.Join(options.go2csPath, "core", "golib"),
		filepath.Join(options.go2csPath, "core", "testing"),
	} {
		if matches, err := filepath.Glob(filepath.Join(dir, "*.cs")); err == nil {
			files = append(files, matches...)
		}
	}

	if len(files) == 0 {
		return "runtime-unavailable"
	}

	sort.Strings(files)
	hash := sha256.New()

	for _, fileName := range files {
		data, err := os.ReadFile(fileName)
		if err != nil {
			return "runtime-unavailable"
		}
		fmt.Fprintf(hash, "%s\x00%d\x00", filepath.Base(fileName), len(data))
		hash.Write(data)
	}

	return "runtime-" + hex.EncodeToString(hash.Sum(nil)[:8])
}

// testInputDigest fingerprints everything that determines a test conversion's outputs: the
// package's Go sources and testdata (globbed FRESH — never from a recorded list, F7), hand-owned
// *_impl.cs companions in the output, the output-affecting conversion options, the staged
// runtime sources, the target platform, the Go toolchain, and the converter revision.
func testInputDigest(inputPath, outputPath string, options Options, revision string) (string, error) {
	hash := sha256.New()

	fixtures, err := testFixturePaths(inputPath)
	if err != nil {
		return "", err
	}

	inputs := make([]string, 0, len(fixtures)+8)
	for _, fixture := range fixtures {
		inputs = append(inputs, "source:"+fixture)
	}

	companions, err := filepath.Glob(filepath.Join(outputPath, "*_impl.cs"))
	if err != nil {
		return "", err
	}
	for _, path := range companions {
		inputs = append(inputs, "output:"+filepath.Base(path))
	}

	sort.Strings(inputs)
	for _, taggedPath := range inputs {
		tag, rel, _ := strings.Cut(taggedPath, ":")
		root := inputPath
		if tag == "output" {
			root = outputPath
		}
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			return "", err
		}
		fmt.Fprintf(hash, "%s\x00%d\x00", taggedPath, len(data))
		hash.Write(data)
	}

	fmt.Fprintf(hash, "\x00%s\x00%s\x00%s\x00%s\x00%s",
		options.targetPlatform, conversionOptionsDigest(options), runtimeSourcesDigest(options),
		runtime.Version(), revision)
	return hex.EncodeToString(hash.Sum(nil)), nil
}

func writeNoTestsManifest(production *packages.Package, inputPath, outputPath string, target []string, options Options) error {
	projectName, _ := getProjectName(inputPath, options)
	manifest := testManifest{
		SchemaVersion: 1, CapabilitiesVersion: 1, PackageImportPath: production.PkgPath,
		ProjectName: projectName, TestProject: projectName + ".tests.csproj", GoVersion: runtime.Version(),
		TargetGOOS: target[0], TargetGOARCH: target[1], SourceRevision: gitRevision(inputPath),
		ConverterRevision: converterRevision(), ProductionFiles: []string{}, TestSources: []testSource{},
		Fixtures: []string{}, Tests: []testDeclaration{}, Dependencies: []string{}, Capabilities: supportedTestCapabilities(),
		RequiredCapabilities: []string{}, UnsupportedCapabilities: []string{},
	}
	digest, err := testInputDigest(inputPath, outputPath, options, manifest.ConverterRevision)
	if err != nil {
		return fmt.Errorf("compute no-tests manifest input digest: %w", err)
	}
	manifest.InputDigest = digest

	return writeJSONFile(filepath.Join(outputPath, testManifestFileName), manifest)
}

func writeJSONFile(fileName string, value any) error {
	data, err := json.MarshalIndent(value, "", "  ")
	if err != nil {
		return err
	}
	data = append(data, '\n')
	if needToWriteFile(fileName, data) {
		return os.WriteFile(fileName, data, 0644)
	}
	return nil
}

// converterRevision identifies the converter BINARY that produced a manifest. The executable
// hash comes first (F7): hashing the on-disk source directory would report a fresh revision for
// a STALE go2cs.exe — precisely the stale-binary false-green failure mode this project has been
// burned by. VCS build info (when unmodified) and the source-directory digest are fallbacks.
func converterRevision() string {
	if executable, err := os.Executable(); err == nil {
		if data, readErr := os.ReadFile(executable); readErr == nil {
			digest := sha256.Sum256(data)
			return "exe-" + hex.EncodeToString(digest[:8])
		}
	}

	revision := "development"
	modified := false
	if info, ok := debug.ReadBuildInfo(); ok {
		for _, setting := range info.Settings {
			switch setting.Key {
			case "vcs.revision":
				if setting.Value != "" {
					revision = setting.Value
				}
			case "vcs.modified":
				modified = setting.Value == "true"
			}
		}
	}
	if !modified && revision != "development" {
		return revision
	}

	if _, sourceFile, _, ok := runtime.Caller(0); ok {
		sourceFiles, globErr := filepath.Glob(filepath.Join(filepath.Dir(sourceFile), "*.go"))
		if globErr == nil && len(sourceFiles) > 0 {
			sort.Strings(sourceFiles)
			hash := sha256.New()
			complete := true
			for _, fileName := range sourceFiles {
				data, readErr := os.ReadFile(fileName)
				if readErr != nil {
					complete = false
					break
				}
				fmt.Fprintf(hash, "%s\x00%d\x00", filepath.Base(fileName), len(data))
				hash.Write(data)
			}
			if complete {
				return "source-" + hex.EncodeToString(hash.Sum(nil)[:8])
			}
		}
	}

	return revision + "+modified"
}

func gitRevision(path string) string {
	cmd := exec.Command("git", "-C", path, "rev-parse", "HEAD")
	output, err := cmd.Output()
	if err != nil {
		return ""
	}
	return strings.TrimSpace(string(output))
}

func executeTestAction(inputPath, outputPath string, options Options) error {
	projectName, _ := getProjectName(inputPath, options)
	testProject := filepath.Join(outputPath, projectName+".tests.csproj")
	if err := validateTestManifest(inputPath, outputPath, options); err != nil {
		return err
	}
	manifest, err := readTestManifest(outputPath)
	if err != nil {
		return err
	}

	// Package-level infrastructure blocking applies only when NO converted test can run (a
	// capability-blocked TestMain gates everything; or every declared test blocked itself).
	// Individually blocked tests among runnable siblings are excluded-disclosed instead (F4).
	if blocked := manifestCapabilityBlock(manifest); len(blocked) > 0 {
		result := map[string]any{
			"package": filepath.Base(inputPath), "status": "infrastructure-blocked", "matched": false,
			"errors": []string{"unsupported testing capabilities: " + strings.Join(blocked, ", ")},
		}
		if err := writeJSONFile(filepath.Join(outputPath, "go2cs_test_comparison.json"), result); err != nil {
			return err
		}
		return fmt.Errorf("converted tests are infrastructure-blocked: %s", strings.Join(blocked, ", "))
	}

	if !manifestHasEligibleTests(manifest) {
		if options.testAction == "all" || options.testAction == "compare" {
			result := map[string]any{"package": filepath.Base(inputPath), "status": "not-applicable", "matched": true, "errors": []string{}}
			if err := writeJSONFile(filepath.Join(outputPath, "go2cs_test_comparison.json"), result); err != nil {
				return err
			}
		}
		fmt.Println("No eligible Go tests for the requested target.")
		return nil
	}

	if _, err := os.Stat(testProject); err != nil {
		return fmt.Errorf("test project is missing or stale; run -tests -test-action convert first: %w", err)
	}

	switch options.testAction {
	case "build":
		_, err := runCommandWithTimeout(options.testTimeout, outputPath, options, "dotnet", "build", testProject)
		return err
	case "run":
		output, err := runCommandWithTimeout(options.testTimeout, outputPath, options, "dotnet", "run", "--project", testProject, "--", "--json")
		fmt.Print(output)
		return err
	case "compare", "all":
		return compareGoAndConvertedTests(inputPath, outputPath, testProject, options)
	default:
		return nil
	}
}

func readTestManifest(outputPath string) (testManifest, error) {
	var manifest testManifest
	data, err := os.ReadFile(filepath.Join(outputPath, testManifestFileName))
	if err != nil {
		return manifest, fmt.Errorf("test manifest is missing: %w", err)
	}
	if err := json.Unmarshal(data, &manifest); err != nil {
		return manifest, fmt.Errorf("test manifest is invalid: %w", err)
	}
	return manifest, nil
}

func manifestHasEligibleTests(manifest testManifest) bool {
	for _, test := range manifest.Tests {
		if test.Kind == "test" && test.Status == "included" {
			return true
		}
	}
	return false
}

// manifestCapabilityBlock returns the capability names that leave the package with NO runnable
// converted tests: a capability-blocked TestMain gates every test (Go routes all tests through
// it), and a package whose every declared test blocked itself has nothing to run. A blocked test
// among runnable siblings does NOT block the package (F4) — it is excluded-disclosed.
func manifestCapabilityBlock(manifest testManifest) []string {
	capabilityReason := func(declaration testDeclaration) []string {
		if declaration.Status != "unsupported" || !strings.HasPrefix(declaration.Reason, unsupportedCapabilityReasonPrefix) {
			return nil
		}
		return strings.Split(strings.TrimPrefix(declaration.Reason, unsupportedCapabilityReasonPrefix), ", ")
	}

	if manifest.TestMain != nil {
		if blocked := capabilityReason(*manifest.TestMain); len(blocked) > 0 {
			return blocked
		}
	}

	blockedCapabilities := HashSet[string]{}
	hasIncludedTest := false
	hasBlockedTest := false

	for _, test := range manifest.Tests {
		if test.Kind != "test" {
			continue
		}
		if test.Status == "included" {
			hasIncludedTest = true
			continue
		}
		if blocked := capabilityReason(test); len(blocked) > 0 {
			hasBlockedTest = true
			blockedCapabilities.UnionWith(blocked)
		}
	}

	if hasIncludedTest || !hasBlockedTest {
		return nil
	}

	blocked := blockedCapabilities.Keys()
	sort.Strings(blocked)
	return blocked
}

func validateTestManifest(inputPath, outputPath string, options Options) error {
	manifest, err := readTestManifest(outputPath)
	if err != nil {
		return err
	}
	target := strings.Split(options.targetPlatform, "/")
	if len(target) != 2 || manifest.TargetGOOS != target[0] || manifest.TargetGOARCH != target[1] {
		return fmt.Errorf("test manifest is stale: target is %s/%s, requested %s", manifest.TargetGOOS, manifest.TargetGOARCH, options.targetPlatform)
	}
	// The digest recomputes over the CURRENT inputs (fresh fixture glob — a newly added testdata
	// file is a staleness signal the manifest's recorded list could never carry, F7).
	digest, err := testInputDigest(inputPath, outputPath, options, converterRevision())
	if err != nil {
		return fmt.Errorf("validate test manifest inputs: %w", err)
	}
	if digest != manifest.InputDigest {
		return fmt.Errorf("test manifest is stale: input digest changed (run -tests -test-action convert)")
	}
	return nil
}

type normalizedTestEvent struct {
	Test    string  `json:"test"`
	Action  string  `json:"action"`
	Output  string  `json:"output,omitempty"`
	Elapsed float64 `json:"elapsed,omitempty"`
}

type testComparison struct {
	Package  string            `json:"package"`
	Status   string            `json:"status"`
	Go       map[string]string `json:"go"`
	CSharp   map[string]string `json:"csharp"`
	Matched  bool              `json:"matched"`
	Skipped  []string          `json:"skipped"`
	Excluded []string          `json:"excluded"`
	Errors   []string          `json:"errors"`
}

// matchTerminalStatuses compares the two sides' terminal statuses per test. A test matches when
// both sides report the SAME terminal status (F1) — skip==skip is agreement, disclosed via the
// returned skipped list rather than flagged as failure (real stdlib suites skip routinely).
func matchTerminalStatuses(names []string, goResults, csResults map[string]string) (mismatches, skipped []string) {
	for _, name := range names {
		goStatus, goOK := goResults[name]
		csStatus, csOK := csResults[name]

		if !goOK || !csOK || goStatus != csStatus {
			mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q", name, goStatus, csStatus))
			continue
		}

		if goStatus == "skip" {
			skipped = append(skipped, name)
		}
	}

	return mismatches, skipped
}

// manifestCensusGaps returns the top-level test names present in the RAW `go test -json` results
// that the manifest cannot account for (F6 census gate). Discovery and comparison otherwise share
// a single point of failure: eligibleTerminalTestResults filters BOTH sides by the manifest, so a
// discovery bug self-censors — a test the converter never discovered is silently removed from the
// comparison and the package can be declared "validated" without it. The census runs against the
// UNFILTERED Go results: every name go test actually ran must be declared in the manifest under
// SOME status (included, capability-blocked, or disclosed-unsupported — examples and fuzz
// seed-corpus runs land here too); subtest names roll up to their top-level parent. Any gap fails
// the comparison — a package cannot validate past a test the manifest never accounted for.
func manifestCensusGaps(goResults map[string]string, manifest testManifest) []string {
	declared := HashSet[string]{}
	for _, test := range manifest.Tests {
		declared.Add(test.Name)
	}
	if manifest.TestMain != nil {
		declared.Add(manifest.TestMain.Name)
	}

	gaps := HashSet[string]{}
	for name := range goResults {
		topLevelName, _, _ := strings.Cut(name, "/")
		if !declared.Contains(topLevelName) {
			gaps.Add(topLevelName)
		}
	}

	result := gaps.Keys()
	sort.Strings(result)
	return result
}

// excludedDeclarations lists every disclosed-unsupported declaration the comparison excludes
// (F2/F3): benchmarks, fuzz targets, Examples, and capability-blocked tests are filtered from
// BOTH sides of the oracle, so the comparison record must say what was excluded and why —
// silent filtering is the exact silent-pass channel req §2.7 forbids.
func excludedDeclarations(manifest testManifest) []string {
	excluded := make([]string, 0)

	for _, test := range manifest.Tests {
		if test.Status != "included" {
			excluded = append(excluded, fmt.Sprintf("%s (%s): %s", test.Name, test.Kind, test.Reason))
		}
	}

	if manifest.TestMain != nil && manifest.TestMain.Status != "included" {
		excluded = append(excluded, fmt.Sprintf("%s (%s): %s", manifest.TestMain.Name, manifest.TestMain.Kind, manifest.TestMain.Reason))
	}

	return excluded
}

func compareGoAndConvertedTests(inputPath, outputPath, testProject string, options Options) error {
	goOutput, goErr := runCommandWithTimeout(options.testTimeout, inputPath, options, "go", "test", "-json", "-count=1", ".")
	csOutput, csErr := runCommandWithTimeout(options.testTimeout, outputPath, options, "dotnet", "run", "--project", testProject, "--", "--json",
		"--result", filepath.Join(outputPath, "go2cs_test_results.json"), "--junit", filepath.Join(outputPath, "go2cs_test_results.xml"))

	goResults := terminalTestResults(goOutput)
	csResults := terminalTestResults(csOutput)
	var manifest testManifest
	var censusGaps []string
	manifestData, manifestErr := os.ReadFile(filepath.Join(outputPath, testManifestFileName))
	if manifestErr == nil {
		manifestErr = json.Unmarshal(manifestData, &manifest)
		if manifestErr == nil {
			// F6 census gate: computed over the RAW Go results BEFORE the manifest-driven
			// filtering below — the filter shares the manifest with discovery, so only the
			// unfiltered stream can expose a declaration discovery missed.
			censusGaps = manifestCensusGaps(goResults, manifest)
			goResults = eligibleTerminalTestResults(goResults, manifest)
			csResults = eligibleTerminalTestResults(csResults, manifest)
		}
	}
	names := make([]string, 0, len(goResults)+len(csResults))
	seen := HashSet[string]{}
	for name := range goResults {
		if seen.Add(name) {
			names = append(names, name)
		}
	}
	for name := range csResults {
		if seen.Add(name) {
			names = append(names, name)
		}
	}
	sort.Strings(names)

	status := "validated"
	if !manifestHasEligibleTests(manifest) {
		status = "not-applicable"
	}
	result := testComparison{
		Package: filepath.Base(inputPath), Status: status, Go: goResults, CSharp: csResults,
		Matched: true, Skipped: []string{}, Excluded: excludedDeclarations(manifest), Errors: []string{},
	}
	if manifestErr != nil {
		result.Matched = false
		result.Status = "conversion-blocked"
		result.Errors = append(result.Errors, "test manifest: "+manifestErr.Error())
	} else if blocked := manifestCapabilityBlock(manifest); len(blocked) > 0 {
		result.Matched = false
		result.Status = "infrastructure-blocked"
		result.Errors = append(result.Errors, "unsupported testing capabilities: "+strings.Join(blocked, ", "))
	}

	if len(censusGaps) > 0 {
		// F6: go test ran declarations the manifest never accounted for — a DISCOVERY defect,
		// not a test failure. The package must not validate past it.
		result.Matched = false
		result.Errors = append(result.Errors, "census: go test reported tests the manifest does not declare: "+strings.Join(censusGaps, ", "))
	}

	mismatches, skipped := matchTerminalStatuses(names, goResults, csResults)
	if len(mismatches) > 0 {
		result.Matched = false
		result.Errors = append(result.Errors, mismatches...)
	}
	result.Skipped = append(result.Skipped, skipped...)

	if goErr != nil {
		result.Matched = false
		result.Status = "failing"
		result.Errors = append(result.Errors, "go test: "+goErr.Error())
	}
	if csErr != nil {
		result.Matched = false
		if result.Status != "infrastructure-blocked" {
			// Parsed events prove the converted host RAN: a nonzero exit with results is a
			// genuine test failure (`failing`). `conversion-blocked` is reserved for actual
			// conversion/build/run infrastructure causes — the host produced no events at all.
			if len(csResults) > 0 {
				result.Status = "failing"
			} else {
				result.Status = "conversion-blocked"
			}
		}
		result.Errors = append(result.Errors, "converted tests: "+csErr.Error())
	}
	if !result.Matched && result.Status == "validated" {
		result.Status = "failing"
	}
	if err := writeJSONFile(filepath.Join(outputPath, "go2cs_test_comparison.json"), result); err != nil {
		return err
	}
	if !result.Matched {
		return fmt.Errorf("Go/C# test comparison failed: %s", strings.Join(result.Errors, "; "))
	}
	fmt.Printf("Validated %d tests against go test (%d skipped identically on both sides, %d disclosed-unsupported declarations excluded).\n",
		len(goResults), len(result.Skipped), len(result.Excluded))
	return nil
}

func terminalTestResults(output string) map[string]string {
	result := make(map[string]string)
	for _, line := range strings.Split(output, "\n") {
		var event normalizedTestEvent
		if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
			continue
		}
		switch event.Action {
		case "pass", "fail", "skip", "timeout", "infrastructure-error":
			result[event.Test] = event.Action
		}
	}
	return result
}

func eligibleTerminalTestResults(results map[string]string, manifest testManifest) map[string]string {
	eligible := HashSet[string]{}
	for _, test := range manifest.Tests {
		if test.Kind == "test" && test.Status == "included" {
			eligible.Add(test.Name)
		}
	}

	filtered := make(map[string]string)
	for name, status := range results {
		topLevelName, _, _ := strings.Cut(name, "/")
		if eligible.Contains(topLevelName) {
			filtered[name] = status
		}
	}
	return filtered
}

func runCommandWithTimeout(timeout time.Duration, workingDir string, options Options, name string, args ...string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), timeout)
	defer cancel()
	cmd := exec.CommandContext(ctx, name, args...)
	cmd.Dir = workingDir
	target := strings.Split(options.targetPlatform, "/")
	cmd.Env = append(os.Environ(), "go2csPath="+ensureTrailingSeparator(options.go2csPath))
	if len(target) == 2 {
		cmd.Env = append(cmd.Env, "GOOS="+target[0], "GOARCH="+target[1])
	}
	output, err := cmd.CombinedOutput()
	if ctx.Err() == context.DeadlineExceeded {
		return string(output), fmt.Errorf("%s timed out after %s", name, timeout)
	}
	if err != nil {
		return string(output), fmt.Errorf("%s %s failed: %w\n%s", name, strings.Join(args, " "), err, strings.TrimSpace(string(output)))
	}
	return string(output), nil
}

func ensureTrailingSeparator(path string) string {
	return strings.TrimRight(path, `/\`) + string(filepath.Separator)
}

func removeString(values []string, target string) []string {
	result := values[:0]
	for _, value := range values {
		if value != target {
			result = append(result, value)
		}
	}
	return result
}

func samePath(left, right string) bool {
	leftAbs, _ := filepath.Abs(left)
	rightAbs, _ := filepath.Abs(right)
	return strings.EqualFold(filepath.Clean(leftAbs), filepath.Clean(rightAbs))
}

func escapeCSharp(value string) string {
	return strings.ReplaceAll(strings.ReplaceAll(value, `\`, `\\`), `"`, `\"`)
}

func xmlEscape(value string) string {
	replacer := strings.NewReplacer("&", "&amp;", "\"", "&quot;", "<", "&lt;", ">", "&gt;")
	return replacer.Replace(value)
}
