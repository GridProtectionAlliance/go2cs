package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"strconv"
	"strings"
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

// containsUntypedNamedConstRef reports whether any subexpression is a reference to a named
// untyped numeric constant (see isUntypedNamedConstRef) — used to distinguish a computed
// constant operand that renders with a golib `Untyped*` wrapper somewhere inside
// (`4 * goarch.PtrSize`) from pure-literal constant arithmetic (`2 * 3`), which needs no cast.
func (v *Visitor) containsUntypedNamedConstRef(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if e, ok := n.(ast.Expr); ok && v.isUntypedNamedConstRef(e) {
			found = true
			return false
		}

		return true
	})

	return found
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

// isLargeIntLiteralOperand reports whether expr is an integer literal whose value falls OUTSIDE the
// C# int32 range. convBasicLit emits such a literal with a `(nint)`/unsigned cast (it no longer fits
// a bare `int`), which makes it a non-`int` operand — so under a native-int bitwise operator
// (`uintptrMask & 0x00ffffffffff`) it is `nuint & nint` → CS0019, like a computed-const operand. An
// in-range literal (`x & 7`) is emitted bare and C#'s constant conversion fits it, so it is excluded.
func (v *Visitor) isLargeIntLiteralOperand(expr ast.Expr) bool {
	lit, ok := expr.(*ast.BasicLit)

	if !ok || lit.Kind != token.INT {
		return false
	}

	if tv, ok := v.info.Types[expr]; ok && tv.Value != nil {
		if i, exact := constant.Int64Val(tv.Value); exact {
			return i > math.MaxInt32 || i < math.MinInt32
		}

		// Value exceeds int64 (a large unsigned literal) — also outside int32.
		return true
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
			// A NAMED type over uintptr takes the cast to the NAMED type, not the underlying:
			// golib `uintptr` is a STRUCT with its own operators plus implicit conversions to
			// nuint, so `Errno - (uintptr)c` leaves multiple built-in candidates (CS0034 —
			// syscall Errno.Error). The named wrapper's UintptrBridgeOperators provide the
			// one-step UntypedInt conversion (`(Errno)APPLICATION_ERROR`), and the named type's
			// single predefined-type conversion (nuint) then binds unambiguously. Non-uintptr
			// named types keep the underlying cast (`(Tag)c` would be CS0030 — see above).
			if _, isNamed := t.(*types.Named); isNamed && basic.Kind() == types.Uintptr {
				return convertToCSTypeName(v.getTypeName(t, false))
			}

			return convertToCSTypeName(v.getTypeName(t.Underlying(), false))
		}
	}

	return ""
}

