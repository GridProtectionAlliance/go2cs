package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	v.targetFile.WriteString(v.newline)
	v.writeOutput("defer(")

	// TODO: Setup to match GoStmt operations
	callExpr := v.convCallExpr(deferStmt.Call, DefaultLambdaContext())

	// C# defer implementation expects an Action delegate
	if strings.HasSuffix(callExpr, "()") {
		callExpr = strings.TrimSuffix(callExpr, "()")
	} else {
		callExpr = "() => " + callExpr
	}

	v.targetFile.WriteString(callExpr)
	v.targetFile.WriteString(");")
}
