package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strconv"
	"strings"
)

func (v *Visitor) convCompositeLit(compositeLit *ast.CompositeLit, context KeyValueContext) string {
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

	exprType := v.getExprType(compositeLit.Type)
	arrayTypeContext := DefaultArrayTypeContext()
	callContext := DefaultCallExprContext()
	callContext.keyValueIdent = context.ident
	compositeSuffix := ""

	if _, ok := exprType.(*types.Array); ok {
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".array()"
	}

	if _, ok := exprType.(*types.Slice); ok {
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".slice()"
	}

	if _, ok := exprType.(*types.Map); ok {
		callContext.keyValueSource = MapSource
	}

	var lbracePrefix, rbracePrefix string

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	// Check if first element is a key value pair expression
	if callContext.keyValueSource == ArraySource && len(compositeLit.Elts) > 0 {
		// Check for indexed specifiers representing sparse array/slice initialization
		if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
			// Find maximum value of key in composite literals
			maxKeyValue := 0

			for _, elt := range compositeLit.Elts {
				if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
					if basicLit, ok := keyValue.Key.(*ast.BasicLit); ok {
						if keyValue, err := strconv.ParseInt(basicLit.Value, 0, 64); err == nil {
							if int(keyValue) > maxKeyValue {
								maxKeyValue = int(keyValue)
							}
						} else {
							// If key is not a constant, then it is not a sparse array
							maxKeyValue = 0
							break
						}
					}
				}
			}

			if maxKeyValue > 0 {
				// Sparse array/slice initialization can be initialized like a map
				callContext.keyValueSource = MapSource
				arrayTypeContext.compositeInitializer = false
				arrayTypeContext.maxLength = maxKeyValue + 1
				compositeSuffix = ""
			}
		}
	}

	var newSpace string

	if v.options.preferVarDecl || arrayTypeContext.compositeInitializer {
		newSpace = " "
	}

	contexts := []ExprContext{arrayTypeContext}
	result.WriteString(fmt.Sprintf("new%s%s%s{", newSpace, convertToCSTypeName(v.convExpr(compositeLit.Type, contexts)), lbracePrefix))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	}

	result.WriteString(fmt.Sprintf("%s%s}%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext), rbracePrefix, compositeSuffix))

	return result.String()
}
