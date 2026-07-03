package main

import (
	"go/ast"
	"go/token"
	"go/types"

	"golang.org/x/tools/go/packages"
)

// packageCaptureModeMethods holds the package's pointer-receiver methods that take the
// address of a receiver field (`&recv.field`). Such a method needs the real receiver
// box (it emits `<capturedName>.of(Type.ᏑField)`), which only exists when the method is
// invoked through the ж (pointer) overload. So a value var on which one of these methods
// is called must be heap-boxed and the call routed through the ж overload. Populated by a
// synchronous pre-pass before escape analysis; read-only afterward. Keyed by *types.Func
// (interned per method across files). Includes generic receivers, which use the direct-ж
// emission (see packageDirectBoxReceiverMethods) — both forms still need the value var
// boxed and the call routed through the ж overload.
var packageCaptureModeMethods map[*types.Func]bool

// packageDirectBoxReceiverMethods holds the field-address capture-mode methods that are
// emitted with the box AS the receiver directly (`this ж<T> Ꮡx` + `ref var x = ref Ꮡx.Value;`),
// where `&x.field` references the parameter box (`Ꮡx.of(Type.ᏑField)`). This replaces the
// static-ThreadLocal capture for ALL such methods (generic and non-generic): the ThreadLocal
// is a shared static reassigned per call and races across threads for distinct receivers —
// broken for concurrent types like sync/atomic — whereas the box parameter has no shared state
// (and avoids a per-call ThreadLocal allocation). For generics it also puts T in scope.
var packageDirectBoxReceiverMethods map[*types.Func]bool

// collectCaptureModeMethods records every non-generic pointer-receiver method whose body
// takes the address of a receiver field — across the package AND its (transitive) imports,
// since a calling package needs to know that an imported method (e.g. sync/atomic.Int32.Store)
// is capture-mode. `LoadAllSyntax` makes dependency ASTs + type info available; the *types.Func
// objects are interned, so call-site lookups match.
func collectCaptureModeMethods(pkg *packages.Package) {
	visited := map[*packages.Package]bool{}
	captureModeCandidates = nil

	var scan func(p *packages.Package)

	scan = func(p *packages.Package) {
		if p == nil || visited[p] || p.TypesInfo == nil {
			return
		}

		visited[p] = true

		for _, file := range p.Syntax {
			scanFileForCaptureModeMethods(file, p.TypesInfo)
		}

		for _, imported := range p.Imports {
			scan(imported)
		}
	}

	scan(pkg)

	// Transitive fixpoint: a method that calls a direct-ж method on its own receiver must itself
	// become direct-ж (so it has a receiver box `Ꮡrecv` to route the call through). Repeat until
	// stable, since the callee may only have been marked direct-ж in an earlier pass.
	for changed := true; changed; {
		changed = false

		for _, candidate := range captureModeCandidates {
			origin := candidate.funcObj.Origin()

			if packageDirectBoxReceiverMethods[origin] {
				continue
			}

			if bodyCallsDirectBoxMethodOnReceiver(candidate.body, candidate.recvName, candidate.info) ||
				bodyCallsCaptureModeMethodOnReceiverField(candidate.body, candidate.recvName, candidate.info) {
				packageCaptureModeMethods[origin] = true
				packageDirectBoxReceiverMethods[origin] = true
				changed = true
			}
		}
	}
}

