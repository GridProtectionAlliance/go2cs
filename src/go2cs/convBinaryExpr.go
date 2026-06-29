package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
)

// isComputedConstOperand reports whether expr is a COMPUTED constant expression — one that carries a
// constant Go value but is neither a bare literal nor a named-constant reference (a binary/unary
// expression such as `(1 << k) - 1` or `^(pageSize - 1)`). Because it can involve a non-const native
// value (e.g. a `(int)` cast), it emits as a non-constant C# expression and so does not benefit from
// C#'s implicit constant conversion — under a native-int bitwise operator it must be cast explicitly.
func (v *Visitor) isComputedConstOperand(expr ast.Expr) bool {
	if tv, ok := v.info.Types[expr]; !ok || tv.Value == nil {
		return false
	}

	// A literal or a (named-)constant reference is handled by C#'s constant conversion / the
	// named-const-ref cast; only a computed expression needs this explicit cast.
	switch expr.(type) {
	case *ast.BasicLit, *ast.Ident, *ast.SelectorExpr:
		return false
	}

	return true
}

// isUntypedNamedConstRef reports whether the expression is a reference (ident or selector) to an
// untyped numeric constant — which the converter emits as a golib `Untyped*` wrapper. It is false
// for literals (those render as ordinary C# literals and follow normal numeric promotion rules).
func (v *Visitor) isUntypedNamedConstRef(expr ast.Expr) bool {
	var sel *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		sel = e
	case *ast.SelectorExpr:
		sel = e.Sel
	default:
		return false
	}

	if constObj, ok := v.info.ObjectOf(sel).(*types.Const); ok {
		if basic, ok := constObj.Type().(*types.Basic); ok {
			return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
		}
	}

	return false
}

// isBigIntegerBackedConstRef reports whether the expression references an untyped numeric constant
// whose value does not fit int64/uint64 (or float64) and is therefore emitted as `GoUntyped`
// (= System.Numerics.BigInteger). Such a constant has no implicit operator with the built-in
// numeric types, so it must be cast even in comparisons.
func (v *Visitor) isBigIntegerBackedConstRef(expr ast.Expr) bool {
	if !v.isUntypedNamedConstRef(expr) {
		return false
	}

	var sel *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		sel = e
	case *ast.SelectorExpr:
		sel = e.Sel
	}

	constObj, ok := v.info.ObjectOf(sel).(*types.Const)

	if !ok {
		return false
	}

	switch constObj.Val().Kind() {
	case constant.Int:
		s := constObj.Val().ExactString()
		_, errUint := strconv.ParseUint(s, 0, 64)
		_, errInt := strconv.ParseInt(s, 0, 64)
		return errUint != nil && errInt != nil
	case constant.Float:
		_, err := strconv.ParseFloat(constObj.Val().String(), 64)
		return err != nil
	}

	return false
}

// isWideShiftType reports whether a C# integer type does NOT promote to `int` when used as the
// left operand of a shift — i.e. the shift is performed in that type's own width. For these,
// shifting a bare `1` (an `int` literal) overflows before any result cast, so the operand must be
// cast instead. Narrower types (uint16/byte/…) promote to `int`, so a result cast suffices.
func isWideShiftType(csType string) bool {
	switch csType {
	case "uint32", "uint64", "int64", "nuint":
		return true
	}

	return false
}

