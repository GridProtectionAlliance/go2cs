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
	callContext := DefaultCallExprContext()
	callContext.keyValueIdent = context.ident
	isArrayType := false
	isSliceType := false
	isMapType := false
	compositeSuffix := ""

	if _, ok := exprType.(*types.Array); ok {
		isArrayType = true
		callContext.keyValueSource = ArraySource
	}

	if _, isSliceType = exprType.(*types.Slice); isSliceType {
		isArrayType = true
		callContext.keyValueSource = ArraySource
		compositeSuffix = ".slice()"
	}

	if _, isMapType = exprType.(*types.Map); isMapType {
		callContext.keyValueSource = MapSource
	}

	lbracePrefix := ""
	rbracePrefix := ""

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	lbraceChar := "("
	rbraceChar := ")"

	if isArrayType {
		lbraceChar = "{"
		rbraceChar = "}"
	}

	if isMapType {
		lbraceChar = ""
		rbraceChar = ""
	}

	arrayTypeContext := DefaultArrayTypeContext()
	arrayTypeContext.compositeInitializer = isArrayType

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

	contexts := []ExprContext{arrayTypeContext}

	var newSpace string

	if v.options.preferVarDecl || arrayTypeContext.compositeInitializer {
		newSpace = " "
	}

	result.WriteString(fmt.Sprintf("new%s%s%s%s", newSpace, convertToCSTypeName(v.convExpr(compositeLit.Type, contexts)), lbracePrefix, lbraceChar))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	}

	if isMapType {
		result.WriteString(fmt.Sprintf("{%s}", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext)))
	} else {
		result.WriteString(fmt.Sprintf("%s%s%s%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext), rbracePrefix, rbraceChar, compositeSuffix))
	}

	return result.String()
}
