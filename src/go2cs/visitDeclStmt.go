package main

import (
	"go/ast"
)

func (v *Visitor) visitDeclStmt(declStmt *ast.DeclStmt) {
	switch decl := declStmt.Decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(decl)
	default:
		panic("@visitDeclStmt - Unexpected `ast.DeclStmt` type: " + v.getPrintedNode(decl))
	}
}
