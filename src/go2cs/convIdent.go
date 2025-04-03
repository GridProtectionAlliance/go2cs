package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) convIdent(ident *ast.Ident, context IdentContext) string {
	if ident.Name == "nil" {
		if context.isPointer {
			return "nil"
		}

		return "default!"
	}

	if context.isPointer {
		return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
	}

	if context.isType {
		return convertToCSTypeName(v.getIdentName(ident))
	}

	if context.isMethod {
		return getSanitizedFunctionName(v.getIdentName(ident))
	}

	return getSanitizedIdentifier(v.getIdentName(ident))
}
