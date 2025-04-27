package main

import (
	"fmt"
	"go/ast"
	"go/types"
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
		// Check if the identifier is an unsafe pointer
		if basic, ok := v.getIdentType(ident).(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
			return fmt.Sprintf("%s.val", v.getIdentName(ident))
		}

		var identEscapesHeap bool
		obj := v.info.ObjectOf(ident)

		if obj != nil {
			identEscapesHeap = v.identEscapesHeap[obj]
		}

		// Check if the identifier is not already a pointer type or is a parameter or escapes heap,
		// in these cases, we need to add the address operator to reference the pointer variable
		if _, ok := v.getIdentType(ident).(*types.Pointer); !ok || v.identIsParameter(ident) || identEscapesHeap {
			return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
		}
	}

	if context.isType {
		return convertToCSTypeName(v.getIdentName(ident))
	}

	if context.isMethod {
		return getSanitizedFunctionName(v.getIdentName(ident))
	}

	return getSanitizedIdentifier(v.getIdentName(ident))
}
