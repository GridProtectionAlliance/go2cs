package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// TestBuildSolutionXML checks the emitted .slnx structure: the configuration block, the
// root-level infrastructure projects (golib + go2cs-gen, no folder), the per-package
// import-path folders, project ordering, CRLF line endings, and no BOM — matching the format
// of the existing src/go2cs.slnx.
func TestBuildSolutionXML(t *testing.T) {
	coreProjects := []string{
		"core/fmt/fmt.csproj",
		"core/golib/golib.csproj",
		"core/internal/abi/internal.abi.csproj",
	}
	testProjects := []string{
		"core/fmt/fmt_test.csproj",
	}

	xml := buildSolutionXML(coreProjects, testProjects)

	wantContains := []string{
		"<Solution>",
		"<Configurations>",
		"<Platform Name=\"Any CPU\" />",
		"<Platform Name=\"x64\" />",
		"<Platform Name=\"x86\" />",
		// go2cs-gen and golib are root-level projects (2-space indent), NOT wrapped in a folder —
		// the folder tree is reserved for Go import paths.
		"\r\n  <Project Path=\"gen/go2cs-gen/go2cs-gen.csproj\" />\r\n",
		"\r\n  <Project Path=\"core/golib/golib.csproj\" />\r\n",
		// Every package nests under a folder named by its bare Go import path (no /core/ umbrella).
		// Folders carry a deterministic Id attribute (see folderID) — match the opening tag and
		// name, tolerating the trailing ` Id="…"`.
		"<Folder Name=\"/fmt/\" Id=\"",
		"<Project Path=\"core/fmt/fmt.csproj\" />",
		"<Folder Name=\"/internal/abi/\" Id=\"",
		"<Project Path=\"core/internal/abi/internal.abi.csproj\" />",
		"<Folder Name=\"/tests/\" Id=\"",
		"<Project Path=\"core/fmt/fmt_test.csproj\" />",
		"</Solution>",
	}

	for _, want := range wantContains {
		if !strings.Contains(xml, want) {
			t.Errorf("solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// The old /core/ umbrella and /generators/ folder are gone — folders are bare import paths
	// and the two infrastructure projects live at the root.
	for _, unwanted := range []string{"<Folder Name=\"/core/\"", "<Folder Name=\"/generators/\""} {
		if strings.Contains(xml, unwanted) {
			t.Errorf("solution XML should not contain %q\n---\n%s", unwanted, xml)
		}
	}

	// golib is not grouped under a /golib/ import-path folder.
	if strings.Contains(xml, "<Folder Name=\"/golib/\"") {
		t.Errorf("golib should be a root project, not a /golib/ folder\n---\n%s", xml)
	}

	// No BOM; starts directly with the root element (like src/go2cs.slnx).
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("solution XML must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	// CRLF line endings only — no bare LF should remain once CRLF pairs are removed.
	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("solution XML contains a bare LF; expected CRLF only")
	}
}

// TestBuildRecurseSolutionXML checks a per-project recurse solution (ModuleConverter): the config block,
// the three %GOPATH%-mirroring solution folders in ENFORCED (non-alphabetic) order — /src/ then /pkg/ then
// /core/ — the startup (anchor) app project marked DefaultStartup under /src/, third-party deps under
// /pkg/, the runtime + analyzer under /core/, CRLF line endings, and no BOM.
func TestBuildRecurseSolutionXML(t *testing.T) {
	startup := "example.com.app.csproj" // the app is the solution's startup project

	folders := []solutionFolder{
		{name: "/src/", projects: []string{"example.com.app.csproj"}},
		{name: "/pkg/", projects: []string{"../../pkg/github.com/google/uuid/github.com.google.uuid.csproj"}},
		{name: "/core/", projects: []string{"../../core/golib/golib.csproj", "../../gen/go2cs-gen/go2cs-gen.csproj"}},
	}

	xml := buildRecurseSolutionXML(folders, startup)

	wantContains := []string{
		"<Solution>",
		"<Configurations>",
		"<Platform Name=\"Any CPU\" />",
		"<Folder Name=\"/src/\">",
		"<Project Path=\"example.com.app.csproj\" DefaultStartup=\"true\" />",
		"<Folder Name=\"/pkg/\">",
		"<Project Path=\"../../pkg/github.com/google/uuid/github.com.google.uuid.csproj\" />",
		"<Folder Name=\"/core/\">",
		"<Project Path=\"../../core/golib/golib.csproj\" />",
		"<Project Path=\"../../gen/go2cs-gen/go2cs-gen.csproj\" />",
		"</Solution>",
	}

	for _, want := range wantContains {
		if !strings.Contains(xml, want) {
			t.Errorf("recurse solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// Enforced folder order: /src/ before /pkg/ before /core/ — deliberately NOT alphabetic.
	srcIdx := strings.Index(xml, `<Folder Name="/src/">`)
	pkgIdx := strings.Index(xml, `<Folder Name="/pkg/">`)
	coreIdx := strings.Index(xml, `<Folder Name="/core/">`)

	if !(srcIdx < pkgIdx && pkgIdx < coreIdx) {
		t.Errorf("solution folders out of enforced order (src=%d pkg=%d core=%d):\n%s", srcIdx, pkgIdx, coreIdx, xml)
	}

	// No BOM; CRLF only.
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("recurse solution must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("recurse solution contains a bare LF; expected CRLF only")
	}
}

// TestBuildRecurseSolutionXMLSkipsEmptyFolders verifies a dependency package's own solution — which has no
// main-module (src) project — omits the /src/ folder entirely while keeping /pkg/ and /core/ in enforced
// order (mirroring how the /tests/ folder is omitted from the stdlib solution when empty).
func TestBuildRecurseSolutionXMLSkipsEmptyFolders(t *testing.T) {
	folders := []solutionFolder{
		{name: "/src/", projects: nil},
		{name: "/pkg/", projects: []string{"github.com.google.uuid.csproj"}},
		{name: "/core/", projects: []string{"../../core/golib/golib.csproj", "../../gen/go2cs-gen/go2cs-gen.csproj"}},
	}

	xml := buildRecurseSolutionXML(folders, "github.com.google.uuid.csproj")

	if strings.Contains(xml, `<Folder Name="/src/">`) {
		t.Errorf("expected no /src/ folder when there are no main-module projects\n%s", xml)
	}

	if !strings.Contains(xml, `<Folder Name="/pkg/">`) || !strings.Contains(xml, `<Folder Name="/core/">`) {
		t.Errorf("expected both /pkg/ and /core/ folders\n%s", xml)
	}

	// The third-party anchor is still the startup project in its own solution.
	if !strings.Contains(xml, `<Project Path="github.com.google.uuid.csproj" DefaultStartup="true" />`) {
		t.Errorf("dependency anchor should be marked DefaultStartup in its own solution\n%s", xml)
	}
}

// TestBuildSolutionXMLNoTestsFolderWhenEmpty verifies the /tests/ folder is omitted
// entirely when there are no converted test projects (the current state — Phase 4 has not
// emitted any yet).
func TestBuildSolutionXMLNoTestsFolderWhenEmpty(t *testing.T) {
	xml := buildSolutionXML([]string{"core/golib/golib.csproj"}, nil)

	if strings.Contains(xml, "/tests/") {
		t.Errorf("expected no /tests/ folder when there are no test projects\n%s", xml)
	}
}

// TestCollectConvertedProjects builds a fake output tree and confirms the walk collects
// package projects, splits out *_test.csproj into the test bucket, skips a stray copy of
// golib, and returns forward-slash solution-relative paths.
func TestCollectConvertedProjects(t *testing.T) {
	root := t.TempDir()

	files := []string{
		"core/fmt/fmt.csproj",
		"core/fmt/fmt_test.csproj",
		"core/internal/abi/internal.abi.csproj",
		"core/golib/golib.csproj", // stray copy — must be skipped (added explicitly elsewhere)
		"core/fmt/fmt.cs",         // not a csproj — ignored
	}

	for _, rel := range files {
		full := filepath.Join(root, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}
		if err := os.WriteFile(full, []byte("x"), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	converter := &StdLibConverter{go2csPath: root}

	coreProjects, testProjects, err := converter.collectConvertedProjects()

	if err != nil {
		t.Fatalf("collectConvertedProjects: %v", err)
	}

	coreSet := strings.Join(coreProjects, "\n")

	if !strings.Contains(coreSet, "core/fmt/fmt.csproj") {
		t.Errorf("expected fmt.csproj in core projects, got %v", coreProjects)
	}
	if !strings.Contains(coreSet, "core/internal/abi/internal.abi.csproj") {
		t.Errorf("expected internal.abi.csproj in core projects, got %v", coreProjects)
	}
	if strings.Contains(coreSet, "golib") {
		t.Errorf("golib copy under core/golib should be skipped, got %v", coreProjects)
	}
	if strings.Contains(coreSet, ".cs\n") || strings.HasSuffix(coreSet, ".cs") {
		t.Errorf("non-csproj file should be ignored, got %v", coreProjects)
	}

	if len(testProjects) != 1 || testProjects[0] != "core/fmt/fmt_test.csproj" {
		t.Errorf("expected exactly [core/fmt/fmt_test.csproj] in test projects, got %v", testProjects)
	}
}
