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

// lambdaBoxRefAddressForm renders `&m` / `&m.field` when `m` is a heap-boxed local captured by-box
// inside the lambda currently being converted (see LambdaCapture.boxRefVars). The address is taken
// through the box `Ꮡm` — a capturable reference — rather than the uncapturable ref-local alias.
// Returns ("", false) when the operand is not rooted at a box-ref var, so the caller falls through
// to the normal address handling.
func (v *Visitor) lambdaBoxRefAddressForm(unaryExpr *ast.UnaryExpr) (string, bool) {
	if v.lambdaCapture == nil || !v.lambdaCapture.conversionInLambda {
		return "", false
	}

	switch operand := unaryExpr.X.(type) {
	case *ast.Ident:
		// &m → Ꮡm
		if v.isLambdaBoxRefVar(v.info.ObjectOf(operand)) {
			return AddressPrefix + strings.TrimPrefix(getSanitizedIdentifier(v.getIdentName(operand)), "@"), true
		}
	case *ast.SelectorExpr:
		// &m.field (value struct field) → Ꮡm.of(Type.ᏑField)
		baseIdent, ok := operand.X.(*ast.Ident)

		if !ok || !v.isLambdaBoxRefVar(v.info.ObjectOf(baseIdent)) {
			return "", false
		}

		if _, ok := v.getType(operand.X, true).(*types.Struct); !ok {
			return "", false
		}

		boxName := strings.TrimPrefix(getSanitizedIdentifier(v.getIdentName(baseIdent)), "@")
		typeName := v.dynamicStructTypeName(operand.X)
		fieldRef := fmt.Sprintf("%s.%s%s", typeName, AddressPrefix, removeSanitizationMarker(v.convExpr(operand.Sel, nil)))

		return fmt.Sprintf("%s%s.of(%s)", AddressPrefix, boxName, fieldRef), true
	}

	return "", false
}

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr, context UnaryExprContext) string {
	// Check if the unary expression is a pointer dereference
	if unaryExpr.Op == token.AND {
		// Inside a lambda, a captured heap-boxed local is referenced through its box (the ref-local
		// alias `ref var m = ref Ꮡm.val` can't be captured — CS8175). Build address forms from the
		// box name directly, bypassing the value-rewrite that renders the box as `Ꮡm.val`.
		if boxForm, ok := v.lambdaBoxRefAddressForm(unaryExpr); ok {
			return boxForm
		}

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

		// Address of a field of a pointer *variable* that is not the receiver: `&e.v` where `e` is
		// a `*entry` identifier. `e` is already the heap box, so field-ref through it directly —
		// `e.of(Type.ᏑField)` — without the Ꮡ(value) copy form. Restricted to a bare identifier so a
		// complex pointer expression (e.g. `&(*(*T)(unsafe.Pointer(x))).u`) keeps its existing form;
		// the receiver case is handled above and a value-struct field falls through to the struct
		// branch below.
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if baseIdent, ok := selectorExpr.X.(*ast.Ident); ok {
				if ptrType, ok := v.getType(baseIdent, false).(*types.Pointer); ok {
					if _, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
						structExpr := v.convExpr(baseIdent, nil)

						// A pointer *parameter* is deref'd to a value alias (`ref var s = ref Ꮡs.val`),
						// so its box is `Ꮡs` — field-ref through the box. A pointer *local* already
						// holds the box directly, so it is used as-is.
						if v.identIsParameter(baseIdent) {
							structExpr = AddressPrefix + structExpr
						}

						typeName := convertToCSTypeName(v.getTypeName(ptrType.Elem(), false))
						fieldRef := fmt.Sprintf("%s.%s%s", typeName, AddressPrefix, removeSanitizationMarker(v.convExpr(selectorExpr.Sel, nil)))
						return fmt.Sprintf("%s.of(%s)", structExpr, fieldRef)
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
						// When the base is itself a nested field selector or an array/slice index —
						// `&work.sweepWaiters.lock` (field of a field) or `&stackpool[i].item.mu`
						// (field of an indexed element) of a boxed global — recurse to build the base's
						// address through `.of(...)` / `.at<T>(i)` chaining, e.g.
						// `Ꮡwork.of(workType.ᏑsweepWaiters).of(…Ꮡlock)` /
						// `Ꮡstackpool.at<T>(i).of(…Ꮡmu)`. Prefixing `Ꮡ` onto `work.sweepWaiters` /
						// `stackpool[i]` instead binds to the box variable (`Ꮡwork` / `Ꮡstackpool`,
						// whose value has no such member / no indexer) → CS1061 / CS0021.
						switch selectorExpr.X.(type) {
						case *ast.SelectorExpr, *ast.IndexExpr:
							baseAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.of(%s)", baseAddr, fieldRef)
						}

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
					// For an indexed reference into an array, we use the "ж.at<T>(index)" syntax.
					csTypeName := convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:])

					// An ANONYMOUS-struct element is lifted to a synthesized name keyed by the element
					// EXPRESSION (e.g. `mheap.central[i]` → `mheap_central`), which neither the
					// array-type-name string-parse nor getCSTypeName(type) recovers — a complex element
					// (e.g. a `pad [const]byte` field) renders as a raw/malformed `struct{…}` (invalid
					// C# → masking parse error). Resolve it through dynamicStructTypeName(indexExpr),
					// the same per-expression registry the `.of(…)` field path uses.
					if arrayType, ok := exprType.Underlying().(*types.Array); ok {
						if _, isStruct := arrayType.Elem().Underlying().(*types.Struct); isStruct {
							if _, isNamed := arrayType.Elem().(*types.Named); !isNamed {
								if lifted := v.dynamicStructTypeName(indexExpr); lifted != "" {
									csTypeName = lifted
								}
							}
						}
					}

					// When the array is a FIELD of a heap-boxed value — `&trace.stackTab[i]` where
					// `trace` is an address-taken global — its address must go through the box-field
					// accessor, not a naive `Ꮡ` prefix on `trace.stackTab` (which would bind to the box
					// variable `Ꮡtrace`, whose value type has no `stackTab` → CS1061). Take the array's
					// address recursively: `Ꮡtrace.of(…ᏑstackTab).at<T>(i)`.
					if sel, ok := indexExpr.X.(*ast.SelectorExpr); ok && v.isHeapBoxedExpr(sel) {
						arrayAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: indexExpr.X}, DefaultUnaryExprContext())
						return fmt.Sprintf("%s.at<%s>(%s)", arrayAddr, csTypeName, v.convExpr(indexExpr.Index, nil))
					}

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
		// Strip a leading '@' keyword-escape: the box name is `Ꮡ`+raw (e.g. `Ꮡbase`), since
		// `Ꮡ@base` is not a valid identifier (a '@' is only valid as a leading prefix).
		if v.isAddressedGlobal(ident) {
			return AddressPrefix + strings.TrimPrefix(v.convExpr(unaryExpr.X, nil), "@")
		}

		if escapesHeap && !isInherentlyHeapAllocatedType(v.getIdentType(ident)) {
			// If the variables escapes to heap, existing pointer reference should exist
			return AddressPrefix + strings.TrimPrefix(v.convExpr(unaryExpr.X, nil), "@")
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

	// A negated integer-valued float constant used in an integer context — `math.Inf(-1.0)`,
	// where Inf takes an int — must emit the integer form (`-1`), not `-1.0D` (CS1503). The
	// inner literal alone looks like a float (the conversion happens at the unary level), so this
	// mirrors the convBasicLit FLOAT case but keyed off the unary expression's resolved type.
	if unaryExpr.Op == token.SUB {
		if lit, ok := unaryExpr.X.(*ast.BasicLit); ok && lit.Kind == token.FLOAT {
			if tv, ok := v.info.Types[unaryExpr]; ok && tv.Type != nil && tv.Value != nil {
				if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					return tv.Value.ExactString()
				}
			}
		}
	}

	if unaryExpr.Op == token.SUB && v.isUnsignedType(unaryExpr.X) {
		// C# forbids unary minus on an unsigned operand (CS0023). Go's `-x` on an
		// unsigned value is two's-complement negation that wraps mod 2^N (e.g. the
		// `x & -x` lowest-set-bit idiom in math/bits). `(T)0 - x` has identical
		// wrap-around semantics and keeps the unsigned type T.
		typeName := convertToCSTypeName(v.getTypeName(v.getType(unaryExpr.X, false), false))
		return fmt.Sprintf("((%s)0 - %s)", typeName, v.convExpr(unaryExpr.X, nil))
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}
