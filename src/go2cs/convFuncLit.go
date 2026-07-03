package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	if v.currentFuncSignature == nil {
		v.currentFuncSignature = v.info.Types[funcLit].Type.(*types.Signature)
	}

	litSig, _ := v.info.TypeOf(funcLit).(*types.Signature)

	// visitReturnStmt derives a bare `return`'s emitted RESULTS from the return signature. A nested
	// literal must return against ITS OWN results, not the enclosing function's — a bare `return` inside
	// a VOID closure otherwise gets the OUTER function's named results (`forEachGRace(func(gp1 *g) { …
	// return … })` inside a func returning named `(n, ok)` emitted `return (n, ok);` → CS8030). This is a
	// SEPARATE field from currentFuncSignature, which must stay the ENCLOSING function's signature so the
	// receiver/parameter detection (isBoxedPointerLocal, varIsDerefdPointerParam) still resolves a
	// CAPTURED pointer param (an outer parameter) correctly. Save/restore around the body.
	savedReturnSignature := v.currentReturnSignature

	if litSig != nil {
		v.currentReturnSignature = litSig
	}

	// Does THIS literal need its own func() execution context, and additionally the
	// named-return-defer handling (named results that deferred code, including recover, mutates)?
	// A deferred-call target is excluded — its defer/recover belong to the enclosing function.
	var litHasDefer, litHasRecover, litNamedDefer bool
	var litNamedNames []string

	if litSig != nil && !context.deferCall {
		litHasDefer, litHasRecover = v.funcBodyDeferRecover(funcLit.Body)
		litNamedDefer, litNamedNames = v.detectNamedReturnDefer(litSig, litHasDefer, litHasRecover)
	}

	// A function literal with NAMED results needs their declarations at the top of its block —
	// Go zero-initializes them and a bare `return` returns them. iter.Pull's
	// `next = func() (v1 V, ok1 bool) { …; return }` emitted `return (v1, ok1);` with nothing
	// declared (CS0103 — the wave-1 iter errors). The litNamedDefer path composes its own decls;
	// this covers plain and defer-without-mutation literals.
	litHasNamedResults := false

	if litSig != nil && funcLit.Type.Results != nil {
		for _, field := range funcLit.Type.Results.List {
			for _, name := range field.Names {
				if !isDiscardedVar(name.Name) {
					litHasNamedResults = true
				}
			}
		}
	}

	// A function literal's body is converted with ITS OWN namedReturnDeferMode, not the enclosing
	// function's (which must not leak in — otherwise the closure's `return expr` would be rewritten
	// against the OUTER named results). Save/restore around the body conversion.
	savedNamedReturnDeferMode := v.namedReturnDeferMode
	savedNamedReturnNames := v.namedReturnNames
	v.namedReturnDeferMode = litNamedDefer
	v.namedReturnNames = litNamedNames

	// A func literal BODY is function scope even when the literal sits in a PACKAGE-LEVEL var
	// initializer (`var Support = sync.OnceValue(func() bool { var size uint32; … })` —
	// internal/syscall/windows): visitFuncDecl never ran, so inFunction was false and the
	// literal's locals emitted as package fields (`internal static uint32 size;` inside the
	// lambda — a CS1002 syntax cascade gating os/fmt). Save/restore around the body.
	savedInFunction := v.inFunction
	v.inFunction = true

	defer func() {
		v.namedReturnDeferMode = savedNamedReturnDeferMode
		v.namedReturnNames = savedNamedReturnNames
		v.currentReturnSignature = savedReturnSignature
		v.inFunction = savedInFunction
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
		switch {
		case context.deferredDecls != nil:
			// go/defer/return thread an explicit builder for their own hoisting.
			context.deferredDecls.WriteString(strings.TrimRight(decls, " "))
		case v.hoistedDecls != nil:
			// The enclosing statement (assignment RHS, composite-literal element, call argument)
			// hoists these decls to a valid position before the statement.
			v.hoistedDecls.WriteString(strings.TrimRight(decls, " "))
		default:
			result.WriteString(decls)
		}
	}

	var parameterSignature string

	// For C#, lambda return type is inferred and not explicitly declared
	_, parameterSignature = v.convFuncType(funcLit.Type)

	blockStatementContext := DefaultBlockStmtContext()
	blockStatementContext.format.useNewLine = false

	// In namedReturnDefer mode the literal's body sits inside an extra block + func() wrapper, so
	// indent it one level deeper.
	if litNamedDefer {
		v.indentLevel++
	}

	v.pushBlock()
	v.visitBlockStmt(funcLit.Body, blockStatementContext)
	body := v.popBlockAppend(false)

	if litNamedDefer {
		v.indentLevel--
	}

	// An IIFE keeps a block body (it may need a func() wrapper and reads more like the Go
	// source); other single-return literals collapse to an expression-bodied lambda. A
	// namedReturnDefer literal always keeps a block (it returns its named results after the
	// func() wrapper).
	if v.firstStatementIsReturn && !context.isIIFE && !litNamedDefer && !litHasNamedResults {
		// Find return statement in string and remove it
		returnIndex := strings.Index(body, "return ")

		if returnIndex != -1 {
			body = body[returnIndex+7:]

			// Remove the BLOCK's closing brace — always the last non-whitespace rune of the
			// visited block; the statement's `;` always separates it from the expression. The
			// old TrimSuffix+LastIndex pair cut at the last `}` ANYWHERE, truncating a return
			// expression containing its own `}` — `return []Value{ValueOf(yield(in[0]))}`
			// emitted `new ΔValue[]{ValueOf(yield(@in[0]))` with `}.slice()` chopped
			// (reflect/iter.go MakeFunc literals, CS1513 x2).
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, "}")
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, ";")
		}
	} else {
		body = strings.TrimSpace(body)
	}

	// Declare plain named results at the top of the literal's block (the litNamedDefer arm
	// below declares its own, outside the func() wrapper).
	if litHasNamedResults && !litNamedDefer && strings.HasPrefix(body, "{") {
		body = "{" + v.namedReturnDeclLines(litSig, v.indentLevel+1) + strings.TrimPrefix(body, "{")
	}

	// Build the lambda body (what follows `=>`). A function literal that uses defer/recover gets
	// its own `func((defer, recover) => …)` execution context (so its deferred code runs and
	// recovers when invoked); when it also has named results that deferred code mutates, the
	// named results are declared outside that wrapper and returned after it. A deferred-call
	// target is the exception (its defer/recover belong to the already-wrapped enclosing function).
	var inner string

	switch {
	case litNamedDefer:
		// `{ T r = default!; func((defer, recover) => <body>); return r; }`
		returnExpr := strings.Join(litNamedNames, ", ")

		if len(litNamedNames) > 1 {
			returnExpr = "(" + returnExpr + ")"
		}

		inner = fmt.Sprintf("{%s%s%sfunc((defer, recover) => %s);%s%sreturn %s;%s%s}",
			v.namedReturnDeclLines(litSig, v.indentLevel+1),
			v.newline, v.indent(v.indentLevel+1), body,
			v.newline, v.indent(v.indentLevel+1), returnExpr,
			v.newline, v.indent(v.indentLevel))
	case litHasDefer || litHasRecover:
		inner = wrapIIFEFuncContext(body, litHasDefer, litHasRecover)
	default:
		inner = body
	}

	if context.isIIFE {
		// Immediately-invoked function literal: emit `paramNames => BODY` (names only — the
		// delegate-cast in convCallExpr supplies the types).
		result.WriteString(v.iifeParamNames(litSig) + " => " + inner)
	} else {
		result.WriteString("(" + parameterSignature + ") => " + inner)
	}

	return result.String()
}
