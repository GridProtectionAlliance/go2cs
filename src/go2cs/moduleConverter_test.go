package main

import (
	"path/filepath"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestModuleConverterClassify locks the recurse partitioning rules: stdlib is recognized by its
// location under $GOROOT/src (covering GOROOT-vendored packages), the app by main-module
// membership, third-party by dependency-module membership, and pseudo/test packages are skipped.
func TestModuleConverterClassify(t *testing.T) {
	goRoot := filepath.FromSlash("C:/go")
	goRootSrc := filepath.Join(goRoot, "src")
	m := &ModuleConverter{options: Options{goRoot: goRoot}}

	appDir := filepath.FromSlash("C:/work/app")
	libDir := filepath.FromSlash("C:/work/lib")
	cacheDir := filepath.FromSlash("C:/gopath/pkg/mod/github.com/x/y@v1.2.3")

	appMod := &packages.Module{Path: "example.com/app", Main: true, Dir: appDir}
	libMod := &packages.Module{Path: "example.com/lib", Main: false, Dir: libDir}
	depMod := &packages.Module{Path: "github.com/x/y", Main: false, Dir: cacheDir}

	cases := []struct {
		name string
		pkg  *packages.Package
		want packageClass
	}{
		{"stdlib fmt", &packages.Package{PkgPath: "fmt", Dir: filepath.Join(goRootSrc, "fmt")}, classStdLib},
		{"goroot vendored", &packages.Package{PkgPath: "golang.org/x/net/dns/dnsmessage", Dir: filepath.Join(goRootSrc, "vendor", "golang.org", "x", "net", "dns", "dnsmessage")}, classStdLib},
		{"app main module", &packages.Package{PkgPath: "example.com/app", Dir: appDir, Module: appMod}, classApp},
		{"app subpackage", &packages.Package{PkgPath: "example.com/app/internal/util", Dir: filepath.Join(appDir, "internal", "util"), Module: appMod}, classApp},
		{"replace lib (third-party)", &packages.Package{PkgPath: "example.com/lib", Dir: libDir, Module: libMod}, classThirdParty},
		{"module cache dep", &packages.Package{PkgPath: "github.com/x/y", Dir: cacheDir, Module: depMod}, classThirdParty},
		{"unsafe", &packages.Package{PkgPath: "unsafe", Dir: filepath.Join(goRootSrc, "unsafe")}, classSkip},
		{"builtin", &packages.Package{PkgPath: "builtin", Dir: filepath.Join(goRootSrc, "builtin")}, classSkip},
		{"test variant", &packages.Package{PkgPath: "example.com/app_test", Dir: appDir, Module: appMod}, classSkip},
		{"empty dir", &packages.Package{PkgPath: "example.com/weird", Dir: ""}, classSkip},
		{"no module, not goroot", &packages.Package{PkgPath: "example.com/orphan", Dir: filepath.FromSlash("C:/elsewhere")}, classSkip},
	}

	for _, tc := range cases {
		if got := m.classify(tc.pkg); got != tc.want {
			t.Errorf("classify(%q) = %d, want %d", tc.name, got, tc.want)
		}
	}
}

// TestModuleConverterConvertSetOrder verifies the graph the module converter builds from a
// classified closure orders third-party leaves before the app that imports them, and that stdlib
// imports (outside the convert-set) do not appear or constrain the order. It exercises the same
// AddPackage/addImportEdges/topologicalSort path partition+buildEdges drive, without a real load.
func TestModuleConverterConvertSetOrder(t *testing.T) {
	m := NewModuleConverter(Options{})

	// Convert-set: an app main package and a co-located lib package; stdlib "fmt"/"strings" are
	// NOT added (they are referenced, not converted).
	m.graph.AddPackage("example.com/app", "/work/app")
	m.graph.AddPackage("example.com/lib", "/work/lib")

	m.graph.addImportEdges("example.com/app", []string{"example.com/lib", "fmt", "strings"})
	m.graph.addImportEdges("example.com/lib", []string{"strings"})
	m.graph.sortAdjacency()
	m.graph.topologicalSort()

	want := []string{"example.com/lib", "example.com/app"}

	if len(m.graph.sortedQueue) != len(want) || m.graph.sortedQueue[0] != want[0] || m.graph.sortedQueue[1] != want[1] {
		t.Fatalf("convert order = %v, want %v (lib before app; stdlib excluded)", m.graph.sortedQueue, want)
	}

	// The app depends only on the lib within the convert-set — stdlib edges are filtered out.
	appDeps := m.graph.packages["example.com/app"].Dependencies

	if len(appDeps) != 1 || appDeps[0] != "example.com/lib" {
		t.Errorf("app convert-set dependencies = %v, want [example.com/lib]", appDeps)
	}
}

// TestModuleConverterOutputDir verifies output routing (P3): a read-only module-cache dependency is
// routed to a writable $(go2csPath)pkg\<import-path> location with the @version stripped, while the
// app and co-located `replace` modules convert in place, co-located with their source.
func TestModuleConverterOutputDir(t *testing.T) {
	goPath := filepath.FromSlash("C:/gopath")
	go2csPath := filepath.FromSlash("C:/gopath/src/go2cs")
	m := &ModuleConverter{options: Options{goPath: goPath, go2csPath: go2csPath}}

	cacheDir := filepath.Join(goPath, "pkg", "mod", "github.com", "fatih", "color@v1.13.0")
	appDir := filepath.FromSlash("C:/work/app")
	libDir := filepath.FromSlash("C:/work/lib")

	cases := []struct {
		name    string
		pkgPath string
		srcDir  string
		want    string
	}{
		{"module-cache dep to pkg (version stripped)", "github.com/fatih/color", cacheDir, filepath.Join(go2csPath, "pkg", "github.com", "fatih", "color")},
		{"app in place", "example.com/app", appDir, appDir},
		{"co-located replace in place", "example.com/lib", libDir, libDir},
	}

	for _, tc := range cases {
		if got := m.outputDirFor(tc.pkgPath, tc.srcDir); got != tc.want {
			t.Errorf("outputDirFor(%q) = %q, want %q", tc.name, got, tc.want)
		}
	}
}
