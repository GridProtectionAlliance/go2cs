package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
	"unicode"
)

// isSimpleIdentifierName reports whether s is a bare (possibly @-escaped) identifier —
// used to recognize a C# named-argument label ahead of a `: ` separator.
func isSimpleIdentifierName(s string) bool {
	s = strings.TrimPrefix(s, "@")

	if s == "" {
		return false
	}

	for i, r := range s {
		if unicode.IsLetter(r) || r == '_' || (i > 0 && unicode.IsDigit(r)) {
			continue
		}

		return false
	}

	return true
}

func (v *Visitor) convExprList(exprs []ast.Expr, prevEndPos token.Pos, callContext *CallExprContext) string {
	if len(exprs) == 0 {
		return ""
	}

	result := &strings.Builder{}

	keyValueContext := DefaultKeyValueContext()
	forceMultiLine := false
	hasSpreadOperator := false
	var interfaceTypes map[int]types.Type
	var callArgs []string
	var replacementArgs []string

	if callContext != nil {
		keyValueContext.source = callContext.keyValueSource
		keyValueContext.ident = callContext.keyValueIdent
		keyValueContext.arrayBacked = callContext.keyValueArrayBacked
		keyValueContext.compositeType = callContext.keyValueCompositeType
		keyValueContext.deferredDecls = callContext.deferredDecls
		forceMultiLine = callContext.forceMultiLine
		hasSpreadOperator = callContext.hasSpreadOperator
		interfaceTypes = callContext.interfaceTypes
		callArgs = callContext.callArgs
		replacementArgs = callContext.replacementArgs
	}

	for i, expr := range exprs {
		exprOnNewLine := false

		if forceMultiLine {
			result.WriteString(v.newline)
			result.WriteString(v.indent(v.indentLevel))
		} else {
			if i == 0 && prevEndPos.IsValid() {
				exprOnNewLine = v.isLineFeedBetween(prevEndPos, expr.Pos())
			} else {
				result.WriteRune(',')
				exprOnNewLine = v.isLineFeedBetween(exprs[i-1].End(), expr.Pos())
				v.writeStandAloneCommentString(result, expr.Pos(), nil, " ")
			}

			if forceMultiLine || exprOnNewLine {
				result.WriteString(v.newline)
				v.indentLevel++
				result.WriteString(v.indent(v.indentLevel))
			} else if i > 0 {
				result.WriteRune(' ')
			}
		}

		basicLitContext := DefaultBasicLitContext()
		identContext := DefaultIdentContext()

		// Check for call context, such as arguments allows u8 strings or is a pointer type
		if callContext != nil {
			// Index out of bounds default to false here, so variadic params are handled correctly
			basicLitContext.u8StringOK = callContext.u8StringArgOK[i] && callArgs == nil
			basicLitContext.sourceIsRuneArray = callContext.sourceIsRuneArray

			// A DEFERRED call's args feed generic deferǃ type parameters: a u8 span cannot be
			// a generic type argument (u8StringOK forced off above), so a Go-string literal arg
			// takes the explicit (@string) cast instead — otherwise the parameter infers as C#
			// string and the method-group-to-Action conversion fails (fmt's
			// `defer p.catchPanic(p.arg, verb, "Format")`, CS0123 x4).
			basicLitContext.castToGoString = callContext.useGoStringArg[i] || (callArgs != nil && callContext.u8StringArgOK[i])

			// Check if the argument is a pointer type
			identContext.isPointer = callContext.argTypeIsPtr[i]

			// Check if the argument is a type parameter
			identContext.isType = callContext.sourceIsTypeParams
		}

		// A func-literal argument needs a LambdaContext carrying the call's deferredDecls builder so
		// its capture declarations hoist to the enclosing statement instead of emitting inline in
		// the argument list (invalid C#). Harmless for non-func-literal args.
		lambdaContext := DefaultLambdaContext()

		if callContext != nil {
			lambdaContext.deferredDecls = callContext.deferredDecls
		}

		contexts := []ExprContext{basicLitContext, identContext, keyValueContext, lambdaContext, callContext}

		var resultExpr string

		// A SPREAD argument (`append(p, d.globalColorTable...)`, gif reader.go) passes the
		// slice's ELEMENTS — already the variadic element type — so the per-argument
		// interface conversion must not fire: it wrapped the whole Palette in a single
		// Color VALUE adapter (no ꓸꓸꓸ member, CS1061) and RECORDED a bogus
		// GoImplement<Palette, Color> whose generated impl couldn't compile (CS1503).
		spreadArg := hasSpreadOperator && i == len(exprs)-1

		// Go expands a MULTI-VALUE call's results into the enclosing call's parameters —
		// `compInfo(nfcData.lookup(s))` (norm forminfo.go): lookup returns (uint16, int)
		// feeding compInfo(v, sz). C# has no splat, so the inner call hoists into temp
		// markers passed expanded (`var (ᴛ1, ᴛ2) = …; compInfo(ᴛ1, ᴛ2)`; CS7036 ×4).
		// Gated to the single-argument shape with a hoist target; defer/go marker paths
		// (callArgs) keep their own machinery.
		tupleExpanded := false

		if len(exprs) == 1 && callArgs == nil && !spreadArg && callContext != nil && callContext.deferredDecls != nil {
			if innerCall, isCall := expr.(*ast.CallExpr); isCall {
				if tuple, isTuple := v.getExprType(innerCall).(*types.Tuple); isTuple && tuple.Len() > 1 {
					innerExpr := v.convExpr(innerCall, contexts)
					tempNames := make([]string, tuple.Len())

					// Per-file monotonic marker index: two expansions in nested scopes of one
					// function otherwise collide (norm's Properties if-arm + tail, CS0136 ×4).
					for ti := range tuple.Len() {
						tempNames[ti] = fmt.Sprintf("%s%d", TempVarMarker, v.tupleTempIndex+ti+1)
					}

					v.tupleTempIndex += tuple.Len()

					callContext.deferredDecls.WriteString(v.newline)
					callContext.deferredDecls.WriteString(v.indent(v.indentLevel))
					callContext.deferredDecls.WriteString(fmt.Sprintf("var (%s) = %s;", strings.Join(tempNames, ", "), innerExpr))
					callContext.deferredDecls.WriteString(v.newline)
					resultExpr = strings.Join(tempNames, ", ")
					tupleExpanded = true
				}
			}
		}

		if tupleExpanded {
			// expanded above — skip the per-argument conversion chain
		} else if interfaceType, ok := interfaceTypes[i]; ok && interfaceType != nil && !spreadArg {
			// A POINTER argument converting to an interface must render as the pointer VALUE —
			// the box `Ꮡfs`, not the deref'd receiver ref-local `fs` — since Go's interface
			// holds the *T and the pointer-adapter emission wraps the box itself
			// (`new runtimeSourceᴵSource(Ꮡfs)`; math/rand's read call, CS1503).
			if _, argIsPtr := v.getType(expr, false).(*types.Pointer); argIsPtr {
				identContext.isPointer = true
				contexts = []ExprContext{basicLitContext, identContext, keyValueContext, lambdaContext, callContext}
			}

			resultExpr = v.convertToInterfaceType(interfaceType, v.getType(expr, false), v.convExpr(expr, contexts))
		} else {
			resultExpr = v.convExpr(expr, contexts)
		}

		if replacementArgs != nil && i < len(replacementArgs) && len(replacementArgs[i]) > 0 {
			resultExpr = strings.ReplaceAll(replacementArgs[i], DynamicCastArgMarker, resultExpr)
		}

		// Apply an explicit per-argument cast when requested (e.g. an untyped-constant element
		// of an `append` call cast to the slice's element type to avoid C# overload ambiguity).
		if callContext != nil && callContext.castArgToType != nil {
			if castType, ok := callContext.castArgToType[i]; ok && len(castType) > 0 {
				// A KEYED struct-literal element renders as a C# named argument
				// (`Y: value`) — cast the VALUE only, mirroring the wrapArgWithNew handling
				// below (casting the label too is a syntax error).
				label, value, keyed := strings.Cut(resultExpr, ": ")

				if !keyed || !isSimpleIdentifierName(label) {
					label, value, keyed = "", resultExpr, false
				}

				// Skip when the value already opens with the target cast (convBinaryExpr
				// casts a same-type named/narrow binary result on its own — a redundant
				// `(uint16)((uint16)…)` would only add noise); apply only where the render
				// actually promoted (image/png's `(b >> 7) * 0xff` has no leading cast).
				if !strings.HasPrefix(value, fmt.Sprintf("(%s)(", castType)) {
					if keyed {
						resultExpr = fmt.Sprintf("%s: (%s)(%s)", label, castType, value)
					} else {
						resultExpr = fmt.Sprintf("(%s)(%s)", castType, value)
					}
				}
			}
		}

		// A constrained slice type parameter passed where a CONCRETE slice<E> is expected (Go
		// assignability: S ~[]E is assignable to []E — slices' rotateRight/pdqsort helper calls)
		// materializes through the SHARING slice<T>(ISlice<T>) constructor — a cast cannot apply
		// (the source is interface-constrained; C# forbids user conversions from interfaces).
		if callContext != nil && callContext.wrapArgWithNew != nil {
			if newType, ok := callContext.wrapArgWithNew[i]; ok && len(newType) > 0 {
				// A KEYED struct-literal element renders as a C# named argument
				// (`keyHash: value`) — the constructor wrap applies to the VALUE only
				// (wrapping the label was CS0149, internal/concurrent's hashFunc fields).
				if label, value, found := strings.Cut(resultExpr, ": "); found && isSimpleIdentifierName(label) {
					resultExpr = fmt.Sprintf("%s: new %s(%s)", label, newType, value)
				} else {
					resultExpr = fmt.Sprintf("new %s(%s)", newType, resultExpr)
				}
			}
		}

		// Re-wrap a func-typed argument as a lambda so a constraint-proxy delegate position can
		// apply the ж<element>↔proxy user-defined conversion that a bare method-group conversion
		// cannot (`newPoint: nistec.NewP224Point` → `newPoint: () => nistec.NewP224Point()`). The
		// param list is supplied by the caller; C# infers each parameter's type from the target
		// delegate, so only names are needed. Applies to the VALUE of a keyed element only.
		if callContext != nil && callContext.wrapArgWithLambda != nil {
			if params, ok := callContext.wrapArgWithLambda[i]; ok {
				if label, value, found := strings.Cut(resultExpr, ": "); found && isSimpleIdentifierName(label) {
					resultExpr = fmt.Sprintf("%s: (%s) => %s(%s)", label, params, value, params)
				} else {
					resultExpr = fmt.Sprintf("(%s) => %s(%s)", params, resultExpr, params)
				}
			}
		}

		arg := &strings.Builder{}

		arg.WriteString(resultExpr)

		// If the last expression has a spread operator, use elipsis property as source
		// this way elements are passed as arguments instead of a slice or array
		if hasSpreadOperator && i == len(exprs)-1 {
			// A STRING-LITERAL spread — `append(b, "runtime error: "...)` (runtime error.go) — renders
			// the literal as a `"…"u8` ReadOnlySpan<byte>, which has no spread property (CS1061). Wrap
			// it as the member-accessible @string, whose `ꓸꓸꓸ` returns the Span<byte> the variadic
			// overload binds — the same wrap the `string(r)...` conversion spread already uses. A
			// non-literal spread source (slice, @string variable) is unchanged.
			spreadExpr := expr

			for {
				if paren, ok := spreadExpr.(*ast.ParenExpr); ok {
					spreadExpr = paren.X
					continue
				}

				break
			}

			if lit, ok := spreadExpr.(*ast.BasicLit); ok && lit.Kind == token.STRING {
				truncated := arg.String()
				arg.Reset()
				arg.WriteString(fmt.Sprintf("((@string)%s)", truncated))
			}

			// A string CONCAT spread — `append(buf, "signal.NotifyContext("+name...)`
			// (os/signal) — renders operand-wise as mixed u8/@string span forms whose
			// '+' has no overload (CS0019); wrap the whole expression as @string so the
			// spread yields one Span<byte>.
			if bin, ok := spreadExpr.(*ast.BinaryExpr); ok && bin.Op == token.ADD {
				if exprType := v.getExprType(spreadExpr); exprType != nil {
					if basic, ok := exprType.Underlying().(*types.Basic); ok && basic.Info()&types.IsString != 0 {
						truncated := arg.String()
						arg.Reset()
						arg.WriteString(fmt.Sprintf("((@string)(%s))", truncated))
					}
				}
			}

			arg.WriteString("." + EllipsisOperator)
		}

		if callArgs == nil {
			result.WriteString(arg.String())
		} else {
			// A deferred-call argument that is an UNTYPED-constant REFERENCE (`io.SeekStart`,
			// a `static readonly UntypedInt`) poisons deferǃ's delegate inference — the type
			// parameter resolves to the wrapper where the deferred method group takes the
			// default type (CS0123 ×2, poll fd_windows' deferred Seek). Cast it to the
			// constant's DEFAULT Go type; a literal is already a typed C# constant. An arg
			// the defer machinery ALREADY cast to its parameter type (castArgToType) must
			// stay — stacking the default-type cast on top re-poisons inference with the
			// default where the parameter differs (syscall exec_windows'
			// DUPLICATE_CLOSE_SOURCE, uint32 param — CS1503 regression on the first cut).
			alreadyCast := false

			if callContext != nil && callContext.castArgToType != nil {
				if castType, ok := callContext.castArgToType[i]; ok && len(castType) > 0 {
					alreadyCast = true
				}
			}

			var constExpr ast.Expr

			switch e := expr.(type) {
			case *ast.Ident:
				constExpr = e
			case *ast.SelectorExpr:
				constExpr = e.Sel
			}

			if !alreadyCast && constExpr != nil {
				if constIdent, ok := constExpr.(*ast.Ident); ok {
					if constObj, ok := v.info.ObjectOf(constIdent).(*types.Const); ok {
						if basic, ok := constObj.Type().(*types.Basic); ok && basic.Info()&types.IsUntyped != 0 {
							wrapped := fmt.Sprintf("(%s)%s", v.getCSTypeName(types.Default(basic).(*types.Basic)), arg.String())
							arg.Reset()
							arg.WriteString(wrapped)
						}
					}
				}
			}

			result.WriteString(fmt.Sprintf("%s%d", TempVarMarker, i+1))
			callArgs[i] = arg.String()
		}

		if exprOnNewLine {
			v.indentLevel--
		}

		if forceMultiLine && i != len(exprs)-1 {
			result.WriteRune(';')
		}
	}

	return result.String()
}
