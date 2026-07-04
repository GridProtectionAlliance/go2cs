package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

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

		if interfaceType, ok := interfaceTypes[i]; ok && interfaceType != nil {
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
				resultExpr = fmt.Sprintf("(%s)(%s)", castType, resultExpr)
			}
		}

		// A constrained slice type parameter passed where a CONCRETE slice<E> is expected (Go
		// assignability: S ~[]E is assignable to []E — slices' rotateRight/pdqsort helper calls)
		// materializes through the SHARING slice<T>(ISlice<T>) constructor — a cast cannot apply
		// (the source is interface-constrained; C# forbids user conversions from interfaces).
		if callContext != nil && callContext.wrapArgWithNew != nil {
			if newType, ok := callContext.wrapArgWithNew[i]; ok && len(newType) > 0 {
				resultExpr = fmt.Sprintf("new %s(%s)", newType, resultExpr)
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
