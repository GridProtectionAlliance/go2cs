package main

import (
	"go/ast"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr) string {
	ident := getIdentifier(starExpr.X)

	if ident != nil && v.identIsParameter(ident) {
		// Check if the star expression is a pointer to pointer dereference
		if v.isPointer(ident) {
			if _, ok := starExpr.X.(*ast.StarExpr); ok {
				return v.convExpr(starExpr.X, nil) + ".val"
			}
		}

		// Prefer to use local reference instead of dereferencing a pointer
		return v.convExpr(starExpr.X, nil)
	}

	return v.convExpr(starExpr.X, nil) + ".val"
}
