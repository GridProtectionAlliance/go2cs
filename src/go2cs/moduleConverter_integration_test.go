// moduleConverter_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestRecurseSyntheticModule is the P5 synthetic integration test: it runs the real -recurse
// conversion over a two-module fixture — an `app` main package importing a co-located `lib` module
// via `replace`, which imports the standard library — and confirms the recurse loop end to end: the
// closure is partitioned, the lib converts before the app (topological order), each converts into the
// parallel tree under an isolated output root (the app to src\, the dependency lib to pkg\ — keeping
// the Go source pure), the cross-package call resolves, references wire relatively to the lib and via
// $(go2csPath) to the stdlib, and a folder-grouped recurse solution is emitted. Deterministic and
// network-free.
func TestRecurseSyntheticModule(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// The convert-set is exactly {app, lib}, ordered least-dependencies-first (lib before app).
	queue := converter.graph.sortedQueue

	if len(queue) != 2 || queue[0] != "example.com/lib" || queue[1] != "example.com/app" {
		t.Fatalf("convert order = %v, want [example.com/lib example.com/app]", queue)
	}

	// Each package converts into the parallel tree under the deploy root — the app to src\<import>,
	// the dependency lib to pkg\<import> — leaving the original Go source directories pure.
	mainCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "main.cs"))
	greetingCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "greeting.cs"))

	if !strings.Contains(mainCs, "lib.Greeting") {
		t.Errorf("app main.cs missing the cross-package call lib.Greeting:\n%s", mainCs)
	}

	if !strings.Contains(greetingCs, "strings.TrimSpace") {
		t.Errorf("lib greeting.cs missing strings.TrimSpace:\n%s", greetingCs)
	}

	// The original Go source dirs must stay pure — no C# artifacts written in place.
	if _, err := os.Stat(filepath.Join(appDir, "main.cs")); !os.IsNotExist(err) {
		t.Errorf("app Go source dir was polluted with in-place C# output (main.cs)")
	}

	// The app csproj references the converted lib relatively within the isolated output tree and
	// the stdlib fmt via the independently selectable $(go2csPath) runtime root.
	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	libProject := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))
	libReference, err := filepath.Rel(appProjectDir, libProject)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative lib ProjectReference %q:\n%s", libReference, appCsproj)
	}

	if !strings.Contains(appCsproj, "$(go2csPath)core\\fmt\\fmt.csproj") {
		t.Errorf("app csproj missing the stdlib fmt reference:\n%s", appCsproj)
	}

	// A per-project solution sits next to the app csproj, over the app + its transitive converted
	// dependency (lib) + the shared runtime (golib) + the analyzer (go2cs-gen) — no stdlib listed. This
	// is the build-everything solution for the app; no separate flat deploy-root solution is written.
	appSlnx := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "example.com.app.slnx"))

	for _, want := range []string{"example.com.app.csproj", "example.com.lib.csproj", "golib.csproj", "go2cs-gen.csproj"} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing %q:\n%s", want, appSlnx)
		}
	}

	// Projects are grouped into the %GOPATH%-mirroring solution folders, emitted in the enforced
	// src → pkg → core order (deliberately NOT alphabetic): the app under /src/, its converted dependency
	// (lib) under /pkg/, and the runtime + analyzer under /core/.
	for _, want := range []string{`<Folder Name="/src/">`, `<Folder Name="/pkg/">`, `<Folder Name="/core/">`} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing folder %q:\n%s", want, appSlnx)
		}
	}

	srcIdx := strings.Index(appSlnx, `<Folder Name="/src/">`)
	pkgIdx := strings.Index(appSlnx, `<Folder Name="/pkg/">`)
	coreIdx := strings.Index(appSlnx, `<Folder Name="/core/">`)

	if !(srcIdx < pkgIdx && pkgIdx < coreIdx) {
		t.Errorf("solution folders not in enforced src→pkg→core order (src=%d pkg=%d core=%d):\n%s", srcIdx, pkgIdx, coreIdx, appSlnx)
	}

	// The app project is marked the Visual Studio default startup project.
	if !strings.Contains(appSlnx, `example.com.app.csproj" DefaultStartup="true"`) {
		t.Errorf("app per-project solution does not mark the app as the startup project:\n%s", appSlnx)
	}

	// The retired flat deploy-root solution must no longer be generated.
	if _, err := os.Stat(filepath.Join(options.recurseOutputRoot, "go2cs-recurse.slnx")); !os.IsNotExist(err) {
		t.Errorf("unexpected flat deploy-root solution go2cs-recurse.slnx was written")
	}

	if strings.Contains(appSlnx, "fmt.csproj") {
		t.Errorf("app per-project solution should not list stdlib projects (found fmt.csproj):\n%s", appSlnx)
	}
}

