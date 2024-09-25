package main

import (
	"go/ast"
)

func (v *Visitor) visitIncDecStmt(incDecStmt *ast.IncDecStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(incDecStmt) + " */")
}
