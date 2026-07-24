package main

import (
	"archive/zip"
	"context"
	"net/url"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestPipelineResolvesAndRecursivelyConvertsDependency(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds and runs go2cs")
	}

	repoRoot, err := filepath.Abs(filepath.Join("..", ".."))
	if err != nil {
		t.Fatal(err)
	}

	proxyRoot := t.TempDir()
	writeProxyModule(t, proxyRoot)
	proxyPath := filepath.ToSlash(proxyRoot)
	if filepath.VolumeName(proxyRoot) != "" {
		proxyPath = "/" + proxyPath
	}

	t.Setenv("GO2CS_BIN", "")
	t.Setenv("GOPROXY", (&url.URL{Scheme: "file", Path: proxyPath}).String())
	t.Setenv("GOSUMDB", "off")

	runner := newPipelineRunner(repoRoot)
	defer runner.close()
	runner.cacheDir = t.TempDir()
	converterName := "go2cs"
	if runtime.GOOS == "windows" {
		converterName += ".exe"
	}
	// A cache left by an older checkout must never be trusted across server processes. The first
	// conversion rebuilds the current checkout and replaces this deliberately invalid executable.
	if err := os.WriteFile(filepath.Join(runner.cacheDir, converterName), []byte("stale converter"), 0o700); err != nil {
		t.Fatal(err)
	}

	result, err := runner.convert(context.Background(), convertRequest{
		Runtime: runtimeCore,
		Source: `package main

import (
	"fmt"
	"example.com/tourhelper"
)

func main() {
	fmt.Println(tourhelper.Message())
}
`,
	})
	if err != nil {
		t.Fatal(err)
	}
	if !result.Successful {
		t.Fatalf("conversion failed:\n%s", result.Stage.Output)
	}
	if !strings.Contains(result.Stage.Output, "$ go mod tidy") ||
		!strings.Contains(result.Stage.Output, "Converting example.com/tourhelper") {
		t.Fatalf("dependency resolution/conversion missing from transcript:\n%s", result.Stage.Output)
	}
	if !strings.Contains(result.CSharp, "tourhelper.Message") {
		t.Fatalf("submitted app C# missing dependency call:\n%s", result.CSharp)
	}
	if !strings.Contains(result.Project, `pkg\example.com\tourhelper\example.com.tourhelper.csproj`) {
		t.Fatalf("app project missing relative dependency reference:\n%s", result.Project)
	}
	if strings.Contains(result.Project, `$(go2csPath)pkg`) {
		t.Fatalf("app project coupled converted dependency to go2csPath:\n%s", result.Project)
	}

	minimal, err := runner.convert(context.Background(), convertRequest{
		Runtime: runtimeCore,
		Source: `package main

func main() {}
`,
	})
	if err != nil {
		t.Fatal(err)
	}
	if !minimal.Successful {
		t.Fatalf("one-package conversion failed:\n%s", minimal.Stage.Output)
	}
}

func writeProxyModule(t *testing.T, proxyRoot string) {
	t.Helper()

	versionDir := filepath.Join(proxyRoot, "example.com", "tourhelper", "@v")
	if err := os.MkdirAll(versionDir, 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(versionDir, "list"), []byte("v1.0.0\n"), 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(versionDir, "v1.0.0.info"),
		[]byte(`{"Version":"v1.0.0","Time":"2026-01-01T00:00:00Z"}`), 0o600); err != nil {
		t.Fatal(err)
	}
	moduleFile := []byte("module example.com/tourhelper\n\ngo 1.23.1\n")
	if err := os.WriteFile(filepath.Join(versionDir, "v1.0.0.mod"), moduleFile, 0o600); err != nil {
		t.Fatal(err)
	}

	archive, err := os.Create(filepath.Join(versionDir, "v1.0.0.zip"))
	if err != nil {
		t.Fatal(err)
	}
	zipWriter := zip.NewWriter(archive)
	files := map[string][]byte{
		"go.mod": moduleFile,
		"helper.go": []byte(`package tourhelper

func Message() string { return "dependency converted" }
`),
	}
	for name, content := range files {
		entry, err := zipWriter.Create("example.com/tourhelper@v1.0.0/" + name)
		if err != nil {
			t.Fatal(err)
		}
		if _, err := entry.Write(content); err != nil {
			t.Fatal(err)
		}
	}
	if err := zipWriter.Close(); err != nil {
		t.Fatal(err)
	}
	if err := archive.Close(); err != nil {
		t.Fatal(err)
	}
}
