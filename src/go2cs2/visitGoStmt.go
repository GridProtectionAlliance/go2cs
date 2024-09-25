package main

import (
	"go/ast"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(goStmt) + " */")
}
