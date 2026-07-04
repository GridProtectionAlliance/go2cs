package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// typeCollidingFieldName renames a struct field whose C# name would equal its enclosing
// struct's type name (C# forbids it — CS0542) by prefixing the disambiguation marker.
func typeCollidingFieldName(name string) string {
	return ShadowVarMarker + name
}

// fieldCollidesWithType reports whether a field selector's name equals the C# type name of the
// struct it belongs to (`type Node struct{ Node *Node }` → field `Node` in struct `Node`).
func (v *Visitor) fieldCollidesWithType(sel *ast.Ident, x ast.Expr) bool {
	xType := v.info.TypeOf(x)

	if xType == nil {
		return false
	}

	if ptr, ok := xType.Underlying().(*types.Pointer); ok {
		xType = ptr.Elem()
	}

	named, ok := xType.(*types.Named)

	if !ok {
		return false
	}

	// Only a package-level named type keeps its Go name as the C# type name. A function-local
	// (or otherwise lifted) type is emitted under a qualified name (e.g. `Uncommon_u`), so its
	// field does not collide in C# even when the Go field and type names match — and
	// visitStructType only renames the field declaration in the package-level case.
	obj := named.Obj()

	if obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope() {
		return false
	}

	return getSanitizedIdentifier(sel.Name) == getSanitizedIdentifier(obj.Name())
}

// structFieldBoxName returns the member name for a struct field's box accessor (`Type.Ꮡ<member>`),
// matching the field's DECLARED C# name (visitStructType uses getCoreSanitizedIdentifier plus the
// type-colliding rename) and the TypeGenerator's `Ꮡ<member>` static. It deliberately does NOT apply
// the package-level nameCollisions rename (the type-vs-method `Δ` prefix) that convExpr/convIdent
// would: a struct field is struct-scoped, so a field named like a package type/method (`trace`,
// `stack`, `p`) is declared unrenamed (`trace`) — emitting `ᏑΔtrace` here would not match the
// generated `Ꮡtrace` static (CS0117). The leading '@' keyword-escape is stripped (`Ꮡ@base` is
// invalid — '@' only leads; the generator strips it the same way via GetUnsanitizedIdentifier).
func (v *Visitor) structFieldBoxName(sel *ast.Ident, structExpr ast.Expr) string {
	name := getCoreSanitizedIdentifier(sel.Name)

	if v.fieldCollidesWithType(sel, structExpr) {
		name = typeCollidingFieldName(name)
	}

	return removeSanitizationMarker(name)
}

// structFieldReachable reports whether a field named `name` is reachable on the struct — either
// as a direct field or promoted through an embedded (anonymous) field, including an embedded
// pointer. The deref decision for a pointer's field selector must consider promoted fields too:
// otherwise `x.PromotedField` on a `ж<T>` box is emitted without a deref, and the box has no such
// member (CS1061). Go forbids embedding cycles, so the recursion terminates.
func structFieldReachable(structType *types.Struct, name string) bool {
	for i := range structType.NumFields() {
		field := structType.Field(i)

		if field.Name() == name {
			return true
		}

		if !field.Embedded() {
			continue
		}

		embType := field.Type()

		if ptr, ok := embType.Underlying().(*types.Pointer); ok {
			embType = ptr.Elem()
		}

		if embStruct, ok := embType.Underlying().(*types.Struct); ok {
			if structFieldReachable(embStruct, name) {
				return true
			}
		}
	}

	return false
}

// exprIsPointerLocalField reports whether expr is a field selector `base.field` whose base is a
// pointer LOCAL (a `*T` variable that is neither a parameter nor the receiver). Such a local holds
// the heap box `ж<T>` directly, so `base.field` reached through the value-returning `~` deref is an
// rvalue; the field's address `&base.field` must instead go through the box accessor
// `base.of(T.Ꮡfield)`. A pointer parameter and the receiver are deref-aliased to a value
// (`ref var p = ref Ꮡp.Value`), so their fields are already assignable — those are excluded (handled
// by exprIsDerefdPointerParam / the receiver paths).
func (v *Visitor) exprIsPointerLocalField(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	baseIdent, ok := sel.X.(*ast.Ident)

	if !ok {
		return false
	}

	baseType := v.info.TypeOf(baseIdent)

	if baseType == nil {
		return false
	}

	if _, isPtr := baseType.Underlying().(*types.Pointer); !isPtr {
		return false
	}

	if v.identIsParameter(baseIdent) {
		return false
	}

	if obj := v.info.ObjectOf(baseIdent); obj != nil && v.currentFuncSignature != nil && v.currentFuncSignature.Recv() == obj {
		return false
	}

	return true
}

// exprFieldRootsAtAddressedGlobal reports whether expr is a VALUE field selector (`global.field`,
// possibly nested through further value fields) that roots at a package value global whose address
// is taken (heap-boxed). Such a field's real address is `Ꮡglobal.of(T.Ꮡfield)`; a ж-only method
// (e.g. an atomic `func (x *Uint32) Store`) called on it must route through that box, since a plain
// value/ref of the field cannot bind the box receiver (CS1929). The walk bails at any pointer hop —
// beyond a pointer the field already has a real address, and those forms (pointer locals/params,
// the deref'd receiver) are handled by the boxed/local-field branches above and must not be
// disturbed (routing a ref-accessible receiver field through `&` would need a `Ꮡrecv` box that a
// non-direct-ж receiver lacks → CS0103). A pointer FIELD carries its own box and is excluded.
func (v *Visitor) exprFieldRootsAtAddressedGlobal(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.FieldVal {
		return false
	}

	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		if t := v.info.TypeOf(base); t != nil {
			if _, isPtr := t.Underlying().(*types.Pointer); isPtr {
				return false
			}
		}

		if s, ok := base.(*ast.SelectorExpr); ok {
			base = s.X
			continue
		}

		break
	}

	ident, ok := base.(*ast.Ident)

	return ok && v.isAddressedGlobal(ident)
}

