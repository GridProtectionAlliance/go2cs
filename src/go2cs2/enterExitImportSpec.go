package main

import (
	"fmt"
	"go/ast"
	"log"
	"strings"
)

func (v *Visitor) enterImportSpec(x *ast.ImportSpec, doc *ast.CommentGroup) {
	v.currentImportPath = strings.Trim(x.Path.Value, "\"")

	if !v.options.parseCgoTargets && v.currentImportPath == "C" {
		log.Fatalf("cgo target parsing is not supported: file \"%s\"", v.fset.Position(x.Pos()).Filename)
	}

	v.importQueue.Add(v.currentImportPath)

	importPath := strings.ReplaceAll(v.currentImportPath, "/", ".")

	v.writeDocString(v.packageImports, doc, x.Pos())

	// TODO: Update to handle alias / static options
	csUsing := fmt.Sprintf("using %s = %s%s;", importPath, importPath, ClassSuffix)

	v.packageImports.WriteString(csUsing)

	v.writeCommentString(v.packageImports, x.Comment, x.End())
	v.packageImports.WriteString(v.newline)
}

func (v *Visitor) exitImportSpec(x *ast.ImportSpec) {
}
