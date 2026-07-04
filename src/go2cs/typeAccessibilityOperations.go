package main

import (
	"go/ast"
	"go/types"
)

// packagePublicizedTypes holds unexported named types in the package that must be emitted as
// `public` because they are used as the type of an exported (public) struct field. C# requires a
// field's type to be at least as accessible as the field itself, so an exported field of an
// unexported type would otherwise fail with CS0052 (and its generated constructor with CS0051).
//
// Populated by a synchronous per-package pre-pass (collectPublicizedTypes), then read-only during
// concurrent file visiting. Keyed by the type's *types.TypeName object, interned per type.
var packagePublicizedTypes map[types.Object]bool

// collectPublicizedTypes records every unexported named type that is referenced by an exported
// field of any package-level struct (directly, or through a pointer/slice/array/map/channel
// element). Scanning the exported fields of every struct in one pass is sufficient: a publicized
// struct's own exported-field types are already covered because that struct was itself scanned.
func collectPublicizedTypes(pkg *types.Package) {
	if packagePublicizedTypes == nil {
		packagePublicizedTypes = map[types.Object]bool{}
	}

	scope := pkg.Scope()

	for _, name := range scope.Names() {
		obj := scope.Lookup(name)

		switch obj := obj.(type) {
		case *types.TypeName:
			structType, ok := obj.Type().Underlying().(*types.Struct)

			if !ok {
				continue
			}

			for i := range structType.NumFields() {
				field := structType.Field(i)

				// Only an exported field forces its type to be at least as accessible; an
				// unexported (internal) field of an internal type is fine.
				if !field.Exported() {
					continue
				}

				collectUnexportedNamedTypes(field.Type(), pkg)
			}
		case *types.Var, *types.Const:
			// An EXPORTED package-level var (or typed const) of an unexported type — Go's
			// `var ErrNetClosing = errNetClosing{}` (internal/poll) — emits a public static
			// field whose type must be at least as accessible (CS0052). Go consumers can
			// legally hold the value and call its exported methods, so publicizing the type
			// is the faithful mapping.
			if obj.Exported() {
				collectUnexportedNamedTypes(obj.Type(), pkg)
			}
		case *types.Func:
			// An EXPORTED function's parameter/result types face the same rule on the public
			// static method (CS0050/CS0051) — `func Peek() snapshot` with `snapshot`
			// unexported.
			if !obj.Exported() {
				continue
			}

			if sig, ok := obj.Type().(*types.Signature); ok {
				if params := sig.Params(); params != nil {
					for i := range params.Len() {
						collectUnexportedNamedTypes(params.At(i).Type(), pkg)
					}
				}

				if results := sig.Results(); results != nil {
					for i := range results.Len() {
						collectUnexportedNamedTypes(results.At(i).Type(), pkg)
					}
				}
			}
		}
	}
}

// collectUnexportedNamedTypes records the unexported named types of this package that the given
// type references, peeling pointer/slice/array/map/channel wrappers to reach the element types.
func collectUnexportedNamedTypes(t types.Type, pkg *types.Package) {
	switch t := t.(type) {
	case *types.Named:
		obj := t.Obj()

		if obj.Pkg() == pkg && !obj.Exported() {
			packagePublicizedTypes[obj] = true
		}
	case *types.Pointer:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Slice:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Array:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Map:
		collectUnexportedNamedTypes(t.Key(), pkg)
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Chan:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	}
}

// isPublicizedType reports whether the named type identified by ident must be emitted as public
// (it is used as the type of an exported struct field; see packagePublicizedTypes).
func (v *Visitor) isPublicizedType(ident *ast.Ident) bool {
	if ident == nil || packagePublicizedTypes == nil {
		return false
	}

	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	return obj != nil && packagePublicizedTypes[obj]
}
