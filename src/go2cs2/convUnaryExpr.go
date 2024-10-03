package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr) string {
	if unaryExpr.Op == token.AND {
		return AddressPrefix + v.convExpr(unaryExpr.X, nil)
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}
