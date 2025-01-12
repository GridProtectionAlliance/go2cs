package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitGenDecl(genDecl *ast.GenDecl, parentBlock *ast.BlockStmt) {
	for _, spec := range genDecl.Specs {
		switch specType := spec.(type) {
		case *ast.ImportSpec:
			v.visitImportSpec(specType, genDecl.Doc)
		case *ast.ValueSpec:
			v.visitValueSpec(specType, genDecl.Tok, parentBlock)
		case *ast.TypeSpec:
			v.visitTypeSpec(specType, genDecl.Doc)
		default:
			panic(fmt.Sprintf("unexpected GenDecl Spec: %#v", specType))
		}
	}
}
