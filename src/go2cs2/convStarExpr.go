package main

import (
	"go/ast"
)

func (v *Visitor) convStarExpr(ident *ast.StarExpr) string {
	// TODO: Could be pointer deref or pointer type declaration
	return v.convExpr(ident.X) + ".val"
}
