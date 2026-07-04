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

	// A sub-slice of a CONSTRAINED TYPE PARAMETER (`s[lo:hi]` / `s[lo:hi:max]` where s is
	// `S ~[]E`) must yield S again, sharing backing — Go's named-slice sub-slice semantics
	// (pdqsort's recursion assigns it back to S-typed values, CS0266). golib's
	// subslice<S, E>/subslice3<S, E> reconstruct via S.Wrap; the type arguments are emitted
	// explicitly (E is constraint-only — C# cannot infer it).
	if tp, ok := types.Unalias(v.info.TypeOf(sliceExpr.X)).(*types.TypeParam); ok {
		if sliceType := typeParamSliceCore(tp); sliceType != nil {
			low, high, max := "-1", "-1", "-1"

			if sliceExpr.Low != nil {
				low = v.convExpr(sliceExpr.Low, nil)
			}

			if sliceExpr.High != nil {
				high = v.convExpr(sliceExpr.High, nil)
			}

			if sliceExpr.Max != nil {
				max = v.convExpr(sliceExpr.Max, nil)
			}

			elemType := convertToCSTypeName(v.getTypeName(sliceType.Elem(), false))

			if sliceExpr.Max != nil {
				return fmt.Sprintf("subslice3<%s, %s>(%s, %s, %s, %s)", getSanitizedIdentifier(tp.Obj().Name()), elemType, ident, low, high, max)
			}

			return fmt.Sprintf("subslice<%s, %s>(%s, %s, %s)", getSanitizedIdentifier(tp.Obj().Name()), elemType, ident, low, high)
		}

		// A sub-slice of a `string | []byte` UNION-constrained value goes through the
		// IByteSeq<byte> Range indexer, which returns the INTERFACE — but Go types the result
		// as the type parameter again, so it assigns back to / passes as / returns as `bytes`
		// (time format_rfc3339, CS0266/CS0310/CS0029 ×6). C# permits the explicit interface →
		// type-parameter conversion (a runtime-checked unbox to @string / slice<byte>; the
		// slice case shares backing, matching Go). A 3-index slice cannot occur on a
		// string-including union (Go forbids it on strings), so only the range forms cast.
		if typeParamIsStringByteUnion(tp) && !sliceExpr.Slice3 {
			name := getSanitizedIdentifier(tp.Obj().Name())
			var inner string

			switch {
			case sliceExpr.Low == nil && sliceExpr.High == nil:
				inner = ident + "[..]"
			case sliceExpr.High == nil:
				inner = ident + "[" + v.getRangeIndexer(sliceExpr.Low) + "..]"
			case sliceExpr.Low == nil:
				inner = ident + "[.." + v.getRangeIndexer(sliceExpr.High) + "]"
			default:
				inner = ident + "[" + v.getRangeIndexer(sliceExpr.Low) + ".." + v.getRangeIndexer(sliceExpr.High) + "]"
			}

			return fmt.Sprintf("((%s)(%s))", name, inner)
		}
	}

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

		// A Go `(*[N]T)(ptr)[:n]` produces a `[]T` slice over the pointed-to memory. Emit the golib
		// `slice<T>` (the C# representation of every other `[]T`), NOT a bare `Span<T>`: a `Span<T>` does
		// not range as `(index, element)` tuples (CS8130 on `for i := range s`) and has no `Ꮡ(s, i)`
		// element-address (CS0411), whereas `slice<T>` supports both (it is `IArray<T>`). The slice is
		// built from a `ReadOnlySpan<T>` over the raw pointer; its constructor takes a C# `int` length,
		// and a Go `int`/`uint` bound is `nint`/`nuint` (no implicit conversion → CS1503), so getRangeIndexer
		// narrows it to `int` (through the underlying for a named numeric), leaving an int literal as-is.
		// This is the `(*[N]T)(ptr)` unsafe-cast form only (always memory-layout-dependent code); the slice
		// copies the pointed-to memory, which is self-consistent for code that only uses the resulting slice.
		return fmt.Sprintf("new slice<%s>(new ReadOnlySpan<%s>((%s*)%s, %s))", ptrType, ptrType, csPtrType, string(identRunes[prefixLength:]), v.getRangeIndexer(sliceExpr.High))
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
					// which has no slice/range members — reach its underlying `array<T>` via `.Value`
					// (`b.Value[..]`). For an ANONYMOUS array (`*[N]T` → `ref array<T> p`) the value IS the
					// `array<T>`, sliced directly. A `~` on either would deref a non-pointer (CS0023).
					if _, isNamed := ptr.Elem().(*types.Named); isNamed {
						ident += ".Value"
					}
				} else {
					// A pointer-to-array BOX (a local, field, or call result) is dereferenced first.
					// A NAMED array's deref yields the wrapper (no slice/range members) — reach its
					// underlying array<T> via `.Value`, mirroring the deref-aliased branch above
					// (runtime proc.go's `mp.cgoCallers[:cgoOff]`, cgoCallers a `*cgoCallers`).
					if _, isNamed := ptr.Elem().(*types.Named); isNamed {
						ident = "(" + PointerDerefOp + ident + ").Value"
					} else {
						ident = "(" + PointerDerefOp + ident + ")"
					}
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

	// sliceExpr[:High:Max] => sliceExpr.slice(-1, High, Max). The golib `.slice(nint low, nint high,
	// nint max)` method takes nint, so a High/Max bound of a wide integer (uintptr/uint/…) is cast to
	// int (CS1503 otherwise — runtime/mprof `stk[:b.nstk:b.nstk]` with a uintptr b.nstk).
	if sliceExpr.Low == nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(-1, " + v.castWideIntegerToInt(sliceExpr.High) + ", " + v.castWideIntegerToInt(sliceExpr.Max) + ")"
	}

	// sliceExpr[Low:High:Max] => sliceExpr.slice(Low, High, Max)
	if sliceExpr.Low != nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(" + v.castWideIntegerToInt(sliceExpr.Low) + ", " + v.castWideIntegerToInt(sliceExpr.High) + ", " + v.castWideIntegerToInt(sliceExpr.Max) + ")"
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
// castWideIntegerToInt converts an integer index/bound expression, casting it to `int` only when its
// type does not already bind an `int`/`nint` parameter — used for an element-address index (`&arr[i]` →
// `Ꮡ(arr, i)`, the golib `Ꮡ(IArray<T>, int)` / `(…, nint)` overloads) and for a 3-index slice's
// `.slice(low, high, max)` bounds (the golib method takes `nint`). Go `int` (→ C# nint) and the small
// integer types (int8/16/32, uint8/16, which implicitly widen to `int`) bind directly and are left
// uncast to avoid churn; an unsigned 32-bit-or-wider or 64-bit value (uint/uint32/uint64/uintptr/int64)
// does not implicitly convert to `int`/`nint` (CS1503) and is cast (through its underlying for a named
// numeric). Go's own slice bounds are `int`, so the `(int)` narrowing matches Go semantics.
func (v *Visitor) castWideIntegerToInt(expr ast.Expr) string {
	converted := v.convExpr(expr, nil)

	if exprType := v.getType(expr, false); exprType != nil {
		if basic, ok := exprType.Underlying().(*types.Basic); ok {
			switch basic.Kind() {
			case types.Uint, types.Uint32, types.Uint64, types.Uintptr, types.Int64:
				return v.intCastOperand(expr, converted)
			}
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

// typeParamSliceCore returns the slice CORE type of a type parameter constrained `~[]E` (all of
// the constraint's type-set terms share the []E underlying), or nil. A type parameter's
// Underlying() is its constraint INTERFACE — the core type lives in the embedded terms.
func typeParamSliceCore(tp *types.TypeParam) *types.Slice {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	var core *types.Slice

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if s, ok := et.Term(j).Type().Underlying().(*types.Slice); ok {
					core = s
				} else {
					return nil
				}
			}
		default:
			if s, ok := et.Underlying().(*types.Slice); ok {
				core = s
			}
		}
	}

	return core
}

