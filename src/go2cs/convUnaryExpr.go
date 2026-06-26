package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// isHeapBoxedExpr reports whether the expression refers to a variable that the
// converter has given a heap-boxed pointer companion (the "Ꮡname" form) — i.e. it
// escapes to the heap and is not an inherently heap-allocated type. Package-level
// value vars and non-escaping locals return false (their address must be taken via
// the Ꮡ(value) constructor form). Mirrors the escape check in convUnaryExpr.
func (v *Visitor) isHeapBoxedExpr(expr ast.Expr) bool {
	ident := getIdentifier(expr)

	if ident == nil {
		return false
	}

	// A package-level var whose address is taken is backed by a heap box (the
	// "Ꮡname" companion), same as an escaping local.
	if v.isAddressedGlobal(ident) {
		return true
	}

	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	if obj == nil {
		return false
	}

	return v.identEscapesHeap[obj] && !isInherentlyHeapAllocatedType(v.getIdentType(ident))
}

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr, context UnaryExprContext) string {
	// Check if the unary expression is a pointer dereference
	if unaryExpr.Op == token.AND {
		// Since pointer-based receiver functions are converted to C# as ref-based
		// extension functions, we handle these cases separately
		var recv *types.Var
		var recvName string
		var refRecv bool
		var isRecvPointer bool

		if v.currentFuncSignature != nil {
			recv = v.currentFuncSignature.Recv()

			// Check if current function is a receiver function
			if recv != nil {
				// Get the receiver type
				recvType := v.currentFuncSignature.Recv().Type()

				// Check if receiver is a pointer type
				if _, ok := recvType.(*types.Pointer); ok {
					isRecvPointer = true
					recvName = recv.Name()
				}
			}
		}

		// Address of a field of the pointer receiver: &x.field. The receiver is a
		// pointer, so the selector target's type is a pointer (not a struct) and the
		// struct-field handling below would miss it. Taking `Ꮡ(x.field)` would box a
		// copy (the ж(in T) ctor copies), so writes through the pointer would be lost
		// — e.g. sync/atomic's `func (x *Int32) Store(v) { StoreInt32(&x.v, v) }`.
		// Use the field-ref form so the pointer aliases the real field. The method is emitted
		// with the box AS its receiver (`this ж<T> Ꮡx`, see packageDirectBoxReceiverMethods),
		// so we field-ref through that parameter box (direct-ж). This replaces the older static
		// ThreadLocal capture, which is a shared static reassigned per call and races across
		// threads for distinct receivers — broken for concurrent types like sync/atomic.
		if isRecvPointer {
			if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
				if ident, ok := selectorExpr.X.(*ast.Ident); ok && ident.Name == recvName {
					recvType := recv.Type()

					if pointer, ok := recvType.(*types.Pointer); ok {
						recvType = pointer.Elem()
					}

					if _, ok := recvType.(*types.Named); ok {
						fieldRef := fmt.Sprintf("%s.%s%s", convertToCSTypeName(v.getTypeName(recvType, false)), AddressPrefix, removeSanitizationMarker(v.convExpr(selectorExpr.Sel, nil)))

						// Direct-ж: the receiver box is the parameter `Ꮡx` (see
						// packageDirectBoxReceiverMethods), so field-ref through it directly. No
						// static ThreadLocal — that form races across threads for distinct
						// receivers (broken for concurrent types like sync/atomic).
						return fmt.Sprintf("%s%s.of(%s)", AddressPrefix, recvName, fieldRef)
					}
				}
			}
		}

		// Check if the unary expression is an address of a structure field
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if _, ok := v.getType(selectorExpr.X, true).(*types.Struct); ok {
				if isRecvPointer {
					// Check if selector's target is an identifier matching the receiver name
					if ident, ok := selectorExpr.X.(*ast.Ident); ok && ident.Name == recvName {
						refRecv = true
					}
				}

				if refRecv {
					// For a receiver reference to a structure field, we use the "Ꮡ(StructType.Field)" syntax
					return fmt.Sprintf("%s(%s.%s)", AddressPrefix, v.convExpr(selectorExpr.X, nil), removeSanitizationMarker(v.convExpr(selectorExpr.Sel, nil)))
				} else {
					// For a structure field, we use the "ж.of(StructType.ᏑField)" syntax.
					// dynamicStructTypeName resolves anonymous struct types lifted in
					// another file of the package (e.g. `&cpu.X86.HasADX`).
					structExpr := v.convExpr(selectorExpr.X, nil)
					typeName := v.dynamicStructTypeName(selectorExpr.X)
					fieldRef := fmt.Sprintf("%s.%s%s", typeName, AddressPrefix, removeSanitizationMarker(v.convExpr(selectorExpr.Sel, nil)))

					if v.isHeapBoxedExpr(selectorExpr.X) {
						// The operand already has a heap-boxed pointer companion (e.g. an
						// escaping local `Ꮡs`): use the identifier form "Ꮡs.of(...)".
						return fmt.Sprintf("%s%s.of(%s)", AddressPrefix, structExpr, fieldRef)
					}

					// The operand is an addressable value with no pointer companion (e.g.
					// a package-global struct var): construct the pointer on the fly with
					// the "Ꮡ(value).of(...)" call form (mirrors the whole-value case below).
					return fmt.Sprintf("%s(%s).of(%s)", AddressPrefix, structExpr, fieldRef)
				}
			}
		}

		// Check if the unary expression is an address of an indexed array or slice
		if indexExpr, ok := unaryExpr.X.(*ast.IndexExpr); ok {
			exprType := v.getType(indexExpr.X, true)
			ident := getIdentifier(indexExpr.X)

			if ident != nil {
				if isRecvPointer {
					// Check if index target is an identifier matching the receiver name
					if ident.Name == recvName {
						refRecv = true
					}
				}

				if _, ok := exprType.Underlying().(*types.Slice); ok {
					if refRecv {
						// For a receiver reference to a slice, we use the "Ꮡ(slice[index])" syntax
						return fmt.Sprintf("%s(%s[%s])", AddressPrefix, v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
					} else {
						// For address of an indexed reference into slice we use the "Ꮡ(x, index)" syntax
						return fmt.Sprintf("%s(%s, %s)", AddressPrefix, v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
					}
				}
			}

			typeName := v.getTypeName(exprType, false)

			if strings.HasPrefix(typeName, "[") {
				if refRecv {
					// For a receiver reference to an array, we use the "Ꮡ(array[index])" syntax
					return fmt.Sprintf("%s(%s[%s])", AddressPrefix, v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
				} else {
					// For an indexed reference into an array, we use the "ж.at<T>(index)" syntax
					csTypeName := convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:])
					return fmt.Sprintf("%s%s.at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convExpr(indexExpr.Index, nil))
				}
			}
		}

		// Check if unary target is a pointer to a pointer
		if _, ok := v.getType(unaryExpr.X, true).(*types.Pointer); ok {
			return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
		}

		// Check if unary target is not a variable or a field
		if _, ok := unaryExpr.X.(*ast.Ident); !ok {
			return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
		}

		var escapesHeap bool
		ident := getIdentifier(unaryExpr.X)

		obj := v.info.Defs[ident]

		if obj == nil {
			obj = v.info.Uses[ident]
		}

		if obj != nil {
			escapesHeap = v.identEscapesHeap[obj]
		}

		// A package-level var whose address is taken is backed by a heap box, so its
		// "Ꮡname" companion already exists — reference it directly rather than boxing a copy.
		if v.isAddressedGlobal(ident) {
			return AddressPrefix + v.convExpr(unaryExpr.X, nil)
		}

		if escapesHeap && !isInherentlyHeapAllocatedType(v.getIdentType(ident)) {
			// If the variables escapes to heap, existing pointer reference should exist
			return AddressPrefix + v.convExpr(unaryExpr.X, nil)
		}

		// Otherwise, call the address of function to get a pointer reference
		return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
	}

	if unaryExpr.Op == token.ARROW {
		// Check if the unary expression is channel receive operation
		if _, ok := v.getType(unaryExpr.X, true).(*types.Chan); ok {
			var tupleResult string

			if v.options.useChannelOperators {
				if context.isTupleResult {
					tupleResult = ", " + OverloadDiscriminator
				}

				return fmt.Sprintf("%s(%s%s)", ChannelLeftOp, v.convExpr(unaryExpr.X, nil), tupleResult)
			}

			if context.isTupleResult {
				tupleResult = OverloadDiscriminator
			}

			return fmt.Sprintf("%s.Receive(%s)", v.convExpr(unaryExpr.X, nil), tupleResult)
		}
	}

	if unaryExpr.Op == token.XOR {
		return "~" + v.convExpr(unaryExpr.X, nil)
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}
