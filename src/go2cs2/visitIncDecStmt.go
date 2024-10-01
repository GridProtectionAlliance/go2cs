package main

import (
	"go/ast"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt) {
	v.writeOutputLn("/* visitIncDecStmt: " + v.getPrintedNode(incDecStmt) + " */")
}
