package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitBranchStmt(branchStmt *ast.BranchStmt) {
	v.writeOutputLn("/* visitBranchStmt: " + v.getPrintedNode(branchStmt) + " */")

	if branchStmt.Tok == token.FALLTHROUGH {
		v.caseFallthrough = true
	}
}
