package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convTypeAssertExpr(typeAssertExpr *ast.TypeAssertExpr) string {
	if typeAssertExpr.Type == nil {
		return fmt.Sprintf("%s.type()", v.convExpr(typeAssertExpr.X, nil))
	}

	return fmt.Sprintf("%s._<%s>()", v.convExpr(typeAssertExpr.X, nil), convertToCSTypeName(v.convExpr(typeAssertExpr.Type, nil)))
}
