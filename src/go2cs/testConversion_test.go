// testConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/types"
	"os"
	"path/filepath"
	"reflect"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

func TestIsGoTestName(t *testing.T) {
	tests := []struct {
		name string
		want bool
	}{
		{"Test", true},
		{"TestValue", true},
		{"Test_underscore", true},
		{"Testlower", false},
		{"BenchmarkValue", false},
	}

	for _, test := range tests {
		if got := isGoTestName(test.name, "Test"); got != test.want {
			t.Errorf("isGoTestName(%q, Test) = %v, want %v", test.name, got, test.want)
		}
	}
}

func TestManifestEligibility(t *testing.T) {
	manifest := testManifest{Tests: []testDeclaration{
		{Name: "BenchmarkOnly", Kind: "benchmark", Status: "unsupported"},
	}}
	if manifestHasEligibleTests(manifest) {
		t.Fatal("benchmark must not make a manifest test-eligible")
	}
	manifest.Tests = append(manifest.Tests, testDeclaration{Name: "TestValue", Kind: "test", Status: "included"})
	if !manifestHasEligibleTests(manifest) {
		t.Fatal("included TestValue should make a manifest test-eligible")
	}
}

func TestEligibleTerminalResultsExcludeUnsupportedTestKinds(t *testing.T) {
	manifest := testManifest{Tests: []testDeclaration{
		{Name: "TestValue", Kind: "test", Status: "included"},
		{Name: "ExampleValue", Kind: "example", Status: "unsupported"},
		{Name: "BenchmarkValue", Kind: "benchmark", Status: "unsupported"},
	}}
	got := eligibleTerminalTestResults(map[string]string{
		"TestValue":             "pass",
		"TestValue/child":       "pass",
		"ExampleValue":          "pass",
		"BenchmarkValue":        "pass",
		"UnregisteredTestValue": "pass",
	}, manifest)
	want := map[string]string{"TestValue": "pass", "TestValue/child": "pass"}
	if !reflect.DeepEqual(got, want) {
		t.Fatalf("eligible results = %#v, want %#v", got, want)
	}
}

// F1 guard: identical terminal statuses on both sides — including skip==skip — are agreement,
// with skips disclosed rather than flagged; any divergence or one-sided result is a mismatch.
func TestSkipParityCountsAsMatched(t *testing.T) {
	goResults := map[string]string{
		"TestPass":    "pass",
		"TestSkip":    "skip",
		"TestDiverge": "pass",
		"TestGoOnly":  "pass",
	}
	csResults := map[string]string{
		"TestPass":    "pass",
		"TestSkip":    "skip",
		"TestDiverge": "fail",
	}

	names := []string{"TestDiverge", "TestGoOnly", "TestPass", "TestSkip"}
	mismatches, skipped, disclosed := matchTerminalStatuses(names, goResults, csResults, nil, nil)

	if !reflect.DeepEqual(skipped, []string{"TestSkip"}) {
		t.Fatalf("skipped = %#v, want [TestSkip]", skipped)
	}
	if len(disclosed) != 0 {
		t.Fatalf("disclosed = %#v, want none — no disclosure manifest means strict comparison", disclosed)
	}
	if len(mismatches) != 2 {
		t.Fatalf("mismatches = %#v, want exactly TestDiverge and TestGoOnly", mismatches)
	}
	for _, mismatch := range mismatches {
		if !strings.HasPrefix(mismatch, "TestDiverge:") && !strings.HasPrefix(mismatch, "TestGoOnly:") {
			t.Fatalf("unexpected mismatch entry %q", mismatch)
		}
	}
}

