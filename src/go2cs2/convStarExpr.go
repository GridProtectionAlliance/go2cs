package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr) string {
	// TODO: Could be pointer deref or pointer type declaration

	// Check if the star expression is a pointer to pointer dereference
	if _, ok := v.getType(starExpr.X, true).(*types.Pointer); ok {
		if _, ok := starExpr.X.(*ast.StarExpr); ok {
			return v.convExpr(starExpr.X, nil) + ".val"
		}
	}

	return v.convExpr(starExpr.X, nil)
}
