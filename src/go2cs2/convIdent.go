package main

import (
	"go/ast"
)

func (v *Visitor) convIdent(ident *ast.Ident, context IdentContext) string {
	if ident.Name == "nil" {
		return "null"
	}

	if context.typeIsPointer {
		return AddressPrefix + v.getIdentName(ident)
	}

	return v.getIdentName(ident)
}