// Disclosure-oracle guards: a hand-disclosed Go=pass/C#=fail divergence whose captured C#
// failure output carries the pinned signature is reclassified disclosed-divergent, NOT a
// mismatch; the SAME test failing with any other output is still a mismatch (the signature pin
// must catch a regression that changes the failure); and with no disclosure manifest at all the
// comparison stays strict.
func TestDisclosedDivergenceOracle(t *testing.T) {
	goResults := map[string]string{"TestBuilderAllocs": "pass", "TestHealthy": "pass"}
	csResults := map[string]string{"TestBuilderAllocs": "fail", "TestHealthy": "pass"}
	names := []string{"TestBuilderAllocs", "TestHealthy"}
	disclosures := map[string]testDisclosure{
		"TestBuilderAllocs": {Name: "TestBuilderAllocs", Class: "alloc-count-semantics", Signature: "Builder allocs = ", Reason: "AllocsPerRun shim is byte-derived"},
	}

	mismatches, _, disclosed := matchTerminalStatuses(names, goResults, csResults, disclosures,
		map[string]string{"TestBuilderAllocs": "builder_test.go:196: Builder allocs = 648; want 1"})
	if len(mismatches) != 0 || !reflect.DeepEqual(disclosed, []string{"TestBuilderAllocs"}) {
		t.Fatalf("matching signature must disclose, not mismatch: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}

	mismatches, _, disclosed = matchTerminalStatuses(names, goResults, csResults, disclosures,
		map[string]string{"TestBuilderAllocs": "builder_test.go:189: IndexRune('x') = 3; want 2"})
	if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], "does not match the disclosed") {
		t.Fatalf("a different failure signature is a regression, not the disclosed divergence: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}

	mismatches, _, disclosed = matchTerminalStatuses(names, goResults, csResults, nil, nil)
	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("no manifest must mean strict comparison: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}

	// Direction guard: a disclosure only ever covers Go=pass/C#=fail — never the reverse pair,
	// and never a status other than fail (an infrastructure-error is not the documented shape).
	mismatches, _, disclosed = matchTerminalStatuses([]string{"TestBuilderAllocs"},
		map[string]string{"TestBuilderAllocs": "fail"}, map[string]string{"TestBuilderAllocs": "pass"},
		disclosures, map[string]string{"TestBuilderAllocs": "Builder allocs = 648"})
	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("Go=fail/C#=pass must never be disclosed: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}
	mismatches, _, disclosed = matchTerminalStatuses([]string{"TestBuilderAllocs"},
		map[string]string{"TestBuilderAllocs": "pass"}, map[string]string{"TestBuilderAllocs": "infrastructure-error"},
		disclosures, map[string]string{"TestBuilderAllocs": "Builder allocs = 648"})
	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("C#=infrastructure-error must never be disclosed: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}

	// A disclosed test that agrees (pass==pass) is plain agreement — the stale disclosure is inert.
	mismatches, _, disclosed = matchTerminalStatuses([]string{"TestBuilderAllocs"},
		map[string]string{"TestBuilderAllocs": "pass"}, map[string]string{"TestBuilderAllocs": "pass"},
		disclosures, nil)
	if len(disclosed) != 0 || len(mismatches) != 0 {
		t.Fatalf("an agreeing disclosed test is plain agreement: mismatches=%#v disclosed=%#v", mismatches, disclosed)
	}
}

// Loader guards: an absent manifest is the normal no-disclosure case; a present manifest must be
// complete — an empty signature would substring-match ANY failure and defeat the integrity pin,
// and duplicate names would make the pin ambiguous.
func TestDisclosureManifestLoading(t *testing.T) {
	dir := t.TempDir()

	disclosures, err := loadTestDisclosures(dir)
	if err != nil || disclosures != nil {
		t.Fatalf("absent manifest should load as nil, nil — got %#v, %v", disclosures, err)
	}

	writeManifest := func(content string) {
		if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(content), 0644); err != nil {
			t.Fatal(err)
		}
	}

	writeManifest(`{"schemaVersion": 1, "disclosures": [
		{"name": "TestBuilderAllocs", "class": "alloc-count-semantics", "signature": "Builder allocs = ", "reason": "shim is byte-derived"}]}`)
	disclosures, err = loadTestDisclosures(dir)
	if err != nil || len(disclosures) != 1 || disclosures["TestBuilderAllocs"].Signature != "Builder allocs = " {
		t.Fatalf("valid manifest should parse — got %#v, %v", disclosures, err)
	}

	writeManifest(`{"schemaVersion": 1, "disclosures": [
		{"name": "TestBuilderAllocs", "class": "alloc-count-semantics", "signature": "", "reason": "shim is byte-derived"}]}`)
	if _, err = loadTestDisclosures(dir); err == nil {
		t.Fatal("an empty signature must be rejected — it would match any failure")
	}

	writeManifest(`{"schemaVersion": 1, "disclosures": [
		{"name": "TestX", "class": "alloc-profile", "signature": "a", "reason": "r"},
		{"name": "TestX", "class": "alloc-profile", "signature": "b", "reason": "r"}]}`)
	if _, err = loadTestDisclosures(dir); err == nil {
		t.Fatal("duplicate disclosure names must be rejected")
	}
}

// F2/F3 guard: every disclosed-unsupported declaration (examples, benchmarks, capability-blocked
// tests, blocked TestMain) appears in the comparison record's exclusion list — never silently
// filtered.
func TestExcludedDeclarationsAreDisclosed(t *testing.T) {
	manifest := testManifest{
		Tests: []testDeclaration{
			{Name: "TestIncluded", Kind: "test", Status: "included"},
			{Name: "ExampleValue", Kind: "example", Status: "unsupported", Reason: "example execution is deferred to Phase 4D"},
			{Name: "BenchmarkValue", Kind: "benchmark", Status: "unsupported", Reason: "benchmark execution is deferred to Phase 4D"},
			{Name: "TestBlocked", Kind: "test", Status: "unsupported", Reason: unsupportedCapabilityReasonPrefix + "T.Deadline"},
		},
		TestMain: &testDeclaration{Name: "TestMain", Kind: "test-main", Status: "unsupported", Reason: unsupportedCapabilityReasonPrefix + "M.SomethingNew"},
	}

	excluded := excludedDeclarations(manifest)
	want := []string{
		"ExampleValue (example): example execution is deferred to Phase 4D",
		"BenchmarkValue (benchmark): benchmark execution is deferred to Phase 4D",
		"TestBlocked (test): " + unsupportedCapabilityReasonPrefix + "T.Deadline",
		"TestMain (test-main): " + unsupportedCapabilityReasonPrefix + "M.SomethingNew",
	}
	if !reflect.DeepEqual(excluded, want) {
		t.Fatalf("excluded = %#v, want %#v", excluded, want)
	}
}

// F4 guard: a capability-blocked TestMain gates the whole package; blocked tests gate the
// package ONLY when no runnable test remains.
func TestManifestCapabilityBlockScope(t *testing.T) {
	blockedTest := testDeclaration{Name: "TestBlocked", Kind: "test", Status: "unsupported", Reason: unsupportedCapabilityReasonPrefix + "T.Deadline"}
	includedTest := testDeclaration{Name: "TestRuns", Kind: "test", Status: "included"}

	if blocked := manifestCapabilityBlock(testManifest{Tests: []testDeclaration{blockedTest, includedTest}}); blocked != nil {
		t.Fatalf("a blocked test among runnable siblings must not block the package, got %v", blocked)
	}

	if blocked := manifestCapabilityBlock(testManifest{Tests: []testDeclaration{blockedTest}}); !reflect.DeepEqual(blocked, []string{"T.Deadline"}) {
		t.Fatalf("all-blocked package should report [T.Deadline], got %v", blocked)
	}

	manifest := testManifest{
		Tests:    []testDeclaration{includedTest},
		TestMain: &testDeclaration{Name: "TestMain", Kind: "test-main", Status: "unsupported", Reason: unsupportedCapabilityReasonPrefix + "M.SomethingNew"},
	}
	if blocked := manifestCapabilityBlock(manifest); !reflect.DeepEqual(blocked, []string{"M.SomethingNew"}) {
		t.Fatalf("blocked TestMain must gate the package, got %v", blocked)
	}
}

// F6 guard: the census gate compares the RAW go test results against the manifest's
// declarations — a name go test ran that the manifest never declared (under ANY status) fails
// the comparison, closing the shared-filter silent-pass channel between discovery and
// comparison. Subtests roll up to their top-level parent; examples/fuzz seed-corpus runs are
// accounted through their disclosed-unsupported declarations.
func TestManifestCensusDetectsUndeclaredTests(t *testing.T) {
	manifest := testManifest{
		Tests: []testDeclaration{
			{Name: "TestKnown", Kind: "test", Status: "included"},
			{Name: "ExampleValue", Kind: "example", Status: "unsupported"},
			{Name: "FuzzSeed", Kind: "fuzz", Status: "unsupported"},
		},
		TestMain: &testDeclaration{Name: "TestMain", Kind: "test-main", Status: "included"},
	}

	goResults := map[string]string{
		"TestKnown":       "pass",
		"TestKnown/child": "pass",
		"ExampleValue":    "pass",
		"FuzzSeed":        "pass",
	}
	if gaps := manifestCensusGaps(goResults, manifest); len(gaps) != 0 {
		t.Fatalf("fully declared results should have no census gaps, got %v", gaps)
	}

	goResults["TestGhost"] = "pass"
	goResults["TestGhost/sub"] = "pass"
	goResults["TestPhantom"] = "fail"
	if gaps := manifestCensusGaps(goResults, manifest); !reflect.DeepEqual(gaps, []string{"TestGhost", "TestPhantom"}) {
		t.Fatalf("census gaps = %v, want [TestGhost TestPhantom]", gaps)
	}
}

// F15 guard: stdlib dependencies ALWAYS resolve to the overlaid go-src-converted tree (the
// mixed-tree ruling) — deterministically, with no filesystem probing; golib and non-stdlib
// references pass through untouched.
func TestResolveTestProjectReferenceRewritesStdLibToConvertedTree(t *testing.T) {
	stdlib := PackageInfo{IsStdLib: true, ProjectReference: `$(go2csPath)core\bytes\bytes.csproj`}
	if got, want := resolveTestProjectReference(stdlib), `$(go2csPath)go-src-converted\bytes\bytes.csproj`; got != want {
		t.Fatalf("stdlib reference = %q, want %q", got, want)
	}

	nested := PackageInfo{IsStdLib: true, ProjectReference: `$(go2csPath)core\sync\atomic\sync.atomic.csproj`}
	if got, want := resolveTestProjectReference(nested), `$(go2csPath)go-src-converted\sync\atomic\sync.atomic.csproj`; got != want {
		t.Fatalf("nested stdlib reference = %q, want %q", got, want)
	}

	golib := PackageInfo{IsStdLib: true, ProjectReference: `$(go2csPath)core\golib\golib.csproj`}
	if got := resolveTestProjectReference(golib); got != golib.ProjectReference {
		t.Fatalf("golib reference must pass through, got %q", got)
	}

	local := PackageInfo{IsStdLib: false, ProjectReference: `..\sibling\sibling.csproj`}
	if got := resolveTestProjectReference(local); got != local.ProjectReference {
		t.Fatalf("non-stdlib reference must pass through, got %q", got)
	}

	// F15b ruling: `testing` ALWAYS resolves to the hand-owned shim — the auto-converted
	// go-src-converted/testing is excluded from test graphs (one testing package, period).
	shimmed := PackageInfo{IsStdLib: true, PackageName: "testing", ProjectReference: `$(go2csPath)core\testing\testing.csproj`}
	if got, want := resolveTestProjectReference(shimmed), `$(go2csPath)core\testing\testing.csproj`; got != want {
		t.Fatalf("testing reference = %q, want the shim %q", got, want)
	}
}

// F15 alias-load guard: under -tests a stdlib import's exported-type-alias metadata (package_info.cs)
// must load from the SAME overlaid go-src-converted tree the test compiles against, not the baseline
// core stub — which for most packages (runtime = impl-stubs only) has no package_info.cs, so the alias
// map was empty and `err.(runtime.Error)` rendered the raw, undefined `runtime.Error` (CS0426) instead
// of `runtimeꓸError` => `runtime_package.ΔError`. `testing` stays on the core shim; non-test and
// non-stdlib pass through. This keeps the alias-load tree consistent with resolveTestProjectReference.
func TestResolveAliasLoadTargetDirTracksConvertedTree(t *testing.T) {
	// -tests stdlib: the alias load follows the project ref onto the overlaid go-src-converted tree.
	const runtimeDir, runtimeRef = `$(go2csPath)core\runtime`, `$(go2csPath)core\runtime\runtime.csproj`

	if got, want := resolveAliasLoadTargetDir(runtimeDir, runtimeRef, "runtime", true, Options{convertTests: true}), `$(go2csPath)go-src-converted\runtime`; got != want {
		t.Fatalf("-tests stdlib alias-load dir = %q, want %q", got, want)
	}

	// Non-test: pass through — a normal -stdlib build's stdlib IS the baseline tree.
	if got := resolveAliasLoadTargetDir(runtimeDir, runtimeRef, "runtime", true, Options{}); got != runtimeDir {
		t.Fatalf("non-test alias-load dir must pass through, got %q", got)
	}

	// `testing` stays on the hand-owned core shim (mirrors resolveTestProjectReference's sole exception).
	const shimDir, shimRef = `$(go2csPath)core\testing`, `$(go2csPath)core\testing\testing.csproj`

	if got := resolveAliasLoadTargetDir(shimDir, shimRef, "testing", true, Options{convertTests: true}); got != shimDir {
		t.Fatalf("testing alias-load dir must stay on the core shim, got %q", got)
	}

	// Non-stdlib passes through untouched.
	const thirdPartyDir = `$(go2csPath)pkg\example.com\mod`

	if got := resolveAliasLoadTargetDir(thirdPartyDir, thirdPartyDir+`\example.com.mod.csproj`, "example.com.mod", false, Options{convertTests: true}); got != thirdPartyDir {
		t.Fatalf("non-stdlib alias-load dir must pass through, got %q", got)
	}
}

// B1 guard: under -tests the regenerated PRODUCTION csproj resolves its stdlib references
// through the same F15 mapping as the colocated test project — raw `$(go2csPath)core\<pkg>`
// refs clobbered the committed go-src-converted production csprojs and pulled the baseline
// stub tree into the test build graph. Outside -tests the reference passes through unchanged.
func TestResolveProductionProjectReferenceAppliesF15UnderTests(t *testing.T) {
	stdlib := PackageInfo{IsStdLib: true, ProjectReference: `$(go2csPath)core\internal\reflectlite\internal.reflectlite.csproj`}

	if got, want := resolveProductionProjectReference(stdlib, Options{convertTests: true}), `$(go2csPath)go-src-converted\internal\reflectlite\internal.reflectlite.csproj`; got != want {
		t.Fatalf("-tests production stdlib reference = %q, want %q", got, want)
	}
	if got := resolveProductionProjectReference(stdlib, Options{}); got != stdlib.ProjectReference {
		t.Fatalf("non-tests production reference must pass through, got %q", got)
	}

	shimmed := PackageInfo{IsStdLib: true, PackageName: "testing", ProjectReference: `$(go2csPath)core\testing\testing.csproj`}
	if got, want := resolveProductionProjectReference(shimmed, Options{convertTests: true}), `$(go2csPath)core\testing\testing.csproj`; got != want {
		t.Fatalf("-tests production testing reference = %q, want the shim %q", got, want)
	}
}

// B2b guard: the embedded test project template pins DisableTransitiveProjectReferences so the
// test compilation's reference view is EXACTLY the direct refs the test converter computed — a
// transitive ref contributing a child `go.<pkg>` namespace (internal/testenv -> io/fs, go.io)
// collides with the converter's in-namespace `using io = io_package;` alias emission (CS0576).
func TestTestProjectTemplateDisablesTransitiveProjectReferences(t *testing.T) {
	if !strings.Contains(string(testCsprojTemplate), "<DisableTransitiveProjectReferences>true</DisableTransitiveProjectReferences>") {
		t.Fatal("test-csproj-template.xml must set DisableTransitiveProjectReferences=true")
	}
}

// B3 guard: package_test_info.cs brings the EXTERNAL test package class into `using static`
// scope beside the production class — metadata attributes merged from the test variants can
// reference types declared in <pkg>_test (an errWriter helper cast to io.Writer), which the
// seeded production-only using cannot resolve. Also guards the appended [GoPackage] anchor
// block and re-run idempotence.
func TestAppendExternalTestPackageClassAddsTestUsingAndAnchor(t *testing.T) {
	dir := t.TempDir()
	fileName := filepath.Join(dir, "package_test_info.cs")

	seed := "using go;\r\nusing static go.value_package;\r\n\r\nnamespace go;\r\n\r\n[GoPackage(\"value\")]\r\npublic static partial class value_package\r\n{\r\n}\r\n"
	if err := os.WriteFile(fileName, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	external := &packages.Package{Name: "value_test"}
	if err := appendExternalTestPackageClass(fileName, "go", "value", external); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(fileName)
	if err != nil {
		t.Fatal(err)
	}

	contents := string(data)
	if !strings.Contains(contents, "using static go.value_package;\r\nusing static go.value_test_package;") {
		t.Fatalf("test package using must follow the production using:\n%s", contents)
	}
	if !strings.Contains(contents, "[GoPackage(\"value_test\")]\r\npublic static partial class value_test_package\r\n{\r\n}") {
		t.Fatalf("external test package anchor class must be appended:\n%s", contents)
	}

	if err := appendExternalTestPackageClass(fileName, "go", "value", external); err != nil {
		t.Fatal(err)
	}
	again, err := os.ReadFile(fileName)
	if err != nil {
		t.Fatal(err)
	}
	if string(again) != contents {
		t.Fatalf("appendExternalTestPackageClass must be idempotent:\nfirst:\n%s\nsecond:\n%s", contents, string(again))
	}
}

// Change C guard: the REFERENCE model is gated STRICTLY on the suite being black-box only —
// an internal (in-package) variant forces the recompile model, because its files reach
// unexported production symbols only a same-assembly compile can bind.
func TestSelectTestProjectModelGatesOnBlackBoxOnly(t *testing.T) {
	internal := &packages.Package{Name: "value"}
	external := &packages.Package{Name: "value_test"}

	if got := selectTestProjectModel(nil, external); got != testProjectReference {
		t.Fatalf("black-box-only suite (internal==nil) must select the reference model, got %v", got)
	}
	if got := selectTestProjectModel(internal, external); got != testProjectRecompile {
		t.Fatalf("a suite with an internal variant must select the recompile model, got %v", got)
	}
	if got := selectTestProjectModel(internal, nil); got != testProjectRecompile {
		t.Fatalf("an internal-only suite must select the recompile model, got %v", got)
	}
}

// Change C fallback gate: a reference-model conversion must fall back to the recompile model
// exactly when the external variant's records demand a PRODUCTION anchor — a partial/adapter
// merged into a production type declaration, impossible across an assembly boundary. Records
// that anchor to the test class (bare test-local impls, adapter-class pairs) must NOT trigger
// the fallback, or every black-box package with any interface cast loses the model.
func TestRecordsRequireProductionAnchorGatesReferenceModel(t *testing.T) {
	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	if recordsRequireProductionAnchor("value_package", "value") {
		t.Fatal("an empty record set must not require a production anchor")
	}

	// A bare (test-package-local) implementer and an adapter-class-marked foreign pair both
	// generate in the test class — no fallback.
	interfaceImplementations["io_package.Writer"] = NewHashSet([]string{"errWriter"})
	adapterClassImplementations.Add("io_package.Writer|strings_package.Builder")
	interfaceImplementations["io_package.Writer"].Add("strings_package.Builder")

	if recordsRequireProductionAnchor("value_package", "value") {
		t.Fatal("test-anchored records (bare impl, adapter-class pair) must not require a production anchor")
	}

	// A production-qualified pointer implementer needs a ж adapter generated on the production
	// class — reference model impossible, fallback required.
	interfaceImplementations["io_package.Writer"].Add(PointerPrefix + "<value_package.Buffer>")

	if !recordsRequireProductionAnchor("value_package", "value") {
		t.Fatal("a production-qualified pointer implementer must require the production anchor")
	}

	// A record rendering a production type through its imported ꓸ alias form hides the
	// production qualifier from the partition predicates — conservatively production-anchored.
	resetPackageState(&packages.Package{})
	packageNamespace = "go"
	implicitConversions["value"+TypeAliasDot+"Kind"] = NewHashSet([]string{"@string"})

	if !recordsRequireProductionAnchor("value_package", "value") {
		t.Fatal("a ꓸ-alias-form production type reference must require the production anchor")
	}
}

// Change C project shape: a REFERENCE-model test project binds the production package through a
// colocated ProjectReference and carries NO production compile items; the recompile model keeps
// the original recompiled shape and no production reference.
func TestWriteTestProjectReferenceModelBindsProductionProject(t *testing.T) {
	dir := t.TempDir()
	importPackageDirs = map[string]importedPackageMeta{}
	productionFiles := []string{"value.cs"}
	testFiles := []string{"value_test.cs"}

	projectFile := filepath.Join(dir, "value.tests.csproj")
	if err := writeTestProject(projectFile, "value", "go", testProjectReference, productionFiles, testFiles, nil, nil, Options{go2csPath: dir}); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(projectFile)
	if err != nil {
		t.Fatal(err)
	}

	contents := string(data)
	if !strings.Contains(contents, `<ProjectReference Include="value.csproj" />`) {
		t.Fatalf("reference model must reference the colocated production csproj:\n%s", contents)
	}
	if strings.Contains(contents, `<Compile Include="value.cs" />`) {
		t.Fatalf("reference model must not recompile production sources:\n%s", contents)
	}
	if !strings.Contains(contents, `<Compile Include="value_test.cs" />`) {
		t.Fatalf("reference model must keep the converted test sources as compile items:\n%s", contents)
	}

	recompileFile := filepath.Join(dir, "value.recompile.tests.csproj")
	if err := writeTestProject(recompileFile, "value", "go", testProjectRecompile, productionFiles, testFiles, nil, nil, Options{go2csPath: dir}); err != nil {
		t.Fatal(err)
	}

	data, err = os.ReadFile(recompileFile)
	if err != nil {
		t.Fatal(err)
	}

	contents = string(data)
	if strings.Contains(contents, `<ProjectReference Include="value.csproj" />`) {
		t.Fatalf("recompile model must not reference the production csproj:\n%s", contents)
	}
	if !strings.Contains(contents, `<Compile Include="value.cs" />`) {
		t.Fatalf("recompile model must recompile production sources:\n%s", contents)
	}
}

// Change C anchor seed: the reference-model package_test_info.cs declares the external test
// package class as its FIRST (and only) class — the go2cs-gen anchor — carrying [GoPackage]
// directly, and must not declare the production package class (the referenced production
// assembly is the single identity for those types).
func TestReferenceModelSeedAnchorsTestClassOnly(t *testing.T) {
	seed := referenceModelTestPackageInfoSeed("go", "value_test_package", "value_test")

	if !strings.Contains(seed, "[GoPackage(\"value_test\")]\r\npublic static partial class value_test_package\r\n{\r\n}\r\n") {
		t.Fatalf("seed must declare the attributed external test package class:\n%s", seed)
	}
	if strings.Contains(seed, "class value_package") {
		t.Fatalf("seed must not declare the production package class:\n%s", seed)
	}
	for _, marker := range []string{"<ImportedTypeAliases>", "<ExportedTypeAliases>", "<InterfaceImplementations>", "<ImplicitConversions>"} {
		if !strings.Contains(seed, marker) {
			t.Fatalf("seed is missing the %s writer marker section:\n%s", marker, seed)
		}
	}
}

// Change C reference closure: under the reference model the test project binds the production
// types from the REFERENCED production assembly, so C# requires the assemblies of every
// CONVERTER-INTRODUCED structural interface base the production interfaces carry (io/fs's
// `File : io.ReadCloser`). Those base edges live in no test import and no alias `using`, so
// productionStructuralBaseImports must surface them — and ONLY them, never the whole import
// graph, which would contribute child namespaces that break the CS0576 alias machinery.
func TestProductionStructuralBaseImportsSurfacesForeignInterfaceBases(t *testing.T) {
	// io/fs shape: File STRUCTURALLY satisfies io.ReadCloser (Read + Close) even though Go's
	// fs.File lists the methods explicitly rather than embedding io — the converter emits
	// `File : io.ReadCloser`, and binding File in the test compile then needs the io assembly.
	fsysDir := t.TempDir()
	writeModuleFiles(t, fsysDir, map[string]string{
		"go.mod": "module example/closure\n\ngo 1.23\n",
		"fs.go": "package closure\n" +
			"import \"io\"\n" +
			"type FileInfo interface{ Name() string }\n" +
			"type File interface {\n" +
			"\tStat() (FileInfo, error)\n" +
			"\tRead([]byte) (int, error)\n" +
			"\tClose() error\n" +
			"}\n" +
			"func ReadAll(f File) ([]byte, error) { return io.ReadAll(f) }\n",
	})

	if got := productionStructuralBaseImports(loadProductionForDir(t, fsysDir)); len(got) != 1 || got[0] != "io" {
		t.Fatalf("File structurally implements io.ReadCloser, so the io assembly must be surfaced; got %v", got)
	}

	// Negative: an exported interface matching no imported interface, alongside an imported
	// package used only inside a function body, surfaces NOTHING — the closure must stay minimal
	// and never widen to the production package's plain import graph.
	plainDir := t.TempDir()
	writeModuleFiles(t, plainDir, map[string]string{
		"go.mod": "module example/plain\n\ngo 1.23\n",
		"plain.go": "package plain\n" +
			"import \"strings\"\n" +
			"type Greeter interface{ Greet() string }\n" +
			"func Upper(s string) string { return strings.ToUpper(s) }\n",
	})

	if got := productionStructuralBaseImports(loadProductionForDir(t, plainDir)); len(got) != 0 {
		t.Fatalf("no exported interface matches an imported interface, so the closure must be empty; got %v", got)
	}
}

// F14b guard: a dependency that fails to resolve fails the test-project emission loudly, naming
// the dependency — never a silent reference drop.
func TestWriteTestProjectFailsLoudlyOnDependencyError(t *testing.T) {
	dir := t.TempDir()
	importPackageDirs = map[string]importedPackageMeta{}

	err := writeTestProject(filepath.Join(dir, "broken.tests.csproj"), "broken", "go", testProjectRecompile, nil, nil, nil,
		[]string{"go2cs.invalid/definitely/not/resolvable"}, Options{go2csPath: dir})

	if err == nil {
		t.Fatal("expected a loud dependency resolution error, got nil")
	}
	if !strings.Contains(err.Error(), "go2cs.invalid/definitely/not/resolvable") {
		t.Fatalf("error must name the unresolvable dependency, got %q", err)
	}
}

// F7 guard: the converter revision is the RUNNING executable's content hash — a stale binary
// self-identifies (the source-directory digest reported fresh revisions for stale binaries).
func TestConverterRevisionIsStableExecutableDigest(t *testing.T) {
	first := converterRevision()
	second := converterRevision()
	if first != second || !strings.HasPrefix(first, "exe-") {
		t.Fatalf("converter revision is not a stable executable digest: %q, %q", first, second)
	}
}

func TestInputDigestDetectsSourceChanges(t *testing.T) {
	dir := t.TempDir()
	source := filepath.Join(dir, "value.go")
	if err := os.WriteFile(source, []byte("package value\n"), 0644); err != nil {
		t.Fatal(err)
	}
	options := Options{targetPlatform: "windows/amd64"}
	first, err := testInputDigest(dir, dir, options, "converter")
	if err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(source, []byte("package value\nvar changed = true\n"), 0644); err != nil {
		t.Fatal(err)
	}
	second, err := testInputDigest(dir, dir, options, "converter")
	if err != nil {
		t.Fatal(err)
	}
	if first == second {
		t.Fatal("input digest did not change after source content changed")
	}
}

// F7 guard: a NEWLY ADDED testdata fixture invalidates the digest — the fixture set is globbed
// fresh at digest time, never taken from the manifest's recorded list.
func TestInputDigestDetectsNewTestdataFixture(t *testing.T) {
	dir := t.TempDir()
	if err := os.WriteFile(filepath.Join(dir, "value.go"), []byte("package value\n"), 0644); err != nil {
		t.Fatal(err)
	}
	options := Options{targetPlatform: "windows/amd64"}
	first, err := testInputDigest(dir, dir, options, "converter")
	if err != nil {
		t.Fatal(err)
	}

	if err := os.MkdirAll(filepath.Join(dir, "testdata"), 0755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(dir, "testdata", "new-fixture.txt"), []byte("fixture\n"), 0644); err != nil {
		t.Fatal(err)
	}

	second, err := testInputDigest(dir, dir, options, "converter")
	if err != nil {
		t.Fatal(err)
	}
	if first == second {
		t.Fatal("input digest did not change after a testdata fixture was added")
	}
}

// F7 guard: output-affecting conversion options are part of the digest.
func TestInputDigestDetectsConversionOptionChanges(t *testing.T) {
	dir := t.TempDir()
	if err := os.WriteFile(filepath.Join(dir, "value.go"), []byte("package value\n"), 0644); err != nil {
		t.Fatal(err)
	}

	first, err := testInputDigest(dir, dir, Options{targetPlatform: "windows/amd64", useChannelOperators: true}, "converter")
	if err != nil {
		t.Fatal(err)
	}
	second, err := testInputDigest(dir, dir, Options{targetPlatform: "windows/amd64", useChannelOperators: false}, "converter")
	if err != nil {
		t.Fatal(err)
	}
	if first == second {
		t.Fatal("input digest did not change after an output-affecting option changed")
	}
}

func loadTestVariantForDir(t *testing.T, dir string) (*packages.Package, *packages.Package) {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}
	production := findProductionPackage(loaded, dir)
	if production == nil {
		t.Fatal("production package was not loaded")
	}
	internal, _ := findTestVariants(loaded, production)
	if internal == nil {
		t.Fatal("same-package test variant was not loaded")
	}
	return production, internal
}

// writeModuleFiles materializes a throwaway Go module (name -> contents) into dir.
func writeModuleFiles(t *testing.T, dir string, files map[string]string) {
	t.Helper()

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
}

// loadProductionForDir loads just the production package for a module dir (no test variant
// required) with full type information, for exercising the go/types-driven reference helpers.
func loadProductionForDir(t *testing.T, dir string) *packages.Package {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}
	production := findProductionPackage(loaded, dir)
	if production == nil {
		t.Fatal("production package was not loaded")
	}
	if len(production.Errors) > 0 {
		t.Fatalf("production package load failed: %v", production.Errors)
	}
	return production
}

func TestUnsupportedTestingCapabilityIsDiscovered(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":        "module example/capability\n\ngo 1.23\n",
		"value.go":      "package capability\n",
		"value_test.go": "package capability\nimport \"testing\"\nfunc TestDeadline(t *testing.T) { _, _ = t.Deadline() }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	if len(declarations) != 1 || declarations[0].Name != "TestDeadline" {
		t.Fatalf("declarations = %#v, want just TestDeadline", declarations)
	}
	declaration := declarations[0]
	if declaration.Status != "unsupported" || !strings.Contains(declaration.Reason, "T.Deadline") {
		t.Fatalf("TestDeadline should be capability-blocked naming T.Deadline, got status %q reason %q", declaration.Status, declaration.Reason)
	}
	if !NewHashSet(declaration.RequiredCapabilities).Contains("T.Deadline") {
		t.Fatalf("required capabilities %v do not contain T.Deadline", declaration.RequiredCapabilities)
	}
	if NewHashSet(supportedTestCapabilities()).Contains("T.Deadline") {
		t.Fatal("T.Deadline unexpectedly appears in the runtime capability list")
	}
}

// AllocsPerRun support guard: the shim implements testing.AllocsPerRun (byte-derived — see
// core/testing/testing.cs), so a test requiring it must convert as INCLUDED while the
// requirement still appears in the manifest's per-test attribution.
func TestAllocsPerRunCapabilityIsSupported(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/allocs\n\ngo 1.23\n",
		"value.go": "package allocs\n",
		"value_test.go": "package allocs\n" +
			"import \"testing\"\n" +
			"func TestNoAllocs(t *testing.T) {\n" +
			"\tif allocs := testing.AllocsPerRun(100, func() {}); allocs != 0 {\n" +
			"\t\tt.Errorf(\"allocs = %v\", allocs)\n" +
			"\t}\n" +
			"}\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	if len(declarations) != 1 || declarations[0].Name != "TestNoAllocs" {
		t.Fatalf("declarations = %#v, want just TestNoAllocs", declarations)
	}
	declaration := declarations[0]
	if declaration.Status != "included" {
		t.Fatalf("TestNoAllocs should be included (testing.AllocsPerRun is supported), got status %q reason %q", declaration.Status, declaration.Reason)
	}
	if !NewHashSet(declaration.RequiredCapabilities).Contains("testing.AllocsPerRun") {
		t.Fatalf("required capabilities %v do not contain testing.AllocsPerRun", declaration.RequiredCapabilities)
	}
}

// The shim's CoverMode() returns "" (Go's exact coverage-off value), so tests branching on it —
// strings' TestIndexRune is the sole stdlib caller in the #3-4 packages — must census as included.
func TestCoverModeCapabilityIsSupported(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/covermode\n\ngo 1.23\n",
		"value.go": "package covermode\n",
		"value_test.go": "package covermode\n" +
			"import \"testing\"\n" +
			"func TestCoverageOffPath(t *testing.T) {\n" +
			"\tif testing.CoverMode() != \"\" {\n" +
			"\t\tt.Skip(\"coverage instrumentation active\")\n" +
			"\t}\n" +
			"}\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	if len(declarations) != 1 || declarations[0].Name != "TestCoverageOffPath" {
		t.Fatalf("declarations = %#v, want just TestCoverageOffPath", declarations)
	}
	declaration := declarations[0]
	if declaration.Status != "included" {
		t.Fatalf("TestCoverageOffPath should be included (testing.CoverMode is supported), got status %q reason %q", declaration.Status, declaration.Reason)
	}
	if !NewHashSet(declaration.RequiredCapabilities).Contains("testing.CoverMode") {
		t.Fatalf("required capabilities %v do not contain testing.CoverMode", declaration.RequiredCapabilities)
	}
}

