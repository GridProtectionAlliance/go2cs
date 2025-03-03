package main

import (
	"fmt"
	"go/ast"
	"go/types"
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
				v.convertToInterfaceType(targetType, concreteType, "")
			}
		}
	}

	// Get the type information for this expression
	typesInfo := v.info.TypeOf(typeAssertExpr)
	var safeAssertDescriminator string

	// Check if this is used in a multi-value context, thus making it a safe assertion
	if tuple, isTuple := typesInfo.(*types.Tuple); isTuple && tuple.Len() == 2 {
		safeAssertDescriminator = TrueMarker
	}

	return fmt.Sprintf("%s._<%s>(%s)", v.convExpr(typeAssertExpr.X, nil), convertToCSTypeName(v.convExpr(typeAssertExpr.Type, nil)), safeAssertDescriminator)
}
