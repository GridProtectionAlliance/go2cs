// This file emits a Visual Studio .slnx solution for a whole-standard-library (-stdlib)
// conversion, so the freshly converted stdlib is openable / buildable as a single unit
// right after a run instead of relying on a hand-maintained solution that drifts.
package main

import (
	"crypto/sha1"
	"fmt"
	"io/fs"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
)

const (
	// generatedSolutionFileName is the .slnx solution emitted at the output root
	// (-go2cspath) after a -stdlib conversion. It is the auto-generated counterpart to the
	// committed src/go-src-converted.slnx (adopted 2026-07-10, replacing the hand-maintained .sln).
	generatedSolutionFileName = "go-src-converted.slnx"

	// golibProjectReference / genProjectReference are the solution-relative paths to the
	// shared runtime and the source-generator/analyzer project. They mirror the
	// $(go2csPath) references baked into every generated .csproj (where $(go2csPath)
	// resolves to $(SolutionDir)), so the solution locates them wherever those csproj
	// references already resolve — no absolute, machine-specific path is emitted.
	golibProjectReference = "core/golib/golib.csproj"
	genProjectReference   = "gen/go2cs-gen/go2cs-gen.csproj"
)

// GenerateSolutionFile writes a Visual Studio .slnx solution at the output root that
// references every converted stdlib project, any per-package test projects, the shared
// golib runtime, and the go2cs-gen analyzer. All references are relative to the solution
// file so the generated solution is portable.
func (c *StdLibConverter) GenerateSolutionFile() error {
	fmt.Println("Generating solution file...")

	coreProjects, testProjects, err := c.collectConvertedProjects()

	if err != nil {
		return err
	}

	// golib is the shared runtime; it lives under core/ in the repo source tree (not in the
	// converter's output). Add it so it is counted and present; buildSolutionXML emits it at
	// the solution root (no folder), not among the import-path package folders.
	coreProjects = append(coreProjects, golibProjectReference)

	// Sort for deterministic, stable output regardless of filesystem walk order.
	sort.Strings(coreProjects)
	sort.Strings(testProjects)

	contents := buildSolutionXML(coreProjects, testProjects)
	solutionFile := filepath.Join(c.go2csPath, generatedSolutionFileName)

	// Only rewrite when the content actually changed so repeated runs stay a no-op.
	if needToWriteFile(solutionFile, []byte(contents)) {
		if err := os.WriteFile(solutionFile, []byte(contents), 0644); err != nil {
			return fmt.Errorf("failed to write solution file \"%s\": %w", solutionFile, err)
		}
	}

	// Project count: converted core packages + golib (in coreProjects) + tests + the analyzer.
	fmt.Printf("Solution file generated: %s (%d projects)\n", solutionFile, len(coreProjects)+len(testProjects)+1)

	return nil
}

// collectConvertedProjects walks the emitted core/ output tree and returns the
// solution-relative (forward-slash) paths of every package .csproj, split into regular
// package projects and *_test.csproj test projects. golib is excluded here — the caller
// adds it explicitly so its reference is emitted even on a filtered run that produced no
// core output.
func (c *StdLibConverter) collectConvertedProjects() (coreProjects []string, testProjects []string, err error) {
	coreDir := filepath.Join(c.go2csPath, "core")

	if info, statErr := os.Stat(coreDir); statErr != nil || !info.IsDir() {
		// Nothing converted (or a filtered run produced no core output). The solution still
		// lists golib + the analyzer, which is harmless and keeps the output deterministic.
		return nil, nil, nil
	}

	walkErr := filepath.WalkDir(coreDir, func(path string, d fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if d.IsDir() || !strings.HasSuffix(d.Name(), ".csproj") {
			return nil
		}

		rel, relErr := filepath.Rel(c.go2csPath, path)

		if relErr != nil {
			return relErr
		}

		rel = filepath.ToSlash(rel)

		// golib is added explicitly by GenerateSolutionFile; skip any copy that happens to
		// live under core/golib so it is not listed twice.
		if rel == golibProjectReference || strings.HasPrefix(rel, "core/golib/") {
			return nil
		}

		// Per-package test projects (a Phase 4 artifact) are grouped separately. Nothing
		// emits them today, so this branch is inert until the convention lands.
		if strings.HasSuffix(d.Name(), "_test.csproj") {
			testProjects = append(testProjects, rel)
		} else {
			coreProjects = append(coreProjects, rel)
		}

		return nil
	})

	if walkErr != nil {
		return nil, nil, fmt.Errorf("failed to scan converted projects under \"%s\": %w", coreDir, walkErr)
	}

	return coreProjects, testProjects, nil
}