// TestRecurseNuGetReferences is the -recurse=nuget counterpart to TestRecurseSyntheticModule: it runs the
// same two-module fixture (app importing a co-located lib via replace, each importing the standard library)
// but with nugetRefs enabled, and confirms the reference rewrite. The go2cs standard library (fmt),
// runtime (golib) and analyzer (go2cs-gen) become NuGet PackageReferences (go.fmt / go.lib / go.gen), while
// the app's own converted dependency (lib) stays a LOCAL ProjectReference; an output-root
// Directory.Build.props defaults GoStdLibVersion; and the per-project solution drops golib/analyzer and
// the /core/ folder.
// Deterministic and network-free (asserts the emitted .csproj/.props/.slnx text; no dotnet restore).
func TestRecurseNuGetReferences(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		nugetRefs:           true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	libProject := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))

	// The go2cs standard library, runtime and analyzer are referenced from NuGet.
	for _, want := range []string{
		`<PackageReference Include="go.fmt" Version="$(GoStdLibVersion)" />`,
		`<PackageReference Include="go.lib" Version="$(GoStdLibVersion)" />`,
		`<PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />`,
	} {
		if !strings.Contains(appCsproj, want) {
			t.Errorf("app csproj missing NuGet reference %q:\n%s", want, appCsproj)
		}
	}

	// No local $(go2csPath) references remain for the stdlib or the runtime.
	for _, notWant := range []string{
		`$(go2csPath)core\fmt\fmt.csproj`,
		`$(go2csPath)core\golib\golib.csproj`,
		`$(go2csPath)gen\go2cs-gen\go2cs-gen.csproj`,
	} {
		if strings.Contains(appCsproj, notWant) {
			t.Errorf("app csproj still has local reference %q under -recurse=nuget:\n%s", notWant, appCsproj)
		}
	}

	// The app's OWN converted dependency stays a relative LOCAL ProjectReference.
	libReference, err := filepath.Rel(appProjectDir, libProject)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative local lib ProjectReference %q:\n%s", libReference, appCsproj)
	}

	// The Directory.Build.props is emitted at the isolated output root and defaults GoStdLibVersion.
	props := readGenerated(t, filepath.Join(options.recurseOutputRoot, "Directory.Build.props"))

	if strings.Contains(props, `<go2csPath>`) {
		t.Errorf("output-root Directory.Build.props should not couple go2csPath to converted output:\n%s", props)
	}

	if base := goVersion(); base != "" {
		if !strings.Contains(props, `<PropertyGroup Condition="'$(GoStdLibVersion)' == ''">`) {
			t.Errorf("Directory.Build.props missing the conditional GoStdLibVersion default:\n%s", props)
		}

		if want := "<GoStdLibVersion>" + base + ".*</GoStdLibVersion>"; !strings.Contains(props, want) {
			t.Errorf("Directory.Build.props missing floating version default %q:\n%s", want, props)
		}
	}

	// The per-project solution drops golib/analyzer (now NuGet) and the /core/ folder, but still lists the
	// app (src) and its converted dependency lib (pkg).
	appSlnx := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "example.com.app.slnx"))

	for _, notWant := range []string{"golib.csproj", "go2cs-gen.csproj", `<Folder Name="/core/">`} {
		if strings.Contains(appSlnx, notWant) {
			t.Errorf("app per-project solution should not list %q under -recurse=nuget:\n%s", notWant, appSlnx)
		}
	}

	for _, want := range []string{"example.com.app.csproj", "example.com.lib.csproj", `<Folder Name="/src/">`, `<Folder Name="/pkg/">`} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing %q:\n%s", want, appSlnx)
		}
	}
}

func writeModuleFile(t *testing.T, path, content string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		t.Fatalf("mkdir %s: %v", filepath.Dir(path), err)
	}

	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatalf("write %s: %v", path, err)
	}
}

func readGenerated(t *testing.T, path string) string {
	t.Helper()

	data, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("read generated %s: %v", path, err)
	}

	return string(data)
}
