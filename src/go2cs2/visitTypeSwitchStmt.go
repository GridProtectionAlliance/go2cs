package main

import (
	"go/ast"
)

func (v *Visitor) visitTypeSwitchStmt(typeSwitchStmt *ast.TypeSwitchStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(typeSwitchStmt) + " */")
}
