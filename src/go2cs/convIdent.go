package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) convIdent(ident *ast.Ident, context IdentContext) string {
	if ident.Name == "nil" {
		return "default!"
	}

	if context.isPointer {
		return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
	}

	if context.isType {
		return convertToCSTypeName(v.getIdentName(ident))
	}

	return getSanitizedIdentifier(v.getIdentName(ident))
}