// exprIsValueFieldOfPointerRvalue reports whether expr is a VALUE (non-pointer) struct field whose
// selector chain, after peeling value-field selectors, roots at a pointer-to-struct RVALUE expression
// that already yields a box `ж<T>` directly — a pointer-returning CALL (`getg()`, `q.tail.ptr()`,
// `Δp.chunkOf(ci)`, `getg().m.p.ptr()`) or a pointer ELEMENT index (`batch[i]`). The Go auto-deref
// renders the field access as `(~root).field`, an rvalue, so a `[GoRecv] ref` (pointer-receiver)
// method called on it cannot bind (CS1510). Unlike a deref-aliased ident param/receiver (which has a
// `ref`), the root call/index value IS the box, so the receiver is materialized through the
// &-machinery as `root.of(T.Ꮡfield)` — never a `Ꮡ(value)` copy (which would lose the write).
//
// This is the rvalue COMPLEMENT of exprIsValueFieldOfPointer: that one roots at a pointer FIELD
// selector or pointer LOCAL ident; this one roots at a NON-ident, NON-selector pointer expression (a
// call/index). The two domains are disjoint, so the routing branches never overlap.
func (v *Visitor) exprIsValueFieldOfPointerRvalue(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			// A pointer field/chain mid-way is exprIsValueFieldOfPointer's territory, not this.
			if _, ok := v.getType(b, false).(*types.Pointer); ok {
				return false
			}

			// Keep peeling only through VALUE-field selectors.
			if selection, ok := v.info.Selections[b]; !ok || selection.Kind() != types.FieldVal {
				return false
			}

			base = b.X
		case *ast.Ident:
			// An ident root (pointer local/param/receiver) is handled by the sibling predicates — not
			// this rvalue case.
			return false
		default:
			// A type CONVERSION (`(*T)(p)`) renders as a C# CAST — a low-precedence form on which a
			// trailing `.of(…)` mis-binds to the inner operand. Exclude it so a pointer reinterpret
			// (`(*structTypeUncommon)(unsafe.Pointer(t))`) keeps its existing `Ꮡ(…)` form (S1 territory).
			if call, ok := b.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
				return false
			}

			// A genuine CALL / INDEX / other rvalue: route only when it is a pointer-to-struct box (the
			// value it yields IS a `ж<T>`, so `root.of(T.Ꮡfield)` field-refs the real storage).
			ptrType, ok := v.getType(b, false).(*types.Pointer)

			if !ok {
				return false
			}

			_, ok = ptrType.Elem().Underlying().(*types.Struct)

			return ok
		}
	}
}

// callExprIsTypeConversion reports whether a CallExpr is a Go type CONVERSION (`T(x)`, `(*T)(p)`) —
// its Fun denotes a TYPE — rather than a genuine function/method call. A conversion renders as a C#
// cast (`(T)x`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner
// operand; a genuine call renders as a postfix `f(…)` that `.of(…)` chains off cleanly. The
// pointer-rvalue field-receiver routing excludes conversions for this reason.
func (v *Visitor) callExprIsTypeConversion(callExpr *ast.CallExpr) bool {
	tv, ok := v.info.Types[callExpr.Fun]
	return ok && tv.IsType()
}

// exprIsValueFieldOfPointer reports whether expr is a VALUE (non-pointer) struct field whose base is
// a pointer-to-struct *field selector* (`o.h.wait`, `gp.m.mLockProfile`) — a pointer reached by
// dereferencing another field. Such a pointer deref is an rvalue (`(~o.h)`), so the value field on it
// is NOT addressable, and a pointer-receiver method called on it cannot bind ([GoRecv] ref / ж
// overload, CS1510 / CS1929). Taking the field's address goes through the box-field accessor
// (`o.h.of(holder.Ꮡwait)`, real storage), which the &-machinery renders. The base is intentionally
// restricted to a SELECTOR: a bare ident base is the method's RECEIVER or a deref'd pointer PARAMETER
// (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&`
// would emit `Ꮡf.of(…)`, but a value-ref receiver has no `Ꮡf` box → regression, the historical
// ReceiverFieldMethodCall failure) or a pointer LOCAL (handled by exprIsPointerLocalField above). A
// pointer FIELD is excluded (it is already a box — exprIsAlreadyBoxedPointerFieldOrElement).
func (v *Visitor) exprIsValueFieldOfPointer(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	// Walk the base, peeling value-field selectors, until reaching the pointer the chain is rooted
	// at — `o.h.wait` (base `o.h` is the pointer field) or `mp.mLockProfile.waitTime` (peel
	// `mp.mLockProfile` to `mp`, a `*m` pointer local). The root may be a pointer-to-struct SELECTOR
	// (always an rvalue deref) or a pointer-to-struct LOCAL identifier (the box is accessed via `~`,
	// also an rvalue). It must NOT be the method's RECEIVER or a deref'd pointer PARAMETER: those are
	// emitted as an addressable `ref`, so `f.c.Get()` binds directly and routing them through `&`
	// would emit `Ꮡf.of(…)` with no `Ꮡf` box (the historical ReceiverFieldMethodCall regression).
	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			if ptrType, ok := v.getType(b, false).(*types.Pointer); ok {
				_, ok := ptrType.Elem().Underlying().(*types.Struct)
				return ok
			}

			// A value-field selector — peel to its own base and keep walking toward the root.
			base = b.X
		case *ast.Ident:
			// Root identifier: route a pointer-to-struct LOCAL only (its box is dereferenced via `~`,
			// an rvalue). A deref'd pointer parameter and the receiver are addressable refs.
			ptrType, ok := v.getType(b, false).(*types.Pointer)

			if !ok {
				return false
			}

			if _, ok := ptrType.Elem().Underlying().(*types.Struct); !ok {
				return false
			}

			if v.identIsParameter(b) {
				return false
			}

			if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && b.Name == recvName {
				return false
			}

			return true
		default:
			return false
		}
	}
}