// scanFileForCaptureModeMethods marks the file's capture-mode methods in the shared set.
func scanFileForCaptureModeMethods(file *ast.File, info *types.Info) {
	ast.Inspect(file, func(node ast.Node) bool {
		funcDecl, ok := node.(*ast.FuncDecl)

		if !ok || funcDecl.Recv == nil || funcDecl.Body == nil || len(funcDecl.Recv.List) == 0 {
			return true
		}

		recvField := funcDecl.Recv.List[0]

		if len(recvField.Names) == 0 {
			return true
		}

		recvName := recvField.Names[0].Name

		funcObj, _ := info.Defs[funcDecl.Name].(*types.Func)

		if funcObj == nil {
			return true
		}

		signature, _ := funcObj.Type().(*types.Signature)

		if signature == nil || signature.Recv() == nil {
			return true
		}

		pointer, ok := signature.Recv().Type().(*types.Pointer)

		if !ok {
			return true
		}

		if _, ok := pointer.Elem().(*types.Named); !ok {
			return true
		}

		// Record every pointer-receiver candidate for the transitive fixpoint below (a method
		// that calls a direct-ж method on its receiver must itself become direct-ж).
		captureModeCandidates = append(captureModeCandidates, &captureCandidate{
			funcObj:  funcObj,
			body:     funcDecl.Body,
			recvName: recvName,
			info:     info,
		})

		if bodyTakesReceiverFieldAddress(funcDecl.Body, recvName) || bodyReturnsReceiver(funcDecl.Body, recvName) || bodyUsesReceiverAsPointerValue(funcDecl.Body, recvName, info) || bodyCapturesReceiverInClosure(funcDecl.Body, recvName, signature.Recv(), info) {
			// Key by the generic origin so instantiated call sites (Set[int]) match.
			origin := funcObj.Origin()
			packageCaptureModeMethods[origin] = true

			// All field-address capture methods use the direct-ж receiver — the box is passed
			// AS the receiver parameter (`this ж<T> Ꮡx`), not stashed in a static ThreadLocal.
			// The ThreadLocal capture is a shared static reassigned per call, which races across
			// threads for distinct receivers (broken for concurrent types like sync/atomic); the
			// direct-ж form has no shared state and is also alloc-free. Applies to generic AND
			// non-generic receivers.
			packageDirectBoxReceiverMethods[origin] = true
		}

		return true
	})
}

// captureCandidate is a pointer-receiver method recorded for the transitive direct-ж fixpoint.
type captureCandidate struct {
	funcObj  *types.Func
	body     *ast.BlockStmt
	recvName string
	info     *types.Info
}

// captureModeCandidates holds every pointer-receiver method seen while scanning the package and
// its imports, used by the transitive fixpoint in collectCaptureModeMethods.
var captureModeCandidates []*captureCandidate

