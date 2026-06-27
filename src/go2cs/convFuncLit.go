package main

import (
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	if v.currentFuncSignature == nil {
		v.currentFuncSignature = v.info.Types[funcLit].Type.(*types.Signature)
	}

	// A function literal is emitted as its own C# lambda with normal return handling. The
	// enclosing function's namedReturnDeferMode (which rewrites `return e` into a named-result
	// assignment) must not leak into it — otherwise the closure's `return expr` would be
	// rewritten against the OUTER function's named results. Suppress it for the literal's body
	// and restore it afterward.
	savedNamedReturnDeferMode := v.namedReturnDeferMode
	savedNamedReturnNames := v.namedReturnNames
	v.namedReturnDeferMode = false
	v.namedReturnNames = nil

	defer func() {
		v.namedReturnDeferMode = savedNamedReturnDeferMode
		v.namedReturnNames = savedNamedReturnNames
	}()

	if v.lambdaCapture == nil {
		v.lambdaCapture = newLambdaCapture()
		v.capturedVarCount = make(map[string]int)
	}

	v.enterLambdaConversion(funcLit)
	defer v.exitLambdaConversion()

	// Create a map of parameters to avoid capturing them
	paramNames := make(map[string]bool)

	if funcLit.Type.Params != nil {
		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// Filter out any captures that are actually parameters
	if captures, exists := v.lambdaCapture.stmtCaptures[funcLit]; exists {
		for ident := range captures {
			if paramNames[ident.Name] {
				delete(captures, ident)
			}
		}

		// If no captures remain, remove the empty map
		if len(captures) == 0 {
			delete(v.lambdaCapture.stmtCaptures, funcLit)
		}
	}

	v.prepareStmtCaptures(funcLit)

	result := strings.Builder{}

	if decls := v.generateCaptureDeclarations(); decls != "" {
		if context.deferredDecls == nil {
			result.WriteString(decls)
		} else {
			context.deferredDecls.WriteString(strings.TrimRight(decls, " "))
		}
	}

	var parameterSignature string

	// For C#, lambda return type is inferred and not explicitly declared
	_, parameterSignature = v.convFuncType(funcLit.Type)

	blockStatementContext := DefaultBlockStmtContext()
	blockStatementContext.format.useNewLine = false

	v.pushBlock()
	v.visitBlockStmt(funcLit.Body, blockStatementContext)
	body := v.popBlockAppend(false)

	if v.firstStatementIsReturn {
		// Find return statement in string and remove it
		returnIndex := strings.Index(body, "return ")

		if returnIndex != -1 {
			body = body[returnIndex+7:]
			body = strings.TrimSuffix(body, "}")

			// Remove trailing brace
			braceIndex := strings.LastIndex(body, "}")

			if braceIndex != -1 {
				body = body[:braceIndex]
			}

			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, ";")
		}
	} else {
		body = strings.TrimSpace(body)
	}

	if context.isIIFE {
		// Immediately-invoked, no-arg function literal: emit the func() execution context so it
		// runs immediately with its own defer/recover scope (the call's `()` is omitted by
		// convCallExpr). The body's `defer(…)`/`recover()` bind to these parameters, not the
		// enclosing function's wrapper.
		result.WriteString("func((defer, recover) => " + body + ")")
	} else {
		result.WriteString("(" + parameterSignature + ") => " + body)
	}

	return result.String()
}
