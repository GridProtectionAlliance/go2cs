// testConversion.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
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

	// The package's HAND-OWNED disclosed-divergence manifest (see testDisclosure). Unlike the
	// go2cs_test_* artifacts above, this file is never generated: it is authored by hand,
	// committed beside the converted package, and reviewed like source.
	testDisclosureFileName = "go2cs_test_disclosures.json"

	// The EXTERNAL test package's metadata anchor (B4/B5) — the compilation unit hosting the
	// GoImplement/GoImplicitConv attributes whose generated adapters/partials must anchor to
	// the <name>_test package class. "external test package" is Go's own term for `package
	// <name>_test`, matching the vocabulary used throughout this file.
	//
	// ⚠ The `_test.cs` SUFFIX IS LOAD-BEARING — it is the exclusion mechanism: the production
	// csproj's committed `*_test.cs` Compile Remove and productionCSFiles both skip this file by
	// that glob alone, WITHOUT a shared-csproj-template edit (which would churn every behavioral
	// csproj on re-transpile). Any future rename must keep the suffix or pay that churn.
	//
	// Renamed from the original `package_info_test.cs` (2026-07-21): a near-anagram of
	// testPackageInfoFileName above, the two sorted adjacent to `package_info.cs` in every
	// converted package directory, and nothing in either name said which class it anchors to.
	externalTestPackageInfoFileName = "package_info_external_test.cs"
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

// testProjectModel selects how the generated test project binds the PRODUCTION package.
type testProjectModel int

const (
	// testProjectRecompile compiles the production .cs INTO the test assembly alongside the
	// converted test sources (the original -tests model). Required whenever the suite has an
	// in-package (internal) test variant — its files reach unexported production symbols,
	// which only a same-assembly compile can bind — and whenever the external variant records
	// production-anchored metadata (see recordsRequireProductionAnchor).
	testProjectRecompile testProjectModel = iota

	// testProjectReference references the colocated production csproj instead of recompiling
	// its sources, so the production ASSEMBLY stays the single identity for the production
	// types. A black-box (external-only) suite touches only the package's exported API, which
	// resolves cross-assembly exactly as it does for every other converted consumer — while a
	// recompile there DUPLICATES the production types: a referenced stdlib assembly whose API
	// mentions a production type (strings.ToLowerSpecial(unicode.SpecialCase, …)) names the
	// type in the PRODUCTION assembly, and the test assembly's recompiled copy is a distinct
	// type — CS0012 (unicode's letter_test). Applies to every black-box-only package
	// (unicode, unicode/utf8, path, …), never to a suite with an internal variant.
	testProjectReference
)

func (m testProjectModel) String() string {
	if m == testProjectReference {
		return "reference"
	}

	return "recompile"
}

// selectTestProjectModel gates the reference model STRICTLY on the suite being black-box only:
// no internal variant exists (internal == nil), so no test file can need same-assembly access
// to unexported production symbols. The selection can still FALL BACK to the recompile model
// when the external variant's converted records demand a production anchor
// (errProductionAnchoredRecords — see processTestConversion).
func selectTestProjectModel(internal, external *packages.Package) testProjectModel {
	if internal == nil && external != nil {
		return testProjectReference
	}

	return testProjectRecompile
}

// errProductionAnchoredRecords signals that a reference-model conversion attempt collected
// GoImplement/GoImplicitConv records whose GENERATED code must anchor to the production
// package class (a partial struct merged into a production type declaration, or conversion
// operators on one) — impossible across an assembly boundary, where the referenced production
// types are closed. The caller falls back to the recompile model, which reconverts with the
// production types local.
var errProductionAnchoredRecords = errors.New("external test variant records production-anchored metadata")

// recordsRequireProductionAnchor reports whether the LIVE record globals — the just-converted
// external variant's collected records — contain any entry that must anchor to the production
// class, evaluated with the recompile-model partition predicates (isTestAnchoredImplementRecord /
// isTestAnchoredConversionRecord). Under the reference model nothing is seeded and
// testLocalTypePrefixes stays empty, so every production type renders package-qualified, and any
// record landing in the production partition is one whose generated partial/adapter/operator
// would need to merge with a production declaration. A record that renders a production type
// through its imported ꓸ type-alias form (`<pkg>ꓸ<Type>`, TypeAliasDot) is likewise treated as
// production-anchored — conservatively, since the partition predicates cannot see the production
// qualifier inside the alias identifier.
func recordsRequireProductionAnchor(productionClassName, productionPackageName string) bool {
	_, productionAnchored := splitExternalVariantRecords(productionClassName)

	if !productionAnchored.isEmpty() {
		return true
	}

	aliasPrefix := getSanitizedIdentifier(productionPackageName) + TypeAliasDot
	names := make([]string, 0)

	for ifaceName, implementations := range interfaceImplementations {
		names = append(names, ifaceName)
		names = append(names, implementations.Keys()...)
	}

	for ifaceName, implementations := range promotedInterfaceImplementations {
		names = append(names, ifaceName)
		names = append(names, implementations.Keys()...)
	}

	for _, proxy := range constraintProxies {
		names = append(names, proxy[0], proxy[1])
	}

	for _, conversions := range []map[string]HashSet[string]{implicitConversions, invertedImplicitConversions, indirectImplicitConversions} {
		for sourceType, targetTypes := range conversions {
			names = append(names, sourceType)
			names = append(names, targetTypes.Keys()...)
		}
	}

	for _, conversions := range []map[string]map[string]string{numericConversions, indirectNumericConversions} {
		for sourceType, targetTypes := range conversions {
			names = append(names, sourceType)

			for targetType := range targetTypes {
				names = append(names, targetType)
			}
		}
	}

	for _, name := range names {
		if strings.Contains(name, aliasPrefix) {
			return true
		}
	}

	return false
}

// isGo2CSRoot reports whether dir is a go2cs project-reference root — the directory the
// $(go2csPath) MSBuild property points at, identified by the shared runtime living at
// core\golib\golib.csproj beneath it.
func isGo2CSRoot(dir string) bool {
	if dir == "" {
		return false
	}

	_, err := os.Stat(filepath.Join(dir, "core", "golib", "golib.csproj"))
	return err == nil
}

