package main

import (
	"go/ast"
)

func (v *Visitor) visitDeclStmt(declStmt *ast.DeclStmt) {
	v.writeOutputLn("/* visitDeclStmt: " + v.getPrintedNode(declStmt) + " */")
}
