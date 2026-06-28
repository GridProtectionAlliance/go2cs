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
			basicLitContext.castToGoString = callContext.useGoStringArg[i] && callArgs == nil

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

		arg := &strings.Builder{}

		arg.WriteString(resultExpr)

		// If the last expression has a spread operator, use elipsis property as source
		// this way elements are passed as arguments instead of a slice or array
		if hasSpreadOperator && i == len(exprs)-1 {
			arg.WriteString("." + EllipsisOperator)
		}

		if callArgs == nil {
			result.WriteString(arg.String())
		} else {
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