// exprIsValueFieldOfDerefdPointerRoot reports whether expr is a VALUE struct field whose selector
// chain, after peeling value-field selectors, roots at a deref-aliased pointer PARAMETER or the
// pointer RECEIVER — a bare ident emitted as `ref var x = ref Ꮡx.Value`, whose box is `Ꮡx`. Examples:
// `Δp.scav.index` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock` (root `mp`, a `*m` param),
// `h.userArena.readyList` (root `h`, a `*mheap` param).
//
// This is the deliberate COMPLEMENT of exprIsValueFieldOfPointer, which roots at a pointer FIELD/chain
// or a pointer LOCAL and EXCLUDES the param/receiver root: a value field-chain on such a root is
// addressable, so a `[GoRecv] ref` method binds on it directly and must be left alone. A DIRECT-ж
// (box-receiver) method, however, needs the real nested field box `Ꮡx.of(T.Ꮡf1).of(…Ꮡf2)` (which the
// &-machinery renders once it recurses through this root) — the value chain is not a box (CS1929).
// Callers MUST therefore gate on a direct-ж method (selectorCallsDirectBoxMethod), so a `[GoRecv]` ref
// method on the same chain keeps binding directly (no churn).
func (v *Visitor) exprIsValueFieldOfDerefdPointerRoot(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			// A pointer field/chain mid-way is exprIsValueFieldOfPointer's territory, not this.
			if _, ok := v.getType(b, false).(*types.Pointer); ok {
				return false
			}

			// Keep peeling only through VALUE-field selectors.
			if selection, ok := v.info.Selections[b]; !ok || selection.Kind() != types.FieldVal {
				return false
			}

			base = b.X
		case *ast.Ident:
			// Root identifier: a deref-aliased pointer PARAMETER or the pointer RECEIVER (box `Ꮡx`).
			// A pointer LOCAL is excluded — it is handled by exprIsValueFieldOfPointer / the
			// exprIsPointerLocalField branch.
			if _, ok := v.getType(b, false).(*types.Pointer); !ok {
				return false
			}

			// The pointer RECEIVER has a box `Ꮡrecv` ONLY when the enclosing method is itself direct-ж
			// (`this ж<T> Ꮡrecv`). A `[GoRecv] ref` receiver has no box, so routing through `Ꮡrecv`
			// would be CS0103 — leave it (that would need transitive direct-ж propagation, a separate
			// capture-mode concern). Checked before identIsParameter since the receiver is not a param.
			if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && b.Name == recvName {
				return isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
			}

			// A genuine pointer PARAMETER is always deref-aliased with a box `Ꮡp`.
			if v.identIsParameter(b) {
				return true
			}

			return false
		default:
			return false
		}
	}
}

// exprIsAlreadyBoxedPointerFieldOrElement reports whether expr is a field selector or an indexed
// element whose OWN type is a Go pointer — so its C# value is already a `ж<T>` box (e.g.
// cpuProfile's `log *profBuf`, accessed as `cpuprof.log`). A direct-ж (capture-mode) or
// pointer-receiver method called on it binds to that box directly; taking its address via the
// &-machinery would double-box to `ж<ж<T>>` (CS1929). A VALUE field/element (an atomic `Int32`, a
// plain struct field) is NOT already a box and still needs the box machinery. This discriminates a
// pointer FIELD of a boxed global (`cpuprof.log`, already a box) from a deref'd pointer PARAMETER
// (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`) — the latter is a bare ident, not a
// selector/index, so it is correctly left to the box routing.
func (v *Visitor) exprIsAlreadyBoxedPointerFieldOrElement(expr ast.Expr) bool {
	switch expr.(type) {
	case *ast.SelectorExpr, *ast.IndexExpr:
		_, isPtr := v.info.TypeOf(expr).(*types.Pointer)
		return isPtr
	}

	return false
}

// exprIsIndexedValueElement reports whether expr is an indexed element `container[i]` of an
// ADDRESSABLE container (an array or slice — NOT a map, whose elements are not addressable) whose
// element type is a VALUE (not already a pointer/box). A pointer-receiver / direct-ж method called
// on such an element — `bh.Value[i].Load()` (an array of atomic `UnsafePointer`) — operates on the
// element VALUE, so the `[GoRecv] ref` / `ж` overload cannot bind (CS1510 / CS1929). The receiver
// must be routed through the element's box via the &-machinery (`Ꮡ(slice, i)` / `…at<T>(i)`).
func (v *Visitor) exprIsIndexedValueElement(expr ast.Expr) bool {
	indexExpr, ok := expr.(*ast.IndexExpr)

	if !ok {
		return false
	}

	// A generic instantiation `Type[Arg]` is also an *ast.IndexExpr; require the indexed operand to
	// be an array/slice VALUE so a type-instantiation (or a map index) is excluded.
	containerType := v.getType(indexExpr.X, true)

	if containerType == nil {
		return false
	}

	var elem types.Type

	switch container := containerType.Underlying().(type) {
	case *types.Array:
		elem = container.Elem()
	case *types.Slice:
		elem = container.Elem()
	default:
		return false
	}

	// A pointer element is already a box (exprIsAlreadyBoxedPointerFieldOrElement); only a value
	// element needs the box machinery.
	_, isPtr := elem.Underlying().(*types.Pointer)

	return !isPtr
}

// selectorCallsDirectBoxMethod reports whether the selector calls a DIRECT-ж (box-receiver) method —
// one emitted as `this ж<T>` (it takes the address of a field of its receiver, or otherwise needs
// the box), rather than `[GoRecv] this ref T`. Only a direct-ж method requires its receiver be a
// box; a `[GoRecv] ref` method binds to any addressable value directly. Used to decide whether an
// indexed value element must be routed through its box for the call (an addressable element already
// satisfies a `[GoRecv] ref` method, so routing it would be needless churn).
func (v *Visitor) selectorCallsDirectBoxMethod(selectorExpr *ast.SelectorExpr) bool {
	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	return ok && funcObj != nil && packageDirectBoxReceiverMethods[funcObj.Origin()]
}

