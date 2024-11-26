package main

import (
	"go/ast"
)

func (v *Visitor) visitDeclStmt(declStmt *ast.DeclStmt, source ParentBlockContext) {
	parentBlock := source.parentBlock

	switch decl := declStmt.Decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(decl, parentBlock)
	default:
		panic("@visitDeclStmt - Unexpected `ast.DeclStmt` type: " + v.getPrintedNode(decl))
	}
}
