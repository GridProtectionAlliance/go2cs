package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	// Analyze captures specifically for this go statement
	v.enterLambdaConversion(goStmt)
	defer v.exitLambdaConversion()

	// Prepare captures specific to this go statement
	v.prepareStmtCaptures(goStmt)

	result := strings.Builder{}

	if decls := v.generateCaptureDeclarations(); decls != "" {
		result.WriteString(decls)
	} else {
		result.WriteString(v.newline)
		result.WriteString(v.indent(v.indentLevel))
	}

	// The following is basically "go!". We use the unicode bang-type character
	// as a valid C# identifier symbol, where the standard "!" is not. This is
	// to disambiguate the method name from the namespace when calling function.
	result.WriteString("go\u01C3(")

	callExpr := strings.TrimSpace(v.convCallExpr(goStmt.Call))

	// C# `go` method implementation expects an Action or WaitCallback delegate
	if strings.HasSuffix(callExpr, "()") {
		// Call Action delegate overload
		callExpr = strings.TrimSuffix(callExpr, "()")
	} else {
		// Call WaitCallback delegate overload
		callExpr = "_ => " + callExpr
	}

	result.WriteString(callExpr)
	result.WriteString(");")

	v.targetFile.WriteString(result.String())
}
