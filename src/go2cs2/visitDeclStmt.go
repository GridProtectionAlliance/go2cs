package main

import (
	"go/ast"
)

func (v *Visitor) visitDeclStmt(declStmt *ast.DeclStmt, parentBlock *ast.BlockStmt) {
	//v.writeOutputLn("/* visitDeclStmt: " + v.getPrintedNode(declStmt) + " */")

	switch decl := declStmt.Decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(decl, parentBlock)
	default:
		panic("Unexpected Decl type: " + v.getPrintedNode(decl))
	}
}
