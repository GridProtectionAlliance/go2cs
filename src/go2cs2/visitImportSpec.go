package main

import (
	"fmt"
	"go/ast"
	"log"
	"strings"
)

func (v *Visitor) visitImportSpec(importSpec *ast.ImportSpec, doc *ast.CommentGroup) {
	v.currentImportPath = strings.Trim(importSpec.Path.Value, "\"")

	if !v.options.parseCgoTargets && v.currentImportPath == "C" {
		log.Fatalf("cgo target parsing is not supported: file \"%s\"", v.fset.Position(importSpec.Pos()).Filename)
	}

	v.importQueue.Add(v.currentImportPath)

	importPath := strings.ReplaceAll(v.currentImportPath, "/", ".")

	v.writeDocString(v.packageImports, doc, importSpec.Pos())

	// TODO: Update to handle alias / static options
	csUsing := fmt.Sprintf("using %s = %s%s;", importPath, importPath, ClassSuffix)

	v.packageImports.WriteString(csUsing)

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}
