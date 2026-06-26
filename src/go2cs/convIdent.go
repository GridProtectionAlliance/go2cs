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
			// Sanitize the name so a C# keyword identifier (e.g. an `unsafe.Pointer`
			// parameter named `new`, as in internal/runtime/atomic) is escaped to `@new`
			// rather than emitted bare, which C# parses as the `new` operator (CS1526).
			return fmt.Sprintf("%s.val", getSanitizedIdentifier(v.getIdentName(ident)))
		}

		// A direct-ж method's receiver used as a bare pointer value (e.g. `recv.field = recv`):
		// the receiver box is the parameter `Ꮡrecv`, so emit that. A value-ref receiver
		// (`this ref T recv`) has no box and its deref'd value cannot stand in for the pointer
		// — this is why such methods are marked direct-ж (see packageDirectBoxReceiverMethods).
		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && ident.Name == recvName && isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
			return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
		}

		var identEscapesHeap bool
		obj := v.info.ObjectOf(ident)

		if obj != nil {
			identEscapesHeap = v.identEscapesHeap[obj]
		}

		identType := v.getIdentType(ident)

		// Check if the identifier is not already a pointer type or is a parameter or escapes heap,
		// in these cases, we need to add the address operator to reference the pointer variable
		if _, ok := identType.(*types.Pointer); !ok || v.identIsParameter(ident) || (identEscapesHeap && !isInherentlyHeapAllocatedType(identType)) {
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
