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
		// A reference to a function-local (lifted) named type must use the lifted C# name
		// (`makePoint_point`), not the bare Go name — e.g. a composite literal `point{…}` or a
		// cast `point(x)`. Only lifted/anonymous types are in liftedTypeMap, so package-level
		// types are unaffected.
		if identType := v.getIdentType(ident); identType != nil {
			if liftedName, ok := v.liftedTypeMap[identType]; ok {
				return liftedName
			}
		}

		return convertToCSTypeName(v.getIdentName(ident))
	}

	if context.isMethod {
		name := getSanitizedFunctionName(v.getIdentName(ident))

		// A field whose name equals its enclosing struct's type name is renamed with the
		// disambiguation marker (CS0542); match the renamed declaration at the access site.
		if context.fieldCollidesWithType {
			name = typeCollidingFieldName(name)
		}

		return name
	}

	// A heap-boxed local captured by-box inside a lambda is read through its box: the ref-local
	// alias `ref var m = ref Ꮡm.val` can't be captured by the closure (CS8175), so a value use must
	// deref the box directly (`Ꮡm.val`). The box `Ꮡm` is a capturable reference. Address uses
	// (`&m`, `&m.field`) are rendered from the box name in convUnaryExpr, bypassing this rewrite.
	if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda && v.isLambdaBoxRefVar(v.info.ObjectOf(ident)) {
		return AddressPrefix + v.lambdaBoxBase(ident) + ".val"
	}

	return getSanitizedIdentifier(v.getIdentName(ident))
}
