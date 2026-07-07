package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr, context IndexExprContext) string {
	var contexts []ExprContext
	var ptrDeref string

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		// Check if the type is a map and its key is an empty interface. A CONSTRAINED TYPE
		// PARAMETER with a map core (`M ~map[K]V` — the maps package) indexes through the same
		// IMap<K, V> surface (its comma-ok two-value indexer is on the interface), so it takes
		// this branch too; the concrete *types.Map check alone missed it, emitting a single-arg
		// index whose V result failed the (v, ok) deconstruction (CS8130/CS8129). Underlying:
		// a NAMED map type (dwarf's `type abbrevTable map[uint32]abbrev`) indexes through the
		// same generated map-wrapper surface — the bare assertion missed it, emitting a
		// single-value index under a (v, ok) deconstruction (CS8129 on the struct element).
		mapType, isMap := typeAndVal.Type.Underlying().(*types.Map)

		if !isMap {
			if tp, isTypeParam := types.Unalias(typeAndVal.Type).(*types.TypeParam); isTypeParam {
				if core := typeParamMapCore(tp); core != nil {
					mapType, isMap = core, true
				}
			}
		}

		if isMap {
			// A POINTER-keyed map (`map[*typeInfo]bool`, encoding/gob buildEncEngine) indexed by
			// a deref-aliased pointer parameter needs the parameter's BOX as the key — the
			// alias `ref var info = ref Ꮡinfo.Value` is the wrapper VALUE, so `building[info]`
			// passed a typeInfo where ж<typeInfo> was expected (CS1503). The isPointer ident
			// context renders the box (`building[Ꮡinfo]`), mirroring the pointer-field struct
			// initializer in convKeyValueExpr.
			_, mapKeyIsPointer := mapType.Key().(*types.Pointer)

			// The box (`Ꮡ`) key rendering applies ONLY when the key is a deref-aliased pointer
			// PARAMETER: its value alias (`ref var info = ref Ꮡinfo.Value`) is the wrapper VALUE, so
			// `m[info]` needs `m[Ꮡinfo]` to supply the `ж<T>` key. A LOCAL already holding a pointer
			// (`var tΔ1 = scan.typ`, reflect's FieldByNameFunc — tΔ1 is `ж<structType>`) IS the key
			// and must NOT get `Ꮡ`: `ᏑtΔ1` has no box accessor (CS0103 ×4). Restrict to a bare
			// parameter ident (matching the deref-alias's parameter scope).
			if mapKeyIsPointer {
				if ident, isBare := indexExpr.Index.(*ast.Ident); !isBare || !v.identIsParameter(ident) {
					mapKeyIsPointer = false
				}
			}

			// Comma-ok map access (`v, ok := m[k]`): use golib's two-value indexer
			// `m[key, ꟷ]`, which returns `(value, present)`.
			if context.isTupleResult {
				keyContexts := []ExprContext{}

				if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
					basicLitContext := DefaultBasicLitContext()
					basicLitContext.u8StringOK = false
					keyContexts = append(keyContexts, basicLitContext)
				} else if mapKeyIsPointer {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					keyContexts = append(keyContexts, identContext)
				}

				return fmt.Sprintf("%s[%s, %s]", v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, keyContexts), OverloadDiscriminator)
			}

			// Check if the key type is an empty interface
			if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
				context := DefaultBasicLitContext()
				context.u8StringOK = false
				contexts = []ExprContext{context}
			} else if mapKeyIsPointer {
				identContext := DefaultIdentContext()
				identContext.isPointer = true
				contexts = []ExprContext{identContext}
			}
		} else if _, isPtr := typeAndVal.Type.(*types.Pointer); isPtr {
			// The deref-aliased-parameter exception applies only when the base ITSELF is the
			// parameter ident (`p[i]` renders through the value alias). A pointer FIELD reached
			// through a selector — `mp.cgoCallers[0]`, where cgoCallers is `*cgoCallers` (runtime
			// proc.go) — is a real ж box and needs the `.Value` deref: the old root-ident test
			// mistook the selector's parameter ROOT for the indexed pointer and skipped it
			// (CS0021 on every named-array-wrapper box index).
			if ident, isBare := indexExpr.X.(*ast.Ident); !isBare || !v.identIsParameter(ident) {
				ptrDeref = ".Value"
			}
		}
	}

	if v.isGenericTypeArgument(indexExpr) {
		context := DefaultIdentContext()
		context.isType = true

		if len(contexts) > 0 {
			contexts = append(contexts, contexts...)
		} else {
			contexts = []ExprContext{context}
		}

		// The base (X) renders WITHOUT its own generic type arguments: this branch appends the
		// explicit `<Index>` here, so letting convSelectorExpr also append the inferred instance
		// args (the generic-function-value path) produced `pkg.Func<T><T>` (CS1525/CS0119/CS8124).
		xContext := DefaultLambdaContext()
		xContext.suppressGenericTypeArgs = true

		return fmt.Sprintf("%s%s<%s>", v.convExpr(indexExpr.X, []ExprContext{xContext}), ptrDeref, v.convExpr(indexExpr.Index, contexts))
	}

	index := v.convExpr(indexExpr.Index, contexts)

	// A STRING base indexed by a wide/unsigned integer: a string LITERAL renders as a
	// ReadOnlySpan<byte> (`"…"u8`) whose indexer takes int — a uintptr index is CS1503
	// (runtime heapdump.go's `"0123456789abcdef"[pc&15]`, pc a uintptr). Go converts any
	// integer index to int for the access; route the wide kinds (uint/uint32/uint64/
	// uintptr/int64) through the same `(int)` cast the element-address seams use. An
	// `@string` variable's indexer binds an int argument too, so the cast is safe for
	// both renders; an int/small-integer index is emitted unchanged (no churn).
	if baseType := v.getType(indexExpr.X, false); baseType != nil {
		if basic, ok := baseType.Underlying().(*types.Basic); ok && basic.Info()&types.IsString != 0 {
			// A string LITERAL base (`"…"u8`) is an int-only-indexed ReadOnlySpan<byte>, so a
			// plain Go `int` index (→ C# nint) needs the (int) cast too; an @string variable's
			// indexer accepts nint, so it keeps the wide-only cast (no churn on int indices).
			if _, isLit := indexExpr.X.(*ast.BasicLit); isLit {
				index = v.castStringLiteralIndexToInt(indexExpr.Index)
			} else {
				index = v.castWideIntegerToInt(indexExpr.Index)
			}
		}

		// A SLICE/ARRAY base indexed by a PLAIN BASIC integer kind with no implicit
		// conversion to the golib nint indexer — int64/uint/uint32/uint64 (`r.s[r.i]`,
		// bytes Reader's int64 cursor; CS1503 long→nint) — takes an explicit (nint) cast,
		// matching Go's implicit index conversion. Kinds that widen implicitly (int32,
		// int16, byte, …) are unchanged, and a NAMED index type is left alone — its own
		// conversion surface binds the indexer (a `(nint)` cast of a named-over-uintptr
		// would chain two user conversions, CS0030 — SparseArrayNamedIntKey's errno keys).
		switch baseType.Underlying().(type) {
		case *types.Slice, *types.Array:
			idxType := types.Unalias(v.getType(indexExpr.Index, false))

			if basic, isBasic := idxType.(*types.Basic); isBasic {
				switch basic.Kind() {
				case types.Uint, types.Uint32, types.Uint64, types.Uintptr, types.Int64:
					index = fmt.Sprintf("(nint)(%s)", index)
				}
			} else if named, isNamed := idxType.(*types.Named); isNamed {
				// A NAMED type over signed `int64` — internal/trace's `type ProcID int64` indexing
				// `spans[procID]` (CS1503) — has no bare path to the indexer: there is no `this[long]`
				// overload, `int64→nint` does not narrow implicitly, and `int64→ulong` (which would
				// bind `this[ulong]`) is a signed→unsigned conversion. `(nint)(x)` composes as one
				// user conversion (named→long) plus one built-in (long→nint). Every OTHER kind is
				// deliberately EXCLUDED — no churn, and casting some would even break: an UNSIGNED
				// named type binds the golib `this[ulong]` overload bare (`type kindT uint`/uint32/
				// uint64), and a nuint-backed wrapper (uint/uintptr) is CS0030 under a `(nint)` cast;
				// an int/int32/nint underlying narrows implicitly (`type rank int` stays bare).
				if ub, ok := named.Underlying().(*types.Basic); ok && ub.Kind() == types.Int64 {
					index = fmt.Sprintf("(nint)(%s)", index)
				}
			} else if tp, isTP := idxType.(*types.TypeParam); isTP && typeParamIsInteger(tp) {
				// A numeric TYPE PARAMETER index — internal/trace's `dataTable[EI ~uint64]`
				// indexing `d.dense[id]` — has no C# cast to nint (a constrained type parameter is
				// not directly convertible). Route through golib's ConvertToUInt64<T> bridge (the
				// integer-type-param conversion family, cf. rand.N's E(x)), then narrow to nint.
				index = fmt.Sprintf("(nint)(ConvertToUInt64<%s>(%s))", v.getCSTypeName(tp), index)
			}
		}
	}

	baseExpr := v.convExpr(indexExpr.X, nil)

	// A type-CONVERSION base renders as a C# cast, and postfix binds tighter than a cast — both
	// the pointer auto-deref `.Value` and the index itself would re-bind onto the cast's INNER
	// operand: Go malloc.go's `(*[2]uint64)(x)[0] = 0` emitted
	// `(ж<array<uint64>>)(uintptr)(x).Value[0]` — the `.Value` read the inner @unsafe.Pointer's
	// uintptr, then indexed a nuint (CS0021). Wrap the cast before appending. Fifth instance of
	// the cast-precedence family.
	if call, ok := indexExpr.X.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
		baseExpr = "(" + baseExpr + ")"
	}

	return fmt.Sprintf("%s%s[%s]", baseExpr, ptrDeref, index)
}