// overflowingConstLiteral reports the folded literal form of a SIGNED compile-time integer constant
// expression whose value falls OUTSIDE the C# int32 range, or "" otherwise. C# evaluates an operator
// expression like `1 << 63` or `12345 * 1000000000` in int32 and overflows at compile time in checked
// mode (CS0220), whereas Go evaluates the constant in its (64-bit) type; emitting the folded value
// (`9223372036854775807L`) computes it correctly. Scope notes:
//   - Restricted to SIGNED targets: an unsigned constant shift already emits with a width-cast operand
//     (`(uint64)1 << 40`) from the named-numeric path, so folding it would needlessly lose the readable
//     `1<<40` form. The signed builtin-int64 path is the one that lacks the width cast and overflows.
//   - In-range constants return "" so ordinary constant arithmetic keeps its readable operator form;
//     this only fires on the overflow cases.
func (v *Visitor) overflowingConstLiteral(expr ast.Expr) string {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return ""
	}

	basic, ok := tv.Type.Underlying().(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 {
		return ""
	}

	val := constant.ToInt(tv.Value)

	if val.Kind() != constant.Int {
		return ""
	}

	if basic.Info()&types.IsUnsigned != 0 {
		// UNSIGNED targets normally keep the readable operator form: a TYPED unsigned constant
		// shift emits with a width-cast operand (`(uint64)1 << 40`) from the retype path below,
		// an int64-range untyped constant subtree is folded by the SIGNED arm when recursion
		// reaches it (`(281474976710655L)` in runtime mranges), and a named-const reference
		// renders via its Untyped* wrapper. The one unfixable shape is an untyped constant
		// OPERATOR subtree whose value exceeds int64 entirely (`1 << 63` nested inside
		// `(1 << 63) - 1` — go/types lands the uint64 conversion on the outermost node, so the
		// inner shift stays untyped, no width cast reaches it, and C# computes it in int32:
		// math/rand Int63n, CS0220). Fold exactly those trees, and only under a plain uint64
		// (a native-width uintptr/nuint target would need a further cast the fold cannot
		// safely synthesize — that pre-existing caveat keeps its visible error).
		if v.getCSTypeName(basic) != "uint64" || !v.constExprHasBeyondInt64UntypedOperatorSubexpr(expr) {
			return ""
		}

		u, exact := constant.Uint64Val(val)

		if !exact {
			return ""
		}

		return strconv.FormatUint(u, 10) + "UL"
	}

	i, exact := constant.Int64Val(val)

	if !exact || (i >= math.MinInt32 && i <= math.MaxInt32) {
		return ""
	}

	return strconv.FormatInt(i, 10) + "L"
}

