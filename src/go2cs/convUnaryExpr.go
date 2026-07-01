package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// convArrayIndex emits an array/slice index expression for the golib `ж.at<T>(nint)`
// element-address accessor. Go permits any integer type as an array/slice index and
// converts it to `int` for the access; the `at` accessor takes `nint`, but C# has no
// implicit nuint/uint/ulong→nint conversion, so a non-`int` index (e.g. a `uintptr`
// loop var, or a `uint % 2` whose C# result type widens to `long`) must be narrowed
// explicitly. An `int` index, or an untyped int constant (which renders as a plain int
// literal), needs no cast. Mirrors Go's index-to-int conversion in the emitted C#.
func (v *Visitor) convArrayIndex(index ast.Expr) string {
	expr := v.convExpr(index, nil)

	if basic, ok := v.getType(index, true).(*types.Basic); ok {
		info := basic.Info()

		if info&types.IsInteger != 0 && info&types.IsUntyped == 0 && basic.Kind() != types.Int {
			return fmt.Sprintf("(nint)(%s)", expr)
		}
	}

	return expr
}

// boxAccessorType qualifies the struct type name in a box-field accessor (`Type.Ꮡfield`, used by
// `receiver.of(Type.Ꮡfield)`) with its package class ONLY when it would otherwise be shadowed by
// the `.of()` receiver variable of the same name — pervasive in runtime, where a local is routinely
// named after its own type: `m := getg().m; &m.park` → `m.of(runtime_package.m.Ꮡpark)`; a bare
// `m.Ꮡpark` binds to the variable `m` (a `ж<m>`), which has no `Ꮡpark` (CS1061). A converted struct
// is nested in its package's static class, so the qualifier is that class (`runtime_package.m`) —
// which a same-named local cannot shadow — not a bare `m` nor `go.m` (the struct is not a direct
// member of the `go` namespace). Qualifying only on a collision keeps every other box accessor
// unchanged (no golden churn, Go-like un-namespaced form preserved). A `Ꮡ`-prefixed receiver (`Ꮡx`)
// can never equal a bare type name, so those never qualify; an already-qualified cross-package type
// (contains '.') is returned unchanged.
func boxAccessorType(typeName, receiver string) string {
	// An already-qualified cross-package type (contains '.') is left alone.
	if strings.Contains(typeName, ".") {
		return typeName
	}

	// A type whose name collides with a same-package method/function (`type funcInfo` vs
	// `func (f *Func) funcInfo()`) is declared Δ-renamed (`ΔfuncInfo`), since the package static
	// method shadows the namespace type. The box accessor must use that renamed name too — a bare
	// `funcInfo.Ꮡnfuncdata` binds to the method group (CS0119). Callers that pass the RAW Go name are
	// normalized here; callers that pass an already-sanitized name arrive Δ-prefixed already.
	if nameCollisions[typeName] {
		typeName = getCollisionAvoidanceIdentifier(typeName)
	}

	// Qualify the bare type name with its package static class (which a local variable can NEVER shadow)
	// in either case:
	//   - The type is COLLISION-renamed (Δ-prefixed): a Go local named after its type (`p`) is renamed
	//     to the SAME `Δp`, so any such local in the enclosing function shadows a bare `Δp.Ꮡfield`. At
	//     runtime malloc's `persistentalloc1`, `persistent = &mp.p.ptr().palloc` →
	//     `(~mp).p.ptr().of(Δp.Ꮡpalloc)` binds `Δp` to the local `Δp` declared just below (CS0841;
	//     CS1061 regardless). The receiver (`(~mp).p.ptr()`) is not the colliding local, so the receiver
	//     check alone misses it. A type is never shadow-renamed (types are package-level), so a
	//     Δ-prefixed accessor type is always a collision type — qualifying is always value-correct.
	//   - The type name equals the `.of()` RECEIVER variable (`m := getg().m; &m.park` →
	//     `m.of(runtime_package.m.Ꮡpark)`) — a bare `m.Ꮡpark` binds to the `ж<m>` variable `m`.
	if strings.HasPrefix(typeName, ShadowVarMarker) || typeName == receiver {
		return getSanitizedImport(packageName+PackageSuffix) + "." + typeName
	}

	return typeName
}

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