// A Test function may drive an in-process benchmark itself: testing.Benchmark runs a func(*B)
// closure (reading b.N) and returns a BenchmarkResult whose NsPerOp() the test inspects —
// unicode's TestCalibrate is the stdlib case. The host implements Benchmark/B.N/BenchmarkResult
// (core/testing/testing.cs), so such a test must census as included rather than being
// disclosed-unsupported. (Top-level BenchmarkXxx DECLARATIONS remain unsupported by their kind;
// see TestManifestEligibility — this only covers Test functions that CALL testing.Benchmark.)
func TestBenchmarkCapabilityIsSupported(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/benchcap\n\ngo 1.23\n",
		"value.go": "package benchcap\n",
		"value_test.go": "package benchcap\n" +
			"import \"testing\"\n" +
			"func TestCalibrateLike(t *testing.T) {\n" +
			"\tr := testing.Benchmark(func(b *testing.B) {\n" +
			"\t\tfor i := 0; i < b.N; i++ {\n" +
			"\t\t}\n" +
			"\t})\n" +
			"\tif r.NsPerOp() < 0 {\n" +
			"\t\tt.Fatalf(\"ns/op = %d\", r.NsPerOp())\n" +
			"\t}\n" +
			"}\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	if len(declarations) != 1 || declarations[0].Name != "TestCalibrateLike" {
		t.Fatalf("declarations = %#v, want just TestCalibrateLike", declarations)
	}
	declaration := declarations[0]
	if declaration.Status != "included" {
		t.Fatalf("TestCalibrateLike should be included (testing.Benchmark/B.N/BenchmarkResult.NsPerOp are supported), got status %q reason %q", declaration.Status, declaration.Reason)
	}
	required := NewHashSet(declaration.RequiredCapabilities)
	for _, capability := range []string{"testing.Benchmark", "B.N", "BenchmarkResult.NsPerOp"} {
		if !required.Contains(capability) {
			t.Fatalf("required capabilities %v do not contain %q", declaration.RequiredCapabilities, capability)
		}
	}
}

