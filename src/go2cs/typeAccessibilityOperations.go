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

	defer cascadePublicizedMethodTypes()

	for _, name := range scope.Names() {
		obj := scope.Lookup(name)

		switch obj := obj.(type) {
		case *types.TypeName:
			// An EXPORTED defined type over an unexported NAMED type — `type EncoderBuffer
			// encoder` (image/png). Its [GoType("encoder")] wrapper's Value property, ctor,
			// and implicit operators all expose the wrapped `encoder`; a public wrapper over
			// an internal type is CS0051/CS0053/CS0056/CS0057 (and C# conversion operators
			// MUST be public, CS0558, so an internal wrapper is not an option). Publicize the
			// written RHS to match the wrapper's accessibility.
			if obj.Exported() {
				collectPublicizedWrapperRHS(obj, pkg)

				// An EXPORTED type's EXPORTED methods are emitted public; an unexported
				// param/result type is then less accessible than the public method
				// (crypto/internal/bigmod's `func (x *Nat) Equal(y *Nat) choice`, choice
				// unexported → CS0050). Publicize those signature types. The fixpoint cascade
				// below then carries them through any further exported-method chains.
				if named, ok := obj.Type().(*types.Named); ok {
					collectMethodSignatureUnexportedTypes(named, pkg)
				}

				// An EXPORTED named FUNC type is emitted as a public C# delegate; an unexported
				// type in its signature is then less accessible than the delegate (CS0059,
				// x/text/unicode/bidi's `type Option func(*options)` → `public delegate void
				// Option(ж<options> _)`). Publicize those signature types like an exported method's.
				if sig, ok := obj.Type().Underlying().(*types.Signature); ok {
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

// cascadePublicizedMethodTypes extends the publicized set through method signatures: a publicized
// type's EXPORTED methods are emitted public (see the receiver-access logic in visitFuncDecl), so
// their parameter/result types face the same accessibility rule (CS0050/CS0051). Runs to a fixpoint
// since each newly publicized type exposes its own exported methods.
func cascadePublicizedMethodTypes() {
	for {
		before := len(packagePublicizedTypes)

		for obj := range packagePublicizedTypes {
			named, ok := types.Unalias(obj.Type()).(*types.Named)

			if !ok {
				continue
			}

			collectMethodSignatureUnexportedTypes(named, obj.Pkg())

			// A publicized WRAPPER (an unexported `type A b` reached here through the field/
			// method cascade) also exposes its wrapped RHS through the public wrapper —
			// propagate through the RHS, same as the exported-wrapper seed below.
			if tn, ok := obj.(*types.TypeName); ok {
				collectPublicizedWrapperRHS(tn, obj.Pkg())
			}
		}

		if len(packagePublicizedTypes) == before {
			break
		}
	}
}

// collectMethodSignatureUnexportedTypes publicizes the unexported named types that appear in
// the EXPORTED methods' parameter/result signatures of a named type whose methods are emitted
// public (an exported type, or an unexported-but-publicized one). An exported method returning
// an unexported type is CS0050 (crypto/internal/bigmod's `Nat.Equal() choice`); a param of one
// is CS0051.
func collectMethodSignatureUnexportedTypes(named *types.Named, pkg *types.Package) {
	for i := range named.NumMethods() {
		method := named.Method(i)

		if !method.Exported() {
			continue
		}

		sig, ok := method.Type().(*types.Signature)

		if !ok {
			continue
		}

		if params := sig.Params(); params != nil {
			for j := range params.Len() {
				collectUnexportedNamedTypes(params.At(j).Type(), pkg)
			}
		}

		if results := sig.Results(); results != nil {
			for j := range results.Len() {
				collectUnexportedNamedTypes(results.At(j).Type(), pkg)
			}
		}
	}
}

// collectPublicizedWrapperRHS publicizes the written RHS of a defined type whose [GoType]
// wrapper is emitted public (exported, or unexported-but-publicized). `type EncoderBuffer
// encoder` exposes `encoder` through the wrapper's Value/ctor/implicit operators, so the
// wrapped named type must be at least as accessible. Only a NAMED, in-package, unexported RHS
// needs it — an unnamed RHS (array/struct literal) has no accessibility of its own.
func collectPublicizedWrapperRHS(tn *types.TypeName, pkg *types.Package) {
	if packageTypeSpecRHS == nil {
		return
	}

	rhs, ok := packageTypeSpecRHS[tn]

	if !ok || rhs == nil {
		return
	}

	if _, isNamed := types.Unalias(rhs).(*types.Named); isNamed {
		collectUnexportedNamedTypes(rhs, pkg)
	}
}

// receiverTypeIsPublicized reports whether the (possibly pointer-wrapped) receiver type is an
// unexported named type that is nonetheless emitted `public` (see packagePublicizedTypes). Its
// exported methods must then be public too, or a cross-assembly caller holding the value through
// the exported var/field/return cannot call them (encoding/binary's BigEndian.Uint32 — CS1061).
func receiverTypeIsPublicized(t types.Type) bool {
	if packagePublicizedTypes == nil {
		return false
	}

	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	if named, ok := types.Unalias(t).(*types.Named); ok {
		return packagePublicizedTypes[named.Obj()]
	}

	return false
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
