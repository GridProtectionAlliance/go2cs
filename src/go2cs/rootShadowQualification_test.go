package main

import (
	"go/types"
	"testing"
)

// Guards for the `go.go` root-shadow qualification and the record/reference canonicalization the
// -tests pipeline depends on. All four defects these cover are invisible to the behavioral corpus
// (it never runs `-tests` and no behavioral package imports a `go/*` package), so they are guarded
// here at the unit level instead. Each was proven to fail before its fix — see the per-test notes.

// setShadowState installs the package-scoped globals rootQualified/rootNamespaceShadowed read, and
// restores them when the test ends.
func setShadowState(t *testing.T, namespace string, childNamespaces map[string]bool) {
	t.Helper()

	previousNamespace, previousChildren := packageNamespace, packageChildNamespaces

	t.Cleanup(func() {
		packageNamespace, packageChildNamespaces = previousNamespace, previousChildren
	})

	packageNamespace = namespace

	if childNamespaces == nil {
		childNamespaces = map[string]bool{}
	}

	packageChildNamespaces = childNamespaces
}

// Blocker A(b): a using-directive target composed DIRECTLY from packageNamespace (the -tests
// production-class anchor and the test host's `using go.testing_runtime;`) bypassed rootQualified
// entirely, so its leading `go` re-bound to the `go.go` namespace a go/* import contributes —
// math/rand/v2's regress_test.go imports go/format, and the resulting CS0234 accounted for 13 of
// the package's 22 compile errors. Before globalQualifyRooted these targets emitted bare.
func TestGlobalQualifyRootedForcesGlobalUnderRootShadow(t *testing.T) {
	cases := []struct {
		name            string
		namespace       string
		childNamespaces map[string]bool
		rooted          string
		want            string
	}{
		{
			name:      "no shadow leaves the bare root prefix",
			namespace: "go.math.rand",
			rooted:    "go.math.rand.rand_package",
			want:      "go.math.rand.rand_package",
		},
		{
			name:            "go/* in the import closure forces global::",
			namespace:       "go.math.rand",
			childNamespaces: map[string]bool{"go.go": true},
			rooted:          "go.math.rand.rand_package",
			want:            "global::go.math.rand.rand_package",
		},
		{
			name:      "a go/* package's own namespace forces global::",
			namespace: "go.go.format",
			rooted:    "go.testing_runtime",
			want:      "global::go.testing_runtime",
		},
		{
			name:            "already-global target is left alone (idempotent)",
			namespace:       "go.math.rand",
			childNamespaces: map[string]bool{"go.go": true},
			rooted:          "global::go.testing_runtime",
			want:            "global::go.testing_runtime",
		},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			setShadowState(t, tc.namespace, tc.childNamespaces)

			if got := globalQualifyRooted(tc.rooted); got != tc.want {
				t.Errorf("globalQualifyRooted(%q) = %q, want %q", tc.rooted, got, tc.want)
			}
		})
	}
}

// Blocker A(a): the shadow gate is computed from the import closure of the package being
// converted. Under -tests the PRODUCTION sources are recompiled into the test assembly, so the
// gate has to see the union of the production and _test.go closures — computeImportAliasRenames
// folds siblingClosureImportPaths in for exactly that reason. Without the union the production
// half emitted bare `go.` prefixes into an assembly that does contain `go.go`.
func TestSiblingClosureContributesRootShadow(t *testing.T) {
	previous := siblingClosureImportPaths
	t.Cleanup(func() { siblingClosureImportPaths = previous })

	previousQualified := packageQualifiedNamespaces
	t.Cleanup(func() { packageQualifiedNamespaces = previousQualified })

	// An import-free stand-in for the production package: the closure under test is the SIBLING
	// (_test.go) half, which computeImportAliasRenames folds in on top of the package's own.
	production := types.NewPackage("math/rand/v2", "rand")

	setShadowState(t, "go.math.rand", nil)
	packageQualifiedNamespaces = map[string]bool{}

	// Production-only closure: no go/* package, so no shadow.
	siblingClosureImportPaths = nil
	computeImportAliasRenames(nil, production, packageNamespace)

	if rootNamespaceShadowed() {
		t.Fatal("production-only closure reported a go.go root shadow")
	}

	// The _test.go half imports go/format (regress_test.go), which must now register the shadow.
	setShadowState(t, "go.math.rand", nil)
	packageQualifiedNamespaces = map[string]bool{}
	siblingClosureImportPaths = []string{"go/format"}
	computeImportAliasRenames(nil, production, packageNamespace)

	if !rootNamespaceShadowed() {
		t.Fatal("sibling test closure importing go/format did not register the go.go root shadow")
	}
}