// findGo2CSRootAbove walks dir's ancestor chain (inclusive) and returns the first go2cs
// project-reference root, or "" when none exists above dir.
func findGo2CSRootAbove(dir string) string {
	for current := dir; ; {
		if isGo2CSRoot(current) {
			return current
		}

		parent := filepath.Dir(current)

		if parent == current {
			return ""
		}

		current = parent
	}
}

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
	TestProjectModel        string            `json:"testProjectModel,omitempty"`
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
		Mode:       packages.LoadAllSyntax,
		Dir:        inputPath,
		Tests:      true,
		BuildFlags: options.loaderBuildFlags(),
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

	// Phase-4D file exclusion (option-a ruling): drop Example/Benchmark-only test files from the
	// compile set (both models honor it below). Computed once from both variants — a cross-variant
	// reference keeps a file compiled — and reused across the reference→recompile fallback.
	compileExcluded := selectCompileExcludedTestFiles(internal, external)

	projectName, projectNamespace := getProjectName(inputPath, options)
	supported := NewHashSet(supportedTestCapabilities())
	testInfoPath := filepath.Join(outputPath, testPackageInfoFileName)

	model := selectTestProjectModel(internal, external)
	conversion, err := convertTestVariants(model, production, internal, external, compileExcluded, inputPath, outputPath, projectNamespace, supported, options)

	if errors.Is(err, errProductionAnchoredRecords) {
		// The black-box suite records metadata that must anchor to production types — only a
		// same-assembly recompile can host it. Reconvert under the recompile model: conversion
		// is deterministic and the expensive go/packages load above is reused, so the fallback
		// costs one extra emission pass.
		model = testProjectRecompile
		conversion, err = convertTestVariants(model, production, internal, external, compileExcluded, inputPath, outputPath, projectNamespace, supported, options)
	}

	if err != nil {
		return err
	}

	declarations := conversion.declarations
	testMain := conversion.testMain
	outputFiles := conversion.outputFiles
	allImports := conversion.allImports
	requiredCapabilities := conversion.requiredCapabilities
	includedSources := conversion.includedSources

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

	// B2c: a seeded/merged `using` ALIAS in the test metadata — or a package-qualifier `using`
	// emitted into a converted test SOURCE — can target an assembly the package reaches only
	// TRANSITIVELY (sort's `global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;`
	// targets internal/abi via sort → reflectlite → abi; math/rand's default_test.cs needs
	// os/exec purely because testenv.Command RETURNS *exec.Cmd, so "os/exec" appears in no
	// import list), which DisableTransitiveProjectReferences (B2b) hides from the test compile
	// view (CS0234). Add direct F15-mapped project references for every such target; the
	// manifest's dependency list stays import-derived — alias targets are purely a
	// project-reference concern.
	aliasScanFiles := []string{testInfoPath, filepath.Join(outputPath, externalTestPackageInfoFileName)}

	for _, outputFile := range outputFiles {
		aliasScanFiles = append(aliasScanFiles, filepath.Join(outputPath, outputFile))
	}

	referenceImports := append(append([]string{}, dependencies...), aliasReferenceImports(
		aliasScanFiles, production.PkgPath, dependencies)...)

	// REFERENCE model only: add the foreign assemblies the production package's exported
	// interfaces inherit STRUCTURALLY (productionStructuralBaseImports). These converter-
	// introduced base edges (io/fs's `File : io.ReadCloser`) appear in no test import or alias, so
	// the import-derived + alias-scan set above misses them and binding the referenced production
	// interfaces fails CS0012. The recompile model compiles the production sources locally with
	// their own io reference, so its reference set is unchanged (kept byte-identical).
	if model == testProjectReference {
		referenceImports = append(referenceImports, productionStructuralBaseImports(production)...)
	}

	testProjectName := projectName + ".tests.csproj"
	if err := writeTestProject(filepath.Join(outputPath, testProjectName), projectName, projectNamespace, model, productionFiles, outputFiles, fixtures, referenceImports, options); err != nil {
		return err
	}

	sources, err := classifyTestSources(inputPath, includedSources, compileExcluded, external)
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
		TestProjectModel:        model.String(),
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

// testVariantConversionResult carries everything one convertTestVariants pass produced — the
// model-dependent conversion state a reference→recompile fallback re-run rebuilds from scratch.
type testVariantConversionResult struct {
	declarations         []testDeclaration
	testMain             *testDeclaration
	outputFiles          []string
	allImports           HashSet[string]
	requiredCapabilities HashSet[string]
	includedSources      HashSet[string]
}

// convertTestVariants converts the package's test variants under the given test-project model:
// seeds the package_test_info.cs anchor, discovers and converts each variant, and merges the
// collected metadata into the model's anchor file(s). Under testProjectReference it returns
// errProductionAnchoredRecords when the external variant's records demand a production anchor —
// the caller then re-runs the whole pass under testProjectRecompile (deterministic; the
// go/packages load is shared, so the fallback costs only a second emission pass).
func convertTestVariants(model testProjectModel, production, internal, external *packages.Package, compileExcluded map[string]bool, inputPath, outputPath, projectNamespace string, supported HashSet[string], options Options) (testVariantConversionResult, error) {
	result := testVariantConversionResult{
		declarations:         make([]testDeclaration, 0),
		outputFiles:          make([]string, 0),
		allImports:           HashSet[string]{},
		requiredCapabilities: HashSet[string]{},
		includedSources:      HashSet[string]{},
	}

	if model == testProjectReference {
		// The production package binds as an ORDINARY imported package: its exported metadata
		// (type aliases, implements) loads from the colocated package_info.cs like any other
		// dependency's, its types render package-qualified, and isSameAssemblyPkg answers false
		// so cast sites compose the same foreign adapter names go2cs-gen generates for a
		// project-referenced package. Clearing the self-import binding is what flips all of it
		// (visitImportSpec's isPackageUnderTest, convertTestVariant's testLocalTypePrefixes and
		// loadPackageImplements are each gated on these fields).
		options.testPackagePath = ""
		options.testPackageName = ""
	}

	// Session-scoped, not per-variant (B2/B9): both variants come from the ONE load the caller
	// performed, so the external variant's references to an internal-variant-renamed method (the
	// export_test pattern) resolve by object identity to entries registered during the internal
	// pass — resetPackageState deliberately does not clear this map.
	testMethodRenames = make(map[types.Object]bool)

	testInfoPath := filepath.Join(outputPath, testPackageInfoFileName)

	if model == testProjectReference {
		// The reference model must NOT declare the production package class: the production
		// types' single identity is the referenced production assembly, and a local partial
		// declaration (or generated code anchored to one) would re-introduce exactly the
		// duplicate-type shadow the model exists to eliminate. Seed a test-class-only anchor
		// instead of the production package_info.cs.
		seed := referenceModelTestPackageInfoSeed(projectNamespace, getSanitizedImport(external.Name+PackageSuffix), external.Name)

		if err := os.WriteFile(testInfoPath, []byte(seed), 0644); err != nil {
			return result, fmt.Errorf("seed test package metadata: %w", err)
		}
	} else {
		// Seed package_test_info.cs from the production package_info.cs so the production
		// assembly-level metadata carries over verbatim; each converted variant's ADDITIONS are
		// then merged in by the shared writePackageInfoFile (identical emission semantics to
		// production — pointer-form unwrapping, dedup, pruning — because it IS the production
		// writer).
		productionInfoPath := filepath.Join(outputPath, PackageInfoFileName)
		productionInfo, err := os.ReadFile(productionInfoPath)
		if err != nil {
			return result, fmt.Errorf("read production package metadata (convert the package itself before its tests): %w", err)
		}

		if err := os.WriteFile(testInfoPath, productionInfo, 0644); err != nil {
			return result, fmt.Errorf("seed test package metadata: %w", err)
		}

		// The production sources recompile into the test assembly, so their imports are test
		// project references too. Under the reference model the production ASSEMBLY carries its
		// own dependencies, and the test project references only what the test files import
		// (plus the alias-scan additions — B2c).
		for importPath := range production.Imports {
			result.allImports.Add(importPath)
		}
	}

	for _, variant := range []*packages.Package{internal, external} {
		if variant == nil {
			continue
		}

		if len(variant.Errors) > 0 {
			return result, fmt.Errorf("test package variant %q failed to load: %v", variant.ID, variant.Errors)
		}

		entries := testFileEntries(variant)
		if len(entries) == 0 {
			continue
		}

		for _, entry := range entries {
			result.includedSources.Add(filepath.Clean(entry.filePath))
		}

		// DISCOVERY runs over EVERY test file (below), so an excluded file's Example/Benchmark
		// declarations still reach the manifest under their disclosed-unsupported status. EMISSION
		// runs over the non-excluded files only — the excluded file's C# is never written and it is
		// never a csproj compile item (Phase-4D file-exclusion ruling, selectCompileExcludedTestFiles).
		emitEntries := make([]FileEntry, 0, len(entries))
		for _, entry := range entries {
			if compileExcluded[filepath.Clean(entry.filePath)] {
				continue
			}
			emitEntries = append(emitEntries, entry)
		}

		capabilities := analyzeTestingCapabilities(variant)
		found, foundMain := discoverTestDeclarations(variant, entries, inputPath, capabilities, supported)
		result.declarations = append(result.declarations, found...)

		// Package-level capability reporting aggregates over RUNNABLE declaration kinds only
		// (tests + TestMain) — benchmark/fuzz/example requirements must not block the package,
		// they are excluded-disclosed by their own status (F4: attribution is per-test).
		for _, declaration := range found {
			if declaration.Kind == "test" {
				result.requiredCapabilities.UnionWith(declaration.RequiredCapabilities)
			}
		}

		if foundMain != nil {
			if result.testMain != nil {
				return result, fmt.Errorf("multiple valid TestMain declarations: %s and %s", result.testMain.Source, foundMain.Source)
			}
			result.testMain = foundMain
			result.requiredCapabilities.UnionWith(foundMain.RequiredCapabilities)
		}

		variantOutputs, imports, err := convertTestVariant(variant, emitEntries, outputPath, projectNamespace, options)
		if err != nil {
			return result, err
		}

		// Merge this variant's collected metadata globals while they are still live (the next
		// variant's conversion resets them). Under the RECOMPILE model the EXTERNAL variant's
		// records are split across TWO anchor files (B4/B5): records whose generated code must
		// live in the test package class go to package_info_external_test.cs; production-
		// anchored records stay in package_test_info.cs. Under the REFERENCE model there is a
		// single anchor — the test package class — and a record that would need the production
		// anchor triggers the recompile fallback instead.
		if variant == external {
			if model == testProjectReference {
				if recordsRequireProductionAnchor(getSanitizedImport(production.Name+PackageSuffix), production.Name) {
					return result, errProductionAnchoredRecords
				}

				writePackageInfoFile(testInfoPath, true)
			} else {
				unitName, err := writeExternalVariantMetadata(testInfoPath, outputPath, production.Name)
				if err != nil {
					return result, err
				}

				if unitName != "" {
					result.outputFiles = append(result.outputFiles, unitName)
				}
			}
		} else {
			writePackageInfoFile(testInfoPath, true)
		}

		result.outputFiles = append(result.outputFiles, variantOutputs...)
		result.allImports.UnionWithSet(imports)
	}

	// The reference-model seed already declares the attribute-bearing test package class as its
	// first — and only — class; the append is a recompile-model concern (the production-seeded
	// file needs the test class and its widened `using static` scope added).
	if model == testProjectRecompile {
		if err := appendExternalTestPackageClass(testInfoPath, projectNamespace, production.Name, external); err != nil {
			return result, err
		}
	}

	return result, nil
}

