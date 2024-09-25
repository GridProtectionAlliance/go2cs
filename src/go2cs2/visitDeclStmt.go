package main

import (
	"go/ast"
)

func (v *Visitor) visitDeclStmt(declStmt *ast.DeclStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(declStmt) + " */")
}
