package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	// Analyze captures specifically for this defer statement
	v.enterLambdaConversion(deferStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()
	paramCount := len(deferStmt.Call.Args)

	var renderLambdaParams bool

	// If we have a function literal, only prepare captures there, not on the DeferStmt
	if funcLit, ok := deferStmt.Call.Fun.(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[deferStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, deferStmt)
			lambdaContext.deferredDecls = &strings.Builder{}

		}

		renderLambdaParams = false
	} else {
		v.prepareStmtCaptures(deferStmt)

		// If call expression parameter counts match those of target function signature,
		// we can render call without lambda parameters
		renderLambdaParams = paramCount != v.getFunctionParamCount(deferStmt.Call.Fun)
	}

	if paramCount > 0 {
		lambdaContext.callArgs = make([]string, paramCount)
		lambdaContext.renderParams = renderLambdaParams
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

	if paramCount == 0 {
		result.WriteString("defer(")

		// C# `defer` method implementation expects an Action delegate
		if strings.HasSuffix(callExpr, "()") {
			callExpr = strings.TrimSuffix(callExpr, "()")
		} else {
			callExpr = "() => " + callExpr
		}

		result.WriteString(callExpr)
		result.WriteString(");")
	} else {
		result.WriteString("defer\u01C3(")

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
		result.WriteString(", defer);")
	}

	v.targetFile.WriteString(result.String())
}

func (v *Visitor) getFunctionParamCount(expr ast.Expr) int {
	tv, ok := v.info.Types[expr]

	if !ok {
		return 0
	}

	sig, ok := tv.Type.Underlying().(*types.Signature)

	if !ok {
		return 0
	}

	// Do not compare parameter counts when variadic parameters exist
	if sig.Variadic() {
		return -1
	}

	return sig.Params().Len()
}
