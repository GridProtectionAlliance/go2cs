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

	// Get the element type for arrays/slices/maps to check for interfaces
	var elementType types.Type
	var needsInterfaceCast bool
	var isEmpty bool
	var definedLen int

	switch t := exprType.(type) {
	case *types.Array:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".array()"
		definedLen = int(t.Len())
	case *types.Slice:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".slice()"
	case *types.Map:
		elementType = t.Elem()
		callContext.keyValueSource = MapSource
	case *types.Named:
		if structType, ok := t.Underlying().(*types.Struct); ok {
			// Check composite lit elements against struct fields
			for i := 0; i < structType.NumFields(); i++ {
				field := structType.Field(i)

				if i < len(compositeLit.Elts) {
					// Check if field is an embedded interface
					if field.Embedded() {
						if fieldType := field.Type(); fieldType != nil {
							if needsInterfaceCast, isEmpty := isInterface(fieldType); needsInterfaceCast && !isEmpty {
								// Record implementation
								eltType := v.info.TypeOf(compositeLit.Elts[i])

								if eltType != nil {
									v.convertToInterfaceType(fieldType, eltType, "")
								}
							}
						}
					}
				}
			}
		}
	}

	// Check if element type is an interface
	if elementType != nil {
		needsInterfaceCast, isEmpty = isInterface(elementType)
		if needsInterfaceCast && !isEmpty {
			callContext.interfaceType = elementType
		}
	}

	var lbracePrefix, rbracePrefix string
	lbrace := "{"
	rbrace := "}"

	if named, ok := exprType.(*types.Named); ok {
		if _, ok := named.Underlying().(*types.Struct); ok {
			lbrace = "("
			rbrace = ")"
		}
	} else if _, ok := exprType.(*types.Struct); ok {
		lbrace = "("
		rbrace = ")"
	}

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	// Check for sparse array initialization
	if callContext.keyValueSource == ArraySource && len(compositeLit.Elts) > 0 {
		if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
			maxKeyValue := 0

			for _, elt := range compositeLit.Elts {
				if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
					if basicLit, ok := keyValue.Key.(*ast.BasicLit); ok {
						// Check for rune literal
						if strings.HasPrefix(basicLit.Value, "'") && strings.HasSuffix(basicLit.Value, "'") {
							basicLit.Value = strconv.Itoa(int(basicLit.Value[1 : len(basicLit.Value)-1][0]))
						}

						if keyValue, err := strconv.ParseInt(basicLit.Value, 0, 64); err == nil {
							if int(keyValue) > maxKeyValue {
								maxKeyValue = int(keyValue)
							}
						} else {
							maxKeyValue = 0
							break
						}
					}
				}
			}

			if maxKeyValue > 0 {
				callContext.keyValueSource = MapSource
				arrayTypeContext.compositeInitializer = false

				if definedLen > 0 {
					arrayTypeContext.maxLength = definedLen
				} else {
					arrayTypeContext.maxLength = maxKeyValue + 1
				}

				compositeSuffix = ""
			}
		}
	}

	var newSpace string

	if callContext.keyValueSource == ArraySource || callContext.keyValueSource == MapSource || callContext.keyValueSource == StructSource || arrayTypeContext.compositeInitializer {
		newSpace = " "
	} else {
		newSpace = ""
	}

	identContext := DefaultIdentContext()
	identContext.isType = true

	if context.ident != nil {
		identContext.ident = context.ident
	}

	contexts := []ExprContext{arrayTypeContext, identContext}

	result.WriteString(fmt.Sprintf("new%s%s%s%s", newSpace, v.convExpr(compositeLit.Type, contexts), lbracePrefix, lbrace))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	} else {
		// If constructing a struct with no parameters, pass in a nill value
		if _, ok := exprType.(*types.Named); ok {
			result.WriteString("nil")
		}
	}

	// Convert elements with potential interface casting
	if needsInterfaceCast && !isEmpty {
		elements := &strings.Builder{}

		for i, elt := range compositeLit.Elts {
			if i > 0 {
				elements.WriteString(", ")
			}

			var expr string

			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				// Handle key-value pairs (for maps or sparse arrays)
				keyStr := v.convExpr(kv.Key, nil)
				valStr := v.convExpr(kv.Value, nil)
				expr = fmt.Sprintf("%s, %s", keyStr, v.convertToInterfaceType(elementType, v.getType(kv.Value, false), valStr))
			} else {
				// Handle regular elements
				expr = v.convertToInterfaceType(elementType, v.getType(elt, false), v.convExpr(elt, nil))
			}

			elements.WriteString(expr)
		}
		result.WriteString(elements.String())
	} else {
		result.WriteString(v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext))
	}

	result.WriteString(fmt.Sprintf("%s%s%s", rbracePrefix, rbrace, compositeSuffix))

	return result.String()
}
