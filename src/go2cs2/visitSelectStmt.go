package main

import (
	"go/ast"
)

func (v *Visitor) visitSelectStmt(selectStmt *ast.SelectStmt) {
	v.writeOutputLn("/* visitSelectStmt: " + v.getPrintedNode(selectStmt) + " */")
}
