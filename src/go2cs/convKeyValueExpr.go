package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/types"
	"strconv"
)

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr, context KeyValueContext) string {
	// For a struct field initializer whose field is a pointer type, the value must be emitted as a
	// pointer (box) — e.g. a deref'd pointer parameter `val` used as `key: val` where `key` is `*V`
	// needs `Ꮡval` (like the same value passed as a pointer call argument). Resolve the field from
	// the key name so this works for keyed literals regardless of field order.
	var valueContexts []ExprContext
	var structFieldIfaceType types.Type

	if context.source == StructSource {
		if keyIdent, ok := keyValueExpr.Key.(*ast.Ident); ok {
			if fieldObj := v.info.Uses[keyIdent]; fieldObj != nil {
				if _, isPtr := fieldObj.Type().(*types.Pointer); isPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					valueContexts = []ExprContext{identContext}
				} else if needsCast, isEmpty := isInterface(fieldObj.Type()); needsCast && !isEmpty {
					// An INTERFACE field routes its value through the interface conversion below —
					// a POINTER value (`src: &runtimeSource{}`, math/rand's Rand literal) renders
					// as the box and wraps in the pointer-interface adapter
					// (`src: new runtimeSourceᴵSource(Ꮡ(new runtimeSource()))`); without this the
					// raw box meets the interface-typed ctor parameter (CS1503).
					structFieldIfaceType = fieldObj.Type()

					if _, valIsPtr := v.getExprType(keyValueExpr.Value).(*types.Pointer); valIsPtr {
						identContext := DefaultIdentContext()
						identContext.isPointer = true
						valueContexts = []ExprContext{identContext}
					}
				}
			}
		}
	}

	valueExpr := v.convExpr(keyValueExpr.Value, valueContexts)

	if structFieldIfaceType != nil {
		valueExpr = v.convertToInterfaceType(structFieldIfaceType, v.getExprType(keyValueExpr.Value), valueExpr)
	}

	if context.ident != nil {
		keySourceType := v.getIdentType(context.ident)

		if keySourceType != nil {
			if needsInterfaceCast, isEmpty := isInterface(keySourceType); needsInterfaceCast && !isEmpty {
				valueType := v.getExprType(keyValueExpr.Value)
				valueExpr = v.convertToInterfaceType(keySourceType, valueType, valueExpr)
			}
		}
	}

	if context.source == StructSource {
		// Struct field initializer. The key is a FIELD NAME, which is struct-scoped — it must use
		// the field's DECLARED C# name (getCoreSanitizedIdentifier), NOT the package-level
		// nameCollisions `Δ`-rename that convExpr/convIdent applies. A field named like a colliding
		// package type/method (`type funcInfo` + `func (*Func) funcInfo()` → field `funcInfo` of
		// `Frame`) is declared unrenamed (`funcInfo`), so a keyed literal `Frame{funcInfo: …}` must
		// emit `funcInfo:`, not `ΔfuncInfo:` (which is not a parameter of the generated ctor → CS1739).
		key := v.convExpr(keyValueExpr.Key, nil)

		if keyIdent, ok := keyValueExpr.Key.(*ast.Ident); ok && nameCollisions[keyIdent.Name] {
			key = removeSanitizationMarker(getCoreSanitizedIdentifier(keyIdent.Name))
		}

		return fmt.Sprintf("%s: %s", key, valueExpr)
	} else if context.source == MapSource {
		// Map key/value initializer — or, when array-backed, a SparseArray index initializer
		return fmt.Sprintf("[%s] = %s", v.sparseArrayKey(keyValueExpr.Key, context), valueExpr)
	} else if context.source == ArraySource {
		// Sparse array initializer
		return fmt.Sprintf("%s[%s] = %s", context.ident, v.sparseArrayKey(keyValueExpr.Key, context), valueExpr)
	} else {
		panic(fmt.Sprintf("Unexpected key/value source: %#v", context.source))
	}
}

// sparseArrayKey converts a keyed-composite index expression. For an array-backed composite (a
// `[]T{i: v}` slice/array literal emitted as a SparseArray, int-indexed) a key whose Go type is a
// DEFINED integer type (e.g. runtime's `type lockRank int`, emitted `[GoType("num:nint")]`) is cast
// to `int`: such a type does not implicitly narrow to the indexer's int parameter (CS1503). A plain
// int/untyped-int key, or a real map key, is emitted unchanged.
func (v *Visitor) sparseArrayKey(key ast.Expr, context KeyValueContext) string {
	keyExpr := v.convExpr(key, nil)

	if !context.arrayBacked {
		return keyExpr
	}

	if keyType := v.info.TypeOf(key); keyType != nil {
		if named, ok := keyType.(*types.Named); ok {
			if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
				switch basic.Kind() {
				case types.Int8, types.Uint8, types.Int16, types.Uint16, types.Int32:
					// These widen implicitly to C# int (Int32), so the indexer accepts the
					// defined type directly — no cast needed (keeps output minimal).
				default:
					// int/int64/uint/uint32/uint64/uintptr underlyings do NOT implicitly narrow
					// to int32, so an explicit cast is required for the SparseArray int indexer.
					// A COMPOSITE key (`E2BIG - APPLICATION_ERROR`) must parenthesize, or the
					// cast binds only the first operand. A UINTPTR-backed named numeric converts
					// through TWO user-defined operators (named -> golib uintptr struct -> int),
					// which C# rejects in a single cast (syscall zerrors' Errno keys, CS0030
					// x131) - split the steps; other underlyings reach int with one user-defined
					// plus one standard conversion, so the direct form stays (no golden churn).
					_, isIdent := key.(*ast.Ident)

					// A composite CONSTANT key (`E2BIG - APPLICATION_ERROR`, syscall zerrors
					// x131) folds to its integer value: its rendering mixes named/underlying
					// operands whose operators are ambiguous (CS0034), and no cast placement
					// fixes both operands. Simple IDENT keys keep their symbolic form (visual
					// similarity - `[(int)rankLow]` stays).
					if !isIdent {
						if tv, ok := v.info.Types[key]; ok && tv.Value != nil {
							if i, exact := constant.Int64Val(constant.ToInt(tv.Value)); exact {
								return strconv.FormatInt(i, 10)
							}
						}

						// Non-constant composite: parenthesize so the cast binds the whole key.
						keyExpr = fmt.Sprintf("(%s)", keyExpr)
					}

					// A uintptr-backed named numeric converts through TWO user-defined
					// operators (named -> golib uintptr struct -> int), which C# rejects in a
					// single cast (CS0030) - split the steps, keeping the symbolic operand.
					if basic.Kind() == types.Uintptr {
						return fmt.Sprintf("(int)((%s)%s)", v.getCSTypeName(basic), keyExpr)
					}

					return fmt.Sprintf("(int)%s", keyExpr)
				}
			}
		}
	}

	return keyExpr
}
