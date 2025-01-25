package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr, context TupleResultContext) string {
	// Check if the unary expression is a pointer dereference
	if unaryExpr.Op == token.AND {
		// Check if the unary expression is an address of a structure field
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if _, ok := v.getType(selectorExpr.X, true).(*types.Struct); ok {
				// For a structure field, we use the "ж.of(StructType.ᏑField)" syntax
				return fmt.Sprintf("%s%s.of(%s.%s%s)", AddressPrefix, v.convExpr(selectorExpr.X, nil), v.getTypeName(selectorExpr.X, false), AddressPrefix, v.convExpr(selectorExpr.Sel, nil))
			}
		}

		// Check if the unary expression is an address of an indexed array or slice
		if indexExpr, ok := unaryExpr.X.(*ast.IndexExpr); ok {
			typeName := v.getTypeName(indexExpr.X, true)

			if strings.HasPrefix(typeName, "[") {
				// For an indexed reference into an array or slice, we use the "ж.at<T>(index)" syntax
				csTypeName := convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:])
				return fmt.Sprintf("%s%s.at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convExpr(indexExpr.Index, nil))
			}
		}

		// Check if unary target is a pointer to a pointer
		if _, ok := v.getType(unaryExpr.X, true).(*types.Pointer); ok {
			return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
		}

		// Check if unary target is not a variable or a field
		if _, ok := unaryExpr.X.(*ast.Ident); !ok {
			return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
		}

		return AddressPrefix + v.convExpr(unaryExpr.X, nil)
	}

	if unaryExpr.Op == token.ARROW {
		// Check if the unary expression is channel receive operation
		if _, ok := v.getType(unaryExpr.X, true).(*types.Chan); ok {
			var tupleResult string

			if v.options.useChannelOperators {
				if context.isTupleResult {
					tupleResult = ", " + OverloadDiscriminator
				}

				return fmt.Sprintf("%s(%s%s)", ChannelLeftOp, v.convExpr(unaryExpr.X, nil), tupleResult)
			}

			if context.isTupleResult {
				tupleResult = OverloadDiscriminator
			}

			return fmt.Sprintf("%s.Receive(%s)", v.convExpr(unaryExpr.X, nil), tupleResult)
		}
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}
