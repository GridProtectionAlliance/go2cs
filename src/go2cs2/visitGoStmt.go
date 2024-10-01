package main

import (
	"go/ast"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	v.writeOutputLn("/* visitGoStmt: " + v.getPrintedNode(goStmt) + " */")
}
