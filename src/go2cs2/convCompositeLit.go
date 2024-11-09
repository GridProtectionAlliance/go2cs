package main

import (
	"fmt"
	"go/ast"
	"strings"
)

func (v *Visitor) convCompositeLit(compositeLit *ast.CompositeLit) string {
	result := &strings.Builder{}

	if compositeLit.Type == nil {
		rparenSuffix := ""

		if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].End(), compositeLit.Rbrace) {
			rparenSuffix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
		}

		result.WriteString(fmt.Sprintf("new(%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil)))
		v.writeStandAloneCommentString(result, compositeLit.Rbrace, nil, " ")
		result.WriteString(fmt.Sprintf("%s)", rparenSuffix))

		return result.String()
	}

	isArrayType := false
	sliceSuffix := ""

	// TODO: Handle map type construction
	if arrayType, ok := compositeLit.Type.(*ast.ArrayType); ok {
		isArrayType = true

		if arrayType.Len == nil {
			sliceSuffix = ".slice()"
		}
	}

	lbracePrefix := ""
	rbracePrefix := ""

	if isArrayType {
		lbracePrefix = " "
		rbracePrefix = " "
	}

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	lbraceChar := "("
	rbraceChar := ")"

	if isArrayType {
		lbraceChar = "{"
		rbraceChar = "}"
	}

	contexts := []ExprContext{ArrayTypeContext{compositeInitializer: true}}
	result.WriteString(fmt.Sprintf("new %s%s%s", convertToCSTypeName(v.convExpr(compositeLit.Type, contexts)), lbracePrefix, lbraceChar))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	}

	result.WriteString(fmt.Sprintf("%s%s%s%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil), rbracePrefix, rbraceChar, sliceSuffix))

	return result.String()
}
