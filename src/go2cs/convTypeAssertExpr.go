package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convTypeAssertExpr(typeAssertExpr *ast.TypeAssertExpr) string {
	if typeAssertExpr.Type == nil {
		return fmt.Sprintf("%s.type()", v.convExpr(typeAssertExpr.X, nil))
	}

	// When casting empty interface to an interface, we need to record that the type implements the interface
	exprType := v.getExprType(typeAssertExpr.X)

	if _, isEmptyInterface := isInterface(exprType); isEmptyInterface {
		targetType := v.getExprType(typeAssertExpr.Type)

		if needsInterfaceCast, isEmpty := isInterface(targetType); needsInterfaceCast && !isEmpty {
			concreteType := v.getUnderlyingType(typeAssertExpr.X)

			if concreteType != nil {
				convertToInterfaceType(targetType, concreteType, "")
			}
		}
	}

	return fmt.Sprintf("%s._<%s>()", v.convExpr(typeAssertExpr.X, nil), convertToCSTypeName(v.convExpr(typeAssertExpr.Type, nil)))
}
