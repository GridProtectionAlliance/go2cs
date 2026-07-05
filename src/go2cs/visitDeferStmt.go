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
	lambdaContext.deferOrGoCall = true
	lambdaContext.deferCall = true
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

		// A BUILTIN callee (`defer close(returned)`, net dial) is generic with `in`
		// parameters — its method group neither infers nor converts to Action<T>
		// (CS1503). Render the temp-param lambda (`deferǃ(ᴛ1 => close(ᴛ1), returned,
		// defer)`) so the eager-argument form still evaluates the argument at defer time.
		if calleeIdent, ok := deferStmt.Call.Fun.(*ast.Ident); ok && paramCount > 0 {
			if _, isBuiltin := v.info.ObjectOf(calleeIdent).(*types.Builtin); isBuiltin {
				renderLambdaParams = true
			}
		}
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

		// C# `defer` method implementation expects an Action delegate. The bare
		// method-group form (`defer(k.Close)`) binds only when the callee returns VOID —
		// an error-returning method (`defer k.Close()`, registry Key.Close in time's
		// zoneinfo_windows) is a Func<error> method group (CS1503 ×2). Keep the lambda
		// form there so the call's result is discarded, exactly Go's deferred-call
		// semantics.
		hasResults := false
		namedFuncType := false

		if funType := v.getType(deferStmt.Call.Fun, false); funType != nil {
			if sig, ok := funType.(*types.Signature); ok && sig.Results() != nil && sig.Results().Len() > 0 {
				hasResults = true
			}

			// A NAMED func-type callee (context.CancelFunc) is a DISTINCT C# delegate with
			// no conversion to Action — the bare trimmed form was CS1503 ×5 (net dial's
			// `defer cancel()`); keep the lambda so the invocation converts.
			if named, ok := types.Unalias(funType).(*types.Named); ok {
				if _, isSig := named.Underlying().(*types.Signature); isSig {
					namedFuncType = true
				}
			}
		}

		// A nullary deferred call on a POINTER-receiver method whose receiver is already a
		// pointer binds the BOX method group — the plain-call trim leaves the deref-alias form
		// (`Ꮡconf.Value.releaseSema`, net nss.go's `defer conf.releaseSema()`), a struct
		// VALUE against the [GoRecv] ref extension, which cannot create a delegate (CS1113).
		// `Ꮡconf.releaseSema` binds the ж<T> overload and captures the receiver at defer
		// time — exactly Go's binding.
		boxGroup := ""

		if !hasResults && !namedFuncType {
			boxGroup = v.pointerReceiverBoxMethodGroup(deferStmt.Call.Fun)
		}

		if boxGroup != "" {
			callExpr = boxGroup
		} else if !hasResults && !namedFuncType && strings.HasSuffix(callExpr, "()") {
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
		} else {
			// A matching-arity pointer-receiver method group has the same CS1113 as the
			// nullary form (see above) — bind the BOX overload here too.
			if boxGroup := v.pointerReceiverBoxMethodGroup(deferStmt.Call.Fun); boxGroup != "" {
				callExpr = boxGroup
			}
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

// pointerReceiverBoxMethodGroup renders `X.M` as a BOX-bound method group for a nullary
// defer/go call whose method has a POINTER receiver and whose receiver ident is already
// pointer-typed (`defer conf.releaseSema()` with `conf *resolverConfig`): the plain call
// render deref-aliases the receiver (`Ꮡconf.Value.releaseSema()`), whose method-group
// trim is a struct VALUE against the [GoRecv] ref extension (CS1113). The box form binds
// the ж<T> overload and captures the receiver when the delegate is created — Go's
// binding time. Non-ident receivers keep the existing emission.
func (v *Visitor) pointerReceiverBoxMethodGroup(fun ast.Expr) string {
	selectorExpr, ok := fun.(*ast.SelectorExpr)

	if !ok {
		return ""
	}

	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	if !ok {
		return ""
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return ""
	}

	// A result-returning method group is a Func<...>, which neither defer(Action) nor
	// go(Action) accepts - those keep the lambda form.
	if sig.Results() != nil && sig.Results().Len() > 0 {
		return ""
	}

	if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); !isPtrRecv {
		return ""
	}

	identX, ok := selectorExpr.X.(*ast.Ident)

	if !ok {
		return ""
	}

	recvType := v.getType(selectorExpr.X, false)

	if recvType == nil {
		return ""
	}

	xPtr, alreadyPtr := recvType.(*types.Pointer)

	if !alreadyPtr {
		return ""
	}

	// A PROMOTED method (net interface.go's `defer zc.Unlock()` — Unlock declared on the
	// embedded sync.RWMutex) has no extension on the OUTER box type (CS1061); only a method
	// declared directly on the pointee takes the box group.
	if recvPtr, ok := sig.Recv().Type().(*types.Pointer); !ok || !types.Identical(recvPtr.Elem(), xPtr.Elem()) {
		return ""
	}

	ptrContext := DefaultIdentContext()
	ptrContext.isPointer = true

	return v.convIdent(identX, ptrContext) + "." + v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))
}
