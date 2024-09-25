package main

import (
	"go/ast"
)

func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(switchStmt) + " */")
}
