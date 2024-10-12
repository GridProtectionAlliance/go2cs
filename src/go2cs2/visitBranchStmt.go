package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitBranchStmt(branchStmt *ast.BranchStmt) {
	if branchStmt.Tok != token.FALLTHROUGH {
		v.writeOutputLn("/* visitBranchStmt: " + v.getPrintedNode(branchStmt) + " */")
	}
}
