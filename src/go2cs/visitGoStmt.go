package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	// Analyze captures specifically for this go statement. The seeded enter keeps the
	// enclosing lambda's capture renames visible while the EAGER call arguments render —
	// they are evaluated in the enclosing scope at spawn time (see enterDeferGoLambdaConversion).
	v.enterDeferGoLambdaConversion(goStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()
	lambdaContext.deferOrGoCall = true
	paramCount := len(goStmt.Call.Args)

	// Capture-snapshot declarations belong BEFORE the goǃ statement — including those of a
	// func-literal ARGUMENT when the callee is a func-value ident (net lookup.go's
	// `go dnsWaitGroupDone(ch, func() {})`): without a hoist target the literal's decls
	// were written INLINE in the argument list (`goǃ(fʗ1, ch, var fʗ1 = f; () => {})`,
	// CS1003 ×4).
	lambdaContext.deferredDecls = &strings.Builder{}

	var renderLambdaParams bool

	// If we have a function literal, only prepare captures there, not on the GoStmt
	if funcLit, ok := goStmt.Call.Fun.(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[goStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, goStmt)
		}
	} else {
		v.prepareStmtCaptures(goStmt)

		// If call expression parameter counts match those of target function signature,
		// we can render call without lambda parameters
		renderLambdaParams = paramCount != v.getFunctionParamCount(goStmt.Call.Fun)
	}

	// Resolve the callee signature once (func-literal callees are excluded — convCallExpr
	// already renders those as a lambda): whether it returns a value, and whether it is a
	// NAMED func type. Both decide how the call is adapted to goǃ's void Action delegate below.
	hasResults := false
	namedFuncType := false

	if _, isFuncLit := goStmt.Call.Fun.(*ast.FuncLit); !isFuncLit {
		if funType := v.getType(goStmt.Call.Fun, false); funType != nil {
			if sig, ok := funType.(*types.Signature); ok && sig.Results() != nil && sig.Results().Len() > 0 {
				hasResults = true
			}

			if named, ok := types.Unalias(funType).(*types.Named); ok {
				if _, isSig := named.Underlying().(*types.Signature); isSig {
					namedFuncType = true
				}
			}
		}
	}

	// A value-returning callee passed as a BARE method group is a Func<...>, but every goǃ
	// overload takes a void Action<...> (CS0407 "no overload matches the delegate" —
	// x/net/nettest's `go chunkedCopy(c2, c2)`, chunkedCopy returning error). Force the
	// temp-param lambda form so convCallExpr renders `(ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2)`, which
	// converts to a discarding Action — exactly Go's fire-and-forget goroutine semantics. This
	// mirrors the defer arm's result handling; the defer PARAM case needs no equivalent only
	// because deferǃ additionally has Func<...> overloads, while goǃ does not.
	if hasResults && paramCount > 0 {
		renderLambdaParams = true
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
		// A pointer-receiver nullary callee binds the BOX method group (see
		// pointerReceiverBoxMethodGroup — the trimmed deref-alias form is CS1113). A NAMED
		// func-type or value-returning callee keeps the lambda form instead.
		boxGroup := ""

		if !hasResults && !namedFuncType {
			boxGroup = v.pointerReceiverBoxMethodGroup(goStmt.Call.Fun)
		}

		// C# `go` method implementation expects an Action (or WaitCallback delegate)
		if boxGroup != "" {
			callExpr = boxGroup
		} else if !hasResults && !namedFuncType && strings.HasSuffix(callExpr, "()") {
			callExpr = strings.TrimSuffix(callExpr, "()") // Action delegate
		} else if (hasResults || namedFuncType) && strings.HasSuffix(callExpr, "()") {
			// A NAMED func-type callee (context.CancelFunc) is a DISTINCT C# delegate with no
			// conversion to Action, and a value-returning callee's bare method group is a
			// Func<...> (CS0407); keep the invocation and wrap it in an Action lambda that
			// discards any result — exactly Go's fire-and-forget goroutine semantics.
			callExpr = "() => " + callExpr
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
		} else {
			// A matching-arity pointer-receiver method group has the same CS1113 as the
			// nullary form — the deref-alias render is a struct VALUE against the [GoRecv]
			// ref extension (os/exec's `go c.watchCtx(resultc)`); bind the BOX overload.
			if boxGroup := v.pointerReceiverBoxMethodGroup(goStmt.Call.Fun); boxGroup != "" {
				callExpr = boxGroup
			}
		}

		result.WriteString(callExpr)
		result.WriteString(", ")
		result.WriteString(strings.Join(lambdaContext.callArgs, ", "))
	}

	result.WriteString(");")

	v.targetFile.WriteString(result.String())
}
