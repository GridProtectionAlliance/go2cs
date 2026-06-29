package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
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
// (`ref var p = ref Ꮡp.val`), so their fields are already assignable — those are excluded (handled
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

	// When this selector is the LHS of an assignment, any nested pointer dereference in its base
	// expression must use the assignable `.val` form, not the value-returning `~` operator — a
	// chained `(~o).stack.hi = …` (the inner `o.stack` deref via `~`) is not a variable/property
	// (CS0131). Propagate the assignment context down to the base so inner pointer-field selectors
	// emit `o.val.stack` instead of `(~o).stack`. Only set when assigning, so reads are unchanged.
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

		return fmt.Sprintf("() => %s.%s()", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
	}

	// Check if an expression contains an explicit dereference at any level
	containsExplicitDeref := func(expr ast.Expr) bool {
		found := false
		ast.Inspect(expr, func(n ast.Node) bool {
			if _, isStarExpr := n.(*ast.StarExpr); isStarExpr {
				found = true
				return false // Stop traversal if we found a star expression
			}
			return true // Continue traversal
		})
		return found
	}

	// Get the original expression type and check if it's a pointer
	if exprType := v.info.TypeOf(selectorExpr.X); exprType != nil {
		// Check if there's an explicit dereference at any level in the expression
		if containsExplicitDeref(selectorExpr.X) {
			if callExpr, ok := selectorExpr.X.(*ast.CallExpr); ok {
				// Check if the call expressions is a parenthesized expression
				if _, ok := callExpr.Fun.(*ast.ParenExpr); ok {
					// For a pointer-conversion-then-method like `(*atomic.Uint32)(c).Store(v)`, the
					// converted X is a heap box `ж<T>`. Appending `.val` derefs it to a value, which
					// is only right for a VALUE-receiver method; a POINTER-receiver method (`func
					// (c *T) Store`) binds to the `ж<T>` overload, so the box itself is the receiver
					// and `.val` must be omitted.
					if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
						if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
							if _, isPtr := sig.Recv().Type().(*types.Pointer); isPtr {
								return fmt.Sprintf("(%s).%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}
						}
					}

					return fmt.Sprintf("(%s).val.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
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
									return fmt.Sprintf("%s.val.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
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
	if context.isCallExpr && v.isCaptureModeMethod(selectorExpr) && (v.isHeapBoxedExpr(selectorExpr.X) || v.exprIsCurrentDirectBoxReceiver(selectorExpr.X) || v.exprIsDerefdPointerParam(selectorExpr.X) || v.exprIsPointerLocalField(selectorExpr.X)) {
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

		return getAliasedTypeName(fmt.Sprintf("%s%s.%s", AddressPrefix, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A (non-capture) pointer-receiver method called on a FIELD of a pointer LOCAL — `c.gp.set(v)`
	// where `c` is a `*coro` local and `set` has a pointer receiver. The `[GoRecv]` method needs an
	// addressable receiver, but the value `~` deref of the field is an rvalue (CS1510 on the
	// generated `ref`). Take the field's box address via the &-machinery so the call binds the `ж`
	// overload: `c.of(coro.Ꮡgp).set(v)`.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsPointerLocalField(selectorExpr.X) {
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
