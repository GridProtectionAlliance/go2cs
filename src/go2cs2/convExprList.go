package main

import (
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) convExprList(exprs []ast.Expr, prevEndPos token.Pos, exprContext ExprContext) string {
	result := &strings.Builder{}

	var callExprContext *CallExprContext

	if context, ok := exprContext.(*CallExprContext); ok {
		callExprContext = context
	}

	for i, expr := range exprs {
		exprOnNewLine := false

		if i == 0 && prevEndPos.IsValid() {
			exprOnNewLine = v.isLineFeedBetween(prevEndPos, expr.Pos())
		} else {
			result.WriteString(",")
			exprOnNewLine = v.isLineFeedBetween(exprs[i-1].End(), expr.Pos())
			v.writeStandAloneCommentString(result, expr.Pos(), nil, " ")
		}

		if exprOnNewLine {
			result.WriteString(v.newline)
			v.indentLevel++
			result.WriteString(v.indent(v.indentLevel))
		} else if i > 0 {
			result.WriteRune(' ')
		}

		var context ExprContext

		// If in a call expression, check if the argument allows u8 strings
		if callExprContext != nil {
			context = &BasicLitContext{u8StringOK: callExprContext.u8StringArgOK[i]}
		}

		result.WriteString(v.convExpr(expr, context))

		if exprOnNewLine {
			v.indentLevel--
		}
	}

	return result.String()
}
