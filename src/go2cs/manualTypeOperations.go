package main

import (
	"go/ast"
	"go/types"
)

// Some Go declarations cannot be faithfully auto-converted because their semantics depend on
// hiding a managed pointer inside an integer (e.g. runtime's guintptr family): the CLR cannot
// hold a managed reference as a number across a GC move, so the managed conversion must store
// the ж<T> box DIRECTLY (model precedent: core/sync/atomic Pointer<T>). Those declarations are
// hand-converted in the package's *_impl.cs (marked [module: GoManualConversion], kept in
// src/core/<pkg>/ and restored over auto output by the overlay). The converter SKIPS emitting:
//   - the type declaration itself (a marker comment is left in its place),
//   - every method declared on the type,
//   - adjacent free functions / methods on other types listed in manualConversionFuncs,
//   - GoImplicitConv assembly attributes referencing the type (the manual file declares any
//     conversion operators its call sites need).
//
// Call-site emission is unchanged except conversions handled in convCallExpr: a manual-type
// conversion from unsafe.Pointer(x) emits the referent-preserving ctor form `new T(x)` instead
// of the numeric cast chain `(T)(uintptr)new Pointer(x)` (which would lose the referent).
//
// Keys are RAW Go identifiers (rename analyses apply downstream at emission).
var manualConversionTypes = map[string]map[string]bool{
	"runtime": {
		"guintptr": true,
		"puintptr": true,
		"muintptr": true,
	},
}

// Free functions ("funcName") and methods on other types ("recvTypeName.funcName") owned by the
// same manual files — declarations whose bodies are inseparable from the manual types' semantics.
var manualConversionFuncs = map[string]map[string]bool{
	"runtime": {
		"g.guintptr": true,
		"setGNoWB":   true,
		"setMNoWB":   true,
		// lock_sema.go (lock_sema_impl.cs): the mutex/note key-slot protocol smuggles an *m
		// address through the uintptr slot and parks on OS semaphores — the managed model is a
		// {0, locked} spinlock/latch. Thin wrappers (lock/unlock/noteclear/notetsleep[g]) and
		// the consts stay auto. ⚠ These entries encode lock_SEMA semantics (the windows/darwin/
		// plan9 family — the default host platform): a futex-platform conversion (-platforms
		// linux/amd64) includes lock_futex.go instead, whose notetsleep_internal is 2-parameter
		// and whose key protocol is {0,1,2} — the name-keyed skip would mismatch (CS7036).
		"mutexContended":      true,
		"lock2":               true,
		"unlock2":             true,
		"notewakeup":          true,
		"notesleep":           true,
		"notetsleep_internal": true,
	},
	// internal/abi.TypeOf reads an interface's type-word via unsafe.Pointer to reach a Go runtime
	// type descriptor that has no managed form (the reflection bridge — Phase 4). type_impl.cs
	// synthesizes an abi.Type whose Kind_ is classified from the value's managed System.Type. See
	// docs/Phase4/DESIGN-reflection-bridge.md.
	"internal/abi": {
		"TypeOf": true,
	},
}

// isManualType reports whether the named type (raw Go name) is hand-converted in this package.
func (v *Visitor) isManualType(goTypeName string) bool {
	if typeNames, ok := manualConversionTypes[v.pkg.Path()]; ok {
		return typeNames[goTypeName]
	}

	return false
}

// isManualBoxReceiverMethod reports whether obj is a listed foreign-receiver manual method
// (a manualConversionFuncs "recvTypeName.funcName" entry). Such a method captures the
// receiver's IDENTITY (e.g. g.guintptr wraps the *g itself), so its manual implementation
// takes the receiver BOX (`this ж<T>`) — a deref-aliased call site must pass the box, not
// the value alias (see convSelectorExpr).
func (v *Visitor) isManualBoxReceiverMethod(obj types.Object) bool {
	fn, ok := obj.(*types.Func)

	if !ok || fn.Pkg() == nil {
		return false
	}

	funcNames, ok := manualConversionFuncs[fn.Pkg().Path()]

	if !ok {
		return false
	}

	sig, ok := fn.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	recvType := sig.Recv().Type()

	if ptr, ok := recvType.(*types.Pointer); ok {
		recvType = ptr.Elem()
	}

	named, ok := recvType.(*types.Named)

	if !ok {
		return false
	}

	return funcNames[named.Obj().Name()+"."+fn.Name()]
}

// isManualFuncDecl reports whether the function declaration is owned by a manual conversion:
// either any method whose receiver base type is a manual type, or an explicitly listed
// free function / foreign-receiver method.
func (v *Visitor) isManualFuncDecl(funcDecl *ast.FuncDecl) bool {
	if funcDecl == nil || funcDecl.Name == nil {
		return false
	}

	funcName := funcDecl.Name.Name
	recvName := ""

	if funcDecl.Recv != nil && len(funcDecl.Recv.List) > 0 {
		recvType := funcDecl.Recv.List[0].Type

		if starExpr, ok := recvType.(*ast.StarExpr); ok {
			recvType = starExpr.X
		}

		if ident, ok := recvType.(*ast.Ident); ok {
			recvName = ident.Name
		}
	}

	if recvName != "" && v.isManualType(recvName) {
		return true
	}

	if funcNames, ok := manualConversionFuncs[v.pkg.Path()]; ok {
		if recvName != "" {
			return funcNames[recvName+"."+funcName]
		}

		return funcNames[funcName]
	}

	return false
}