// typeParamIsInteger reports whether every term of the type parameter's constraint type-set has
// an INTEGER underlying (`~int32 | ~int64` — rand.N's intType, reflect rangeNum's sets). Such a
// parameter routes Go conversions through golib's runtime-typed ConvertToType/ConvertToUInt64
// (no C# cast exists to/from a type parameter). Mirrors typeParamSliceCore's walk.
func typeParamIsInteger(tp *types.TypeParam) bool {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	found := false

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if basic, ok := et.Term(j).Type().Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					found = true
				} else {
					return false
				}
			}
		default:
			if basic, ok := et.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
				found = true
			} else {
				return false
			}
		}
	}

	return found
}

// typeParamIsStringByteUnion reports whether the type parameter's constraint is exactly Go's
// `string | []byte` union (in either order) — the shape golib's IByteSeq<byte> models. Used to
// widen a FUNC-LITERAL parameter of that type to the interface (a lambda has no where-clause).
func typeParamIsStringByteUnion(tp *types.TypeParam) bool {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	sawString, sawByteSlice, terms := false, false, 0

	for i := range iface.NumEmbeddeds() {
		union, ok := iface.EmbeddedType(i).(*types.Union)

		if !ok {
			return false
		}

		for j := range union.Len() {
			terms++

			switch u := union.Term(j).Type().Underlying().(type) {
			case *types.Basic:
				if u.Info()&types.IsString != 0 {
					sawString = true
				} else {
					return false
				}
			case *types.Slice:
				if basic, ok := u.Elem().Underlying().(*types.Basic); ok && basic.Kind() == types.Byte {
					sawByteSlice = true
				} else {
					return false
				}
			default:
				return false
			}
		}
	}

	return sawString && sawByteSlice && terms == 2
}

// typeParamMapCore returns the map CORE type of a type parameter constrained `~map[K]V` (all of
// the constraint's type-set terms share the map underlying), or nil. Mirrors typeParamSliceCore.
func typeParamMapCore(tp *types.TypeParam) *types.Map {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	var core *types.Map

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if m, ok := et.Term(j).Type().Underlying().(*types.Map); ok {
					core = m
				} else {
					return nil
				}
			}
		default:
			if m, ok := et.Underlying().(*types.Map); ok {
				core = m
			}
		}
	}

	return core
}
