package main

import (
	"go/ast"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt) {
	v.writeOutputLn("/* visitSwitchStmt: " + v.getPrintedNode(switchStmt) + " */")
}
