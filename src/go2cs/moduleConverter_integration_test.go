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
// parallel tree under the deploy root (the app to src\, the dependency lib to pkg\ — keeping the Go
// source pure), the cross-package call resolves, references wire to the lib ($(go2csPath)pkg) and the
// stdlib ($(go2csPath)core), and a flat recurse solution is emitted. Deterministic and network-free.
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
		go2csPath:           filepath.Join(root, "out"),
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
	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "app", "main.cs"))
	greetingCs := readGenerated(t, filepath.Join(options.go2csPath, "pkg", "example.com", "lib", "greeting.cs"))

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

	// The app csproj references the lib via $(go2csPath)pkg and the stdlib fmt via $(go2csPath)core.
	appCsproj := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "app", "example.com.app.csproj"))

	if !strings.Contains(appCsproj, "$(go2csPath)pkg\\example.com\\lib\\example.com.lib.csproj") {
		t.Errorf("app csproj missing the lib ProjectReference under $(go2csPath)pkg:\n%s", appCsproj)
	}

	if !strings.Contains(appCsproj, "$(go2csPath)core\\fmt\\fmt.csproj") {
		t.Errorf("app csproj missing the stdlib fmt reference:\n%s", appCsproj)
	}

	// A flat recurse solution was written at the deploy root listing both converted projects.
	slnx := readGenerated(t, filepath.Join(options.go2csPath, recurseSolutionFileName))

	if !strings.Contains(slnx, "example.com.app.csproj") || !strings.Contains(slnx, "example.com.lib.csproj") {
		t.Errorf("recurse solution missing the app or lib project:\n%s", slnx)
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
