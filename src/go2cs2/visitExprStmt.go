package main

import (
	"go/ast"
)

func (v *Visitor) visitExprStmt(exprStmt *ast.ExprStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(exprStmt) + " */")
}