func (v *Visitor) isGenericTypeArgument(indexExpr *ast.IndexExpr) bool {
	// The index being a TYPE expression of ANY form — `T`, `*T`, `[]T`, `map[K]V`, `pkg.T` —
	// marks a generic instantiation (internal/xcoff's `saferio.SliceCap[*Section]`, a POINTER
	// type argument, which the Ident/Selector cases below miss). go/types records this on the
	// expression directly; without it the Go bracket form survived while convCallExpr also
	// appended the resolved `<...>`, emitting `SliceCap[ж<…>]<ж<…>>(…)` (CS0021/CS0119).
	if tv, ok := v.info.Types[indexExpr.Index]; ok && tv.IsType() {
		return true
	}

	switch index := indexExpr.Index.(type) {
	case *ast.Ident:
		// Check if this identifier refers to a type
		if obj := v.info.Uses[index]; obj != nil {
			_, isTypeName := obj.(*types.TypeName)
			return isTypeName
		}
	case *ast.SelectorExpr:
		// A CROSS-PACKAGE qualified type argument — `reflect.TypeFor[encoding.BinaryMarshaler]`
		// (encoding/gob). Without this the Ident-only check missed it and the generic
		// instantiation kept the Go bracket form while convCallExpr also appended the C#
		// `<...>` from info.Instances, emitting `TypeFor[encoding.BinaryMarshaler]<…>()`
		// (CS1525). The Sel resolving to a TypeName routes it through the `<T>` branch.
		if obj := v.info.Uses[index.Sel]; obj != nil {
			_, isTypeName := obj.(*types.TypeName)
			return isTypeName
		}
	}

	return false
}
