package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	v.targetFile.WriteString(v.newline)

	// The following is basically "go!". We use the unicode bang-type character
	// as a valid C# identifier symbol, where the standard "!" is not. This is
	// to disambiguate the method name from the namespace when calling function.
	v.writeOutput("go\u01C3(")

	callExpr := v.convCallExpr(goStmt.Call)

	// C# go implementation expects an Action or WaitCallback delegate
	if strings.HasSuffix(callExpr, "()") {
		// Call Action delegate overload
		callExpr = strings.TrimSuffix(callExpr, "()")
	} else {
		// Call WaitCallback delegate overload
		callExpr = "_ => " + callExpr
	}

	v.targetFile.WriteString(callExpr)
	v.targetFile.WriteString(");")
}
