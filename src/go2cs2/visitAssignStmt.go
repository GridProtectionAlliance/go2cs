package main

import (
	"go/ast"
)

// Handles assignment, i.e., short variable declaration, statements, e.g.:
// x := 5
func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(assignStmt) + " */")
}
