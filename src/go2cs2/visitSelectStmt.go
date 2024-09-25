package main

import (
	"go/ast"
)

func (v *Visitor) visitSelectStmt(selectStmt *ast.SelectStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(selectStmt) + " */")
}