// referenceModelTestPackageInfoSeed composes package_test_info.cs for a REFERENCE-model test
// project. The structure mirrors package_info-template.txt (the shared writer requires all four
// marker sections); the FIRST — and only — class declaration is the external test package class,
// which is what the go2cs-gen generators anchor generated adapters and partials to
// (GetFirstClassName), carrying [GoPackage] directly (no second partial exists to make that a
// CS0579). Deliberately absent, versus the recompile model's production-seeded file: the
// production class declaration and every production-anchored record — the referenced production
// assembly already owns them, and a local shadow would duplicate its types.
func referenceModelTestPackageInfoSeed(projectNamespace, testClassName, externalPackageName string) string {
	var b strings.Builder

	b.WriteString("// go2cs metadata anchor for a REFERENCE-model test project (black-box, external-only\r\n")
	b.WriteString("// suite): the test assembly REFERENCES the colocated production project instead of\r\n")
	b.WriteString("// recompiling its sources, so the production assembly is the single identity for the\r\n")
	b.WriteString("// production types and no production class partial may be declared here. The first —\r\n")
	b.WriteString("// and only — class is the external test package class the go2cs-gen generators anchor\r\n")
	b.WriteString("// generated adapters and partials to.\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImportedTypeAliases>\r\n")
	b.WriteString("// </ImportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("using go;\r\n")
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, testClassName))
	b.WriteString("\r\n")
	b.WriteString("// <ExportedTypeAliases>\r\n")
	b.WriteString("// </ExportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <InterfaceImplementations>\r\n")
	b.WriteString("// </InterfaceImplementations>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImplicitConversions>\r\n")
	b.WriteString("// </ImplicitConversions>\r\n")
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n", projectNamespace))
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("[GoPackage(\"%s\")]\r\n", externalPackageName))
	b.WriteString(fmt.Sprintf("public static partial class %s\r\n{\r\n}\r\n", testClassName))

	return b.String()
}

