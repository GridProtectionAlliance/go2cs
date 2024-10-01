package main

import (
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) convExprList(exprs []ast.Expr, prevEndPos token.Pos) string {
	result := &strings.Builder{}

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

		result.WriteString(v.convExpr(expr))

		if exprOnNewLine {
			v.indentLevel--
		}
	}

	return result.String()
}
