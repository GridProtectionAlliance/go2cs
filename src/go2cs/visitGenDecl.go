package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitGenDecl(genDecl *ast.GenDecl) {
	var hasValueSpec bool

	for _, spec := range genDecl.Specs {
		switch specType := spec.(type) {
		case *ast.ImportSpec:
			v.visitImportSpec(specType, genDecl.Doc)
		case *ast.ValueSpec:
			v.visitValueSpec(specType, genDecl.Doc, genDecl.Tok)
			hasValueSpec = true
		case *ast.TypeSpec:
			v.visitTypeSpec(specType, genDecl.Doc)
		default:
			panic(fmt.Sprintf("unexpected GenDecl Spec: %#v", specType))
		}
	}

	if !v.inFunction && hasValueSpec {
		v.targetFile.WriteString(v.newline)
	}
}
