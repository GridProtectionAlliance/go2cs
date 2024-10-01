package main

import (
	"go/ast"
)

func (v *Visitor) visitTypeSwitchStmt(typeSwitchStmt *ast.TypeSwitchStmt) {
	v.writeOutputLn("/* visitTypeSwitchStmt: " + v.getPrintedNode(typeSwitchStmt) + " */")
}
