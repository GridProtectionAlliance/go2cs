package main

import (
	"go/ast"
)

func (v *Visitor) visitForStmt(forStmt *ast.ForStmt) {
	v.writeOutputLn("/* visitForStmt: " + v.getPrintedNode(forStmt) + " */")
}
