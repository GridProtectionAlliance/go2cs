// This file emits a Visual Studio .slnx solution for a whole-standard-library (-stdlib)
// conversion, so the freshly converted stdlib is openable / buildable as a single unit
// right after a run instead of relying on a hand-maintained solution that drifts.
package main

import (
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

const (
	// generatedSolutionFileName is the .slnx solution emitted at the output root
	// (-go2cspath) after a -stdlib conversion. It is the auto-generated counterpart to the
	// hand-maintained src/go-src-converted.sln.
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
	// converter's output), so add it explicitly and let it sort into its natural position
	// among the converted core packages — matching the layout of the existing src/go2cs.slnx.
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

// buildSolutionXML renders the .slnx document. Projects are emitted in the order given
// (callers sort them for determinism), with solution-relative forward-slash paths that
// match the on-disk output layout and the existing src/go2cs.slnx. Output uses CRLF line
// endings and no BOM, matching that file.
func buildSolutionXML(coreProjects []string, testProjects []string) string {
	var sb strings.Builder

	writeLine := func(indent int, text string) {
		sb.WriteString(strings.Repeat("  ", indent))
		sb.WriteString(text)
		sb.WriteString("\r\n")
	}

	writeProject := func(path string) {
		writeLine(2, fmt.Sprintf("<Project Path=\"%s\" />", escapeXMLAttr(path)))
	}

	writeLine(0, "<Solution>")

	writeLine(1, "<Configurations>")
	writeLine(2, "<Platform Name=\"Any CPU\" />")
	writeLine(2, "<Platform Name=\"x64\" />")
	writeLine(2, "<Platform Name=\"x86\" />")
	writeLine(1, "</Configurations>")

	// Source generators / analyzers.
	writeLine(1, "<Folder Name=\"/generators/\">")
	writeProject(genProjectReference)
	writeLine(1, "</Folder>")

	// Shared runtime + converted stdlib packages.
	writeLine(1, "<Folder Name=\"/core/\">")
	for _, project := range coreProjects {
		writeProject(project)
	}
	writeLine(1, "</Folder>")

	// Converted per-package test projects — rendered only once Phase 4 emits them, so an
	// empty /tests/ folder is never written.
	if len(testProjects) > 0 {
		writeLine(1, "<Folder Name=\"/tests/\">")
		for _, project := range testProjects {
			writeProject(project)
		}
		writeLine(1, "</Folder>")
	}

	writeLine(0, "</Solution>")

	return sb.String()
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
