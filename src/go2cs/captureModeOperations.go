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
// emitted with the box AS the receiver directly (`this ж<T> Ꮡx` + `ref var x = ref Ꮡx.val;`),
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

		if bodyTakesReceiverFieldAddress(funcDecl.Body, recvName) || bodyReturnsReceiver(funcDecl.Body, recvName) || bodyUsesReceiverAsPointerValue(funcDecl.Body, recvName) {
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
// pointer value on the RHS of an assignment (`recvField = recvName`, `x := recvName`). Such a
// method needs the real receiver box (the pointer is copied/stored), which the direct-ж receiver
// supplies as `Ꮡrecv` — without it a value-ref receiver (`this ref T recv`) has no pointer to
// hand out, and `recv` (a T value) cannot be assigned to a *T field. The selector-`X` position
// (`recvName.field`, a value field access) is not a bare RHS ident, so it is naturally excluded.
func bodyUsesReceiverAsPointerValue(body *ast.BlockStmt, recvName string) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		assignStmt, ok := node.(*ast.AssignStmt)

		if !ok {
			return true
		}

		for _, rhs := range assignStmt.Rhs {
			if ident, ok := rhs.(*ast.Ident); ok && ident.Name == recvName {
				found = true
				return false
			}
		}

		return true
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