// isPointerReceiverMethodCall reports whether the selector calls a method with a POINTER receiver
// (`func (x *T) M()`), emitted as a `[GoRecv]` extension over `ref T` / a `ж<T>` overload. Such a
// method needs an addressable receiver, so a value-returning `~` deref of a field receiver is an
// rvalue (CS1510 on the generated `ref`).
func (v *Visitor) isPointerReceiverMethodCall(selectorExpr *ast.SelectorExpr) bool {
	sel, ok := v.info.Selections[selectorExpr]

	if !ok || sel.Kind() != types.MethodVal {
		return false
	}

	sig, ok := sel.Obj().Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	_, isPtr := sig.Recv().Type().(*types.Pointer)

	return isPtr
}

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr, context LambdaContext) string {
	// A Go method becomes a C# extension method on the receiver box (`Method(this ж<T>, …)`) emitted in
	// its DEFINING package's class. C# only finds an extension method when that class's NAMESPACE is in
	// scope. For a method whose receiver type lives in a sub-namespace package (e.g. `internal/runtime/
	// atomic` → `go.@internal.runtime`), a file that calls the method but does NOT import the package
	// (legal in Go — calling a method on a value never requires importing the value's package) gets no
	// `using @internal.runtime;`, so the extension method is invisible and the call mis-binds to a wrong
	// promoted overload (CS1929). Register the method's package namespace here so the file-local `using`
	// is emitted regardless of whether the package was explicitly imported.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
		if obj := sel.Obj(); obj != nil {
			v.addMethodPackageNamespaceUsing(obj.Pkg())
		}
	}

	// A Go METHOD EXPRESSION — `(*timers).run`, the unbound method as a func value whose first
	// parameter is the receiver (runtime time.go's `abi.FuncPCABIInternal((*timers).run)`) —
	// selects a method off a TYPE. Emitting the selector naively renders the type in value
	// position (`(ж<timers>).run` — CS0119/CS1503). Go types the expression as the func signature
	// with the receiver prepended; render that signature as the concrete delegate type and cast
	// the method's static form to it: `(Func<ж<timers>, int64, int64>)run` — for a `[GoRecv]`
	// method the RecvGenerator's ж-overload matches the delegate exactly, and a direct-ж method's
	// primary form does. FuncPCABIInternal-style `any` parameters then take a real delegate.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodExpr {
		delegateType := convertToCSTypeName(v.getCSTypeName(v.info.TypeOf(selectorExpr)))
		return fmt.Sprintf("(%s)(%s)", delegateType, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
	}

	// A method call on a manually-converted foreign-receiver method (`gp.guintptr()` on a *g —
	// see manualTypeOperations.go): the manual implementation captures the receiver's IDENTITY,
	// so it takes the receiver BOX (`this ж<g>`). A deref-aliased pointer receiver (`ref var gp
	// = ref Ꮡgp.Value`) renders as the value alias, which binds neither the box form nor identity;
	// emit the box itself — `Ꮡgp.guintptr()`.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
		if obj := sel.Obj(); obj != nil && v.isManualBoxReceiverMethod(obj) && v.exprIsDerefAliasedPointer(selectorExpr.X) {
			if ident, ok := selectorExpr.X.(*ast.Ident); ok {
				return fmt.Sprintf("%s%s.%s", AddressPrefix, v.getIdentName(ident), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}
	}

	// A POINTER-RECEIVER method called on an ELEMENT of a pointer-to-NAMED-ARRAY —
	// `bh[i].Load()` where bh is `*buckhashArray` (`[N]atomic.UnsafePointer`, runtime
	// mprof.go). The wrapper's ref indexer yields `ref TElem`, which cannot bind a ж-form
	// (direct-ж) extension method; route through the ELEMENT BOX — `bh.at<TElem>(i).Load()`
	// — which binds both the ж form and a [GoRecv] ref form (via its generated ж overload),
	// and whose array-index backing writes through to the real element storage.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
		if fn, ok := sel.Obj().(*types.Func); ok {
			if sig, ok := fn.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, recvIsPtr := sig.Recv().Type().(*types.Pointer); recvIsPtr {
					if indexExpr, ok := selectorExpr.X.(*ast.IndexExpr); ok {
						if ptr, ok := v.info.TypeOf(indexExpr.X).(*types.Pointer); ok {
							if named, ok := types.Unalias(ptr.Elem()).(*types.Named); ok {
								if arrayType, ok := named.Underlying().(*types.Array); ok {
									elemTypeName := convertToCSTypeName(v.getDisplayTypeName(arrayType.Elem(), false))

									return fmt.Sprintf("%s.at<%s>(%s).%s",
										v.convExpr(indexExpr.X, nil), elemTypeName,
										v.convExpr(indexExpr.Index, nil),
										v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								}
							}
						}
					}
				}
			}
		}
	}

	// When this selector is the LHS of an assignment, any nested pointer dereference in its base
	// expression must use the assignable `.Value` form, not the value-returning `~` operator — a
	// chained `(~o).stack.hi = …` (the inner `o.stack` deref via `~`) is not a variable/property
	// (CS0131). Propagate the assignment context down to the base so inner pointer-field selectors
	// emit `o.Value.stack` instead of `(~o).stack`. Only set when assigning, so reads are unchanged.
	var xContexts []ExprContext

	if context.isAssignment {
		assignContext := DefaultLambdaContext()
		assignContext.isAssignment = true
		xContexts = []ExprContext{assignContext}
	}

	// Check if this is a method value being used in an assignment
	if v.isMethodValue(selectorExpr, context.isCallExpr) && context.isAssignment {
		// Check if selector expression needs to be converted to a lambda function for assignment
		if ident, ok := selectorExpr.X.(*ast.Ident); ok {
			if v.isPackageIdentifier(ident) {
				// This is a package selector (like fmt.Println) -- no need for lambda
				return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}

		// A BOUND METHOD VALUE with parameters — `d.compute = metricReader(read).compute`
		// (runtime metrics.go; compute takes (*statAggregate, *metricValue)) — forwards through
		// a lambda carrying the METHOD'S OWN parameters: the previous emission hardcoded arity
		// zero, mismatching any non-nullary target delegate (CS1593). Parameters are explicitly
		// typed (fresh pN names — no collision with the receiver expression) so the lambda binds
		// without a target-typed inference context. Note the receiver expression is evaluated
		// inside the lambda (per call) — Go binds it once at method-value creation; acceptable
		// for the compile milestone and the simple receivers observed (documented).
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Params().Len() > 0 {
				var paramDecls, paramUses strings.Builder

				for i := 0; i < sig.Params().Len(); i++ {
					if i > 0 {
						paramDecls.WriteString(", ")
						paramUses.WriteString(", ")
					}

					name := fmt.Sprintf("p%d", i+1)
					paramDecls.WriteString(fmt.Sprintf("%s %s", convertToCSTypeName(v.getTypeName(sig.Params().At(i).Type(), false)), name))
					paramUses.WriteString(name)
				}

				return fmt.Sprintf("(%s) => %s.%s(%s)", paramDecls.String(), v.convExpr(selectorExpr.X, nil),
					v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)), paramUses.String())
			}
		}

		return fmt.Sprintf("() => %s.%s()", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
	}

	// A method VALUE over a POINTER-receiver method in a VALUE context — a call argument
	// (`s.nonDefaultOnce.Do(s.register)`, `registerMetric(…, s.nonDefault.Load)`; internal/godebug).
	// The [GoRecv] emission is a `ref`-receiver extension, and C# cannot create a delegate from an
	// extension whose first parameter is a value type (CS1113/CS1061). Go binds the receiver ADDRESS
	// once at method-value creation (`s.register` ≡ `(&s).register`); emit that same binding through
	// the box — `Ꮡs.register` / `Ꮡs.of(Setting.ᏑnonDefault).Load` — a method group over the
	// generated ж<T> receiver overload (class-typed, delegate-legal). A receiver expression that is
	// already a pointer needs no &-synthesis and keeps the plain emission (the base renders as the
	// box itself).
	if v.isMethodValue(selectorExpr, context.isCallExpr) && !context.isAssignment {
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); isPtrRecv {
					if recvType := v.getType(selectorExpr.X, false); recvType != nil {
						if _, alreadyPtr := recvType.(*types.Pointer); !alreadyPtr {
							boundRecv := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.%s", boundRecv, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
						}

						// A pointer-typed IDENT base whose plain rendering is the deref'd ref-local
						// — the [GoRecv] receiver `s` itself (`s.register` inside another method of
						// *Setting) or a deref'd pointer param. The isPointer ident context renders
						// the pointer VALUE (the box `Ꮡs`), which the ж<T> overload group binds to.
						if ident, ok := selectorExpr.X.(*ast.Ident); ok {
							identContext := DefaultIdentContext()
							identContext.isPointer = true
							return fmt.Sprintf("%s.%s", v.convIdent(ident, identContext), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
						}
					}
				} else if !types.IsInterface(sig.Recv().Type()) {
					// A VALUE-receiver method value in a call-argument context — `Map(c.ToUpper, s)`
					// (bytes ToUpperSpecial; c is unicode.SpecialCase): the emitted method is an
					// EXTENSION on a value type, from which C# cannot create a delegate (CS1113).
					// Forward through the same param-carrying lambda the assignment context uses —
					// `(rune p1) => c.ToUpper(p1)` (invocation through the extension is legal; only
					// delegate creation is not). Interface receivers are excluded: an interface
					// instance method delegate-binds directly. Same documented caveat as the
					// assignment form: the receiver expression re-evaluates per call.
					var paramDecls, paramUses strings.Builder

					for i := 0; i < sig.Params().Len(); i++ {
						if i > 0 {
							paramDecls.WriteString(", ")
							paramUses.WriteString(", ")
						}

						name := fmt.Sprintf("p%d", i+1)
						paramDecls.WriteString(fmt.Sprintf("%s %s", convertToCSTypeName(v.getTypeName(sig.Params().At(i).Type(), false)), name))
						paramUses.WriteString(name)
					}

					return fmt.Sprintf("(%s) => %s.%s(%s)", paramDecls.String(), v.convExpr(selectorExpr.X, nil),
						v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)), paramUses.String())
				}
			}
		}
	}

	// Check if the selector BASE itself is an explicit dereference (or a pointer conversion whose
	// result the special-cased branch below derefs). This must inspect only the base's own outermost
	// shape — unwrapping parens and looking through a conversion-call's operand — NOT the whole
	// subtree: a `*T` star buried in an ARGUMENT (`stringStructOf((*string)(e.data)).str`, where the
	// star belongs to the conversion inside the call's argument) does not dereference the call's
	// RESULT, and treating it as if it did skipped the auto-deref (`.str` on the returned `ж<T>` box,
	// CS1061 — runtime arena.go; same for an extra-paren conversion base, mheap.go's
	// `((*specialWeakHandle)(unsafe.Pointer(…))).handle`).
	containsExplicitDeref := func(expr ast.Expr) bool {
		for {
			switch e := expr.(type) {
			case *ast.ParenExpr:
				expr = e.X
				continue
			case *ast.StarExpr:
				return true
			case *ast.CallExpr:
				// A pointer-CONVERSION base `(*T)(p)` — the Fun is a parenthesized star — reaches the
				// dedicated conversion branch below (which appends `.Value` itself); a plain call result
				// is NOT a deref regardless of stars inside its arguments.
				if parenFun, ok := e.Fun.(*ast.ParenExpr); ok {
					if _, isStar := parenFun.X.(*ast.StarExpr); isStar {
						return true
					}
				}

				return false
			default:
				return false
			}
		}
	}

	// Get the original expression type and check if it's a pointer
	if exprType := v.info.TypeOf(selectorExpr.X); exprType != nil {
		// A STAR base that is still POINTER-typed after its own deref — `(*outer.ptr).Value`
		// where ptr is a DOUBLE pointer (`**Inner`): the star peels one level (`outer.ptr.Value`,
		// a ж<Inner>), and Go's selector auto-deref supplies the second. Skipping the
		// suppression lets the normal pointer-base field handling below add it; treating the
		// star as a full deref left `.Value` on the box (CS1061 — surfaced when the
		// one-star-one-deref fix removed the old double-`.Value` compensation in convStarExpr).
		// Gated to ACTUAL star bases: a pointer-CONVERSION base (`(*T)(p).field`) is also
		// pointer-typed but must keep its dedicated branch below.
		starBaseStillPointer := false
		{
			unwrapped := ast.Expr(selectorExpr.X)

			for {
				if paren, ok := unwrapped.(*ast.ParenExpr); ok {
					unwrapped = paren.X
					continue
				}

				break
			}

			if _, isStar := unwrapped.(*ast.StarExpr); isStar {
				_, starBaseStillPointer = exprType.Underlying().(*types.Pointer)
			}
		}

		// Check if the selector base is itself an explicit dereference (or a pointer conversion)
		if containsExplicitDeref(selectorExpr.X) && !starBaseStillPointer {
			// Unwrap enclosing parens so an extra-paren conversion base — mheap.go's
			// `((*specialWeakHandle)(unsafe.Pointer(…))).handle` — reaches the conversion branch
			// (the same extra-paren blind spot the reinterpret routing had).
			baseExpr := selectorExpr.X

			for {
				if paren, ok := baseExpr.(*ast.ParenExpr); ok {
					baseExpr = paren.X
					continue
				}

				break
			}

			if callExpr, ok := baseExpr.(*ast.CallExpr); ok {
				// Check if the call expressions is a parenthesized expression
				if _, ok := callExpr.Fun.(*ast.ParenExpr); ok {
					// For a pointer-conversion-then-method like `(*atomic.Uint32)(c).Store(v)`, the
					// converted X is a heap box `ж<T>`. Appending `.Value` derefs it to a value, which
					// is only right for a VALUE-receiver method; a POINTER-receiver method (`func
					// (c *T) Store`) binds to the `ж<T>` overload, so the box itself is the receiver
					// and `.Value` must be omitted.
					if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
						if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
							if _, isPtr := sig.Recv().Type().(*types.Pointer); isPtr {
								return fmt.Sprintf("(%s).%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}
						}
					}

					return fmt.Sprintf("(%s).Value.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
				}
			}

			return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
		}

		if selection, ok := v.info.Selections[selectorExpr]; ok && selection.Kind() == types.FieldVal {
			if ptrType, isPtrType := exprType.(*types.Pointer); isPtrType {
				// Check if the expression is *directly* an intra-function identifier (the
				// receiver or a parameter) — a value-ref receiver/param accesses its fields
				// without a deref. Use a direct type assertion, NOT getIdentifier (which digs
				// to the root of a selector chain): for `e.list.root` the X is `e.list` (a
				// pointer field), which must be dereferenced even though the chain roots at `e`.
				if exprIdent, isIdent := selectorExpr.X.(*ast.Ident); v.inFunction && isIdent {
					if obj := v.info.ObjectOf(exprIdent); obj != nil {
						// Check if it's a receiver or parameter pointer variable
						if selVar, ok := obj.(*types.Var); ok {
							// If it's a receiver, skip dereferencing
							if v.currentFuncSignature.Recv() == selVar {
								return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}

							// Check if it's a function parameter
							params := v.currentFuncSignature.Params()

							for i := range params.Len() {
								// It's a function parameter, skip dereferencing
								if params.At(i) == selVar {
									return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								}
							}
						}
					}
				}

				if obj := v.info.ObjectOf(selectorExpr.Sel); obj != nil {
					// Check if the field belongs to the struct that the pointer points to, rather than
					// to the pointer itself, if so, the pointer has been automatically dereferenced
					if _, ok := obj.(*types.Var); ok {
						// Make sure the field is not receiver target. The field may be a DIRECT member or
						// PROMOTED through an embedded field — both auto-dereference the pointer in Go, so
						// the check must recurse into embeds (a direct-fields-only check missed promoted
						// fields → `x.PromotedField` on a `ж<T>` box without a deref → CS1061).
						if structType, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
							if structFieldReachable(structType, selectorExpr.Sel.Name) {
								// If the field belongs to the struct, automatically dereference the pointer
								if context.isAssignment {
									// Left-hand side of assignment cannot use pointer dereference operator
									return fmt.Sprintf("%s.Value.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								} else {
									return fmt.Sprintf("(%s%s).%s", PointerDerefOp, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								}
							}
						}
					}
				}
			}
		}
	}

	// A pointer-receiver method PROMOTED through a single embedded field — `t.modify(…)` where `t` is a
	// `*timeTimer` and `modify` is a `(*timer)` method reached through timeTimer's embedded `timer` field
	// (Go auto-takes `&t.timer` for the promoted receiver). The receiver box (`t` / `Ꮡt`, a
	// `ж<timeTimer>`) is not the `ж<timer>` the promoted method's ж/[GoRecv]-ref overload binds to
	// (CS1929). Descend into the embedded field's box via the &-machinery — `t.of(timeTimer.Ꮡtimer)` —
	// exactly as the *explicit* `t.timer.modify(…)` already renders (the `&receiver.field` branch in
	// convUnaryExpr handles the pointer-param-vs-local box distinction: `Ꮡt` for a deref'd param, `t`
	// for a pointer local). An ALL-VALUE promotion chain of any depth descends hop by hop —
	// `tt.Common()` on reflect's `sliceType` (embeds `abi.SliceType`, which embeds `abi.Type`, the
	// method's receiver) appends an `.of(abi.SliceType.ᏑType)` view per extra hop (CS1929 ×4). A
	// deeper chain with a POINTER embed past the first hop falls through unchanged. A non-promoted
	// call (Index len 1) and an explicit `x.field.method` (also len 1) never match here.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) {
		if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) >= 2 {
			recvType := v.info.TypeOf(selectorExpr.X)

			if ptr, ok := recvType.Underlying().(*types.Pointer); ok {
				recvType = ptr.Elem()
			}

			if structType, ok := recvType.Underlying().(*types.Struct); ok {
				embedField := structType.Field(sel.Index()[0])

				// Only a VALUE (struct) embedded field needs the box descent: the field itself is not a
				// box, so `&field` (`.of(Type.Ꮡfield)`) materializes the `ж<field>` the promoted method
				// binds to. A POINTER embed (`traceWriter` embeds `traceBufPtr` = `*traceBuf`) already
				// yields the box as the field VALUE (`w.traceBuf`), so it is left to the existing
				// field-access handling — taking its address would double-box to `ж<ж<T>>` (CS1929).
				if embedField.Embedded() {
					if ptr, isPtr := embedField.Type().Underlying().(*types.Pointer); isPtr {
						// A POINTER embed whose embedded type is CROSS-PACKAGE has no generated
						// method forwarder: method promotion is syntax-resolved, and a metadata
						// embed promotes FIELDS only (the StructTypeTemplate metadata fallback) —
						// `t.Uncommon()` on `Δrtype` (embeds `*abi.Type`, runtime type.go) is
						// CS1929. Emit the explicit hop through the embed field's box —
						// `t.Type.Value.Uncommon(…)` — the deref'd `.Value` is a ref return, so the
						// `[GoRecv] ref` extension binds addressably. A same-package pointer embed
						// keeps its generated forwarder (no churn).
						if named, ok := ptr.Elem().(*types.Named); ok {
							// The pointer-embed hop stays single-level (Index == [embed, method]);
							// a deeper chain THROUGH a pointer embed falls through unchanged.
							if len(sel.Index()) == 2 && named.Obj().Pkg() != nil && named.Obj().Pkg() != v.pkg {
								// The hop names the FIELD, which is struct-scoped: a field named
								// like a Δ-renamed package type (rtype's embedded `Type` vs the
								// reflectlite `Type` interface) is DECLARED unrenamed, so the hop
								// must not apply the package-level rename (`t.ΔType` is CS1061).
								// A DIRECT-Ж target binds the embed field's box itself; only a
								// [GoRecv] ref target binds the deref'd `.Value` ref-return
								// (abi.Type.Uncommon promoted direct-ж by the pointer-arg
								// detector - `t.Type.Value.Uncommon()` was CS1929).
								deref := ".Value"

								if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && packageDirectBoxReceiverMethods[funcObj.Origin()] {
									deref = ""
								}

								return getAliasedTypeName(fmt.Sprintf("%s.%s%s.%s", v.convExpr(selectorExpr.X, nil),
									v.structFieldBoxName(&ast.Ident{Name: embedField.Name()}, selectorExpr.X), deref, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
							}
						}
					} else {
						// Resolve the FULL promotion chain: every hop (all Index entries but the
						// method's) must be an embedded VALUE struct field. A chain broken by a
						// pointer embed or a non-struct hop falls through unchanged.
						hopFields := []*types.Var{embedField}
						hopStruct, hopOK := embedField.Type().Underlying().(*types.Struct)
						allValueChain := true

						for _, idx := range sel.Index()[1 : len(sel.Index())-1] {
							if !hopOK {
								allValueChain = false
								break
							}

							hop := hopStruct.Field(idx)

							if !hop.Embedded() {
								allValueChain = false
								break
							}

							if _, isPtr := hop.Type().Underlying().(*types.Pointer); isPtr {
								allValueChain = false
								break
							}

							hopFields = append(hopFields, hop)
							hopStruct, hopOK = hop.Type().Underlying().(*types.Struct)
						}

						if allValueChain {
							// When the base is the current method's OWN receiver and that method is NOT
							// direct-ж, the receiver renders as `this ref T` with NO box — the descent's
							// `Ꮡrecv.of(…)` references a nonexistent `Ꮡrecv` (CS0103; runtime mgcscavenge
							// `sc.setEmpty()` inside `(*scavChunkData).alloc/free`). No box is needed
							// either: the embedded field(s) of a `ref` receiver are addressable, so the
							// promoted method's `[GoRecv] ref` overload binds on the explicit field call
							// `recv.embedField(…).method(…)`. (A direct-ж TARGET on the bare receiver would
							// have promoted the enclosing method via the capture-mode fixpoint, so this
							// arm's target always has the `ref` overload.) The rendered==raw check keeps
							// an inner binding that shadows the receiver name (Δ-renamed) on the descent
							// path.
							if recvIdent, isIdent := selectorExpr.X.(*ast.Ident); isIdent {
								if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && recvIdent.Name == recvName &&
									v.getIdentName(recvIdent) == recvIdent.Name && !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
									hopPath := make([]string, 0, len(hopFields))

									for _, hop := range hopFields {
										hopPath = append(hopPath, v.structFieldBoxName(&ast.Ident{Name: hop.Name()}, selectorExpr.X))
									}

									return getAliasedTypeName(fmt.Sprintf("%s.%s.%s", v.convExpr(selectorExpr.X, nil),
										strings.Join(hopPath, "."), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
								}
							}

							// First hop through the &-machinery (box-vs-param distinction), then one
							// `.of(<Owner>.Ꮡ<field>)` view per additional hop — the ж<T> field view
							// composes, landing on the ж<> box of the method's receiver type.
							embedSel := &ast.SelectorExpr{X: selectorExpr.X, Sel: &ast.Ident{Name: embedField.Name()}}
							fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: embedSel}, DefaultUnaryExprContext())

							for k := 1; k < len(hopFields); k++ {
								ownerTypeName := convertToCSTypeName(v.getTypeName(hopFields[k-1].Type(), false))
								fieldAddr = fmt.Sprintf("%s.of(%s.%s%s)", fieldAddr, boxAccessorType(ownerTypeName, ""),
									AddressPrefix, v.structFieldBoxName(&ast.Ident{Name: hopFields[k].Name()}, selectorExpr.X))
							}

							return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
						}
					}
				}
			}
		}
	}

	// A capture-mode method called on a value field of the current direct-ж receiver —
	// `b.u.Load()` where the struct embeds an atomic-like type as a value field `u`. The callee's
	// ж overload needs a `ж<FieldType>` aliasing the real field; emit it as `(&b.u).Load()`, which
	// the `&recv.field` machinery in convUnaryExpr renders as `Ꮡb.of(Bool.Ꮡu)`. The enclosing
	// method was marked direct-ж (so `Ꮡb` is in scope) by bodyCallsCaptureModeMethodOnReceiverField.
	if context.isCallExpr && v.isCaptureModeMethod(selectorExpr) && v.exprIsCaptureModeFieldBase(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// Route a capture-mode method called on a heap-boxed value receiver through the ж
	// (pointer) overload, which sets up the receiver box the method needs for
	// `&recv.field` — e.g. `var i atomic.Int32; i.Store(10)` → `Ꮡi.Store(10)`. The
	// receiver may be a heap-boxed value var (escape analysis), the current method's own direct-ж
	// receiver (its box `Ꮡrecv` is the parameter) — e.g. `func (r *Ring) Next() { return r.init() }`
	// — or a deref'd pointer parameter (its box `Ꮡp` is the parameter), e.g.
	// `func (r *Ring) Link(s *Ring) { s.Prev() }`. In each case route through the box.
	// A receiver whose GO TYPE is already a POINTER and whose RENDERING yields the box directly
	// (`itabTable.find(…)` where `var itabTable *itabTableType` is an addressed global: the
	// PROPERTY value is the ж<itabTableType> receiver) must not route through `Ꮡ` — that passes
	// the global's SLOT box (ж<ж<itabTableType>>), one layer too high (CS1929, runtime iface.go).
	// A DEREF-ALIASED pointer param/receiver is the opposite: its rendering is the value alias
	// (`ref var s = ref Ꮡs.Value`), so it still needs the box route.
	_, receiverIsPointerValue := v.info.TypeOf(selectorExpr.X).(*types.Pointer)
	receiverYieldsBox := receiverIsPointerValue && !v.exprIsDerefAliasedPointer(selectorExpr.X)

	if context.isCallExpr && !receiverYieldsBox && v.isCaptureModeMethod(selectorExpr) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) && (v.isHeapBoxedExpr(selectorExpr.X) || v.exprIsCurrentDirectBoxReceiver(selectorExpr.X) || v.exprIsDerefdPointerParam(selectorExpr.X) || v.exprIsPointerLocalField(selectorExpr.X)) {
		// When the receiver base is itself a FIELD selector or an INDEX into a heap-boxed value —
		// e.g. a boxed global's atomic field `ctrl.total.Add()`, or `trace.stackTab[i].dump()` where
		// `trace` is an address-taken global — the box address must go through the &-machinery, which
		// emits `Ꮡctrl.of(controller.Ꮡtotal)` / `Ꮡtrace.of(…ᏑstackTab).at<T>(i)`. Naively prefixing
		// `Ꮡ` to `ctrl.total` / `trace.stackTab[i]` would instead bind to the box variable `Ꮡctrl` /
		// `Ꮡtrace` (whose value type has no such member) → CS1061.
		switch selectorExpr.X.(type) {
		case *ast.SelectorExpr, *ast.IndexExpr:
			fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
			return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
		}

		// The receiver box is `Ꮡ`+the RAW variable name — a deref-aliased pointer is declared
		// `ref var <shadow-name> = ref Ꮡ<raw>.Value` (the box keeps the raw name; visitFuncDecl /
		// heap decls never shadow-rename the `Ꮡ` companion). convExpr returns the shadow-renamed
		// VALUE alias, so for a collision-renamed var `p`→`Δp` it yields `ᏑΔp` — which is not in
		// scope (the box is `Ꮡp`), CS0103. Build the box from the raw ident name to match.
		recvExpr := v.convExpr(selectorExpr.X, nil)

		// Use the raw Go name for the box base (a deref'd pointer param's box is `Ꮡ`+param.Name()),
		// but only when this var is shadow-renamed (convExpr gave a name different from the box) and
		// not lambda-captured (a closure reads the captured box by its own form). Restricting to that
		// case keeps every already-correct accessor unchanged — no golden churn.
		if ident, ok := selectorExpr.X.(*ast.Ident); ok && recvExpr != ident.Name {
			recvExpr = ident.Name
		}

		return getAliasedTypeName(fmt.Sprintf("%s%s.%s", AddressPrefix, recvExpr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A (non-capture) pointer-receiver method called on a FIELD of a pointer LOCAL — `c.gp.set(v)`
	// where `c` is a `*coro` local and `set` has a pointer receiver. The `[GoRecv]` method needs an
	// addressable receiver, but the value `~` deref of the field is an rvalue (CS1510 on the
	// generated `ref`). Take the field's box address via the &-machinery so the call binds the `ж`
	// overload: `c.of(coro.Ꮡgp).set(v)`.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsPointerLocalField(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only (pointer-receiver) method called on a value field rooted at a package value global —
	// `prof.signalLock.Store(…)`, `trace.seqlock.Load()`, `Δscavenge.gcPercentGoal.Store(…)` (the
	// atomic-field-of-a-global pattern). The field's value `~`/`.` access is not a box, so the
	// ж overload cannot bind (CS1929). Route the receiver through the &-machinery, which renders the
	// real field box `Ꮡglobal.of(T.Ꮡfield)`; the global is heap-boxed by collectAddressedGlobals'
	// matching pointer-receiver-method-on-global-field handling.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprFieldRootsAtAddressedGlobal(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only (pointer-receiver) method called on a VALUE field reached through a POINTER
	// expression — `(~(~gp).m).mLockProfile.waitTime.Add(…)`, `sgp.g.selectDone.CompareAndSwap(…)`
	// — where the base (`gp.m`, `sgp.g`) is a pointer field/chain and the field is a value (atomic)
	// field. The `~`/`.` value access is an rvalue, so the [GoRecv] ref / ж overload cannot bind
	// (CS1510 / CS1929). Route the receiver through the &-machinery, which field-refs the REAL
	// storage as `gp.m.of(mType.ᏑmLockProfile).of(…ᏑwaitTime)` — never a `Ꮡ(value)` copy, which
	// would silently lose the atomic write. (A pointer-LOCAL field is handled by the
	// exprIsPointerLocalField branch above; this covers a pointer field/chain or pointer param.)
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsValueFieldOfPointer(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only / pointer-receiver method called on a VALUE field rooted at a pointer-to-struct RVALUE —
	// `(~getg()).schedlink.set(…)`, `(~batch[i]).schedlink.set(…)`, `(~Δp.chunkOf(ci)).scavenged.setRange(…)`,
	// `(~getg().m.p.ptr()).wbBuf.get2()` — where the base is a pointer-returning CALL or a pointer ELEMENT
	// index (an rvalue, not an addressable ident/field). The `~`-deref field access is an rvalue, so the
	// [GoRecv] ref overload cannot bind (CS1510). The root call/index value IS a `ж<T>` box, so route the
	// receiver through the &-machinery, which renders the real field storage `root.of(T.Ꮡfield)` — never a
	// `Ꮡ(value)` copy (which would silently lose the write). The complement of the param/receiver/field
	// roots handled above (exprIsValueFieldOfPointerRvalue is disjoint from those predicates).
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsValueFieldOfPointerRvalue(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A DIRECT-ж (box-receiver) method called on a VALUE field-chain rooted at a deref-aliased pointer
	// PARAMETER/RECEIVER — `Δp.scav.index.find()`, `mp.trace.seqlock.Load()`, `h.userArena.readyList.remove(s)`.
	// The value field-chain is not a box, so the ж overload cannot bind (CS1929). The param/receiver root
	// has a box (`Ꮡp`/`Ꮡrecv`), so route the receiver through the &-machinery, which renders the real
	// nested storage `Ꮡp.of(T.Ꮡf1).of(…Ꮡf2)` — never a `Ꮡ(value)` copy (which would silently lose an
	// atomic write). GATED to direct-ж: a `[GoRecv] ref` method binds directly on the addressable value
	// field-chain (the deref-alias root is a `ref`), so it is left untouched — rerouting it would churn
	// working output (this is why exprIsValueFieldOfDerefdPointerRoot is the param/receiver complement of
	// exprIsValueFieldOfPointer, which serves the broader isPointerReceiverMethodCall branch above).
	if context.isCallExpr && v.selectorCallsDirectBoxMethod(selectorExpr) && v.exprIsValueFieldOfDerefdPointerRoot(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only / pointer-receiver method called on a VALUE element of an addressable array/slice —
	// `bh.Value[i].Load()` (an array of atomic `UnsafePointer`). The element value is not a box, so the
	// `[GoRecv] ref` / `ж` overload cannot bind (CS1510 / CS1929). Route the receiver through the
	// element's box via the &-machinery, which renders the real element address (`Ꮡ(slice, i)` /
	// `…at<T>(i)`) — never a `Ꮡ(value)` copy, which would lose an atomic write.
	if context.isCallExpr && v.selectorCallsDirectBoxMethod(selectorExpr) && v.exprIsIndexedValueElement(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		elemAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return getAliasedTypeName(fmt.Sprintf("%s.%s", elemAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	return getAliasedTypeName(fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
}

func (v *Visitor) getSelIdentContext(selectorExpr *ast.SelectorExpr) IdentContext {
	context := DefaultIdentContext()
	context.isMethod = true

	// Flag a field selector whose name collides with its enclosing struct's type name so the
	// access is renamed to match the renamed field declaration (CS0542).
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.FieldVal {
		context.fieldCollidesWithType = v.fieldCollidesWithType(selectorExpr.Sel, selectorExpr.X)
	}

	return context
}
