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

// packageDirectBoxReceiverMethods holds the subset of capture-mode methods whose receiver
// is generic (e.g. atomic.Pointer[T]). A static ThreadLocal capture cannot hold the
// receiver's type parameter, so these are emitted with the box AS the receiver directly
// (`this ж<T> Ꮡx` + `ref var x = ref Ꮡx.val;`) and `&x.field` references the parameter box
// (`Ꮡx.of(Type.ᏑField)`) — putting T in scope without a static field. Non-generic
// capture-mode methods continue to use the ThreadLocal capture (see convUnaryExpr).
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

		named, ok := pointer.Elem().(*types.Named)

		if !ok {
			return true
		}

		if bodyTakesReceiverFieldAddress(funcDecl.Body, recvName) {
			// Key by the generic origin so instantiated call sites (Set[int]) match.
			origin := funcObj.Origin()
			packageCaptureModeMethods[origin] = true

			// A generic receiver cannot be captured by the static ThreadLocal; emit it with
			// the box as the receiver directly (direct-ж).
			if named.TypeParams().Len() > 0 {
				packageDirectBoxReceiverMethods[origin] = true
			}
		}

		return true
	})
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
