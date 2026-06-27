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

	// An IIFE keeps a block body (it may need a func() wrapper and reads more like the Go
	// source); other single-return literals collapse to an expression-bodied lambda.
	if v.firstStatementIsReturn && !context.isIIFE {
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
		// Immediately-invoked function literal: emit `paramNames => BODY` (names only — the
		// delegate-cast in convCallExpr supplies the types). BODY runs inside a
		// `func((defer, recover) => …)` execution context only when the literal itself uses
		// defer/recover, so it has its own scope without forcing one on every IIFE.
		sig := v.info.TypeOf(funcLit).(*types.Signature)
		hasDefer, hasRecover := v.funcBodyDeferRecover(funcLit.Body)
		result.WriteString(v.iifeParamNames(sig) + " => " + wrapIIFEFuncContext(body, hasDefer, hasRecover))
	} else {
		// A function literal that itself uses defer/recover needs its own func() execution
		// context so its deferred code runs (and recovers) when the literal is invoked — e.g. an
		// assigned closure `f := func(){ defer … }` or a goroutine body. A deferred-call target is
		// the exception: its recover() belongs to the enclosing function, which is already wrapped.
		inner := body

		if !context.deferCall {
			hasDefer, hasRecover := v.funcBodyDeferRecover(funcLit.Body)
			inner = wrapIIFEFuncContext(body, hasDefer, hasRecover)
		}

		result.WriteString("(" + parameterSignature + ") => " + inner)
	}

	return result.String()
}
