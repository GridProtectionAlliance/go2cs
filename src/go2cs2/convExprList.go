package main

import (
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) convExprList(exprs []ast.Expr, prevEndPos token.Pos, callContext *CallExprContext) string {
	result := &strings.Builder{}

	keyValueContext := DefaultKeyValueContext()
	forceMultiLine := false
	hasSpreadOperator := false

	if callContext != nil {
		keyValueContext.source = callContext.keyValueSource
		keyValueContext.ident = callContext.keyValueIdent
		forceMultiLine = callContext.forceMultiLine
		hasSpreadOperator = callContext.hasSpreadOperator
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
			basicLitContext.u8StringOK = callContext.u8StringArgOK[i]

			// Check if the argument is a pointer type
			identContext.typeIsPointer = callContext.argTypeIsPtr[i]
		}

		contexts := []ExprContext{basicLitContext, identContext, keyValueContext, callContext}

		result.WriteString(v.convExpr(expr, contexts))

		// If the last expression has a spread operator, convert it to a ToArray() call,
		// this way elements or source are passed as arguments instead of a slice/array
		if hasSpreadOperator && i == len(exprs)-1 {
			result.WriteString(".ToArray()")
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