// constExprHasBeyondInt64UntypedOperatorSubexpr reports whether any PROPER subexpression of the
// constant expression is an UNTYPED constant OPERATOR computation (a BinaryExpr — an inner
// `1 << 63`) whose value exceeds int64 — the one shape no other mechanism can rescue: the signed
// fold needs int64 range, the width-cast retype needs a TYPED shift, and named-const references
// (idents/selectors, any range) render via their Untyped* wrappers. C# would evaluate such a
// subtree in int32 before any enclosing cast could widen it.
func (v *Visitor) constExprHasBeyondInt64UntypedOperatorSubexpr(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		e, ok := n.(*ast.BinaryExpr)

		if !ok || ast.Expr(e) == expr {
			return true
		}

		tv, ok := v.info.Types[e]

		if !ok || tv.Value == nil || tv.Type == nil {
			return true
		}

		basic, ok := tv.Type.(*types.Basic)

		if !ok || basic.Info()&types.IsUntyped == 0 {
			return true
		}

		val := constant.ToInt(tv.Value)

		if val.Kind() != constant.Int {
			return true
		}

		// Fits int64 → the signed fold handles it when recursion reaches the subtree.
		if _, exact := constant.Int64Val(val); exact {
			return true
		}

		found = true
		return false
	})

	return found
}

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext, litContext BasicLitContext) string {
	// A constant operator expression whose value overflows int32 must emit as the folded 64-bit
	// literal — C# would compute the operators in int32 and overflow at compile time (CS0220).
	if lit := v.overflowingConstLiteral(binaryExpr); lit != "" {
		return lit
	}

	lhsType := v.getExprType(binaryExpr.X)
	rhsType := v.getExprType(binaryExpr.Y)

	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()
	// Honor an INCOMING u8 suppression (an object/interface vararg argument context, e.g. print's
	// `object[]`) for a string CONCAT only: a literal-concat argument — print's newline+tab join
	// in runtime stack.go's newstack diagnostics — otherwise rendered BOTH halves as `"…"u8`
	// spans; a ReadOnlySpan<byte> cannot box to object (CS1503), and span + span has no
	// operator. With suppression the operands render as plain C# strings, whose `+` and boxing
	// are fine. Gated to token.ADD: comparisons and other binary kinds under a suppressed
	// context (case-when clauses, equality inside varargs — `major == "0"u8`) keep their own
	// operand rendering, which already binds golib's span-aware operators (a first-cut
	// suppression of ALL kinds churned 212 stdlib files and risked @string↔string operator
	// re-binding). The default context has u8StringOK=true, so every other path is unchanged.
	concatSuppressed := !litContext.u8StringOK && binaryExpr.Op == token.ADD
	basicLitContext.u8StringOK = !concatSuppressed && v.isStringType(binaryExpr.X) && v.isStringType(binaryExpr.Y)

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

	// A pointer comparand puts the OTHER operand in pointer (box) context — but not when that
	// operand is itself an INTERFACE: `v == dequeueNil(nil)` (sync/poolqueue; v is `any`, the
	// comparand a defined pointer type) must compare v's held VALUE, and the box form emitted a
	// nonexistent `Ꮡv` (CS0103).
	lhsIsInterfaceType, _ := isInterface(lhsType)
	rhsIsInterfaceType, _ := isInterface(rhsType)

	// An ERASED pointer-core type parameter ([P *T]) reads as an interface (its underlying is
	// the constraint) but IS a pointer (ж<T>): without this the interface exclusion below
	// suppressed the box form for `p == nil`, comparing the deref'd value alias against
	// `default!` (CS0019/CS8761 on an unconstrained T).
	if v.typeIsErasedPointerCore(lhsType) {
		lhsIsInterfaceType = false
	}

	if v.typeIsErasedPointerCore(rhsType) {
		rhsIsInterfaceType = false
	}

	// Go's ONLY legal map comparison is against nil. On a CONSTRAINED map type parameter
	// (`m == nil` — maps.Clone's nil-preserve guard) no operator exists (CS8761); the
	// IMap.IsNil property carries the exact check (backing-store null — an allocated empty
	// map is NOT nil).
	if isEqualityComparison {
		nilCompareOperand := func(operand ast.Expr, other ast.Expr) (string, bool) {
			if ident, ok := other.(*ast.Ident); !ok || ident.Name != "nil" {
				return "", false
			}

			if tp, ok := types.Unalias(v.info.TypeOf(operand)).(*types.TypeParam); ok && typeParamMapCore(tp) != nil {
				return v.convExpr(operand, nil), true
			}

			return "", false
		}

		operandExpr, isMapNilCompare := nilCompareOperand(binaryExpr.X, binaryExpr.Y)

		if !isMapNilCompare {
			operandExpr, isMapNilCompare = nilCompareOperand(binaryExpr.Y, binaryExpr.X)
		}

		if isMapNilCompare {
			if binaryExpr.Op == token.NEQ {
				return fmt.Sprintf("!%s.IsNil", operandExpr)
			}

			return fmt.Sprintf("%s.IsNil", operandExpr)
		}
	}

	// An erased pointer-core operand counts as a pointer comparand too (a P-typed LOCAL holds
	// the box directly, so the other operand takes the box form exactly as against a `*T`).
	rhsIsPointer := isPointer(rhsType) || v.typeIsErasedPointerCore(rhsType)
	identContext.isPointer = (rhsIsPointer || leftIsDerefdPtrParam) && !lhsIsInterfaceType
	leftOperand := v.convExpr(binaryExpr.X, []ExprContext{identContext, basicLitContext})

	binaryOp := binaryExpr.Op.String()

	lhsIsPointer := isPointer(lhsType) || v.typeIsErasedPointerCore(lhsType)
	identContext.isPointer = (lhsIsPointer || rightIsDerefdPtrParam) && !rhsIsInterfaceType
	rightOperand := v.convExpr(binaryExpr.Y, []ExprContext{identContext, basicLitContext})

	// A binary op between an integer-constrained TYPE PARAMETER and an untyped CONSTANT —
	// `n <= 0` in rand.N[Int intType] — leaves the constant as C# `int`, which the lifted
	// IComparisonOperators<Int, Int, bool> does not accept (CS0019). Go types the constant AS
	// the type parameter; materialize it via golib ConvertToType<Int>(…).
	wrapTypeParamConst := func(operandType types.Type, other ast.Expr, otherOperand string) string {
		tp, ok := types.Unalias(operandType).(*types.TypeParam)

		if !ok || !typeParamIsInteger(tp) {
			return otherOperand
		}

		if tv, ok := v.info.Types[other]; ok && tv.Value != nil {
			if otherType := v.info.TypeOf(other); otherType != nil {
				if basic, isBasic := otherType.(*types.Basic); types.Identical(otherType, tp) || (isBasic && basic.Info()&types.IsUntyped != 0) {
					return fmt.Sprintf("ConvertToType<%s>(%s)", v.getCSTypeName(tp), otherOperand)
				}
			}
		}

		return otherOperand
	}

	// A SHIFT count is typed independently in Go (never as the left operand's type parameter),
	// and the emission coerces it to int — wrapping it would cast ConvertToType<E>(1) to int
	// (CS0030). Only non-shift operators take the const materialization.
	if binaryExpr.Op != token.SHL && binaryExpr.Op != token.SHR {
		rightOperand = wrapTypeParamConst(lhsType, binaryExpr.Y, rightOperand)
	}

	leftOperand = wrapTypeParamConst(rhsType, binaryExpr.X, leftOperand)

	if !context.usePattenMatch {
		// Check for comparisons between interface and pointer types. Go compares an interface
		// against a pointer by POINTER IDENTITY (the interface holds the *T); the interface value
		// is a generated IжAdapter wrapping the receiver box, and AreEqual unwraps adapters to
		// compare box identity. The old deref form (`iface == ~p`) boxed a COPY of the pointed-to
		// value — wrong identity semantics, and CS0019 once the adapter replaced the partial-struct
		// comparison operators (InterfaceImplementation's `zoo[0] == f`).
		if binaryOp == "==" || binaryOp == "!=" {
			lhsIsInterface, isEmpty := isInterface(lhsType)
			rhsIsInterface, rhsIsEmpty := isInterface(rhsType)

			// A CONCRETE comparable value against a non-empty interface (`err ==
			// ERROR_ENVVAR_NOT_FOUND`, error vs the Errno named numeric; syscall CS0019 x12)
			// also routes through AreEqual: Go compares the interface's dynamic type+value,
			// which AreEqual's boxed same-type value compare reproduces - no operator between
			// the interface and the implementing struct exists (user-defined operators must be
			// public, which an internal-scoped pair cannot declare). Excludes nil (the
			// dedicated nil arms above), pointers (box identity below), and interfaces.
			concreteOperand := func(t types.Type) bool {
				if t == nil || isPointer(t) {
					return false
				}

				if basic, ok := t.Underlying().(*types.Basic); ok && basic.Kind() == types.UntypedNil {
					return false
				}

				iface, _ := isInterface(t)
				return !iface
			}

			if (lhsIsInterface && !isEmpty && rhsIsPointer) || (rhsIsInterface && !rhsIsEmpty && lhsIsPointer) || (lhsIsInterface && rhsIsInterface) ||
				(lhsIsInterface && concreteOperand(rhsType)) || (rhsIsInterface && concreteOperand(lhsType)) {
				// Handle interface comparison with special runtime function
				if binaryOp == "==" {
					return fmt.Sprintf("AreEqual(%s, %s)", leftOperand, rightOperand)
				}

				return fmt.Sprintf("!AreEqual(%s, %s)", leftOperand, rightOperand)
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
							// A LEFT operand that leads with a unary +/- (`-1 << bits`, archive/tar,
							// debug/dwarf) must be parenthesized: `(int64)-1` is parsed as the type
							// `int64` MINUS `1` (CS0119), not a cast, because the width type is a
							// using-ALIAS (int64=long, …) not a C# keyword. `(int64)(-1)` is unambiguous.
							castLeftOperand := leftOperand

							if strings.HasPrefix(leftOperand, "-") || strings.HasPrefix(leftOperand, "+") {
								castLeftOperand = "(" + leftOperand + ")"
							}

							shiftExpr = fmt.Sprintf("((%s)%s %s %s)", underlyingCS, castLeftOperand, binaryOp, rightOperand)

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

			// The same for a NAMED-numeric result — `tp & (1<<taggedPointerBits - 1)` (runtime
			// tagptr_64bit.go, taggedPointer over uint64): the computed mask renders as an
			// UntypedInt-typed C# expression, and `taggedPointer & int` has no operator (CS0019).
			// Cast the computed operand to the result's UNDERLYING basic (`(uint64)(1<<bits - 1)`
			// — the named type itself would be CS0030), after which the named operand's implicit
			// underlying conversion binds. Gated tightly to an ARITHMETIC-shaped constant operand
			// containing a named untyped-const ref: a BITWISE-shaped constant operand (`m0 & m`,
			// `a | b`) is recursively emitted with its own concrete `(type)(…)` wrap and casting
			// again just doubles it; pure-literal masks convert as C# constants and need no cast.
			arithConstOperand := func(operand ast.Expr) bool {
				for {
					paren, ok := operand.(*ast.ParenExpr)

					if !ok {
						break
					}

					operand = paren.X
				}

				binary, ok := operand.(*ast.BinaryExpr)

				if !ok {
					return false
				}

				switch binary.Op {
				case token.ADD, token.SUB, token.MUL, token.QUO, token.REM:
				default:
					return false
				}

				return v.isComputedConstOperand(operand) && v.containsUntypedNamedConstRef(operand)
			}

			if operandCast := v.concreteNumericCSType(binaryType); operandCast != "" &&
				binaryTypeName != "nuint" && binaryTypeName != "nint" && binaryTypeName != "uintptr" {
				if arithConstOperand(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)(%s)", operandCast, leftOperand)
				}

				if arithConstOperand(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)(%s)", operandCast, rightOperand)
				}
			}

			// A computed untyped-constant operand — a mask like `(1 << k) - 1` or `~(pageSize - 1)`
			// — in a bitwise op with a NATIVE-int result (`nuint`/`nint`/`uintptr`) is emitted as a
			// bare C# `int` expression (it involves a non-const native value such as a cast, so it is
			// not a C# compile-time constant), so `nuint & ((1<<k)-1)` is `nuint & int` → CS0019. Cast
			// such an operand to the native result type. A bare literal is left alone (C#'s
			// constant-conversion implicitly fits it), and a named untyped-const ref is handled above.
			if binaryTypeName == "nuint" || binaryTypeName == "nint" || binaryTypeName == "uintptr" {
				if v.isComputedConstOperand(binaryExpr.X) || v.isLargeIntLiteralOperand(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)%s", binaryTypeName, leftOperand)
				}

				// The right operand of `&^` is complemented (the operator was rendered `& ~`); `~`
				// promotes to `int`, so a CONSTANT operand `p &^ 15` becomes `nuint & ~15` =
				// `nuint & (int)-16` → CS0019 (a negative `int` does not convert to an unsigned native
				// type, even as a constant). Cast the complemented constant to the native type so the
				// complement is performed in that width (`& ~(uintptr)15`). A non-constant native
				// operand (`p &^ mask`) already complements correctly and is left alone.
				yConst := false

				if tv, ok := v.info.Types[binaryExpr.Y]; ok && tv.Value != nil {
					yConst = true
				}

				andNotConst := binaryExpr.Op == token.AND_NOT && yConst

				if v.isComputedConstOperand(binaryExpr.Y) || v.isLargeIntLiteralOperand(binaryExpr.Y) || andNotConst {
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
		// wrapped and follow normal C# rules, so this targets named consts — bare (`q1 * two32`)
		// or inside a COMPUTED constant operand (`arg0 + 4*goarch.PtrSize`, runtime stkframe.go:
		// the product renders as an UntypedInt-typed expression, so the whole sum types as
		// UntypedInt and breaks a following conversion or inference; pure-literal arithmetic
		// like `x + 2*3` has no wrapper and is left alone).
		var castLeft, castRight bool

		untypedConstOperand := func(operand ast.Expr) bool {
			if v.isUntypedNamedConstRef(operand) {
				return true
			}

			// A Go CONVERSION operand (`float64(1<<x) / 1e9`) already renders concretely
			// typed — casting again would just double the cast.
			if call, ok := operand.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
				return false
			}

			return v.isComputedConstOperand(operand) && v.containsUntypedNamedConstRef(operand)
		}

		switch binaryExpr.Op {
		case token.ADD, token.SUB, token.MUL, token.QUO, token.REM:
			castLeft = untypedConstOperand(binaryExpr.X)
			castRight = !castLeft && untypedConstOperand(binaryExpr.Y)
		case token.LSS, token.LEQ, token.GTR, token.GEQ, token.EQL, token.NEQ:
			castLeft = v.isBigIntegerBackedConstRef(binaryExpr.X)
			castRight = !castLeft && v.isBigIntegerBackedConstRef(binaryExpr.Y)
		}

		if castLeft {
			if tn := v.concreteNumericCSType(rhsType); tn != "" {
				if v.needsParentheses(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)(%s)", tn, leftOperand)
				} else {
					leftOperand = fmt.Sprintf("(%s)%s", tn, leftOperand)
				}
			}
		} else if castRight {
			if tn := v.concreteNumericCSType(lhsType); tn != "" {
				if v.needsParentheses(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)(%s)", tn, rightOperand)
				} else {
					rightOperand = fmt.Sprintf("(%s)%s", tn, rightOperand)
				}
			}
		}

		// Go's `&&`/`||` on a NAMED boolean type (`type boolVal bool`) yields that named type,
		// which satisfies an interface return (`case boolVal: return x && y` in go/constant's
		// BinaryOp, returned as the `Value` interface). In C# the `[GoType("bool")]` struct that
		// models the named type has no logical operators, so `left && right` collapses to a bare
		// `bool` — returning it where the named type/interface is expected is CS0029. Cast each
		// operand to `bool`, apply the operator, then cast back to the named type so the result
		// keeps satisfying the interface. Mirrors the unary `!` handling in convUnaryExpr; a
		// predeclared-`bool` result keeps the bare form below (no golden churn).
		if (binaryExpr.Op == token.LAND || binaryExpr.Op == token.LOR) && v.isNamedBooleanType(binaryExpr) {
			typeName := convertToCSTypeName(v.getTypeName(v.getType(binaryExpr, false), false))
			return fmt.Sprintf("((%s)((bool)%s %s (bool)%s))", typeName, leftOperand, binaryOp, rightOperand)
		}

		return fmt.Sprintf("%s%s%s", leftOperand, binaryOp, rightOperand)
	}

	// Handle start of pattern match expression with "is" keyword. Go's `!=` has no C# pattern
	// operator spelling — `x is != y` is a parse error (CS1525); the negation pattern
	// `x is not y` carries the exact semantics. Relational ops (<, <=, >, >=) map verbatim.
	if context.declareIsExpr {
		switch binaryOp {
		case "==":
			binaryOp = ""
		case "!=":
			binaryOp = " not"
		default:
			binaryOp = " " + binaryOp
		}

		return fmt.Sprintf("%s is%s %s", leftOperand, binaryOp, rightOperand)
	}

	// Handle remaining pattern match expressions (an `or`-chained subsequent case value):
	switch binaryOp {
	case "==":
		binaryOp = ""
	case "!=":
		binaryOp = "not "
	default:
		binaryOp = binaryOp + " "
	}

	return fmt.Sprintf("%s%s", binaryOp, rightOperand)
}
