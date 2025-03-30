package main

import (
	"fmt"
	"go/ast"
	"strings"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	// Analyze captures specifically for this go statement
	v.enterLambdaConversion(goStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()
	paramCount := len(goStmt.Call.Args)

	var renderLambdaParams bool

	// If we have a function literal, only prepare captures there, not on the GoStmt
	if funcLit, ok := goStmt.Call.Fun.(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[goStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, goStmt)
			lambdaContext.deferredDecls = &strings.Builder{}

		}
	} else {
		v.prepareStmtCaptures(goStmt)

		// If call expression parameter counts match those of target function signature,
		// we can render call without lambda parameters
		renderLambdaParams = paramCount != v.getFunctionParamCount(goStmt.Call.Fun)
	}

	if paramCount > 0 {
		lambdaContext.callArgs = make([]string, paramCount)
		lambdaContext.renderParams = renderLambdaParams
	}

	result := strings.Builder{}
	wroteDecls := false

	callExpr := strings.TrimSpace(v.convCallExpr(goStmt.Call, lambdaContext))

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

	// The following is basically "go!". We use the unicode bang-type character
	// as a valid C# identifier symbol, where the standard "!" is not. This is
	// to disambiguate the method name from the namespace when calling function.
	result.WriteString("go\u01C3(")

	if paramCount == 0 {
		// C# `go` method implementation expects an Action (or WaitCallback delegate)
		if strings.HasSuffix(callExpr, "()") {
			callExpr = strings.TrimSuffix(callExpr, "()") // Action delegate
		} else {
			callExpr = "_ => " + callExpr // WaitCallback delegate
		}

		result.WriteString(callExpr)
	} else {
		if renderLambdaParams {
			if paramCount > 1 {
				result.WriteRune('(')
			}

			for i := range paramCount {
				if i > 0 {
					result.WriteString(", ")
				}

				result.WriteString(fmt.Sprintf("%s%d", TempVarMarker, i+1))
			}

			if paramCount > 1 {
				result.WriteRune(')')
			}

			result.WriteString(" => ")
		}

		result.WriteString(callExpr)
		result.WriteString(", ")
		result.WriteString(strings.Join(lambdaContext.callArgs, ", "))
	}

	result.WriteString(");")

	v.targetFile.WriteString(result.String())
}
