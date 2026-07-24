// manualTypeOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
	// reflect.Value's entry + value-reader methods (the reflection bridge, Phase 2). Go reads the
	// value through v.ptr as flat memory at computed offsets — no managed form. value_impl.cs carries
	// the boxed managed value directly (a companion `partial struct Value { object boxed }` field) and
	// reads it with System.Reflection + the golib container interfaces. Only the value READERS are
	// hand-owned; Kind/Type/IsValid/CanAddr work from the flag/typ_ the entry sets. Increment 1
	// (scalars, slices, arrays, pointers); struct Field/NumField + map MapRange land next.
	"reflect": {
		"ValueOf":             true,
		"unpackEface":         true,
		"valueInterface":      true, // a free function `valueInterface(v Value, safe bool)`, not a method
		"Value.Interface":     true,
		"Value.Bool":          true,
		"Value.Int":           true,
		"Value.Uint":          true,
		"Value.Float":         true,
		"Value.Complex":       true,
		"Value.String":        true,
		"Value.IsNil":         true,
		"Value.Len":           true,
		"Value.Index":         true,
		"Value.Elem":          true,
		"Value.Bytes":         true,
		"Value.NumField":      true,
		"Value.Field":         true,
		"Value.UnsafePointer": true,
		"Value.Pointer":       true,
		"Value.MapRange":      true,
		"MapIter.Next":        true,
		"MapIter.Key":         true,
		"MapIter.Value":       true,
		// Type side: reflect.rtype's ΔType methods over the abi.Type's System.Type (%T, %+v names).
		"rtype.String":   true,
		"rtype.Name":     true,
		"rtype.Elem":     true,
		"rtype.Field":    true,
		"rtype.NumField": true,
		// reflect.Type must be CANONICAL (Go interns type descriptors so `aType == bType` holds for
		// equal types — internal/fmtsort.compare relies on `aType != bType`). The auto Value.Type()
		// and toType() mint a fresh wrapper per call over a fresh abi.Type box, so identity-equality
		// never matched → map-key sorting reversed. The hand-owned forms in value_impl.cs intern the
		// ΔType wrapper by the underlying System.Type (canonType). See docs/Phase4/DESIGN-reflection-bridge.md.
		"Value.Type": true,
		"toType":     true,
		// deepValueEqual keys its cycle-detection visited map on the values' internal data words
		// (v.ptr / v.pointer()) — eface addresses the bridge never populates, so the auto form NREs
		// converting the null unsafe.Pointer slot (strings/bytes TestSplit/TestSplitAfter, R5).
		// deepequal_impl.cs recurses over the bridge's boxed values and keys cycle detection on
		// managed reference identity. DeepEqual itself stays auto (it only uses the bridged
		// ValueOf/Type/AreEqual).
		"deepValueEqual": true,
		// Phase-3 write-back (the chip): Set writes through the addressable Value's aliased ж box
		// (Go's assignTo semantics over the golib assert machinery); Zero builds valid zero Values
		// (a pointer kind yields the canonical typed-nil box); methodName walks the managed stack
		// (runtime.Caller has no managed form — its getcallersp chain NotImplementedException'd
		// every mustBe* panic path, errors TestAs's first operational hit).
		"Value.Set":  true,
		"Zero":       true,
		"methodName": true,
	},
	// internal/reflectlite mirrors the reflect bridge for the mini-surface sort.Slice
	// exercises (ValueOf → Len, Swapper — sort's TestSlice was the first operational hit):
	// the auto forms reinterpret the interface's eface words, so the first touch derefs a
	// nil ж<abi.Type>. value_impl.cs carries the boxed managed value on a companion
	// `partial struct Value { object boxed }` field (typ_/flag set from the Phase-1
	// synthetic abi.Type, so Kind()/IsValid() work from value.cs unchanged); swapper_impl.cs
	// swaps through golib's non-generic ISlice indexer. See docs/Phase4/DESIGN-reflection-bridge.md.
	"internal/reflectlite": {
		"ValueOf":     true,
		"unpackEface": true,
		"Value.Len":   true,
		"Swapper":     true,
		// Phase-3 write-back — the errors.As surface. The auto forms read the never-populated
		// v.ptr eface word (IsNil answered TRUE for every pointer; Elem returned the invalid
		// Value) or descriptor sub-records synthType never populates (rtype.Elem panicked;
		// implements() reinterpreted the descriptor). Bridged in value_impl.cs / type_impl.cs
		// over the carried System.Type + the golib method-set machinery.
		"Value.Elem":         true,
		"Value.IsNil":        true,
		"Value.Set":          true,
		"rtype.Elem":         true,
		"rtype.Implements":   true,
		"rtype.AssignableTo": true,
		"methodName":         true,
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