// bodyCallsDirectBoxMethodOnReceiver reports whether the body calls a direct-ж method on the
// receiver itself (`recvName.someDirectBoxMethod(...)`). The caller must then also be direct-ж so
// it has a receiver box `Ꮡrecv` to route the call through (the callee's ж overload needs it).
func bodyCallsDirectBoxMethodOnReceiver(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		recvIdent, ok := selectorExpr.X.(*ast.Ident)

		if !ok || recvIdent.Name != recvName {
			return true
		}

		if funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && funcObj != nil {
			if packageDirectBoxReceiverMethods[funcObj.Origin()] {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// bodyCallsCaptureModeMethodOnReceiverField reports whether the body calls a direct-ж
// (capture-mode) method on a VALUE field-chain of the receiver (`recvName.f1.…fn.someDirectBoxMethod(...)`,
// n>=1) — e.g. a struct embedding sync/atomic's Uint8 as a value field `u` and doing `b.u.Load()`, or
// a deeper chain like `p.scav.index.free(…)` (root `p`, value fields `scav`→`index`). The callee's ж
// overload needs a `ж<FieldType>`; the only way to produce one that aliases the real field is
// `Ꮡrecv.of(RecvType.ᏑF1).of(…ᏑFn)`, which requires the caller to itself be direct-ж so its receiver
// box `Ꮡrecv` is in scope. So the caller must be marked direct-ж too (convSelectorExpr then emits the
// field-address box form via exprIsValueFieldOfDerefdPointerRoot). The chain must be all VALUE fields —
// a pointer field mid-chain is already a box and roots elsewhere (exprIsValueFieldOfPointer's territory),
// so it needs no caller box and must not trigger promotion.
func bodyCallsCaptureModeMethodOnReceiverField(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		// The call target must be `recvName.f1.…fn.method` — a value field-chain rooted directly at
		// the receiver. (A bare `recvName.method` is the receiver-direct case handled separately.)
		if !selectorRootsAtReceiverValueFieldChain(selectorExpr.X, recvName, info) {
			return true
		}

		if funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && funcObj != nil {
			if packageDirectBoxReceiverMethods[funcObj.Origin()] {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// selectorRootsAtReceiverValueFieldChain reports whether expr is a VALUE struct-field selector chain
// `recvName.f1.…fn` (n>=1) that roots at the receiver ident recvName, with every hop a VALUE
// (non-pointer) field. This is the capture-mode pre-pass complement of convSelectorExpr's
// exprIsValueFieldOfDerefdPointerRoot: a direct-ж method called on such a chain needs the real nested
// field box `Ꮡrecv.of(T.ᏑF1).of(…ᏑFn)`, which only exists when the enclosing method is itself direct-ж.
// A pointer field anywhere in the chain is already a box (and roots the call elsewhere), so it stops
// the walk — that case must not promote the enclosing method.
func selectorRootsAtReceiverValueFieldChain(expr ast.Expr, recvName string, info *types.Info) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	for {
		// Each hop must be a value field selection (not a method/package ref, not a pointer field).
		if selection, ok := info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
			return false
		}

		if _, isPtr := info.TypeOf(sel).(*types.Pointer); isPtr {
			return false
		}

		switch base := sel.X.(type) {
		case *ast.SelectorExpr:
			sel = base
		case *ast.Ident:
			return base.Name == recvName
		default:
			return false
		}
	}
}

// bodyReturnsReceiver reports whether the body returns the receiver itself (`return recvName`).
// Such a method also needs the real receiver box (it returns the pointer), which the direct-ж
// receiver supplies as `Ꮡrecv` — see visitReturnStmt.
func bodyReturnsReceiver(body *ast.BlockStmt, recvName string) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		returnStmt, ok := node.(*ast.ReturnStmt)

		if !ok {
			return true
		}

		for _, result := range returnStmt.Results {
			if ident, ok := result.(*ast.Ident); ok && ident.Name == recvName {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// bodyUsesReceiverAsPointerValue reports whether the body uses the receiver itself as a bare
// pointer value: on the RHS of an assignment (`recvField = recvName`, `x := recvName`), or as an
// operand of a pointer ==/!= comparison (`p != recvName`). Such a method needs the real receiver
// box (the pointer is copied/stored/compared), which the direct-ж receiver supplies as `Ꮡrecv` —
// without it a value-ref receiver (`this ref T recv`) has no pointer to hand out, and `recv`
// (a T value) cannot be assigned to a *T field or compared with a ж<T>. The selector-`X` position
// (`recvName.field`, a value field access) is not a bare ident, so it is naturally excluded.
func bodyUsesReceiverAsPointerValue(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.AssignStmt:
			for _, rhs := range n.Rhs {
				if ident, ok := rhs.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}
			}
		case *ast.BinaryExpr:
			if n.Op == token.EQL || n.Op == token.NEQ {
				if ident, ok := n.X.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}

				if ident, ok := n.Y.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}
			}
		case *ast.CompositeLit:
			// The receiver placed whole into a COMPOSITE-LITERAL element whose FIELD is a Go
			// pointer — `return funcInfo{f, mod}` inside `func (f *_func) funcInfo()` (runtime
			// symtab.go; funcInfo's first field is the embedded pointer *_func). The pointer-typed
			// field needs the receiver's box, which only exists when the method is direct-ж. Gated
			// on the FIELD's declared type (the element expression's own type is always *T for a
			// pointer receiver): a receiver placed into an INTERFACE-typed field also typechecks in
			// Go, but that emission compiles today and promoting for it would re-route every such
			// method stdlib-wide — deliberately out of scope (its pointer-identity semantics are a
			// separate question).
			structType, ok := info.TypeOf(n).Underlying().(*types.Struct)

			if !ok {
				return true
			}

			for i, elt := range n.Elts {
				fieldIdx := i
				val := elt

				if kv, ok := elt.(*ast.KeyValueExpr); ok {
					val = kv.Value
					fieldIdx = -1

					if keyIdent, ok := kv.Key.(*ast.Ident); ok {
						for j := 0; j < structType.NumFields(); j++ {
							if structType.Field(j).Name() == keyIdent.Name {
								fieldIdx = j
								break
							}
						}
					}
				}

				if ident, ok := val.(*ast.Ident); ok && ident.Name == recvName {
					if fieldIdx >= 0 && fieldIdx < structType.NumFields() {
						if _, isPtr := structType.Field(fieldIdx).Type().(*types.Pointer); isPtr {
							found = true
							return false
						}
					}
				}
			}
		}

		return true
	})

	return found
}