// buildSolutionXML renders the .slnx document. golib (the shared runtime) and go2cs-gen (the
// source generator / analyzer) are emitted as projects at the solution root with no folder;
// every converted package nests under a solution folder named by its full Go import path, so
// the folder tree mirrors `import "..."` (`bufio` → /bufio/, `crypto/aes` → /crypto/aes/).
// Projects are emitted in the order given (callers sort them for determinism), with
// solution-relative forward-slash paths that match the on-disk output layout. Output uses CRLF
// line endings and no BOM, matching the existing src/go2cs.slnx.
func buildSolutionXML(coreProjects []string, testProjects []string) string {
	var sb strings.Builder

	writeLine := func(indent int, text string) {
		sb.WriteString(strings.Repeat("  ", indent))
		sb.WriteString(text)
		sb.WriteString("\r\n")
	}

	writeProject := func(indent int, path string) {
		writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" />", escapeXMLAttr(path)))
	}

	// Folders carry an EXPLICIT deterministic Id: Visual Studio's .slnx loader derives a
	// missing folder Id from the folder's leaf display name, and Go namespaces guarantee
	// duplicate leaves (`/crypto/` vs `/vendor/golang.org/x/crypto/` are both "crypto") — VS
	// then refuses to open the solution ("a Solution Folder with the same unique identifier
	// already exists"). A UUID hashed from the FULL folder path is unique, stable across
	// regenerations, and machine-independent.
	writeFolderOpen := func(folder string) {
		writeLine(1, fmt.Sprintf("<Folder Name=\"%s\" Id=\"%s\">", escapeXMLAttr(folder), folderID(folder)))
	}

	writeLine(0, "<Solution>")

	writeLine(1, "<Configurations>")
	writeLine(2, "<Platform Name=\"Any CPU\" />")
	writeLine(2, "<Platform Name=\"x64\" />")
	writeLine(2, "<Platform Name=\"x86\" />")
	writeLine(1, "</Configurations>")

	// Infrastructure projects sit at the solution ROOT with no folder: golib (the shared
	// runtime) and go2cs-gen (the source generator / analyzer) are not Go packages, and the
	// folder tree is reserved exclusively for Go import paths — so a folder name can never
	// collide with a current or future stdlib package (`generators`, `core`, …). golib arrives
	// inside coreProjects (the caller appends it); it is emitted here and skipped in the
	// package grouping below.
	writeProject(1, genProjectReference)
	writeProject(1, golibProjectReference)

	// Every converted package nests under a solution folder named by its FULL Go import path,
	// so the folder tree mirrors `import "..."` exactly: `bufio` → /bufio/, `crypto/aes` →
	// /crypto/aes/, `archive/tar` → /archive/tar/. A package's own directory is core/<import
	// path>; stripping the leading "core/" yields the import path. Pure intermediate folders
	// (`archive/`, `container/`, and `crypto/` where only sub-packages exist) are implied by
	// their child folder's slash-path and need no explicit declaration — verified against the
	// dotnet/VS SolutionPersistence loader, which accepts leaf-only folder paths and builds the
	// ancestors. Folders still carry an explicit deterministic Id (see folderID) because leaf
	// display names repeat across the tree (`internal`, `testdata`, …). Projects arrive
	// globally sorted, which also sorts the folder groups; emitting groups in first-appearance
	// order keeps the document deterministic and diff-stable.
	importFolder := func(project string) string {
		own := path.Dir(project) // core/<import path> — the package's own directory
		return "/" + strings.TrimPrefix(own, "core/") + "/"
	}

	var folderOrder []string
	folderProjects := make(map[string][]string)

	for _, project := range coreProjects {
		if project == golibProjectReference {
			continue // emitted at the root above, not under an import-path folder
		}

		folder := importFolder(project)

		if _, exists := folderProjects[folder]; !exists {
			folderOrder = append(folderOrder, folder)
		}

		folderProjects[folder] = append(folderProjects[folder], project)
	}

	for _, folder := range folderOrder {
		writeFolderOpen(folder)

		for _, project := range folderProjects[folder] {
			writeProject(2, project)
		}

		writeLine(1, "</Folder>")
	}

	// Converted per-package test projects — rendered only once Phase 4 emits them, so an
	// empty /tests/ folder is never written.
	if len(testProjects) > 0 {
		writeFolderOpen("/tests/")
		for _, project := range testProjects {
			writeProject(2, project)
		}
		writeLine(1, "</Folder>")
	}

	writeLine(0, "</Solution>")

	return sb.String()
}

