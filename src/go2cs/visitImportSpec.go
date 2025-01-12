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

	if importSpec.Name != nil {
		alias := importSpec.Name.Name

		if alias == "." {
			v.packageImports.WriteString(fmt.Sprintf("using static %s%s;", importPath, PackageSuffix))
		} else {
			v.packageImports.WriteString(fmt.Sprintf("using %s = %s%s;", alias, importPath, PackageSuffix))
		}
	} else {
		// Get package name from the import path, last name after last "."
		importName := importPath
		lastDotIndex := strings.LastIndex(importPath, ".")

		if lastDotIndex != -1 {
			importName = importPath[lastDotIndex+1:]
		}

		v.packageImports.WriteString(fmt.Sprintf("using %s = %s%s;", importName, importPath, PackageSuffix))
	}

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}