// bodyCapturesReceiverInClosure reports whether the body references the receiver inside a function
// literal (a closure) — e.g. runtime's `func (p *_panic) nextFrame() { … systemstack(func(){ … p.lr
// … }) }`. The receiver is normally emitted as the deref'd ref-local alias `ref var p = ref Ꮡp.Value`,
// but a C# ref-local cannot be captured by a lambda (CS8175); inside the closure the receiver must
// be referenced through its box `Ꮡp` (the convIdent/convUnaryExpr box-ref forms). That box only
// exists when the method is direct-ж — the box passed AS the receiver param (`this ж<T> Ꮡp`). A
// closure parameter that shadows the receiver name has a distinct object, so the `recv` identity
// check excludes it (no false fire).
func bodyCapturesReceiverInClosure(body *ast.BlockStmt, recvName string, recv *types.Var, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		if found {
			return false
		}

		funcLit, ok := node.(*ast.FuncLit)

		if !ok {
			return true
		}

		// Any reference to the receiver from within this closure (or a closure nested inside it —
		// ast.Inspect recurses) means it is captured and must route through the box.
		ast.Inspect(funcLit.Body, func(inner ast.Node) bool {
			ident, ok := inner.(*ast.Ident)

			if !ok || ident.Name != recvName {
				return true
			}

			if recv != nil && info.ObjectOf(ident) != recv {
				return true
			}

			found = true
			return false
		})

		return !found
	})

	return found
}

// bodyTakesReceiverFieldAddress reports whether the body contains `&recvName.field`.
func bodyTakesReceiverFieldAddress(body *ast.BlockStmt, recvName string) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		unaryExpr, ok := node.(*ast.UnaryExpr)

		if !ok || unaryExpr.Op != token.AND {
			return true
		}

		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if ident, ok := selectorExpr.X.(*ast.Ident); ok && ident.Name == recvName {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// captureModeMethodValueReceiver returns the receiver identifier of a deferred/go call whose
// target is a capture-mode method value on a heap-boxed or addressed-global receiver — e.g.
// `defer locked.Store(0)` where `locked` is a (boxed) atomic. Such a receiver must NOT be
// snapshot-captured into a value copy: the box (`Ꮡlocked`) is the stable defer-time receiver
// (matching Go's `&locked`) and is accessible directly in the lambda, while a value copy has no
// box and cannot call the ж-overload (CS1929/CS0103). Returns nil when no such receiver applies.
func (v *Visitor) captureModeMethodValueReceiver(call *ast.CallExpr) *ast.Ident {
	if call == nil {
		return nil
	}

	sel, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return nil
	}

	recvIdent, ok := sel.X.(*ast.Ident)

	if !ok || !v.isCaptureModeMethod(sel) {
		return nil
	}

	if v.isHeapBoxedExpr(recvIdent) || v.isAddressedGlobal(recvIdent) {
		return recvIdent
	}

	return nil
}

// isCaptureModeMethod reports whether the selector calls a package capture-mode method.
func (v *Visitor) isCaptureModeMethod(selectorExpr *ast.SelectorExpr) bool {
	if packageCaptureModeMethods == nil {
		return false
	}

	funcObj, _ := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	// A call to a generic method resolves to the instantiation (e.g. Set[int]); normalize
	// to the generic origin, which is what the pre-pass recorded.
	return funcObj != nil && packageCaptureModeMethods[funcObj.Origin()]
}

// isDirectBoxReceiverMethod reports whether the given func declaration is a generic
// capture-mode method emitted with the box as its receiver (direct-ж).
func isDirectBoxReceiverMethod(funcDecl *ast.FuncDecl, info *types.Info) bool {
	if packageDirectBoxReceiverMethods == nil || funcDecl == nil || funcDecl.Name == nil {
		return false
	}

	funcObj, _ := info.Defs[funcDecl.Name].(*types.Func)

	return funcObj != nil && packageDirectBoxReceiverMethods[funcObj.Origin()]
}

// exprIsCurrentDirectBoxReceiver reports whether expr is the bare receiver identifier of the
// current method when that method is direct-ж — i.e. its receiver box `Ꮡrecv` is in scope. Used
// to route `recv.method()` (a direct-ж method called on the receiver) through that box.
func (v *Visitor) exprIsCurrentDirectBoxReceiver(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok || v.currentFuncDecl == nil {
		return false
	}

	isPtrRecv, recvName := v.isPointerReceiver()

	return isPtrRecv && ident.Name == recvName && isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
}

