package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	// Analyze captures specifically for this defer statement
	v.enterLambdaConversion(deferStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()

	// If we have a function literal, only prepare captures there, not on the DeferStmt
	if funcLit, ok := deferStmt.Call.Fun.(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[deferStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, deferStmt)
			lambdaContext.deferredDecls = &strings.Builder{}

		}
	} else {
		v.prepareStmtCaptures(deferStmt)
	}

	result := strings.Builder{}
	wroteDecls := false

	callExpr := strings.TrimSpace(v.convCallExpr(deferStmt.Call, lambdaContext))

	if lambdaContext.deferredDecls != nil && lambdaContext.deferredDecls.Len() > 0 {
		result.WriteString(lambdaContext.deferredDecls.String())
		result.WriteString(v.indent(v.indentLevel))
		wroteDecls = true
	}

	if decls := v.generateCaptureDeclarations(); strings.TrimSpace(decls) != "" {
		result.WriteString(decls)
		wroteDecls = true
	}

	if !wroteDecls {
		result.WriteString(v.newline)
		result.WriteString(v.indent(v.indentLevel))
	}

	result.WriteString("defer(")

	// C# `defer` method implementation expects an Action delegate
	if strings.HasSuffix(callExpr, "()") {
		callExpr = strings.TrimSuffix(callExpr, "()")
	} else {
		callExpr = "() => " + callExpr
	}

	result.WriteString(callExpr)
	result.WriteString(");")

	v.targetFile.WriteString(result.String())
}
