package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	ident := v.convExpr(sliceExpr.X, nil)

	// When converting a pointer expression to a slice, we use special handling
	if isMatch, ptrType := isPointerCast(ident); isMatch && v.inFunction && sliceExpr.High != nil {
		v.useUnsafeFunc = true
		prefixLength := len(ptrType) + 5

		// Remove array type prefix from the pointer type, if present
		if strings.HasPrefix(ptrType, "array<") {
			ptrRunes := []rune(ptrType)
			ptrType = string(ptrRunes[6 : len(ptrRunes)-1])
		}

		csPtrType := ptrType

		for isMatch, ptrPtrType := isPointerExpr(ptrType); isMatch; {
			csPtrType = ptrPtrType + "*"
			prefixLength--
			isMatch = false
		}

		identRunes := []rune(ident)

		// The `Span<T>(T* ptr, int length)` constructor takes a C# `int` length; a Go `int`/`uint`
		// bound is `nint`/`nuint`, which has no implicit conversion to `int` (CS1503). getRangeIndexer
		// narrows it to `int` (through the underlying for a named numeric), and leaves an int literal
		// as-is.
		return fmt.Sprintf("new Span<%s>((%s*)%s, %s)", ptrType, csPtrType, string(identRunes[prefixLength:]), v.getRangeIndexer(sliceExpr.High))
	}

	// A slice of a POINTER-TO-ARRAY (`p[lo:hi:max]`, p of type `*[N]T`) auto-derefs in Go. The
	// box `ж<array<T>>` has no slice/range members (its underlying `array<T>` does), so operate on
	// the dereferenced array — `(~p).slice(…)` / `(~p)[..]` — instead of `p.slice(…)` (CS1929). The
	// `(*[N]T)(ptr)` pointer-CAST form is handled by the Span path above (an explicit cast, not a
	// pointer-to-array value), so it is unaffected. A deref-aliased pointer PARAMETER or RECEIVER is
	// EXCLUDED: it is emitted as the array value itself (`ref array<T> p` / `ref pageBits b`), which
	// is sliced directly — a `~` on it would deref a non-pointer (CS0023). Only a pointer-to-array
	// box (a local, a field, a call result) needs the `~`.
	if xType := v.getType(sliceExpr.X, false); xType != nil {
		if ptr, ok := xType.Underlying().(*types.Pointer); ok {
			if _, isArr := ptr.Elem().Underlying().(*types.Array); isArr {
				if v.exprIsDerefAliasedPointer(sliceExpr.X) {
					// A deref-aliased pointer PARAMETER/RECEIVER is the pointed-to value, not a box.
					// For a NAMED array type (`*pageBits` → `ref pageBits b`) that value is the wrapper,
					// which has no slice/range members — reach its underlying `array<T>` via `.val`
					// (`b.val[..]`). For an ANONYMOUS array (`*[N]T` → `ref array<T> p`) the value IS the
					// `array<T>`, sliced directly. A `~` on either would deref a non-pointer (CS0023).
					if _, isNamed := ptr.Elem().(*types.Named); isNamed {
						ident += ".val"
					}
				} else {
					// A pointer-to-array BOX (a local, field, or call result) is dereferenced first.
					ident = "(" + PointerDerefOp + ident + ")"
				}
			}
		}
	}

	// sliceExpr[:] => sliceExpr[..]
	if sliceExpr.Low == nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[..]"
	}

	// sliceExpr[Low:] => sliceExpr[Low..]
	if sliceExpr.Low != nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + "..]"
	}

	// sliceExpr[:High] => sliceExpr[..High]
	if sliceExpr.Low == nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[.." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[Low:High] => sliceExpr[Low..High]
	if sliceExpr.Low != nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + ".." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[:High:Max] => sliceExpr.slice(-1, High, Max)
	if sliceExpr.Low == nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(-1, " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	// sliceExpr[Low:High:Max] => sliceExpr.slice(Low, High, Max)
	if sliceExpr.Low != nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(" + v.convExpr(sliceExpr.Low, nil) + ", " + v.convExpr(sliceExpr.High, nil) + ", " + v.convExpr(sliceExpr.Max, nil) + ")"
	}

	expr := v.getPrintedNode(sliceExpr)
	v.showWarning("@convSliceEpr - Failed to convert 'ast.SliceExpr' format %s", expr)
	return fmt.Sprintf("/* %s */", expr)
}

func (v *Visitor) getRangeIndexer(expr ast.Expr) string {
	if isIntegerLiteral(expr) {
		return v.convExpr(expr, nil)
	}

	return v.intCastOperand(expr, v.convExpr(expr, nil))
}

// intCastOperand wraps an already-converted expression in a C# `(int)` cast (for a slice bound, a
// shift count, …). When the operand's Go type is a NAMED numeric type (a `[GoType]` struct over a
// basic), a direct `(int)(x)` is CS0030 — the generated struct only converts to its OWN underlying
// basic — so it casts through the underlying first: `(int)(nuint)(x)`. A plain basic operand keeps
// the bare `(int)(x)` form (no churn).
// castElemAddrIndex converts an element-address index (`&arr[i]` → `Ꮡ(arr, i)`), casting it to
// `int` only when its type does not already bind the golib `Ꮡ(IArray<T>, int)` / `(…, nint)`
// overloads. Go `int` (→ C# nint) and the small integer types (int8/16/32, uint8/16, which
// implicitly widen to `int`) bind directly and are left uncast to avoid churn; an unsigned
// 32-bit-or-wider or 64-bit index (uint/uint32/uint64/uintptr/int64) does not implicitly convert
// to `int`/`nint` (CS1503) and is cast (through its underlying for a named numeric).
func (v *Visitor) castElemAddrIndex(expr ast.Expr) string {
	converted := v.convExpr(expr, nil)

	if basic, ok := v.getType(expr, false).Underlying().(*types.Basic); ok {
		switch basic.Kind() {
		case types.Uint, types.Uint32, types.Uint64, types.Uintptr, types.Int64:
			return v.intCastOperand(expr, converted)
		}
	}

	return converted
}

func (v *Visitor) intCastOperand(expr ast.Expr, converted string) string {
	if named, ok := v.getType(expr, false).(*types.Named); ok {
		if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
			return fmt.Sprintf("(int)(%s)(%s)", v.getCSTypeName(basic), converted)
		}
	}

	return fmt.Sprintf("(int)(%s)", converted)
}

func isIntegerLiteral(expr ast.Expr) bool {
	if basicLit, ok := expr.(*ast.BasicLit); ok {
		if basicLit.Kind == token.INT {
			return true
		}
	}

	return false
}

func isPointerCast(expr string) (bool, string) {
	if strings.HasPrefix(expr, "(") {
		runes := []rune(expr)

		if isMatch, ptrType := isPointerExpr(string(runes[1:])); isMatch {
			return true, ptrType
		}
	}

	return false, ""
}

func isPointerExpr(expr string) (bool, string) {
	runes := []rune(expr)

	// Check if it starts with the expected prefix
	if len(runes) < 2 || string(runes[0:2]) != "ж<" {
		return false, ""
	}

	// Initialize variables
	bracketCount := 1 // We've already encountered one opening bracket
	startPos := 2     // Start after "ж<"

	// Scan through the runes to find the matching closing bracket
	for i := startPos; i < len(runes); i++ {
		if runes[i] == '<' {
			bracketCount++
		} else if runes[i] == '>' {
			bracketCount--

			if bracketCount == 0 {
				// Found the closing bracket for our initial '<'
				return true, string(runes[startPos:i])
			}
		}
	}

	return false, ""
}
