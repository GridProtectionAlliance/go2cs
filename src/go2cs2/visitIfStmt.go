package main

import (
	"go/ast"
)

func (v *Visitor) visitIfStmt(ifStmt *ast.IfStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(ifStmt) + " */")
}
