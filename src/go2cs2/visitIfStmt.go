package main

import (
	"go/ast"
)

func (v *Visitor) visitIfStmt(ifStmt *ast.IfStmt) {
	v.writeOutputLn("/* visitIfStmt: " + v.getPrintedNode(ifStmt) + " */")
}