// F4 guard: capability attribution is per test THROUGH its helper closure — one blocked test
// (via a helper it calls) blocks itself; its supported sibling stays included.
func TestPerTestCapabilityAttributionBlocksOnlyOffendingTest(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/attribution\n\ngo 1.23\n",
		"value.go": "package attribution\n",
		"value_test.go": "package attribution\n" +
			"import \"testing\"\n" +
			"func helperDeadline(t *testing.T) { _, _ = t.Deadline() }\n" +
			"func TestBlocked(t *testing.T) { helperDeadline(t) }\n" +
			"func TestSupported(t *testing.T) { t.Log(\"fine\") }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	byName := map[string]testDeclaration{}
	for _, declaration := range declarations {
		byName[declaration.Name] = declaration
	}

	blocked, ok := byName["TestBlocked"]
	if !ok || blocked.Status != "unsupported" || !strings.Contains(blocked.Reason, "T.Deadline") {
		t.Fatalf("TestBlocked should be capability-blocked through its helper, got %#v", blocked)
	}

	supported, ok := byName["TestSupported"]
	if !ok || supported.Status != "included" {
		t.Fatalf("TestSupported should stay included, got %#v", supported)
	}
}

// F2 guard: Example declarations (zero parameters) are discovered and disclosed as unsupported;
// an invalid lowercase-suffix name stays out (matching `go test`, which would not run it).
func TestExampleDeclarationsAreDiscoveredAndDisclosed(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/examples\n\ngo 1.23\n",
		"value.go": "package examples\n",
		"value_test.go": "package examples\n" +
			"import \"fmt\"\n" +
			"func ExampleValue() { fmt.Println(\"value\") }\n" +
			"func Example() { fmt.Println(\"bare\") }\n" +
			"func Examplelower() { fmt.Println(\"not an example\") }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, internal := loadTestVariantForDir(t, dir)

	analysis := analyzeTestingCapabilities(internal)
	declarations, _ := discoverTestDeclarations(internal, testFileEntries(internal), dir, analysis, NewHashSet(supportedTestCapabilities()))

	byName := map[string]testDeclaration{}
	for _, declaration := range declarations {
		byName[declaration.Name] = declaration
	}

	for _, name := range []string{"Example", "ExampleValue"} {
		declaration, ok := byName[name]
		if !ok || declaration.Kind != "example" || declaration.Status != "unsupported" {
			t.Fatalf("%s should be a disclosed-unsupported example declaration, got %#v", name, declaration)
		}
	}

	if _, ok := byName["Examplelower"]; ok {
		t.Fatal("Examplelower is not a valid example name and must not be discovered")
	}
}

