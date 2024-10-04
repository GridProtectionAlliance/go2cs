package main

import (
	"go/ast"
)

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr) string {
	// TODO: Could be pointer deref or pointer type declaration
	return v.convExpr(starExpr.X, nil) + ".val"
}