// boxBaseName returns the base name (without the `Ꮡ` prefix) of an address-taken var's heap box.
// The box name tracks how the value alias was renamed, and the two rename kinds use DIFFERENT box
// names:
//   - A type-COLLISION rename prepends the marker (`p` colliding with type `p` → `Δp`), but its box
//     keeps the RAW Go name — `ref var Δp = ref heap(new T(), out var Ꮡp)`. So a `&p` reference must
//     use the raw `Ꮡp`; `Ꮡ`+the alias (`ᏑΔp`) is not in scope (CS0103).
//   - A nested-scope SHADOW rename appends the marker + a counter (`i` → `iΔ1`, `iΔ2`), and its box
//     keeps the SHADOW name — `ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1)`. So `&i` correctly uses
//     `ᏑiΔ1` (the alias), and must be left unchanged.
//
// Only the collision form (the alias is exactly `Δ`+rawname) is rewritten to the raw name; every
// other case (no rename, shadow rename, `@`-keyword escape) keeps the existing alias-derived box
// name — no churn.
func (v *Visitor) boxBaseName(ident *ast.Ident) string {
	base := strings.TrimPrefix(getSanitizedIdentifier(v.getIdentName(ident)), "@")

	if base == ShadowVarMarker+ident.Name {
		return ident.Name
	}

	return base
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
			return AddressPrefix + v.boxBaseName(operand), true
		}
	case *ast.SelectorExpr:
		// &m.field → Ꮡm.of(Type.ᏑField). The box `Ꮡm` is the `ж<T>` to field-ref through, whether
		// `m` is a value struct (`Ꮡm` boxes the value) or a pointer-to-struct (`Ꮡm` *is* the Go
		// pointer/`ж<T>`). Both emit the same `.of(...)` form; only the type-name source differs.
		baseIdent, ok := operand.X.(*ast.Ident)

		if !ok || !v.isLambdaBoxRefVar(v.info.ObjectOf(baseIdent)) {
			return "", false
		}

		var typeName string

		switch t := v.getType(operand.X, true).(type) {
		case *types.Struct:
			typeName = v.dynamicStructTypeName(operand.X)
		case *types.Pointer:
			if _, ok := t.Elem().Underlying().(*types.Struct); !ok {
				return "", false
			}

			typeName = convertToCSTypeName(v.getTypeName(t.Elem(), false))
		default:
			return "", false
		}

		boxName := v.boxBaseName(baseIdent)
		fieldRef := fmt.Sprintf("%s.%s%s", boxAccessorType(typeName, ""), AddressPrefix, v.structFieldBoxName(operand.Sel, operand.X))

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
						fieldRef := fmt.Sprintf("%s.%s%s", boxAccessorType(convertToCSTypeName(v.getTypeName(recvType, false)), ""), AddressPrefix, v.structFieldBoxName(selectorExpr.Sel, selectorExpr.X))

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
		// a `*entry` identifier, OR `&o.h.wait` where `o.h` is a pointer-typed FIELD, OR a pointer
		// RVALUE — a pointer-returning CALL (`&getg().schedlink`) or a pointer ELEMENT index
		// (`&batch[i].schedlink`). The base is already a box `ж<T>`, so field-ref through it directly —
		// `e.of(Type.ᏑField)` / `getg().of(Type.ᏑField)` — without the `Ꮡ(value)` copy form (which boxes
		// a COPY, so writes are lost — critical for an atomic field reached through a pointer chain,
		// where the lost write silently corrupts behavior). A more complex pointer expression (a deref
		// of an `unsafe.Pointer` cast, etc.) is none of these, so it keeps the old form. The receiver
		// case is handled above and a value-struct field falls through to the struct branch below.
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			base := selectorExpr.X
			_, baseIsIdent := base.(*ast.Ident)
			_, baseIsSelector := base.(*ast.SelectorExpr)
			_, baseIsIndex := base.(*ast.IndexExpr)

			// A genuine pointer-returning CALL (`getg()`) is a postfix expression `.of(…)` chains off
			// cleanly; a type-CONVERSION CallExpr (`(*T)(p)`) renders as a C# cast (low precedence) on
			// which `.of(…)` would mis-bind, so it keeps the `Ꮡ(value)` form (S1 pointer-reinterpret).
			baseIsCall := false
			if call, ok := base.(*ast.CallExpr); ok {
				baseIsCall = !v.callExprIsTypeConversion(call)
			}

			if baseIsIdent || baseIsSelector || baseIsCall || baseIsIndex {
				if ptrType, ok := v.getType(base, false).(*types.Pointer); ok {
					if _, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
						structExpr := v.convExpr(base, nil)

						// A pointer *parameter* is deref'd to a value alias (`ref var s = ref Ꮡs.val`),
						// so its box is `Ꮡs` — field-ref through the box. A pointer *local* or a
						// pointer *field* already holds/yields the box directly, so it is used as-is.
						// The box keeps the RAW parameter name: a collision/shadow-renamed param `p`→`Δp`
						// is `ref var Δp = ref Ꮡp.val`, so its box is `Ꮡp`, not `ᏑΔp` (CS0103). boxBaseName
						// yields the raw name when shadow-renamed and the sanitized name otherwise (no churn).
						if baseIdent, ok := base.(*ast.Ident); ok && v.identIsParameter(baseIdent) {
							structExpr = AddressPrefix + v.boxBaseName(baseIdent)
						}

						typeName := convertToCSTypeName(v.getTypeName(ptrType.Elem(), false))
						fieldRef := fmt.Sprintf("%s.%s%s", boxAccessorType(typeName, structExpr), AddressPrefix, v.structFieldBoxName(selectorExpr.Sel, selectorExpr.X))
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
					fieldRef := fmt.Sprintf("%s.%s%s", boxAccessorType(typeName, ""), AddressPrefix, v.structFieldBoxName(selectorExpr.Sel, selectorExpr.X))

					// When the base is itself a VALUE field reached through a pointer field —
					// `&gp.m.mLockProfile.waitTime`, base `gp.m.mLockProfile` a value field of the
					// pointer `gp.m` — recurse to take the base's address through the pointer
					// (`gp.m.of(mType.ᏑmLockProfile)`), then field-ref this level: `.of(…ᏑwaitTime)`.
					// The same applies when the base is a value field-chain rooted at a deref-aliased
					// pointer PARAMETER/RECEIVER — `&Δp.scav.index`, base `Δp.scav` rooted at the `*pageAlloc`
					// receiver — recurse through its box `Ꮡp.of(pageAlloc.Ꮡscav)`, then `.of(…Ꮡindex)`.
					// In both cases the `Ꮡ(value)` fallback would copy the chain, losing writes (atomic
					// corruption). (The single-field root `&Δp.scav` is handled directly by the receiver /
					// param-deref branches above; only the multi-level chain needs this recursion.)
					if v.exprIsValueFieldOfPointer(selectorExpr.X) || v.exprIsValueFieldOfDerefdPointerRoot(selectorExpr.X) {
						baseAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
						return fmt.Sprintf("%s.of(%s)", baseAddr, fieldRef)
					}

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
						// For address of an indexed reference into slice we use the "Ꮡ(x, index)" syntax.
						// The golib element-address overloads take `int`/`nint`, so an unsigned/wide index
						// (`&pclntable[funcoff]`, funcoff uint32) is cast to int (CS1503 otherwise).
						return fmt.Sprintf("%s(%s, %s)", AddressPrefix, v.convExpr(indexExpr.X, nil), v.castWideIntegerToInt(indexExpr.Index))
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
					// Prefer the readable file-local package alias for the element type
					// (`.at<atomic.Int32>`) when this file imports the element's package, falling back
					// to the FULLY-QUALIFIED (namespace-rooted) form otherwise: `.at<@internal.runtime.
					// atomic_package.Pointer<…>>` resolves inside `namespace go;` without a `using
					// <pkg>` alias, which the calling file may not import (a Go file can index an
					// atomic-typed array field of a struct without ever naming the element type → no
					// `using atomic` → CS0246). getDisplayTypeName makes that choice per cross-package
					// type. A current-package or basic element renders identically (no churn).
					goFullTypeName := v.getDisplayTypeName(exprType, false)
					csTypeName := convertToCSTypeName(goFullTypeName[strings.Index(goFullTypeName, "]")+1:])

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
					// `trace` is an address-taken global — or a field of a POINTER — `&mp.future[i]`
					// where `mp` is a `*memRecord` — its address must go through the box-field accessor,
					// not a naive `Ꮡ` prefix on `mp.future` (which binds to `Ꮡ(~mp)`, whose box value
					// type has no `future` → CS1061). Take the array field's address recursively, which
					// renders `mp.of(memRecord.Ꮡfuture)` / `Ꮡtrace.of(…ᏑstackTab)`, then index it with
					// `.at<T>(i)`.
					if sel, ok := indexExpr.X.(*ast.SelectorExpr); ok {
						baseIsPointer := false

						if baseType := v.info.TypeOf(sel.X); baseType != nil {
							_, baseIsPointer = baseType.Underlying().(*types.Pointer)
						}

						if v.isHeapBoxedExpr(sel) || baseIsPointer {
							arrayAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: indexExpr.X}, DefaultUnaryExprContext())
							index := v.convArrayIndex(indexExpr.Index)

							// Combine the field-address `of(field)` with the element-address into a single
							// `base.at(field, i)` call. The combined golib overload INFERS the element type
							// from the field accessor, dropping the explicit `.at<E>(i)` type argument — so
							// `Ꮡx.of(counters.Ꮡc).at<atomic.Int32>(0)` becomes `Ꮡx.at(counters.Ꮡc, 0)`.
							// arrayAddr is the recursively-rendered field address `base.of(Type.Ꮡfield)`;
							// rewrite its trailing `.of(field)` only when the field segment is parenthesis-
							// free (a plain `Type.Ꮡfield` accessor), so the final `)` provably matches the
							// last `.of(`. Any other shape falls back to the explicit chained form.
							if ofIndex := strings.LastIndex(arrayAddr, ".of("); ofIndex != -1 && strings.HasSuffix(arrayAddr, ")") {
								field := arrayAddr[ofIndex+len(".of(") : len(arrayAddr)-1]

								if !strings.ContainsAny(field, "()") {
									base := arrayAddr[:ofIndex]
									return fmt.Sprintf("%s.at(%s, %s)", base, field, index)
								}
							}

							return fmt.Sprintf("%s.at<%s>(%s)", arrayAddr, csTypeName, index)
						}
					}

					return fmt.Sprintf("%s%s.at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convArrayIndex(indexExpr.Index))
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
			// If the variable escapes to heap, its `Ꮡname` box already exists. The box keeps the
			// RAW Go name (`ref var Δp = ref heap(new T(), out var Ꮡp)`), so reference it by the raw
			// name — `convExpr` yields the shadow-renamed value alias (`Δp`), giving `ᏑΔp` which is
			// not in scope (CS0103). boxBaseName is a no-op when nothing is shadow-renamed (no churn).
			return AddressPrefix + v.boxBaseName(ident)
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