// collectSiblingTestClosure populates siblingClosureImportPaths with the transitive import closure
// of the package's _test.go variants, so the PRODUCTION conversion pass — whose emitted C# is
// recompiled into the test assembly — qualifies its usings against that assembly's real reference
// closure rather than the production half alone (see siblingClosureImportPaths). Metadata-only
// load (no syntax/types), so it costs a fraction of the LoadAllSyntax pass processTestConversion
// does later. Best-effort: a load failure leaves the set empty and the production conversion
// behaves exactly as before — processTestConversion reports the real error moments later.
func collectSiblingTestClosure(inputPath string, options Options) {
	siblingClosureImportPaths = nil

	targetParts := strings.Split(options.targetPlatform, "/")
	if len(targetParts) != 2 {
		return
	}

	absolute, err := filepath.Abs(inputPath)
	if err != nil {
		return
	}

	loaded, err := packages.Load(&packages.Config{
		Mode:       packages.NeedName | packages.NeedImports | packages.NeedDeps,
		Dir:        absolute,
		Tests:      true,
		BuildFlags: options.loaderBuildFlags(),
		Env: append(os.Environ(),
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}, ".")

	if err != nil {
		return
	}

	closure := HashSet[string]{}

	var walk func(pkg *packages.Package)

	walk = func(pkg *packages.Package) {
		for path, imported := range pkg.Imports {
			if closure.Contains(path) {
				continue
			}

			closure.Add(path)
			walk(imported)
		}
	}

	// Only the TEST variants contribute: the production package's own closure is already walked by
	// computeImportAliasRenames from the loaded types, and the synthetic `<pkg>.test` main package
	// is not part of the emitted assembly.
	for _, pkg := range loaded {
		if strings.Contains(pkg.ID, "[") {
			walk(pkg)
		}
	}

	siblingClosureImportPaths = closure.Keys()
	sort.Strings(siblingClosureImportPaths)
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

// The manifest TestSources status/reason for a _test.go file dropped from the compile set by the
// Phase-4D Example/Benchmark-only file-exclusion policy (selectCompileExcludedTestFiles). Distinct
// from "platform-excluded" (a file build-constraints deselect): this file WAS selected for the
// target but declares nothing the run registry admits, so its compilation is deferred alongside
// its declarations' execution.
const (
	compileExcludedSourceStatus = "example-benchmark-only"
	compileExcludedSourceReason = "file declares only Phase-4D-deferred Example/Benchmark functions; its compilation is deferred to Phase 4D along with their execution"
)

// isPhase4DExcludedTestFunc reports whether a top-level function declaration is a Phase-4D-deferred
// Example or Benchmark — the EXACT classification discoverTestDeclarations applies for its
// "example"/"benchmark" (status "unsupported") cases: no receiver, no results, no type params, and
// either a zero-parameter func Example* or a single-*testing.B-parameter func Benchmark*. A
// Test/TestMain/Fuzz func, a method, or a mis-signatured Example/Benchmark returns false. TestMain
// and Fuzz are DELIBERATELY not treated as excluded here — the ruling scopes the file predicate to
// Example/Benchmark (conservative by design), so a file declaring either stays in the compile set.
func isPhase4DExcludedTestFunc(fn *ast.FuncDecl, info *types.Info) bool {
	if fn.Recv != nil || fn.Name == nil {
		return false
	}

	obj, ok := info.Defs[fn.Name].(*types.Func)
	if !ok {
		return false
	}

	sig, ok := obj.Type().(*types.Signature)
	if !ok || sig.TypeParams().Len() != 0 || sig.Results().Len() != 0 {
		return false
	}

	name := fn.Name.Name

	if sig.Params().Len() == 0 {
		return isGoTestName(name, "Example")
	}

	return sig.Params().Len() == 1 && isGoTestName(name, "Benchmark") && isTestingPointer(sig.Params().At(0).Type(), "B")
}

// testFileExclusionInfo holds the go/types facts the Phase-4D file-exclusion predicate needs for
// one _test.go file: whether every top-level declaration it contributes is a Phase-4D-deferred
// Example/Benchmark function (condition 1), the objects it declares (the reference targets of
// condition 2), and every object it references (so a candidate promoted back to RETAINED can, in
// turn, pull a file IT references back into the compile set — the condition-2 fixpoint).
type testFileExclusionInfo struct {
	path      string
	qualifies bool                  // condition (1): declares only Example/Benchmark functions
	declared  []types.Object        // top-level objects the file declares
	used      map[types.Object]bool // objects the file references (go/types Uses)
}

// classifyTestFileForExclusion evaluates condition (1) for one test file and captures the go/types
// objects condition (2) needs. A file qualifies when it declares at least one Phase-4D-deferred
// Example/Benchmark function and NOTHING else at top level (import declarations aside).
func classifyTestFileForExclusion(file *ast.File, info *types.Info, path string) *testFileExclusionInfo {
	result := &testFileExclusionInfo{path: path, used: make(map[types.Object]bool)}

	qualifies := true
	hasExcludedFunc := false

	for _, decl := range file.Decls {
		switch typed := decl.(type) {
		case *ast.GenDecl:
			if typed.Tok == token.IMPORT {
				continue // imports are not declarations for this predicate
			}
			qualifies = false // a top-level var/const/type disqualifies the file
		case *ast.FuncDecl:
			if isPhase4DExcludedTestFunc(typed, info) {
				hasExcludedFunc = true
				if typed.Name != nil {
					if object := info.Defs[typed.Name]; object != nil {
						result.declared = append(result.declared, object)
					}
				}
			} else {
				qualifies = false // a Test/TestMain/Fuzz func, a method, or a helper disqualifies
			}
		default:
			qualifies = false
		}
	}

	result.qualifies = qualifies && hasExcludedFunc

	// Every referenced object, for the condition-(2) fixpoint. Collected for ALL files: a retained
	// file's references are the exclusion driver, and a candidate's are needed once it is promoted.
	// Defining idents resolve through Defs (not Uses) and are skipped, so a file's own Example name
	// does not count as a reference to itself.
	ast.Inspect(file, func(node ast.Node) bool {
		if ident, ok := node.(*ast.Ident); ok {
			if object := info.Uses[ident]; object != nil {
				result.used[object] = true
			}
		}
		return true
	})

	return result
}

// selectCompileExcludedTestFiles applies the user-approved Phase-4D file-exclusion ruling
// ("option a", 2026-07-24): a _test.go file is dropped from the -tests conversion/compile set iff
//   (1) every top-level declaration it contributes is a Phase-4D-deferred declaration — the file's
//       declarations are EXCLUSIVELY func Example* / func Benchmark* (imports do not count as
//       declarations; any var/const/type, or any other func — a Test/TestMain/Fuzz func, a method,
//       or a mis-signatured Example/Benchmark — disqualifies the file, conservative by design), AND
//   (2) no RETAINED test file references any object the file declares (resolved by go/types object
//       identity across the loaded variant set, never by filename or text).
//
// Phase-4D already excludes Example/Benchmark DECLARATIONS from the run registry uniformly, so a
// file that contributes nothing to the run contributes nothing to the compile. This unblocks the
// compile-poisoning external Example-only files (go/token's example_test.go, whose whitebox+blackbox
// recompile drags cross-assembly type identity into CS0012) WITHOUT touching the differential
// oracle: discovery is left intact, so the excluded file's declarations still appear in the manifest
// under their existing disclosed-unsupported status — the F6 census gate stays truthful and every
// already-filtered Example/Benchmark stays filtered. Only the file's EMISSION and csproj
// compile-membership are dropped. Returns the set of cleaned file paths to exclude.
func selectCompileExcludedTestFiles(variants ...*packages.Package) map[string]bool {
	infos := make([]*testFileExclusionInfo, 0)
	byPath := make(map[string]*testFileExclusionInfo)

	for _, variant := range variants {
		if variant == nil {
			continue
		}

		for i, file := range variant.Syntax {
			if i >= len(variant.CompiledGoFiles) {
				break
			}

			path := variant.CompiledGoFiles[i]
			if !strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
				continue
			}

			cleaned := filepath.Clean(path)
			if _, seen := byPath[cleaned]; seen {
				continue
			}

			info := classifyTestFileForExclusion(file, variant.TypesInfo, cleaned)
			infos = append(infos, info)
			byPath[cleaned] = info
		}
	}

	// Seed the excluded set with every condition-(1) qualifier, then relax it: a qualifier a
	// RETAINED file references must stay compiled (condition 2). Promotion is a fixpoint — a
	// newly-retained file's own references can pull further qualifiers back in — over a set that
	// only ever shrinks, so it converges.
	excluded := make(map[string]bool)
	for _, info := range infos {
		if info.qualifies {
			excluded[info.path] = true
		}
	}

	for changed := true; changed; {
		changed = false

		usedByRetained := make(map[types.Object]bool)
		for _, info := range infos {
			if excluded[info.path] {
				continue
			}
			for object := range info.used {
				usedByRetained[object] = true
			}
		}

		for path := range excluded {
			for _, object := range byPath[path].declared {
				if usedByRetained[object] {
					delete(excluded, path)
					changed = true
					break
				}
			}
		}
	}

	return excluded
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

	// The package under test is RECOMPILED into this assembly, so a record naming one of its types
	// through its fully-qualified class (how an external `<name>_test` variant renders it, having
	// reached it by import path) is naming a LOCAL type — and must emit in the same bare form the
	// seeded production metadata uses, or the two spellings of one resolved pair survive as two
	// GoImplement records and go2cs-gen defines the adapter twice. See stripLocalTypeQualifier.
	if options.testPackageName != "" {
		testLocalTypePrefixes = []string{packageNamespace + "." + getSanitizedImport(options.testPackageName+PackageSuffix)}
	}

	// Load the PRODUCTION package's own GoImplement pairs from its (colocated, already-seeded)
	// package_info.cs (B4/B5): visitImportSpec skips the package-under-test alias load — its
	// types bind locally — which also skipped these sets, so an EXTERNAL test file's cast of a
	// production type could not see the seeded adapters and re-recorded the pair. Must run per
	// variant: resetPackageState above just cleared the sets.
	if options.testPackagePath != "" {
		loadPackageImplements(filepath.Join(outputPath, PackageInfoFileName), options.testPackageName)
	}

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
	collectNilArgPtrParams(allEntries, pkg.TypesInfo)
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
// package_info.cs provides for the production class. It also widens the file's `using static`
// scope to that class (B3): metadata attributes merged from the test variants (GoImplement /
// GoImplicitConv) can reference types DECLARED in the external test package (e.g. an errWriter
// helper cast to io.Writer), which the seeded production-only `using static <ns>.<pkg>_package;`
// cannot resolve — CS0246 on every such attribute argument.
func appendExternalTestPackageClass(testInfoPath, packageNamespace, productionPackageName string, external *packages.Package) error {
	if external == nil {
		return nil
	}

	data, err := os.ReadFile(testInfoPath)
	if err != nil {
		return fmt.Errorf("read test package metadata: %w", err)
	}

	contents := string(data)
	className := getSanitizedImport(external.Name + PackageSuffix)

	productionUsing := fmt.Sprintf("using static %s.%s;", packageNamespace, getSanitizedImport(productionPackageName+PackageSuffix))
	testUsing := fmt.Sprintf("using static %s.%s;", packageNamespace, className)

	if !strings.Contains(contents, testUsing) {
		if !strings.Contains(contents, productionUsing) {
			return fmt.Errorf("seeded test package metadata %q is missing the production using directive %q", testInfoPath, productionUsing)
		}
		contents = strings.Replace(contents, productionUsing, productionUsing+"\r\n"+testUsing, 1)
	}

	block := fmt.Sprintf("\r\n[GoPackage(\"%s\")]\r\npublic static partial class %s\r\n{\r\n}\r\n", external.Name, className)

	if !strings.Contains(contents, block) {
		contents += block
	}

	if contents == string(data) {
		return nil
	}

	return os.WriteFile(testInfoPath, []byte(contents), 0644)
}

// conversionRecordSet snapshots the package-scoped GoImplement/GoImplicitConv record globals so
// the external test variant's records can be written through the shared writePackageInfoFile in
// TWO passes with different anchors (B4/B5) — the writer reads the live globals, so each pass
// installs its partition.
type conversionRecordSet struct {
	interfaceImplements map[string]HashSet[string]
	promotedImplements  map[string]HashSet[string]
	proxies             map[string][2]string
	implicitConvs       map[string]HashSet[string]
	invertedConvs       map[string]HashSet[string]
	indirectConvs       map[string]HashSet[string]
	numericConvs        map[string]map[string]string
	indirectNumerics    map[string]map[string]string
}

func newConversionRecordSet() conversionRecordSet {
	return conversionRecordSet{
		interfaceImplements: make(map[string]HashSet[string]),
		promotedImplements:  make(map[string]HashSet[string]),
		proxies:             make(map[string][2]string),
		implicitConvs:       make(map[string]HashSet[string]),
		invertedConvs:       make(map[string]HashSet[string]),
		indirectConvs:       make(map[string]HashSet[string]),
		numericConvs:        make(map[string]map[string]string),
		indirectNumerics:    make(map[string]map[string]string),
	}
}

func (r conversionRecordSet) install() {
	interfaceImplementations = r.interfaceImplements
	promotedInterfaceImplementations = r.promotedImplements
	constraintProxies = r.proxies
	implicitConversions = r.implicitConvs
	invertedImplicitConversions = r.invertedConvs
	indirectImplicitConversions = r.indirectConvs
	numericConversions = r.numericConvs
	indirectNumericConversions = r.indirectNumerics
}

func (r conversionRecordSet) isEmpty() bool {
	return len(r.interfaceImplements) == 0 && len(r.promotedImplements) == 0 &&
		len(r.proxies) == 0 && len(r.implicitConvs) == 0 && len(r.invertedConvs) == 0 &&
		len(r.indirectConvs) == 0 && len(r.numericConvs) == 0 && len(r.indirectNumerics) == 0
}

// isTestAnchoredImplementRecord decides which -tests metadata anchor hosts an EXTERNAL variant
// GoImplement record (B4/B5). The go2cs-gen generators host generated code in the FIRST class
// of the attribute-bearing file, so anchoring is dictated by where each record's generated form
// must land:
//   - an adapter-CLASS record (interface-sourced or foreign-value ᴠ adapters, per
//     adapterClassImplementations; and every ж pointer adapter for a non-production type) is
//     referenced BARE from test-file cast sites, which are partial pieces of the test package
//     class — the adapter must be its member;
//   - a BARE impl name is a type declared in the external test package itself — its generated
//     partial struct must merge with that declaration in the test package class;
//   - a PRODUCTION-qualified record (`sort_package.IntSlice`, its rooted form, or its
//     namespace-relative form `math.rand_package.Rand`) generates a partial/adapter on the
//     production class — it stays with the production-anchored package_test_info.cs, whose first
//     class is the production class.
func isTestAnchoredImplementRecord(ifaceName, implName, productionClassName string) bool {
	if adapterClassImplementations.Contains(ifaceName + "|" + implName) {
		return true
	}

	inner := implName
	pointerForm := false

	if trimmed, ok := strings.CutPrefix(inner, PointerPrefix+"<"); ok {
		inner = strings.TrimSuffix(trimmed, ">")
		pointerForm = true
	}

	if !strings.Contains(inner, ".") {
		return true
	}

	if pointerForm {
		inner = strings.TrimPrefix(inner, "global::")

		if strings.HasPrefix(inner, productionClassName+".") ||
			strings.HasPrefix(inner, packageNamespace+"."+productionClassName+".") {
			return false
		}

		// The live records qualify the implementer NAMESPACE-RELATIVE — without the `go.` root —
		// so a NESTED package's production type arrives as `math.rand_package.Rand`, matching
		// neither form above and landing in the wrong anchor. (A TOP-LEVEL package worked by
		// accident: its relative qualifier IS the bare `sort_package.` form.) Recognize the
		// relative qualifier so nested packages keep production types production-anchored.
		if relative, ok := strings.CutPrefix(packageNamespace, RootNamespace+"."); ok {
			return !strings.HasPrefix(inner, relative+"."+productionClassName+".")
		}

		return true
	}

	return false
}

// isTestAnchoredConversionRecord decides the anchor for an EXTERNAL variant GoImplicitConv
// record: the generated conversion operators live inside a partial declaration of one of the
// two types, so a pair involving ANY test-package-local (bare) type must anchor to the test
// package class; a pair between qualified (production/foreign) types keeps the production
// anchor, matching the pre-split emission.
func isTestAnchoredConversionRecord(sourceType, targetType string) bool {
	isBare := func(name string) bool {
		if trimmed, ok := strings.CutPrefix(name, PointerPrefix+"<"); ok {
			name = strings.TrimSuffix(trimmed, ">")
		}

		return !strings.Contains(name, ".")
	}

	return isBare(sourceType) || isBare(targetType)
}

// splitExternalVariantRecords partitions the LIVE record globals (the external variant's
// collected records) into the test-anchored and production-anchored sets (B4/B5).
func splitExternalVariantRecords(productionClassName string) (testAnchored, productionAnchored conversionRecordSet) {
	testAnchored = newConversionRecordSet()
	productionAnchored = newConversionRecordSet()

	splitImplements := func(source map[string]HashSet[string], test, production map[string]HashSet[string]) {
		for ifaceName, implementations := range source {
			for implementation := range implementations {
				target := production

				if isTestAnchoredImplementRecord(ifaceName, implementation, productionClassName) {
					target = test
				}

				if existing, ok := target[ifaceName]; ok {
					existing.Add(implementation)
				} else {
					target[ifaceName] = NewHashSet([]string{implementation})
				}
			}
		}
	}

	splitImplements(interfaceImplementations, testAnchored.interfaceImplements, productionAnchored.interfaceImplements)
	splitImplements(promotedInterfaceImplementations, testAnchored.promotedImplements, productionAnchored.promotedImplements)

	for key, proxy := range constraintProxies {
		if isTestAnchoredConversionRecord(proxy[0], proxy[1]) {
			testAnchored.proxies[key] = proxy
		} else {
			productionAnchored.proxies[key] = proxy
		}
	}

	splitConversions := func(source map[string]HashSet[string], test, production map[string]HashSet[string]) {
		for sourceType, targetTypes := range source {
			for targetType := range targetTypes {
				target := production

				if isTestAnchoredConversionRecord(sourceType, targetType) {
					target = test
				}

				if existing, ok := target[sourceType]; ok {
					existing.Add(targetType)
				} else {
					target[sourceType] = NewHashSet([]string{targetType})
				}
			}
		}
	}

	splitConversions(implicitConversions, testAnchored.implicitConvs, productionAnchored.implicitConvs)
	splitConversions(invertedImplicitConversions, testAnchored.invertedConvs, productionAnchored.invertedConvs)
	splitConversions(indirectImplicitConversions, testAnchored.indirectConvs, productionAnchored.indirectConvs)

	splitNumerics := func(source map[string]map[string]string, test, production map[string]map[string]string) {
		for sourceType, targetTypes := range source {
			for targetType, valueType := range targetTypes {
				target := production

				if isTestAnchoredConversionRecord(sourceType, targetType) {
					target = test
				}

				if existing, ok := target[sourceType]; ok {
					existing[targetType] = valueType
				} else {
					target[sourceType] = map[string]string{targetType: valueType}
				}
			}
		}
	}

	splitNumerics(numericConversions, testAnchored.numericConvs, productionAnchored.numericConvs)
	splitNumerics(indirectNumericConversions, testAnchored.indirectNumerics, productionAnchored.indirectNumerics)

	return testAnchored, productionAnchored
}

// externalTestPackageInfoSeed composes the initial contents of package_info_external_test.cs. The
// structure mirrors package_info-template.txt (the shared writer requires all four marker
// sections); the FIRST — and only — class declaration is the external test package class, which
// is what the go2cs-gen generators anchor generated adapters and partials to
// (GetFirstClassName). The class is declared WITHOUT [GoPackage]: the attribute-bearing partial
// lives in package_test_info.cs (appendExternalTestPackageClass), and duplicating the attribute
// on a second partial declaration is CS0579. Both `using static` scopes are included so
// attribute arguments resolve exactly as they do in package_test_info.cs.
func externalTestPackageInfoSeed(projectNamespace, productionClassName, testClassName string) string {
	var b strings.Builder

	b.WriteString("// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /\r\n")
	b.WriteString("// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code\r\n")
	b.WriteString("// (adapter classes, partial-struct implementations, conversion operators) must anchor to\r\n")
	b.WriteString("// the test package class — the source generators host output in the first class of the\r\n")
	b.WriteString("// attribute-bearing file, and test-file cast sites reference the adapters as members of\r\n")
	b.WriteString("// the test package class. Production-anchored records stay in package_test_info.cs.\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImportedTypeAliases>\r\n")
	b.WriteString("// </ImportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("using go;\r\n")
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, productionClassName))
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, testClassName))
	b.WriteString("\r\n")
	b.WriteString("// <ExportedTypeAliases>\r\n")
	b.WriteString("// </ExportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <InterfaceImplementations>\r\n")
	b.WriteString("// </InterfaceImplementations>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImplicitConversions>\r\n")
	b.WriteString("// </ImplicitConversions>\r\n")
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n", projectNamespace))
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("public static partial class %s\r\n{\r\n}\r\n", testClassName))

	return b.String()
}