// solutionFolder groups solution-relative (forward-slash) csproj paths under one top-level .slnx
// solution folder. Name is the slash-wrapped display name (e.g. "/src/"); projects are its members,
// sorted by the caller for deterministic output.
type solutionFolder struct {
	name     string
	projects []string
}

// buildRecurseSolutionXML renders a recurse per-project solution (ModuleConverter): the anchor project +
// its converted dependencies + golib + the analyzer, tying them to the pre-converted stdlib (referenced
// via $(go2csPath)core) for one dotnet build. Projects are grouped into top-level solution folders that
// mirror the %GOPATH% layout — `src` for the project(s) being converted (the app's own main-module
// packages), `pkg` for their dependency packages, and `core` for the go2cs runtime/generator projects
// (golib, go2cs-gen). Folders are emitted in the EXACT order the caller passes them — the enforced
// src → pkg → core order, deliberately NOT alphabetic — and an empty folder is skipped (a dependency's own
// solution has no src package, for instance). These three names are unique leaves, so no folder Id is
// needed (unlike the namespace-nested stdlib solution). The project whose path equals startupProject is
// marked the Visual Studio default startup project via the .slnx DefaultStartup attribute (pass "" for
// none). Output uses CRLF line endings and no BOM, matching the other emitted solutions.
func buildRecurseSolutionXML(folders []solutionFolder, startupProject string) string {
	var sb strings.Builder

	writeLine := func(indent int, text string) {
		sb.WriteString(strings.Repeat("  ", indent))
		sb.WriteString(text)
		sb.WriteString("\r\n")
	}

	writeProject := func(indent int, project string) {
		if project == startupProject {
			writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" DefaultStartup=\"true\" />", escapeXMLAttr(project)))
		} else {
			writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" />", escapeXMLAttr(project)))
		}
	}

	writeLine(0, "<Solution>")

	writeLine(1, "<Configurations>")
	writeLine(2, "<Platform Name=\"Any CPU\" />")
	writeLine(2, "<Platform Name=\"x64\" />")
	writeLine(2, "<Platform Name=\"x86\" />")
	writeLine(1, "</Configurations>")

	for _, folder := range folders {
		if len(folder.projects) == 0 {
			continue // skip an empty folder — e.g. a dependency's own solution has no src package
		}

		writeLine(1, fmt.Sprintf("<Folder Name=\"%s\">", escapeXMLAttr(folder.name)))

		for _, project := range folder.projects {
			writeProject(2, project)
		}

		writeLine(1, "</Folder>")
	}

	writeLine(0, "</Solution>")

	return sb.String()
}

// folderID returns a deterministic UUID for a solution-folder path, hashed from the full
// path so same-named folders at different nesting levels (Go's ubiquitous `internal`,
// `crypto` under both core and vendor, ...) never collide in Visual Studio's identifier
// space. Version/variant bits are set so the value is a well-formed (v5-style) UUID.
func folderID(folderPath string) string {
	sum := sha1.Sum([]byte("go2cs-slnx-folder:" + folderPath))
	b := sum[:16]
	b[6] = (b[6] & 0x0f) | 0x50
	b[8] = (b[8] & 0x3f) | 0x80
	return fmt.Sprintf("%x-%x-%x-%x-%x", b[0:4], b[4:6], b[6:8], b[8:10], b[10:16])
}

// escapeXMLAttr escapes the minimal set of characters that would break an XML attribute.
// Converted project paths are ASCII with '/' and '.', but this keeps the emitter correct
// if a path ever carries an XML-special character.
func escapeXMLAttr(value string) string {
	value = strings.ReplaceAll(value, "&", "&amp;")
	value = strings.ReplaceAll(value, "<", "&lt;")
	value = strings.ReplaceAll(value, ">", "&gt;")
	value = strings.ReplaceAll(value, "\"", "&quot;")
	return value
}