// exprIsCaptureModeFieldBase reports whether expr is a value field whose address can be taken as a
// field box, so a capture-mode method called on it (`base.field.Load()`) routes through
// `(&base.field)`. Two bases qualify: the current method's direct-ж receiver (`recv.field` → box
// `Ꮡrecv.of(RecvType.ᏑField)`; the fixpoint marks such methods direct-ж via
// bodyCallsCaptureModeMethodOnReceiverField), or any pointer expression (`e.field` where `e` is
// `*T` → `e.of(T.ᏑField)`, since `e` is already the box). convUnaryExpr renders `&base.field`
// to the matching form.
func (v *Visitor) exprIsCaptureModeFieldBase(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	// Must be a field selection (not a method value) and a value field (not already a pointer —
	// a pointer field already carries its own box and routes normally).
	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.FieldVal {
		return false
	}

	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	if v.exprIsCurrentDirectBoxReceiver(sel.X) {
		return true
	}

	// A field of a pointer *variable* (`e.field`, e an *T identifier): `e` is the box, so
	// `e.of(...)` works. Restricted to a bare identifier to match convUnaryExpr's `&e.field` form.
	baseIdent, ok := sel.X.(*ast.Ident)

	if !ok {
		return false
	}

	_, isPtr := v.info.TypeOf(baseIdent).(*types.Pointer)

	return isPtr
}

// exprIsDerefdPointerParam reports whether expr is a pointer-typed parameter. Such a parameter is
// emitted deref'd to a value alias (`ref var p = ref Ꮡp.Value`) with the box `Ꮡp` as the actual
// parameter, so a direct-ж method called on it must route through `Ꮡp` (a pointer *local* holds
// the box directly and needs no routing).
func (v *Visitor) exprIsDerefdPointerParam(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok || !v.identIsParameter(ident) {
		return false
	}

	_, isPtr := v.getIdentType(ident).(*types.Pointer)

	return isPtr
}

// exprIsDerefAliasedPointer reports whether expr is a bare identifier that is a deref-aliased
// pointer — a pointer PARAMETER or a pointer RECEIVER, both emitted as the pointed-to value
// (`ref T x`, with the box `Ꮡx` separate), not the box itself. A pointer LOCAL, by contrast, holds
// the box directly. Callers that would otherwise deref a pointer (`~x`) must skip it for these,
// since `x` is already the value (a `~` on it would deref a non-pointer → CS0023).
func (v *Visitor) exprIsDerefAliasedPointer(expr ast.Expr) bool {
	if v.exprIsDerefdPointerParam(expr) {
		return true
	}

	ident, ok := expr.(*ast.Ident)

	if !ok {
		return false
	}

	// The receiver match is by NAME, so a local that SHADOWS the receiver (`r := &y` inside a method
	// with receiver `r`) would mis-take this gate — e.g. `unsafe.Pointer(r)` on the inner pointer
	// local would emit the receiver's `FromRef(ref r)` form against a genuine box (pinning the box
	// reference slot: compiles, silently wrong address). The shadow-rename pass gives every inner
	// same-named binding a `Δ` name (C# forbids shadowing), while the receiver always renders under
	// its raw name — so requiring rendered == raw restricts the match to the actual receiver.
	if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && ident.Name == recvName && v.getIdentName(ident) == ident.Name {
		return true
	}

	return false
}

// bodyCallsCaptureModeMethodOn reports whether the body calls a capture-mode method with
// the given identifier as the (value) receiver — meaning that identifier must be boxed.
func (v *Visitor) bodyCallsCaptureModeMethodOn(ident *ast.Ident, body ast.Node) bool {
	if packageCaptureModeMethods == nil || ident == nil {
		return false
	}

	target := v.info.ObjectOf(ident)

	if target == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		recvIdent, ok := selectorExpr.X.(*ast.Ident)

		if !ok || v.info.ObjectOf(recvIdent) != target {
			return true
		}

		// Only a value receiver needs boxing; a pointer receiver already carries its box.
		if _, isPointer := v.info.TypeOf(recvIdent).(*types.Pointer); isPointer {
			return true
		}

		if v.isCaptureModeMethod(selectorExpr) {
			found = true
			return false
		}

		return true
	})

	return found
}
