package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr) string {
	// Check if the unary expression is a pointer dereference
	if unaryExpr.Op == token.AND {
		// Check if the unary expression is an address of an indexed array or slice
		if indexExpr, ok := unaryExpr.X.(*ast.IndexExpr); ok {
			typeName := v.getTypeName(indexExpr.X, true)

			if strings.HasPrefix(typeName, "[") {
				// For an indexed reference into an array or slice, we use the "ptr.at<T>(index)" syntax
				csTypeName := convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:])
				return fmt.Sprintf("%s%s.at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convExpr(indexExpr.Index, nil))
			}
		}

		return AddressPrefix + v.convExpr(unaryExpr.X, nil)
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}
