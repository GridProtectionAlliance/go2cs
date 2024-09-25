package main

import (
	"go/ast"
)

func (v *Visitor) visitSendStmt(sendStmt *ast.SendStmt) {
	v.writeOutputLn("/* " + v.getPrintedNode(sendStmt) + " */")
}
