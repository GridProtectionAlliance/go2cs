package main

import (
	"go/ast"
)

func (v *Visitor) visitBranchStmt(branchStmt *ast.BranchStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(branchStmt) + " */")
}