// concreteNumericCSType returns the C# name of a concrete (non-untyped) numeric type's UNDERLYING
// basic type, or "". The underlying (e.g. `uint8` for a named `Tag uint8`) is used so an
// untyped-const cast is `(uint8)c`, not `(Tag)c`: an UntypedInt wrapper converts to the basic type
// in one step, then the basic type converts to the named type implicitly — `(Tag)c` would need two
// user-defined conversions and is rejected (CS0030). Basic types are their own underlying.
func (v *Visitor) concreteNumericCSType(t types.Type) string {
	if t == nil {
		return ""
	}

	if basic, ok := t.Underlying().(*types.Basic); ok {
		if basic.Info()&types.IsNumeric != 0 && basic.Info()&types.IsUntyped == 0 {
			return convertToCSTypeName(v.getTypeName(t.Underlying(), false))
		}
	}

	return ""
}

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext) string {
	lhsType := v.getExprType(binaryExpr.X)
	rhsType := v.getExprType(binaryExpr.Y)

	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()
	basicLitContext.u8StringOK = v.isStringType(binaryExpr.X) && v.isStringType(binaryExpr.Y)

	// In a `==`/`!=` comparison, a deref'd pointer PARAMETER must compare its box `Ꮡp`, not its
	// value alias `p` (a struct value). Each operand's pointer context is otherwise taken from the
	// OTHER operand's pointer-ness, so `p != nil` (nil is not a pointer type) would convert p in
	// value form — the wrong comparison, and the start of a nil-terminated walk that then NREs at
	// its re-alias. Forcing the box form here is safe only for a pointer param: a pointer LOCAL is
	// already the box, and forcing isPointer there would emit a non-existent `Ꮡlocal` (see convIdent).
	isEqualityComparison := binaryExpr.Op == token.EQL || binaryExpr.Op == token.NEQ
	leftIsDerefdPtrParam := false
	rightIsDerefdPtrParam := false

	if isEqualityComparison {
		if ident, ok := binaryExpr.X.(*ast.Ident); ok {
			leftIsDerefdPtrParam = v.isDerefdPointerParamIdent(ident)
		}

		if ident, ok := binaryExpr.Y.(*ast.Ident); ok {
			rightIsDerefdPtrParam = v.isDerefdPointerParamIdent(ident)
		}
	}

	rhsIsPointer := isPointer(rhsType)
	identContext.isPointer = rhsIsPointer || leftIsDerefdPtrParam
	leftOperand := v.convExpr(binaryExpr.X, []ExprContext{identContext, basicLitContext})

	binaryOp := binaryExpr.Op.String()

	lhsIsPointer := isPointer(lhsType)
	identContext.isPointer = lhsIsPointer || rightIsDerefdPtrParam
	rightOperand := v.convExpr(binaryExpr.Y, []ExprContext{identContext, basicLitContext})

	if !context.usePattenMatch {
		// Check for comparisons between interface and pointer types,
		// dereferencing pointer type for the comparison if necessary
		if binaryOp == "==" || binaryOp == "!=" {
			lhsIsInterface, isEmpty := isInterface(lhsType)

			if lhsIsInterface && !isEmpty && rhsIsPointer {
				rightOperand = fmt.Sprintf("%s%s", PointerDerefOp, rightOperand)
			} else {
				rhsIsInterface, isEmpty := isInterface(rhsType)

				if rhsIsInterface && !isEmpty && lhsIsPointer {
					leftOperand = fmt.Sprintf("%s%s", PointerDerefOp, leftOperand)
				}

				if lhsIsInterface && rhsIsInterface {
					// Handle interface comparison with special runtime function
					if binaryOp == "==" {
						return fmt.Sprintf("AreEqual(%s, %s)", leftOperand, rightOperand)
					} else {
						return fmt.Sprintf("!AreEqual(%s, %s)", leftOperand, rightOperand)
					}
				}
			}
		} else if binaryOp == "<<" || binaryOp == ">>" {
			rightOperand = v.intCastOperand(binaryExpr.Y, rightOperand)

			// Go's shift operators bind at the multiplicative level (tighter than `+`/`-`),
			// but C#'s `<<`/`>>` bind looser than the additive operators. So Go's
			// `x>>4 + x` (meaning `(x>>4) + x`) would re-associate in C# as `x >> (4 + x)`.
			// Parenthesize the shift so the original grouping is preserved in every context.
			shiftExpr := fmt.Sprintf("(%s %s %s)", leftOperand, binaryOp, rightOperand)

			// When the shifted (left) operand is an untyped constant — `1 << k` — Go gives the
			// whole shift the type it assumes from context (e.g. uintptr when compared with a
			// uintptr), but the bare C# literal makes the result `int`, which then can't compare
			// or combine with the typed operand (CS0034). Re-type the shift to its resolved type.
			if v.isUntypedNumericConstArg(binaryExpr.X) {
				if shiftType := v.info.Types[binaryExpr].Type; shiftType != nil {
					if basic, ok := shiftType.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 && basic.Info()&types.IsUntyped == 0 {
						// Shift width is decided by the UNDERLYING basic type. For a NAMED numeric
						// (`type arenaIdx uint`), the result is additionally wrapped to the named type
						// *through* its underlying — `(arenaIdx)((nuint)1 << k)` — because the
						// `[GoType]` conversion only accepts its exact underlying, never C# `int`
						// (a bare `(arenaIdx)(1 << k)` is CS0030).
						underlyingCS := v.getCSTypeName(basic)
						resolvedCS := convertToCSTypeName(v.getTypeName(shiftType, false))
						isNamed := resolvedCS != underlyingCS

						if isWideShiftType(underlyingCS) {
							// A type that does not promote to `int` in a C# shift (uint/ulong/long/
							// nuint). Cast the LEFT operand so the shift is performed in that type —
							// casting the result would not help, as `1 << 63` already overflowed
							// `int` to a negative value before the cast (e.g. `(uint64)(1 << 63)`
							// → CS0221). `((uint64)1) << 63` shifts in uint64.
							shiftExpr = fmt.Sprintf("((%s)%s %s %s)", underlyingCS, leftOperand, binaryOp, rightOperand)

							if isNamed {
								shiftExpr = fmt.Sprintf("(%s)%s", resolvedCS, shiftExpr)
							}
						} else if underlyingCS != "int" && underlyingCS != "nint" {
							// A narrow type (uint16/byte/…) promotes to `int` in the shift, so the
							// computation is correct; just re-type the result (through the underlying
							// for a named type).
							shiftExpr = fmt.Sprintf("(%s)%s", underlyingCS, shiftExpr)

							if isNamed {
								shiftExpr = fmt.Sprintf("(%s)%s", resolvedCS, shiftExpr)
							}
						} else if isNamed {
							// Underlying is `int`/`nint`; a named type over it still needs the cast
							// routed through the underlying — `(arenaIdx)(nint)(1 << k)`.
							shiftExpr = fmt.Sprintf("(%s)(%s)%s", resolvedCS, underlyingCS, shiftExpr)
						}
					}
				}
			}

			return shiftExpr
		}

		bitwiseOp := binaryOp == "&" || binaryOp == "|" || binaryOp == "^" || binaryOp == "&^"

		if binaryOp == "&^" {
			binaryOp = " & ~"
		} else {
			binaryOp = " " + binaryOp + " "
		}

		// bitwise operations need to be in parentheses in C# and usually case to target type
		if bitwiseOp {
			binaryType := v.info.Types[binaryExpr].Type
			var binaryTypeName string

			if binaryType != nil {
				binaryTypeName = convertToCSTypeName(v.getTypeName(binaryType, false))
			}

			// A named untyped-const operand (UntypedInt wrapper) resolves to `int` under a
			// bitwise operator — including the `~` of `&^` — which breaks a wider context
			// (`Float64bits(f) &^ signBit`, ulong, signBit=1<<63 → `ulong & int`, CS0019). Cast
			// such an operand to the result's UNDERLYING basic type so its width is preserved (the
			// underlying, not the named type — see concreteNumericCSType; `(Tag)c` would be CS0030).
			if operandCast := v.concreteNumericCSType(binaryType); operandCast != "" {
				if v.isUntypedNamedConstRef(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)%s", operandCast, leftOperand)
				}

				if v.isUntypedNamedConstRef(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)%s", operandCast, rightOperand)
				}
			}

			// A computed untyped-constant operand — a mask like `(1 << k) - 1` or `~(pageSize - 1)`
			// — in a bitwise op with a NATIVE-int result (`nuint`/`nint`/`uintptr`) is emitted as a
			// bare C# `int` expression (it involves a non-const native value such as a cast, so it is
			// not a C# compile-time constant), so `nuint & ((1<<k)-1)` is `nuint & int` → CS0019. Cast
			// such an operand to the native result type. A bare literal is left alone (C#'s
			// constant-conversion implicitly fits it), and a named untyped-const ref is handled above.
			if binaryTypeName == "nuint" || binaryTypeName == "nint" || binaryTypeName == "uintptr" {
				if v.isComputedConstOperand(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)%s", binaryTypeName, leftOperand)
				}

				if v.isComputedConstOperand(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)%s", binaryTypeName, rightOperand)
				}
			}

			return fmt.Sprintf("(%s)(%s%s%s)", binaryTypeName, leftOperand, binaryOp, rightOperand)
		}

		// When one operand is a reference to a *named* untyped numeric constant and the other is a
		// concrete numeric type, the constant's emitted form may not interoperate with that type:
		//   - Arithmetic: a wrapper (`UntypedInt`/`UntypedFloat`) has bidirectional implicit
		//     conversions, so e.g. `q1 * two32` (ulong * UntypedInt) resolves to `int` (CS0029).
		//   - Comparison: a value too large for int64/uint64 (or float64) is emitted as `GoUntyped`
		//     (= BigInteger), which has no implicit operator with `double` etc. — `x > Two129`
		//     yields CS0019. (Wrapper comparisons resolve via the implicit conversion, so those are
		//     left alone to avoid redundant casts.)
		// Cast the untyped-const operand to the concrete operand's type. Bare literals are not
		// wrapped and follow normal C# rules, so this targets named consts only.
		var castLeft, castRight bool

		switch binaryExpr.Op {
		case token.ADD, token.SUB, token.MUL, token.QUO, token.REM:
			castLeft = v.isUntypedNamedConstRef(binaryExpr.X)
			castRight = !castLeft && v.isUntypedNamedConstRef(binaryExpr.Y)
		case token.LSS, token.LEQ, token.GTR, token.GEQ, token.EQL, token.NEQ:
			castLeft = v.isBigIntegerBackedConstRef(binaryExpr.X)
			castRight = !castLeft && v.isBigIntegerBackedConstRef(binaryExpr.Y)
		}

		if castLeft {
			if tn := v.concreteNumericCSType(rhsType); tn != "" {
				leftOperand = fmt.Sprintf("(%s)%s", tn, leftOperand)
			}
		} else if castRight {
			if tn := v.concreteNumericCSType(lhsType); tn != "" {
				rightOperand = fmt.Sprintf("(%s)%s", tn, rightOperand)
			}
		}

		return fmt.Sprintf("%s%s%s", leftOperand, binaryOp, rightOperand)
	}

	// Handle start of pattern match expression with "is" keyword:
	if context.declareIsExpr {
		if binaryOp == "==" {
			binaryOp = ""
		} else {
			binaryOp = " " + binaryOp
		}

		return fmt.Sprintf("%s is%s %s", leftOperand, binaryOp, rightOperand)
	}

	// Handle remaining pattern match expressions:
	if binaryOp == "==" {
		binaryOp = ""
	} else {
		binaryOp = binaryOp + " "
	}

	return fmt.Sprintf("%s%s", binaryOp, rightOperand)
}
