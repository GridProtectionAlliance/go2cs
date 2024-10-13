package main

import (
	"go/ast"
)

func (v *Visitor) visitSendStmt(sendStmt *ast.SendStmt, format FormattingContext) {
	v.writeOutputLn("/* visitSendStmt: " + v.getPrintedNode(sendStmt) + " */")
}