// writeExternalVariantMetadata merges the EXTERNAL test variant's live metadata globals into
// the -tests anchor files (B4/B5). Test-anchored records are written into
// package_info_external_test.cs — a separate compilation unit whose first class is the test package
// class — through the SAME shared writer (with the alias globals stashed: `global using`
// aliases must live in exactly one file, CS1537, and GoTypeAlias attributes stay with the
// production-anchored metadata). Production-anchored records and every alias then merge into
// package_test_info.cs as before. Returns the unit's file name when it was written (the caller
// adds it to the test project's compile items), or "" when the variant introduced no
// test-anchored records — utf8-class packages keep their single-file shape byte-identical.
func writeExternalVariantMetadata(testInfoPath, outputPath, productionPackageName string) (string, error) {
	productionClassName := getSanitizedImport(productionPackageName + PackageSuffix)
	testAnchored, productionAnchored := splitExternalVariantRecords(productionClassName)

	unitName := ""

	if !testAnchored.isEmpty() {
		unitPath := filepath.Join(outputPath, externalTestPackageInfoFileName)

		if _, err := os.Stat(unitPath); os.IsNotExist(err) {
			seed := externalTestPackageInfoSeed(packageNamespace, productionClassName, getSanitizedImport(packageName+PackageSuffix))

			if err := os.WriteFile(unitPath, []byte(seed), 0644); err != nil {
				return "", fmt.Errorf("seed external test package metadata: %w", err)
			}
		}

		savedImported, savedExported := importedTypeAliases, exportedTypeAliases
		importedTypeAliases = map[string]string{}
		exportedTypeAliases = map[string]string{}

		testAnchored.install()
		writePackageInfoFile(unitPath, true)

		importedTypeAliases, exportedTypeAliases = savedImported, savedExported
		unitName = externalTestPackageInfoFileName
	}

	// The production-anchored partition (plus the full alias globals) merges into
	// package_test_info.cs; the split partitions stay installed afterward — the external
	// variant is the last one converted, and nothing downstream reads these globals.
	productionAnchored.install()
	writePackageInfoFile(testInfoPath, true)

	return unitName, nil
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
		"testing.AllocsPerRun", "testing.CoverMode", "testing.Short", "testing.Verbose",
		// In-process benchmarking driven from a Test function: testing.Benchmark runs a
		// func(*B) closure and returns a BenchmarkResult, setting B.N and exposing NsPerOp
		// (unicode's TestCalibrate uses this to pick a linear-vs-binary search cutoff). The
		// host implements these (core/testing/testing.cs: Benchmark, B.N, BenchmarkResult).
		// Top-level BenchmarkXxx DECLARATIONS remain unsupported by their kind (they are never
		// registered — see the "benchmark" case in discoverTestDeclarations), so supporting
		// these members only unblocks Test functions that call testing.Benchmark themselves.
		"testing.Benchmark", "B.N", "BenchmarkResult.NsPerOp",
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
	// Emitted INSIDE `namespace go.<pkg>;`, so the leading `go` re-binds to a `go.go` namespace
	// whenever the test closure pulls in a go/* package (math/rand/v2's regress_test.go imports
	// go/format) — CS0234. packageChildNamespaces still holds the last converted test variant's
	// closure here, which is the closure of the assembly this host compiles into.
	b.WriteString(fmt.Sprintf("using %s;\r\n\r\n", globalQualifyRooted(RootNamespace+".testing_runtime")))
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
func writeTestProject(projectFile, projectName, namespace string, model testProjectModel, productionFiles, testFiles, fixtures, dependencies []string, options Options) error {
	references := HashSet[string]{
		`$(go2csPath)core\golib\golib.csproj`:     {},
		`$(go2csPath)core\testing\testing.csproj`: {},
	}

	// REFERENCE model: the production package compiles ONLY in its own project; reference it so
	// its assembly stays the single identity for the production types. Colocated-relative — the
	// -tests contract colocates the test project with the production csproj — so the reference
	// is layout-independent (no $(go2csPath) tree mapping involved).
	if model == testProjectReference {
		references.Add(projectName + ".csproj")
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

	// The template's last-resort go2csPath fallback must be a COMPLETE property value: an
	// $(MSBuildThisFileDirectory)-anchored relative walk-up when one exists, else the absolute
	// path on its own. filepath.Rel fails across Windows drive letters (an H:\ checkout with the
	// default C:\Users\...\go2cs), and concatenating the absolute after the MSBuild prefix
	// produced an unresolvable garbage path — the bare-clone CS0246 golib failure.
	relativeGo2CSPath, relErr := filepath.Rel(filepath.Dir(projectFile), options.go2csPath)
	if relErr == nil {
		relativeGo2CSPath = "$(MSBuildThisFileDirectory)" + strings.TrimRight(filepath.ToSlash(relativeGo2CSPath), "/") + "/"
	} else {
		relativeGo2CSPath = strings.TrimRight(filepath.ToSlash(options.go2csPath), "/") + "/"
	}

	var compileItems strings.Builder
	compileFiles := append([]string{}, testFiles...)

	// The production sources are compile items only under the RECOMPILE model; the reference
	// model binds them through the production project reference above instead.
	if model == testProjectRecompile {
		compileFiles = append(compileFiles, productionFiles...)
	}

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

// aliasReferenceImports returns the import paths of converted packages that `using` ALIASES in
// the scanned files target but that the test project does not directly reference (B2c). Both the
// test metadata files AND the converted test sources are scanned: a seeded global alias, or a
// file-local package-qualifier using emitted into a *_test.cs, can target an assembly the package
// reaches only transitively — including one no import list mentions, when a test-only helper
// RETURNS a type from it — and DisableTransitiveProjectReferences (B2b) hides such assemblies
// from the test compile view, so the alias line itself fails (CS0234). Candidates
// come from the module-aware TRANSITIVE import closure captured at load time
// (importPackageDirs), whose namespace tokens are rendered by the same machinery that emitted
// the aliases — including the /vN major-version collapse — so matching is exact. When several
// closure paths render the same token (math/rand beside math/rand/v2), the lexically first is
// taken, deterministically.
func aliasReferenceImports(infoFiles []string, productionPkgPath string, directDependencies []string) []string {
	direct := NewHashSet(directDependencies)
	tokens := make(map[string][]string)
	bareTokens := make(map[string][]string)

	for importPath := range importPackageDirs {
		if importPath == productionPkgPath || importPath == "testing" || direct.Contains(importPath) {
			continue
		}

		namespace := convertImportPathToNamespace(importPath, PackageSuffix)

		token := RootNamespace + "." + namespace
		tokens[token] = append(tokens[token], importPath)

		// A SINGLE-SEGMENT package emits its alias UNROOTED — `using hash = hash_package;` inside
		// `namespace go.math.rand`, where C#'s outward lookup finds the class in the enclosing root
		// namespace without a qualifier. The rooted token above never matches such a line, so the
		// reference went missing and DisableTransitiveProjectReferences turned it into CS0246
		// (math/rand/v2's chacha8_test.cs: `sha256.New()` RETURNS hash.Hash, so `hash` appears in no
		// import list and only this alias scan can find it). Multi-segment namespaces always emit
		// with at least their leading package segment, so the rooted token still covers them.
		// Matched on a SEGMENT boundary (see bareTokens below) — a substring test would let
		// `hash_package` match `go.hash.maphash_package` and pull in a package nothing references.
		if !strings.Contains(namespace, ".") {
			bareTokens[namespace] = append(bareTokens[namespace], importPath)
		}
	}

	found := HashSet[string]{}

	for _, infoFile := range infoFiles {
		data, err := os.ReadFile(infoFile)
		if err != nil {
			continue
		}

		for _, line := range strings.Split(string(data), "\n") {
			trimmed := strings.TrimSpace(strings.TrimSuffix(line, "\r"))

			if !strings.HasPrefix(trimmed, "global using ") && !strings.HasPrefix(trimmed, "using ") {
				continue
			}

			if strings.HasPrefix(trimmed, "using static ") || !strings.Contains(trimmed, "=") {
				continue
			}

			_, target, _ := strings.Cut(trimmed, "=")
			target = strings.TrimSuffix(strings.TrimSpace(target), ";")

			for token, paths := range tokens {
				// Three match shapes for a multi-segment package's alias target:
				//   Contains(target, token+".")  — token is a leading/middle namespace segment.
				//   HasSuffix(target, token)      — target ends with the fully-ROOTED token,
				//                                    e.g. `go.os.exec_package` or `global::go.os.exec_package`
				//                                    (math/rand's default_test.cs, emitted from namespace go.math).
				//   HasSuffix(token, "."+target)  — target is the UNROOTED tail of the rooted token,
				//                                    e.g. `os.exec_package` matching token `go.os.exec_package`.
				//                                    A test emitted inside a namespace that SHADOWS the root
				//                                    `go` (go/doc/comment's std_test.cs in namespace go.go.doc,
				//                                    internal/abi's abi_test.cs in go.@internal) emits the alias
				//                                    unrooted and relies on C# outward lookup; the single-segment
				//                                    bareTokens path below never covers a multi-segment tail.
				//                                    Anchored on the leading "." so `os.exec_package` cannot
				//                                    match an unrelated `go.xos.exec_package`.
				if strings.Contains(target, token+".") || strings.HasSuffix(target, token) || strings.HasSuffix(token, "."+target) {
					sort.Strings(paths)
					found.Add(paths[0])
				}
			}

			for token, paths := range bareTokens {
				if target == token || strings.HasPrefix(target, token+".") {
					sort.Strings(paths)
					found.Add(paths[0])
				}
			}
		}
	}

	result := found.Keys()
	sort.Strings(result)

	return result
}

// productionStructuralBaseImports returns the import paths of foreign packages whose exported
// interface types the converter emits as STRUCTURAL C# interface bases of the production
// package's own exported interfaces — the reference-model analogue of the B2c alias scan for a
// class of dependency that scan cannot see.
//
// Go interfaces satisfy structurally, but C# interfaces are nominal, so the converter carries
// the implicit satisfaction as C# inheritance at the interface declaration site
// (getStructuralInterfaceBases): io/fs's `fs.File` lists `Read`/`Close` explicitly in Go, but
// the converted `[GoType] partial interface File : io.ReadCloser` names an io base. Under the
// reference model the test project NAMES the production types (`(fs.File, error) Open(...)` in a
// converted test source, and every ImplementGenerator adapter for a test type implementing
// `fs.FS`) but compiles them from the REFERENCED production assembly; binding an interface type
// in C# requires its base interfaces' assemblies to be referenced too. That io base edge is
// CONVERTER-INTRODUCED, so it appears in NO test-file import and NO alias `using` — the
// import-derived + alias-scan reference set (B2c) misses it, `DisableTransitiveProjectReferences`
// (B2b) hides the production assembly's own io reference, and the test compile fails CS0012
// ('io_package.Reader'/'Closer'/'ReadCloser' not referenced — io/fs's whole suite).
//
// This reproduces exactly the structural-base match the converter runs at each interface
// declaration site (the same Exported / non-alias / non-generic / method-set /
// strictly-fewer-methods / types.Implements gates as getStructuralInterfaceBases), so it adds
// precisely the assemblies that binding the production interfaces forces — never the production
// package's whole import graph, which would contribute child namespaces that break the alias
// machinery (CS0576). The recompile model compiles the production sources locally (with their own
// io reference), so it needs no addition; the caller invokes this for the reference model only.
func productionStructuralBaseImports(production *packages.Package) []string {
	if production == nil || production.Types == nil {
		return nil
	}

	imports := production.Types.Imports()
	if len(imports) == 0 {
		return nil
	}

	found := HashSet[string]{}
	scope := production.Types.Scope()

	for _, name := range scope.Names() {
		typeName, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || !typeName.Exported() || typeName.IsAlias() {
			continue
		}

		named, ok := typeName.Type().(*types.Named)

		if !ok {
			continue
		}

		iface, ok := named.Underlying().(*types.Interface)

		if !ok || iface.NumMethods() == 0 {
			continue
		}

		for _, imported := range imports {
			if found.Contains(imported.Path()) {
				continue
			}

			importedScope := imported.Scope()

			for _, candidateName := range importedScope.Names() {
				candidateTypeName, ok := importedScope.Lookup(candidateName).(*types.TypeName)

				if !ok || !candidateTypeName.Exported() || candidateTypeName.IsAlias() {
					continue
				}

				candidate, ok := candidateTypeName.Type().(*types.Named)

				if !ok || candidate.TypeParams().Len() > 0 {
					continue
				}

				candidateIface, ok := candidate.Underlying().(*types.Interface)

				if !ok || candidateIface.NumMethods() == 0 || candidateIface.NumMethods() >= iface.NumMethods() || !candidateIface.IsMethodSet() {
					continue
				}

				if types.Implements(named, candidateIface) {
					found.Add(imported.Path())
					break
				}
			}
		}
	}

	result := found.Keys()
	sort.Strings(result)

	return result
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

// resolveProductionProjectReference maps a PRODUCTION csproj project reference. Under -tests the
// production project is regenerated as part of the test conversion, and its stdlib references
// must agree with the colocated test project's (the F15 mixed-tree ruling above): stdlib
// dependencies resolve from the overlaid go-src-converted tree, `testing` to the hand-owned
// core/testing shim, golib and non-stdlib references untouched. Raw `$(go2csPath)core\<pkg>`
// refs here clobbered the committed go-src-converted production csprojs and pulled the baseline
// stub tree into the one test build graph (B1 — the src/core/errors CS0246 storm). Outside
// -tests the reference passes through unchanged.
func resolveProductionProjectReference(info PackageInfo, options Options) string {
	if options.convertTests {
		return resolveTestProjectReference(info)
	}

	return info.ProjectReference
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

func classifyTestSources(inputPath string, included HashSet[string], compileExcluded map[string]bool, external *packages.Package) ([]testSource, error) {
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
		// compile-excluded is checked BEFORE included: a Phase-4D Example/Benchmark-only file was
		// platform-SELECTED (so it is not platform-excluded) yet is deliberately not compiled, and
		// its distinct status keeps the manifest truthful about why.
		status, reason := "included", ""
		switch {
		case compileExcluded[filepath.Clean(path)]:
			status, reason = compileExcludedSourceStatus, compileExcludedSourceReason
		case !included.Contains(filepath.Clean(path)):
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
	Package   string            `json:"package"`
	Status    string            `json:"status"`
	Go        map[string]string `json:"go"`
	CSharp    map[string]string `json:"csharp"`
	Matched   bool              `json:"matched"`
	Skipped   []string          `json:"skipped"`
	Disclosed []string          `json:"disclosed"`
	Excluded  []string          `json:"excluded"`
	Errors    []string          `json:"errors"`
}

// testDisclosure pins one test-level disclosed divergence — extending the declaration-level
// "disclosed-unsupported" vocabulary (req §2.7) to individual test outcomes. A hand-owned,
// repo-committed manifest beside the converted package lists tests whose Go=pass/C#=fail
// divergence is provably unsatisfiable in the managed runtime (e.g. the AllocsPerRun
// allocation-count/-profile classes: the CLR allocates where Go's compiler stack-allocates, so
// a malloc-counting shim would fail the same asserts). The signature pin is the integrity
// guard: the oracle reclassifies ONLY a failure whose captured C# output contains the pinned
// substring — a disclosed test failing any OTHER way (a regression beyond the documented
// divergence) is still a mismatch, and a package without a manifest compares strictly.
type testDisclosure struct {
	Name      string `json:"name"`
	Class     string `json:"class"`
	Signature string `json:"signature"`
	Reason    string `json:"reason"`
}

type testDisclosureManifest struct {
	SchemaVersion int              `json:"schemaVersion"`
	Disclosures   []testDisclosure `json:"disclosures"`
}

// loadTestDisclosures reads the package's hand-owned disclosure manifest. A missing file is the
// normal case (no disclosures — strict comparison); a malformed or incomplete manifest is an
// error, never a silent no-op, because a broken disclosure must not widen the oracle. Every
// field is required: an empty signature would substring-match ANY failure, defeating the pin.
func loadTestDisclosures(outputPath string) (map[string]testDisclosure, error) {
	data, err := os.ReadFile(filepath.Join(outputPath, testDisclosureFileName))
	if os.IsNotExist(err) {
		return nil, nil
	}
	if err != nil {
		return nil, err
	}

	var manifest testDisclosureManifest
	if err := json.Unmarshal(data, &manifest); err != nil {
		return nil, err
	}

	disclosures := make(map[string]testDisclosure, len(manifest.Disclosures))
	for _, disclosure := range manifest.Disclosures {
		if disclosure.Name == "" || disclosure.Class == "" || disclosure.Signature == "" || disclosure.Reason == "" {
			return nil, fmt.Errorf("disclosure entries require name, class, signature, and reason: %+v", disclosure)
		}
		if _, exists := disclosures[disclosure.Name]; exists {
			return nil, fmt.Errorf("duplicate disclosure for %s", disclosure.Name)
		}
		disclosures[disclosure.Name] = disclosure
	}

	return disclosures, nil
}

// matchTerminalStatuses compares the two sides' terminal statuses per test. A test matches when
// both sides report the SAME terminal status (F1) — skip==skip is agreement, disclosed via the
// returned skipped list rather than flagged as failure (real stdlib suites skip routinely). A
// Go=pass/C#=fail divergence pinned by the package's hand-owned disclosure manifest — exact
// test name AND the pinned signature present in the captured C# failure output — is returned
// as disclosed-divergent instead of a mismatch; any other failure shape of a disclosed test
// (different signature, different status pair, a subtest) remains a strict mismatch.
// addressTokenPattern matches a 0x-hex token in a subtest name — a pointer ADDRESS embedded via
// %v/%p, run-varying on BOTH sides by construction (Go's own reruns disagree with themselves).
var addressTokenPattern = regexp.MustCompile(`0x[0-9a-fA-F]+`)

// pairAddressVariantNames re-keys the ONE-SIDED rows of the two result maps whose names differ
// only by embedded 0x-hex address tokens onto a shared normalized key, so the status match
// compares them as one row (errors' TestAsValidation/*string(0xc…) names). This is the SECOND
// phase of matching — exact names already paired stay untouched, so a deterministic hex literal
// used as a subtest name is never collapsed. Only UNAMBIGUOUS 1:1 pairs are re-keyed: a
// normalized key claimed by multiple names on either side, or colliding with an existing exact
// name, keeps all originals — the rows stay one-sided and the comparison fails loud, never
// masking. csOutputs follows the C# rename so disclosure-signature matching keeps its text.
func pairAddressVariantNames(goResults, csResults, csOutputs map[string]string) {
	goOnly := make(map[string][]string)
	csOnly := make(map[string][]string)

	for name := range goResults {
		if _, matched := csResults[name]; !matched {
			if key := addressTokenPattern.ReplaceAllString(name, "0x?"); key != name {
				goOnly[key] = append(goOnly[key], name)
			}
		}
	}

	for name := range csResults {
		if _, matched := goResults[name]; !matched {
			if key := addressTokenPattern.ReplaceAllString(name, "0x?"); key != name {
				csOnly[key] = append(csOnly[key], name)
			}
		}
	}

	for key, goNames := range goOnly {
		csNames := csOnly[key]

		if len(goNames) != 1 || len(csNames) != 1 {
			continue
		}

		if _, exists := goResults[key]; exists {
			continue
		}

		if _, exists := csResults[key]; exists {
			continue
		}

		goResults[key] = goResults[goNames[0]]
		delete(goResults, goNames[0])
		csResults[key] = csResults[csNames[0]]
		delete(csResults, csNames[0])

		if output, ok := csOutputs[csNames[0]]; ok {
			csOutputs[key] = output
			delete(csOutputs, csNames[0])
		}
	}
}

func matchTerminalStatuses(names []string, goResults, csResults map[string]string, disclosures map[string]testDisclosure, csOutputs map[string]string) (mismatches, skipped, disclosed []string) {
	for _, name := range names {
		goStatus, goOK := goResults[name]
		csStatus, csOK := csResults[name]

		if !goOK || !csOK || goStatus != csStatus {
			if disclosure, ok := disclosures[name]; ok && goStatus == "pass" && csStatus == "fail" {
				if strings.Contains(csOutputs[name], disclosure.Signature) {
					disclosed = append(disclosed, name)
					continue
				}
				mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q (failure does not match the disclosed %s signature %q)",
					name, goStatus, csStatus, disclosure.Class, disclosure.Signature))
				continue
			}
			mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q", name, goStatus, csStatus))
			continue
		}

		if goStatus == "skip" {
			skipped = append(skipped, name)
		}
	}

	return mismatches, skipped, disclosed
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
	csOutputs := terminalTestOutputs(csOutput)
	disclosures, disclosureErr := loadTestDisclosures(outputPath)
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
	pairAddressVariantNames(goResults, csResults, csOutputs)

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
		Matched: true, Skipped: []string{}, Disclosed: []string{}, Excluded: excludedDeclarations(manifest), Errors: []string{},
	}
	if disclosureErr != nil {
		result.Matched = false
		result.Errors = append(result.Errors, "test disclosures: "+disclosureErr.Error())
		disclosures = nil
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

	mismatches, skipped, disclosed := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)
	if len(mismatches) > 0 {
		result.Matched = false
		result.Errors = append(result.Errors, mismatches...)
	}
	result.Skipped = append(result.Skipped, skipped...)
	for _, name := range disclosed {
		disclosure := disclosures[name]
		result.Disclosed = append(result.Disclosed, fmt.Sprintf("%s (%s): %s", name, disclosure.Class, disclosure.Reason))
	}

	if csErr != nil && goErr == nil && len(disclosed) > 0 && len(mismatches) == 0 && len(csResults) > 0 {
		// The converted host exits nonzero BECAUSE the disclosed-divergent tests fail — that
		// exit code is part of the disclosed outcome, not an additional failure signal.
		// Forgiveness is deliberately narrow: go test itself was clean, the host produced
		// results, and every divergence matched its pinned signature (zero mismatches — a
		// truncated run surfaces as one-sided rows, which are mismatches, and stays fatal).
		csErr = nil
	}

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
	if len(disclosed) > 0 {
		classes := HashSet[string]{}
		for _, name := range disclosed {
			classes.Add(disclosures[name].Class)
		}
		classList := classes.Keys()
		sort.Strings(classList)
		fmt.Printf("Validated %d tests against go test (%d skipped identically on both sides, %d disclosed-divergent (%s), %d disclosed-unsupported declarations excluded).\n",
			len(goResults)-len(disclosed), len(result.Skipped), len(disclosed), strings.Join(classList, ", "), len(result.Excluded))
	} else {
		fmt.Printf("Validated %d tests against go test (%d skipped identically on both sides, %d disclosed-unsupported declarations excluded).\n",
			len(goResults), len(result.Skipped), len(result.Excluded))
	}
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

// terminalTestOutputs captures each test's accumulated log output from its terminal event —
// the converted host attaches the joined t.Log/t.Error text to the terminal record — keyed by
// test name, for disclosure signature matching against the C# side's failure messages.
func terminalTestOutputs(output string) map[string]string {
	result := make(map[string]string)
	for _, line := range strings.Split(output, "\n") {
		var event normalizedTestEvent
		if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
			continue
		}
		switch event.Action {
		case "pass", "fail", "skip", "timeout", "infrastructure-error":
			result[event.Test] = event.Output
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
