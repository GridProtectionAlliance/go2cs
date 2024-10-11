package main

import (
	"go/ast"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr) string {
	// TODO: Could be pointer deref or pointer type declaration

	// TODO: Only need to use ".val" suffix when directly dereferencing a pointer
	return v.convExpr(starExpr.X, nil) // + ".val"
}
