package main

import (
	"fmt"
	"go/ast"
	"go/doc/comment"
	"os"
	"path/filepath"
	"strings"
	"sync"
)

// extractPackageDoc returns the package-level Go doc comment (the comment group attached to the
// `package` clause) rendered to GitHub-flavored Markdown, for use as a NuGet package README.
//
// It reads ast.File.Doc directly — a pure read — rather than go/doc.NewFromFiles, which takes
// ownership of and may mutate the AST that the converter subsequently visits. The per-file BSD
// license header is a separate comment group (blank-line-separated from the package clause), so it
// is naturally excluded; only the package documentation is returned.
func extractPackageDoc(files []*ast.File) string {
	var docs []string

	for _, file := range files {
		if file.Doc == nil {
			continue
		}

		// CommentGroup.Text() strips the // or /* */ markers and cleans the text.
		if text := strings.TrimSpace(file.Doc.Text()); text != "" {
			docs = append(docs, text)
		}
	}

	if len(docs) == 0 {
		return ""
	}

	// Parse the godoc markup (headings, code blocks, lists, doc links) and render it to Markdown.
	var parser comment.Parser
	var printer comment.Printer

	// Suppress the "{#hdr-...}" heading-anchor suffix — NuGet's Markdown renderer shows it literally.
	printer.HeadingID = func(*comment.Heading) string { return "" }

	return strings.TrimSpace(string(printer.Markdown(parser.Parse(strings.Join(docs, "\n\n")))))
}

var goVersionOnce sync.Once
var goVersionValue string

// goVersion returns the active Go toolchain version without the "go" prefix (e.g. "1.23.1"),
// resolved once from `go env GOVERSION`. Returns "" if it cannot be determined.
func goVersion() string {
	goVersionOnce.Do(func() {
		if value, err := getGoEnv("GOVERSION"); err == nil {
			goVersionValue = strings.TrimPrefix(strings.TrimSpace(value), "go")
		}
	})

	return goVersionValue
}

// writeReadmeFile emits a README.md into a converted library package directory, wrapping the
// package's Go doc (already rendered to Markdown) so the NuGet package carries readable docs. It is
// idempotent via needToWriteFile, mirroring how the icon and .csproj files are written, and uses
// CRLF line endings to match the converter's other generated text output (and avoid autocrlf churn).
func writeReadmeFile(projectPath string, projectName string, packageDoc string) error {
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)
	readmeFileName := projectPath + "README.md"

	var builder strings.Builder

	builder.WriteString(fmt.Sprintf("# go.%s\n\n", projectName))
	builder.WriteString("> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).\n")

	if version := goVersion(); version != "" {
		builder.WriteString(fmt.Sprintf("> Go version: %s\n", version))
	}

	builder.WriteString("\n")

	if trimmed := strings.TrimSpace(packageDoc); trimmed != "" {
		builder.WriteString(trimmed)
		builder.WriteString("\n\n")
	}

	builder.WriteString("---\n")
	builder.WriteString("Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.\n\n")
	builder.WriteString("Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.\n")

	contents := []byte(strings.ReplaceAll(builder.String(), "\n", "\r\n"))

	if needToWriteFile(readmeFileName, contents) {
		if err := os.WriteFile(readmeFileName, contents, 0644); err != nil {
			return fmt.Errorf("failed to write README file \"%s\": %s", readmeFileName, err)
		}
	}

	return nil
}
