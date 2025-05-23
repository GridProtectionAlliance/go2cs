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
	v.loadImportedTypeAliases(v.currentImportPath)

	importPath := convertImportPathToNamespace(v.currentImportPath, PackageSuffix)

	v.writeDocString(v.packageImports, doc, importSpec.Pos())

	if importSpec.Name != nil {
		alias := importSpec.Name.Name

		if alias == "." {
			v.packageImports.WriteString(fmt.Sprintf("using static %s;", importPath))
		} else {
			v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", getSanitizedImport(alias), importPath))
		}
	} else {
		// Get package name from the import path, last name after last "."
		importName := importPath
		lastDotIndex := strings.LastIndex(importPath, ".")

		if lastDotIndex != -1 {
			importName = importPath[lastDotIndex+1:]

			namespace := importPath[:lastDotIndex]

			if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
				v.requiredUsings.Add(namespace)
			}
		}

		v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", getSanitizedImport(strings.TrimSuffix(importName, PackageSuffix)), importPath))
	}

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}

func convertImportPathToNamespace(importPath string, packageSuffix string) string {
	// Split import path by "/"
	importPathParts := strings.Split(importPath, "/")

	// Update all import path parts to sanitized identifiers
	for i, part := range importPathParts {
		if i == len(importPathParts)-1 {
			part = part + packageSuffix
		}

		importPathParts[i] = getSanitizedImport(part)
	}

	return strings.Join(importPathParts, ".")
}
