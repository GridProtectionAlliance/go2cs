package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// TestBuildSolutionXML checks the emitted .slnx structure: the configuration block, the
// generator/core folders, project ordering, CRLF line endings, and no BOM — matching the
// format of the existing src/go2cs.slnx.
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
		// Folders carry a deterministic Id attribute (see folderID) — match the opening tag and
		// name, tolerating the trailing ` Id="…"`.
		"<Folder Name=\"/generators/\" Id=\"",
		"<Project Path=\"gen/go2cs-gen/go2cs-gen.csproj\" />",
		"<Folder Name=\"/core/\" Id=\"",
		"<Project Path=\"core/fmt/fmt.csproj\" />",
		"<Project Path=\"core/golib/golib.csproj\" />",
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

	// No BOM; starts directly with the root element (like src/go2cs.slnx).
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("solution XML must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	// CRLF line endings only — no bare LF should remain once CRLF pairs are removed.
	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("solution XML contains a bare LF; expected CRLF only")
	}
}

// TestBuildFlatSolutionXML checks the flat recurse solution (ModuleConverter): the config block, a
// plain project list with NO <Folder> grouping, CRLF line endings, and no BOM — matching the shape
// deploy-core.ps1 emits for go2cs-core.slnx. Third-party libs land under pkg/, and an in-place app
// is referenced via a relative (..) path.
func TestBuildFlatSolutionXML(t *testing.T) {
	projects := []string{
		"pkg/github.com/google/uuid/github.com.google.uuid.csproj",
		"../cache-test/example.com.cachetest.csproj",
	}

	xml := buildFlatSolutionXML(projects)

	wantContains := []string{
		"<Solution>",
		"<Configurations>",
		"<Platform Name=\"Any CPU\" />",
		"<Project Path=\"pkg/github.com/google/uuid/github.com.google.uuid.csproj\" />",
		"<Project Path=\"../cache-test/example.com.cachetest.csproj\" />",
		"</Solution>",
	}

	for _, want := range wantContains {
		if !strings.Contains(xml, want) {
			t.Errorf("flat solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// Flat: no namespace folder grouping.
	if strings.Contains(xml, "<Folder") {
		t.Errorf("flat solution must not contain <Folder> elements\n%s", xml)
	}

	// No BOM; CRLF only.
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("flat solution must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("flat solution contains a bare LF; expected CRLF only")
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
