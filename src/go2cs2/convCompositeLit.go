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

		result.WriteString(fmt.Sprintf("new(%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace)))
		v.writeStandAloneCommentString(result, compositeLit.Rbrace, nil, " ")
		result.WriteString(fmt.Sprintf("%s)", rparenSuffix))

		return result.String()
	}

	sliceSuffix := ""

	if arrayType, ok := compositeLit.Type.(*ast.ArrayType); ok {
		if arrayType.Len == nil {
			sliceSuffix = ".slice()"
		}
	}

	rbracePrefix := " "

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	result.WriteString(fmt.Sprintf("new %s { ", v.convExpr(compositeLit.Type)))

	lbraceSuffix := ""

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	}

	result.WriteString(fmt.Sprintf("%s%s%s}%s", lbraceSuffix, v.convExprList(compositeLit.Elts, compositeLit.Lbrace), rbracePrefix, sliceSuffix))

	return result.String()
}
