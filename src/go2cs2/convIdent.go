package main

import (
	"go/ast"
)

func (v *Visitor) convIdent(ident *ast.Ident) string {
	if ident.Name == "nil" {
		return "null"
	}

	return v.getIdentName(ident)
}