// A FOREIGN pair's cast-site spelling must NOT be collapsed onto the class-relative spelling a
// package_info file carries. Doing so makes the consumer's cast match the foreign package's own
// record, which suppresses the LOCAL record go2cs-gen needs in order to emit the implicit
// conversion into the CONSUMER's assembly — expvar / net/http/cgi / internal/trace/traceviewer all
// broke on `HandlerFunc` → `ΔHandler` (CS0029 ×3 + CS1503) when this was collapsed. The
// same-assembly case is handled at emission (TestStripLocalTypeQualifier) instead.
func TestCanonicalRecordIfaceNameKeepsForeignQualification(t *testing.T) {
	cases := []struct {
		name        string
		ifaceName   string
		rootPackage string
		want        string
	}{
		{"bare name is qualified with the recording package class", "Source", "rand", "rand_package.Source"},
		{"universe error stays bare", "error", "rand", "error"},
		{"class-relative form is already canonical", "rand_package.Source", "rand", "rand_package.Source"},
		{"a foreign cast-site chain keeps its qualification", "go.net.http_package.ΔHandler", "http", "net.http_package.ΔHandler"},
		{"a nested type reference is left alone", "x.y_package.Outer.Inner", "y", "x.y_package.Outer.Inner"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			if got := canonicalRecordIfaceName(tc.ifaceName, tc.rootPackage); got != tc.want {
				t.Errorf("canonicalRecordIfaceName(%q, %q) = %q, want %q", tc.ifaceName, tc.rootPackage, got, tc.want)
			}
		})
	}
}

// Blocker B: a record naming a type that compiles into THIS assembly through a fully-qualified
// class must render in the bare local form, so the two spellings of one resolved pair collapse in
// the emitting HashSet. Under -tests that covers the PACKAGE UNDER TEST as well as the current
// package: the external `<name>_test` variant reaches it by import path and renders it qualified,
// while the seeded production metadata carries it short, so math/rand/v2 emitted both
// `GoImplement<PCG, Source>` and `GoImplement<go.math.rand.rand_package.PCG, …Source>` and
// go2cs-gen defined `rand_package.PCGжSource` twice (CS0102 + CS0111 ×5 + CS8646).
func TestStripLocalTypeQualifier(t *testing.T) {
	const localPrefix = "go.math.rand.rand_test_package"

	previous := testLocalTypePrefixes
	t.Cleanup(func() { testLocalTypePrefixes = previous })

	// What convertTestVariant installs for a -tests run of math/rand/v2.
	testLocalTypePrefixes = []string{"go.math.rand.rand_package"}

	cases := []struct {
		name string
		in   string
		want string
	}{
		{"package-under-test type reduces to the bare name", "go.math.rand.rand_package.PCG", "PCG"},
		{"both sides of a pair reduce", "go.math.rand.rand_package.PCG, go.math.rand.rand_package.Source", "PCG, Source"},
		{"the current package's own class reduces too", "go.math.rand.rand_test_package.helper", "helper"},
		{"a foreign reference is untouched", "io_package.Reader", "io_package.Reader"},
		{"a genuinely foreign qualified reference is untouched", "go.net.http_package.ΔHandler", "go.net.http_package.ΔHandler"},
		{"an already-bare name is untouched", "PCG", "PCG"},
		{"a nested member under a local class is untouched", "go.math.rand.rand_package.Outer.Inner", "go.math.rand.rand_package.Outer.Inner"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			if got := stripLocalTypeQualifier(tc.in, localPrefix); got != tc.want {
				t.Errorf("stripLocalTypeQualifier(%q) = %q, want %q", tc.in, got, tc.want)
			}
		})
	}
}

// Outside a -tests conversion no extra prefix is local, so only the current package's own class is
// stripped — the property that keeps every non-test package's metadata byte-identical.
func TestStripLocalTypeQualifierIgnoresForeignPrefixesOutsideTests(t *testing.T) {
	previous := testLocalTypePrefixes
	t.Cleanup(func() { testLocalTypePrefixes = previous })

	testLocalTypePrefixes = nil

	if got := stripLocalTypeQualifier("go.math.rand.rand_package.PCG", "go.expvar_package"); got != "go.math.rand.rand_package.PCG" {
		t.Errorf("stripLocalTypeQualifier stripped a foreign prefix outside -tests: %q", got)
	}
}