// Shared-writer guard: writePackageInfoFile with mergeExisting preserves a section's existing
// entries, merges additions from the current package-scoped globals, dedupes, and sorts —
// identical emission semantics for production package_info.cs and the test path's
// package_test_info.cs (which is why the test path has no private metadata writer to drift).
func TestWritePackageInfoFileMergesExistingSections(t *testing.T) {
	dir := t.TempDir()
	fileName := filepath.Join(dir, "package_test_info.cs")

	seed := strings.Join([]string{
		"namespace go;",
		"",
		"// <ImportedTypeAliases>",
		"global using zeta = go.zeta_package.Zeta;",
		"// </ImportedTypeAliases>",
		"",
		"// <ExportedTypeAliases>",
		"// </ExportedTypeAliases>",
		"",
		"// <InterfaceImplementations>",
		"// </InterfaceImplementations>",
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"[GoPackage(\"value\")]",
		"public static partial class value_package",
		"{",
		"}",
	}, "\r\n")

	if err := os.WriteFile(fileName, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageName = "value"
	packageNamespace = "go"
	importedTypeAliases["alpha"] = "go.alpha_package.Alpha"
	importedTypeAliases["zeta"] = "go.zeta_package.Zeta"

	writePackageInfoFile(fileName, true)

	data, err := os.ReadFile(fileName)
	if err != nil {
		t.Fatal(err)
	}

	contents := string(data)
	alphaIndex := strings.Index(contents, "global using alpha = go.alpha_package.Alpha;")
	zetaIndex := strings.Index(contents, "global using zeta = go.zeta_package.Zeta;")

	if alphaIndex < 0 || zetaIndex < 0 {
		t.Fatalf("merged file must contain both the existing and the added alias:\n%s", contents)
	}
	if alphaIndex > zetaIndex {
		t.Fatal("merged aliases must be sorted (alpha before zeta)")
	}
	if strings.Count(contents, "global using zeta = go.zeta_package.Zeta;") != 1 {
		t.Fatal("merging must not duplicate an existing alias entry")
	}
	if !strings.Contains(contents, "public static partial class value_package") {
		t.Fatal("content outside the sections must be preserved")
	}
}

// Stale-spelling merge guard (container/heap): a GoImplement record persisted by an EARLIER
// converter run under a now-stale spelling must NOT survive a re-merge as a SECOND record for the
// same (impl, interface) pair. A NESTED package-under-test's own interface was once emitted fully
// qualified (`go.container.heap_package.Interface`); stripLocalTypeQualifier (the math/rand/v2
// collapse fix) now canonicalizes it to the bare local `Interface`. The -tests external-variant
// write MERGES the committed package_info_external_test.cs, so the stale qualified line and the freshly
// rendered bare line both reached the emitting HashSet and go2cs-gen composed the adapter twice —
// CS0102 + CS0111 + CS8646 on IntHeapжInterface. writePackageInfoFile now normalizes each merged-in
// GoImplement line through the same qualifyLocalTypeRef canonicalization the fresh render applies,
// so the two collapse to ONE. Guards the container/heap -tests re-validation from a fresh converter.
func TestMergedStaleGoImplementSpellingCollapses(t *testing.T) {
	dir := t.TempDir()
	fileName := filepath.Join(dir, externalTestPackageInfoFileName)

	// The committed (stale) external-test metadata: the interface is spelled fully qualified, as an
	// older converter emitted it before stripLocalTypeQualifier reduced it to the bare local form.
	seed := strings.Join([]string{
		"// <ImportedTypeAliases>",
		"// </ImportedTypeAliases>",
		"",
		"using go;",
		"using static go.container.heap_package;",
		"using static go.container.heap_test_package;",
		"",
		"// <ExportedTypeAliases>",
		"// </ExportedTypeAliases>",
		"",
		"// <InterfaceImplementations>",
		"[assembly: GoImplement<IntHeap, go.container.heap_package.Interface>(Pointer = true)]",
		"// </InterfaceImplementations>",
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"namespace go.container;",
		"",
		"public static partial class heap_test_package",
		"{",
		"}",
	}, "\r\n")

	if err := os.WriteFile(fileName, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageName = "heap_test"
	packageNamespace = "go.container"

	// What convertTestVariant installs for a -tests run of a NESTED package under test, and the
	// interface record the external variant's `heap.Init(&h)` cast rediscovers by import path.
	previous := testLocalTypePrefixes
	t.Cleanup(func() { testLocalTypePrefixes = previous })
	testLocalTypePrefixes = []string{"go.container.heap_package"}
	interfaceImplementations["container.heap_package.Interface"] = NewHashSet([]string{PointerPrefix + "<IntHeap>"})

	writePackageInfoFile(fileName, true)

	data, err := os.ReadFile(fileName)
	if err != nil {
		t.Fatal(err)
	}

	contents := string(data)

	if got := strings.Count(contents, "GoImplement<IntHeap,"); got != 1 {
		t.Fatalf("IntHeap must map to the interface through exactly ONE GoImplement record (a duplicate composes a duplicate adapter, CS0102/CS0111); got %d:\n%s", got, contents)
	}
	if strings.Contains(contents, "go.container.heap_package.Interface") {
		t.Fatalf("the merged record must canonicalize to the bare generator-resolvable spelling, not the stale qualified one:\n%s", contents)
	}
	if !strings.Contains(contents, "[assembly: GoImplement<IntHeap, Interface>(Pointer = true)]") {
		t.Fatalf("the surviving record must be the bare-spelling pointer form:\n%s", contents)
	}
}

// B4/B5 guard (partition predicate): an EXTERNAL variant GoImplement record anchors to the test
// package unit when its generated code must live in the test package class — bare (test-local)
// impls, every non-production ж pointer adapter, and adapter-class-marked (interface-sourced /
// foreign-value ᴠ) pairs; production-qualified records keep the production anchor, where the
// partial-struct/adapter they generate merges with the production declaration.
func TestExternalVariantRecordPartitionAnchors(t *testing.T) {
	resetPackageState(&packages.Package{})
	packageNamespace = "go"
	adapterClassImplementations.Add("io.Writer|value_package.WriterTo")

	cases := []struct {
		iface string
		impl  string
		want  bool
	}{
		{"value_package.Interface", "ByAge", true},                                       // bare value ⇒ test partial struct
		{"value_package.Interface", PointerPrefix + "<multiSorter>", true},               // bare pointer ⇒ test-hosted adapter
		{"rand_package.Source", PointerPrefix + "<go.math.rand.rand_package.PCG>", true}, // true-foreign pointer adapter
		{"io.Writer", PointerPrefix + "<value_package.Builder>", false},                  // production pointer ⇒ production-hosted adapter
		{"io.Writer", PointerPrefix + "<go.value_package.Builder>", false},               // rooted production form
		{"value_package.Interface", "value_package.IntSlice", false},                     // production value ⇒ production partial struct
		{"io.Writer", "value_package.WriterTo", true},                                    // adapter-class-marked ⇒ consumer-hosted ᴠ class
	}

	for _, testCase := range cases {
		if got := isTestAnchoredImplementRecord(testCase.iface, testCase.impl, "value_package"); got != testCase.want {
			t.Errorf("isTestAnchoredImplementRecord(%q, %q) = %v, want %v", testCase.iface, testCase.impl, got, testCase.want)
		}
	}

	if !isTestAnchoredConversionRecord("localData", "value_package.Person") {
		t.Error("a conversion pair involving a bare (test-local) type must anchor to the test unit")
	}
	if isTestAnchoredConversionRecord("value_package.Data", "go.io_package.Writer") {
		t.Error("a conversion pair between qualified types must keep the production anchor")
	}
}

// B4/B5 guard (split write): the external variant's test-anchored records land in
// package_info_external_test.cs — whose FIRST class is the test package class (the generator's anchor)
// and which carries NO [GoPackage] (the attribute-bearing partial stays in
// package_test_info.cs, CS0579) — while production-anchored records and the alias globals merge
// into package_test_info.cs. A variant with NO test-anchored records writes no unit at all
// (utf8-class packages keep their single-file shape).
func TestWriteExternalVariantMetadataSplitsAnchors(t *testing.T) {
	dir := t.TempDir()
	testInfoPath := filepath.Join(dir, testPackageInfoFileName)

	seed := strings.Join([]string{
		"// <ImportedTypeAliases>",
		"// </ImportedTypeAliases>",
		"",
		"using go;",
		"using static go.value_package;",
		"",
		"// <ExportedTypeAliases>",
		"// </ExportedTypeAliases>",
		"",
		"// <InterfaceImplementations>",
		"// </InterfaceImplementations>",
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"namespace go;",
		"",
		"[GoPackage(\"value\")]",
		"public static partial class value_package",
		"{",
		"}",
	}, "\r\n")

	if err := os.WriteFile(testInfoPath, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageName = "value_test"
	packageNamespace = "go"
	interfaceImplementations["value_package.Interface"] = NewHashSet([]string{
		"localSorter", // bare ⇒ test anchor
		"value_package.IntSlice", // production-qualified ⇒ production anchor
	})
	importedTypeAliases["alpha"] = "go.alpha_package.Alpha"

	unitName, err := writeExternalVariantMetadata(testInfoPath, dir, "value")
	if err != nil {
		t.Fatal(err)
	}
	if unitName != externalTestPackageInfoFileName {
		t.Fatalf("unit name = %q, want %q", unitName, externalTestPackageInfoFileName)
	}

	unitData, err := os.ReadFile(filepath.Join(dir, externalTestPackageInfoFileName))
	if err != nil {
		t.Fatal(err)
	}
	unit := string(unitData)

	if !strings.Contains(unit, "[assembly: GoImplement<localSorter, value_package.Interface>]") {
		t.Fatalf("test-anchored record must land in the external unit:\n%s", unit)
	}
	if strings.Contains(unit, "IntSlice") {
		t.Fatalf("production-qualified record must not land in the external unit:\n%s", unit)
	}
	if strings.Contains(unit, "GoPackage") {
		t.Fatalf("the external unit must not duplicate the [GoPackage] attribute (CS0579):\n%s", unit)
	}
	if strings.Contains(unit, "global using alpha") {
		t.Fatalf("global using aliases must stay in package_test_info.cs (CS1537):\n%s", unit)
	}

	classIndex := strings.Index(unit, "public static partial class value_test_package")
	if classIndex < 0 || strings.Contains(unit[:classIndex], "partial class value_package") {
		t.Fatalf("the external unit's FIRST class must be the test package class:\n%s", unit)
	}

	infoData, err := os.ReadFile(testInfoPath)
	if err != nil {
		t.Fatal(err)
	}
	info := string(infoData)

	if !strings.Contains(info, "[assembly: GoImplement<value_package.IntSlice, value_package.Interface>]") {
		t.Fatalf("production-qualified record must merge into package_test_info.cs:\n%s", info)
	}
	if strings.Contains(info, "localSorter") {
		t.Fatalf("test-anchored record must not merge into package_test_info.cs:\n%s", info)
	}
	if !strings.Contains(info, "global using alpha = go.alpha_package.Alpha;") {
		t.Fatalf("alias globals must merge into package_test_info.cs:\n%s", info)
	}

	// A variant with no test-anchored records writes no unit.
	unitOnlyDir := t.TempDir()
	secondInfoPath := filepath.Join(unitOnlyDir, testPackageInfoFileName)
	if err := os.WriteFile(secondInfoPath, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageName = "value_test"
	packageNamespace = "go"
	interfaceImplementations["value_package.Interface"] = NewHashSet([]string{"value_package.IntSlice"})

	unitName, err = writeExternalVariantMetadata(secondInfoPath, unitOnlyDir, "value")
	if err != nil {
		t.Fatal(err)
	}
	if unitName != "" {
		t.Fatalf("no unit expected for a production-only record set, got %q", unitName)
	}
	if _, err := os.Stat(filepath.Join(unitOnlyDir, externalTestPackageInfoFileName)); !os.IsNotExist(err) {
		t.Fatal("no external unit file may be written when the variant has no test-anchored records")
	}
}

// B2c guard: a `using` ALIAS in the test metadata targeting an assembly the package reaches only
// TRANSITIVELY yields a direct project-reference import for that assembly — matched through the
// same namespace rendering the alias emission uses; direct dependencies, the production package,
// `using static` lines, and comment lines contribute nothing.
func TestAliasReferenceImportsAddsTransitiveAliasTargets(t *testing.T) {
	dir := t.TempDir()
	infoPath := filepath.Join(dir, testPackageInfoFileName)

	contents := strings.Join([]string{
		"// Example: global using mypkgꓸTable = go.map<go.@string, nint>;",
		"// <ImportedTypeAliases>",
		"global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;",
		"global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;",
		"using conv = go.@internal.convalias_package;",
		"// </ImportedTypeAliases>",
		"using go;",
		"using static go.value_package;",
	}, "\r\n")

	if err := os.WriteFile(infoPath, []byte(contents), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	importPackageDirs = map[string]importedPackageMeta{
		"internal/abi":         {Dir: dir, Name: "abi"},
		"internal/reflectlite": {Dir: dir, Name: "reflectlite"},
		"internal/convalias":   {Dir: dir, Name: "convalias"},
		"internal/unrelated":   {Dir: dir, Name: "unrelated"},
	}

	got := aliasReferenceImports([]string{infoPath, filepath.Join(dir, "missing.cs")}, "sort", []string{"internal/reflectlite"})
	want := []string{"internal/abi", "internal/convalias"}

	if !reflect.DeepEqual(got, want) {
		t.Fatalf("alias reference imports = %v, want %v", got, want)
	}
}

func loadBothTestVariantsForDir(t *testing.T, dir string) (internal, external *packages.Package) {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}
	production := findProductionPackage(loaded, dir)
	if production == nil {
		t.Fatal("production package was not loaded")
	}
	return findTestVariants(loaded, production)
}

func readConvertedTestFile(t *testing.T, dir, name string) string {
	t.Helper()

	data, err := os.ReadFile(filepath.Join(dir, name))
	if err != nil {
		t.Fatal(err)
	}
	return string(data)
}

// B2 guard (production-name pinning): a `_test.go` METHOD declared over a production TYPE's name
// must Δ-rename the TEST-side declarator — the production .cs on disk keeps the bare name, so the
// pre-fix element rename split one assembly into two disagreeing halves (strings' export_test.go
// `func (r *Replacer) Replacer()`: CS0102 `strings_package` already contains `Replacer` + CS0246
// `ΔReplacer`). The rename must hold at the declaration, at internal-variant call sites, AND at
// EXTERNAL-variant call sites (the export_test pattern — both variants share one load, so the
// session-scoped object-keyed registry carries the internal pass's rename into the external one).
func TestTestVariantPinsProductionTypeAgainstTestMethodCollision(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/pinned\n\ngo 1.23\n",
		"value.go": "package pinned\n\ntype Replacer struct{ n int }\n\nfunc NewReplacer(n int) *Replacer { return &Replacer{n: n} }\n",
		"export_test.go": "package pinned\n\nfunc (r *Replacer) Replacer() int { return r.n }\n\n" +
			"func replacerProbe(r *Replacer) int { return r.Replacer() }\n",
		"external_test.go": "package pinned_test\n\nimport \"example/pinned\"\n\n" +
			"func externalProbe(r *pinned.Replacer) int { return r.Replacer() }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	internal, external := loadBothTestVariantsForDir(t, dir)
	if internal == nil || external == nil {
		t.Fatal("both test variants must load")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	// Session scope mirrors processTestConversion: ONE registry across both variant passes.
	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", options); err != nil {
		t.Fatal(err)
	}

	if nameCollisions["Replacer"] {
		t.Fatal("the production type name must stay pinned — nameCollisions must not Δ-rename `Replacer` in a test variant")
	}

	exportCs := readConvertedTestFile(t, outputPath, "export_test.cs")

	if !strings.Contains(exportCs, ShadowVarMarker+"Replacer(") {
		t.Fatalf("the test-side method declarator must be Δ-renamed:\n%s", exportCs)
	}
	if !strings.Contains(exportCs, "."+ShadowVarMarker+"Replacer()") {
		t.Fatalf("the internal-variant call site must follow the Δ-renamed method:\n%s", exportCs)
	}
	if strings.Contains(exportCs, "ref "+ShadowVarMarker+"Replacer") || strings.Contains(exportCs, PointerPrefix+"<"+ShadowVarMarker+"Replacer>") {
		t.Fatalf("the production TYPE must keep its bare name in every reference:\n%s", exportCs)
	}

	if _, _, err := convertTestVariant(external, testFileEntries(external), outputPath, "go", options); err != nil {
		t.Fatal(err)
	}

	externalCs := readConvertedTestFile(t, outputPath, "external_test.cs")

	if !strings.Contains(externalCs, "."+ShadowVarMarker+"Replacer()") {
		t.Fatalf("an EXTERNAL-variant call site must follow the internal variant's Δ-renamed method:\n%s", externalCs)
	}
}

// B9 guard (dot-import shadowing): a TEST-declared method named like a dot-imported production
// FUNCTION the variant references unqualified must Δ-rename the TEST-side declarator (C# member
// lookup binds the enclosing class's method group before `using static` — sort_test's `Sort(data)`
// bound example_keys_test.go's `By.Sort`, CS1501 ×14), while the dot-imported call keeps its bare
// emission. Discrimination both ways: a same-named method whose production function is never
// referenced (Stable), or referenced only QUALIFIED (Reverse), keeps its plain name.
func TestTestVariantRenamesTestMethodShadowingDotImportedFunction(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod":   "module example/value\n\ngo 1.23\n",
		"value.go": "package value\n\nfunc Sort(x []int) {}\n\nfunc Stable(x []int) {}\n\nfunc Reverse(x []int) {}\n",
		"value_test.go": "package value_test\n\nimport . \"example/value\"\n\n" +
			"type By []int\n\n" +
			"func (by By) Sort() { Sort(by) }\n\n" +
			"func (by By) Stable() {}\n\n" +
			"func probe(by By) {\n\tby.Sort()\n\tby.Stable()\n}\n",
		"qualified_test.go": "package value_test\n\nimport value \"example/value\"\n\n" +
			"type QB []int\n\nfunc (qb QB) Reverse() { value.Reverse(qb) }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	_, external := loadBothTestVariantsForDir(t, dir)
	if external == nil {
		t.Fatal("external test variant was not loaded")
	}

	outputPath := t.TempDir()
	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}
	if _, _, err := convertTestVariant(external, testFileEntries(external), outputPath, "go", options); err != nil {
		t.Fatal(err)
	}

	valueCs := readConvertedTestFile(t, outputPath, "value_test.cs")

	if !strings.Contains(valueCs, ShadowVarMarker+"Sort(") {
		t.Fatalf("the shadowing test method declarator must be Δ-renamed:\n%s", valueCs)
	}
	if !strings.Contains(valueCs, "."+ShadowVarMarker+"Sort()") {
		t.Fatalf("the test method's call sites must follow the Δ-rename:\n%s", valueCs)
	}

	// After removing every Δ-renamed occurrence, a bare `Sort(` must remain — the dot-imported
	// production call keeps its unqualified emission (bound through `using static`).
	if !strings.Contains(strings.ReplaceAll(valueCs, ShadowVarMarker+"Sort", ""), "Sort(") {
		t.Fatalf("the dot-imported production call must keep its bare emission:\n%s", valueCs)
	}

	if strings.Contains(valueCs, ShadowVarMarker+"Stable") {
		t.Fatalf("a same-named method whose production function is never referenced unqualified must keep its plain name:\n%s", valueCs)
	}

	qualifiedCs := readConvertedTestFile(t, outputPath, "qualified_test.cs")

	if strings.Contains(qualifiedCs, ShadowVarMarker+"Reverse") {
		t.Fatalf("a QUALIFIED production reference must not trigger the rename (Sel exclusion):\n%s", qualifiedCs)
	}
}

// Receiver/first-parameter guard: a `_test.go` FREE FUNCTION whose emitted C# signature matches a
// production METHOD's — because the method's receiver becomes the extension method's leading `this`
// parameter — must Δ-rename the TEST-side declarator. Go keeps method names and package-scope
// function names in separate namespaces, so math/big's `func (z nat) norm() nat` (nat.go) and
// `func norm(x nat) nat` (int_test.go) coexist legally, but both emit as `norm(nat)` and `this` does
// not participate in C# signature identity (CS0111 `big_package` already defines `norm`).
// Discrimination: a same-named free function whose parameters do NOT line up with the receiver
// (different type, or an extra parameter) emits distinctly and must keep its plain name.
func TestTestVariantRenamesTestFuncCollidingWithProductionMethodReceiver(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/limbs\n\ngo 1.23\n",
		"value.go": "package limbs\n\ntype nat []uint\n\n" +
			"func (z nat) norm() nat { return z }\n\n" +
			"func (z nat) trim() nat { return z }\n\n" +
			"func (z nat) keep() nat { return z }\n",
		"value_test.go": "package limbs\n\n" +
			// Collides: receiver nat + no params vs one nat param.
			"func norm(x nat) nat { return x.norm() }\n\n" +
			// Does NOT collide: the free function takes an extra parameter.
			"func trim(x nat, n int) nat { return x.trim() }\n\n" +
			// Does NOT collide: the free function's first parameter is a different type.
			"func keep(n int) nat { return nil }\n\n" +
			"func probe(x nat) nat { return norm(trim(keep(0), 1)) }\n",
	}
	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}
	internal, _ := loadBothTestVariantsForDir(t, dir)
	if internal == nil {
		t.Fatal("internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}
	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", options); err != nil {
		t.Fatal(err)
	}

	valueCs := readConvertedTestFile(t, outputPath, "value_test.cs")

	if !strings.Contains(valueCs, ShadowVarMarker+"norm(") {
		t.Fatalf("the colliding test-side FREE FUNCTION declarator must be Δ-renamed:\n%s", valueCs)
	}
	if !strings.Contains(valueCs, ShadowVarMarker+"norm(trim(") {
		t.Fatalf("the test function's call sites must follow the Δ-rename:\n%s", valueCs)
	}

	// The production method keeps its bare name — it is pinned, and only the free function moved.
	if !strings.Contains(valueCs, ".norm()") {
		t.Fatalf("the production METHOD must keep its bare name at the call site:\n%s", valueCs)
	}

	if strings.Contains(valueCs, ShadowVarMarker+"trim") {
		t.Fatalf("an extra-parameter free function does not collide and must keep its plain name:\n%s", valueCs)
	}
	if strings.Contains(valueCs, ShadowVarMarker+"keep") {
		t.Fatalf("a different-first-parameter free function does not collide and must keep its plain name:\n%s", valueCs)
	}
}
