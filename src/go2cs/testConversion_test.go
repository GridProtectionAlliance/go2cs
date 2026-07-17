package main

import (
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
	mismatches, skipped := matchTerminalStatuses(names, goResults, csResults)

	if !reflect.DeepEqual(skipped, []string{"TestSkip"}) {
		t.Fatalf("skipped = %#v, want [TestSkip]", skipped)
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

// F14b guard: a dependency that fails to resolve fails the test-project emission loudly, naming
// the dependency — never a silent reference drop.
func TestWriteTestProjectFailsLoudlyOnDependencyError(t *testing.T) {
	dir := t.TempDir()
	importPackageDirs = map[string]importedPackageMeta{}

	err := writeTestProject(filepath.Join(dir, "broken.tests.csproj"), "broken", "go", nil, nil, nil,
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
