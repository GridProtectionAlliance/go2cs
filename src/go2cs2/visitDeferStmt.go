package main

import (
	"go/ast"
)

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(deferStmt) + " */")
}
