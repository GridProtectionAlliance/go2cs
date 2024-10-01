package main

import (
	"go/ast"
)

func (v *Visitor) visitSendStmt(sendStmt *ast.SendStmt) {
	v.writeOutputLn("/* visitSendStmt: " + v.getPrintedNode(sendStmt) + " */")
}
