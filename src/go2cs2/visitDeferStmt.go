package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	v.targetFile.WriteString(v.newline)
	v.writeOutput("defer(")

	callExpr := v.convCallExpr(deferStmt.Call)

	// C# defer implementation expects a delegate
	callExpr = strings.TrimSuffix(callExpr, "()")

	v.targetFile.WriteString(callExpr)
	v.targetFile.WriteString(");")
}
