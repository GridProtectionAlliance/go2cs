// typeAccessibilityOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// packagePublicizedTypes holds unexported named types in the package that must be emitted as
// `public` because they are used as the type of an exported (public) struct field. C# requires a
// field's type to be at least as accessible as the field itself, so an exported field of an
// unexported type would otherwise fail with CS0052 (and its generated constructor with CS0051).
//
// Populated by a synchronous per-package pre-pass (collectPublicizedTypes), then read-only during
// concurrent file visiting. Keyed by the type's *types.TypeName object, interned per type.
var packagePublicizedTypes map[types.Object]bool

// packagePublicizedLiftedTypes holds anonymous struct/interface types that go2cs LIFTS to a
// synthesized `…ᴛN` named type and that must be emitted `public` because they appear in the
// parameter/result signature of a publicized interface's method (or an exported method/func/
// delegate), where they would otherwise be less accessible than the now-public member (CS0050/
// CS0051). Signature positions only — an exported FIELD/VAR of an anonymous struct is the separate
// CS0052 domain and is left to the named-only walker (see collectSignatureTypes).
//
// Unlike packagePublicizedTypes these have no *types.Object to key on — the lift is a synthesized
// name over a raw anonymous types.Type — so they are interned by the anonymous type itself, with any
// enclosing alias stripped (types.Unalias). The archetype is testing's `type corpusEntry =
// struct{…}`: the ALIAS targets an anonymous struct that lifts to `corpusEntryᴛ1`, referenced by the
// publicized `testDeps` interface's fuzzing methods (CoordinateFuzzing/RunFuzzWorker/ReadCorpus).
//
// Populated by the same synchronous pre-pass as packagePublicizedTypes, then read-only during file
// visiting; consulted at the lift's emission (visitStructType → isPublicizedLiftedType).
var packagePublicizedLiftedTypes map[types.Type]bool

// collectPublicizedTypes records every unexported named type that is referenced by an exported
// field of any package-level struct (directly, or through a pointer/slice/array/map/channel
// element). Scanning the exported fields of every struct in one pass is sufficient: a publicized
// struct's own exported-field types are already covered because that struct was itself scanned.
func collectPublicizedTypes(pkg *types.Package) {
	if packagePublicizedTypes == nil {
		packagePublicizedTypes = map[types.Object]bool{}
	}

	if packagePublicizedLiftedTypes == nil {
		packagePublicizedLiftedTypes = map[types.Type]bool{}
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
							collectSignatureTypes(params.At(i).Type(), pkg)
						}
					}

					if results := sig.Results(); results != nil {
						for i := range results.Len() {
							collectSignatureTypes(results.At(i).Type(), pkg)
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
						collectSignatureTypes(params.At(i).Type(), pkg)
					}
				}

				if results := sig.Results(); results != nil {
					for i := range results.Len() {
						collectSignatureTypes(results.At(i).Type(), pkg)
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

		collectSignatureUnexportedTypes(method, pkg)
	}

	// A defined INTERFACE type's methods live on its UNDERLYING *types.Interface, not on the Named
	// (named.NumMethods() is 0 for an interface). When such an interface is publicized — an unexported
	// interface reached through an exported surface, emitted `public` (testing's `testDeps` through
	// `MainStart(deps testDeps, …)`) — its methods become PUBLIC interface members, so the unexported
	// named types in their parameter/result signatures must be publicized too, or they are less
	// accessible than the public member (CS0051 param, CS0050 result — testDeps.CoordinateFuzzing's
	// `corpusEntry`). The cascade fixpoint then propagates through those types in turn.
	if iface, ok := named.Underlying().(*types.Interface); ok {
		for i := range iface.NumMethods() {
			collectSignatureUnexportedTypes(iface.Method(i), pkg)
		}
	}
}

// collectSignatureUnexportedTypes publicizes the unexported named types (and lifted anonymous
// struct/interface types) in an EXPORTED method's parameter/result signature.
func collectSignatureUnexportedTypes(method *types.Func, pkg *types.Package) {
	if !method.Exported() {
		return
	}

	sig, ok := method.Type().(*types.Signature)

	if !ok {
		return
	}

	if params := sig.Params(); params != nil {
		for j := range params.Len() {
			collectSignatureTypes(params.At(j).Type(), pkg)
		}
	}

	if results := sig.Results(); results != nil {
		for j := range results.Len() {
			collectSignatureTypes(results.At(j).Type(), pkg)
		}
	}
}

// collectPublicizedWrapperRHS publicizes the written RHS of a defined type whose [GoType]
// wrapper is emitted public (exported, or unexported-but-publicized). `type EncoderBuffer
// encoder` exposes `encoder` through the wrapper's Value/ctor/implicit operators, so the
// wrapped named type must be at least as accessible. This also reaches through an UNNAMED
// composite RHS to its element types: `type ringElement [256]fieldElement` exposes
// `fieldElement` through the wrapper's indexer/Value/ToSpan, so the array's element type must
// be publicized too (crypto/internal/mlkem768's fieldElement, CS0050/CS0051/CS0053/CS0054/
// CS0056/CS0057). collectUnexportedNamedTypes peels pointer/slice/array/map/chan to the element
// but has NO struct case, so a struct RHS stays a no-op — an exported field of an unexported
// type is the CS0052 domain and is intentionally left internal.
func collectPublicizedWrapperRHS(tn *types.TypeName, pkg *types.Package) {
	if packageTypeSpecRHS == nil {
		return
	}

	rhs, ok := packageTypeSpecRHS[tn]

	if !ok || rhs == nil {
		return
	}

	collectUnexportedNamedTypes(types.Unalias(rhs), pkg)
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
	case *types.Signature:
		// A FUNC-typed element of an exported field/var — `var SupportedKDFs =
		// map[uint16]func() *hkdfKDF` (crypto/internal/hpke), `var F func() snapshot`, or a
		// []func(x internalT) field — emits a public field whose type embeds the func's
		// parameter/result types (`map<uint16, Func<ж<hkdfKDF>>>`). C# requires those to be at
		// least as accessible as the public field, so an unexported named type reachable ONLY
		// through the signature must be publicized too, or it is less accessible than the field
		// (CS0052). Peeling stops at Signature in the wrapper cases above, so walk the params and
		// results back through the same named-only recursion (which handles a nested func result
		// in turn). A lifted anonymous struct/interface in the signature is the CS0050/CS0051
		// signature domain (collectSignatureTypes), not this exported-field CS0052 walk.
		if params := t.Params(); params != nil {
			for i := range params.Len() {
				collectUnexportedNamedTypes(params.At(i).Type(), pkg)
			}
		}

		if results := t.Results(); results != nil {
			for i := range results.Len() {
				collectUnexportedNamedTypes(results.At(i).Type(), pkg)
			}
		}
	}
}

// collectSignatureTypes is the SIGNATURE-context counterpart of collectUnexportedNamedTypes: in
// addition to publicizing unexported named types, it publicizes the LIFTED anonymous struct/
// interface types that appear in a PUBLIC method/func/delegate signature. A public callable whose
// parameter/result is a less-accessible type is CS0050/CS0051 — and an anonymous struct/interface
// written (or aliased) in the signature is lifted to a synthesized `…ᴛN` type that defaults to
// internal (testing's `type corpusEntry = struct{…}` alias in the publicized `testDeps` interface;
// TypeConversionInterfaceParam's inline `Process(struct{…})`). This is deliberately NOT folded into
// collectUnexportedNamedTypes: an exported FIELD/VAR of an anonymous struct is the CS0052 domain and
// is intentionally left to the named-only walker (a public struct/var over an internal anon field
// type is legal when its own enclosing type is internal), so only signature positions lift here.
func collectSignatureTypes(t types.Type, pkg *types.Package) {
	switch t := t.(type) {
	case *types.Named:
		obj := t.Obj()

		if obj.Pkg() == pkg && !obj.Exported() {
			packagePublicizedTypes[obj] = true
		}
	case *types.Alias:
		// A type ALIAS in the signature (`type corpusEntry = struct{…}`). Strip the alias and route
		// the underlying type back through the switch: an anonymous struct/interface target lifts and
		// is recorded below; a named target is recorded via the *types.Named arm.
		collectSignatureTypes(types.Unalias(t), pkg)
	case *types.Struct:
		collectPublicizedLiftedType(t, pkg)
	case *types.Interface:
		collectPublicizedLiftedType(t, pkg)
	case *types.Pointer:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Slice:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Array:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Map:
		collectSignatureTypes(t.Key(), pkg)
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Chan:
		collectSignatureTypes(t.Elem(), pkg)
	}
}

// collectPublicizedLiftedType records an anonymous struct/interface type that go2cs lifts to a
// synthesized `…ᴛN` named type and that must be emitted `public` because it appears in a public
// callable signature (a publicized interface method, or an exported method/func/delegate). A lifted
// type carries no *types.Object, so it is interned by the anonymous type itself and consulted at
// emission (isPublicizedLiftedType). A NAMED type is not routed here — it carries its own
// accessibility and is handled by packagePublicizedTypes.
func collectPublicizedLiftedType(t types.Type, pkg *types.Package) {
	if t == nil || packagePublicizedLiftedTypes[t] {
		return
	}

	packagePublicizedLiftedTypes[t] = true

	// A publicized lifted struct's EXPORTED fields become public members, so a field whose type is
	// less accessible than the now-public struct is CS0052. Publicize those field types too (a
	// named unexported field type; or a nested lifted anonymous struct — hence collectSignatureTypes,
	// which the recursion guard above keeps finite).
	if st, ok := t.(*types.Struct); ok {
		for i := range st.NumFields() {
			field := st.Field(i)

			if !field.Exported() {
				continue
			}

			collectSignatureTypes(field.Type(), pkg)
		}
	}
}

// isPublicizedLiftedType reports whether the lifted anonymous type must be emitted `public` (see
// packagePublicizedLiftedTypes). Consulted at the lift's emission in visitStructType; the
// alias-stripped form is also checked so a key interned under either representation matches.
func isPublicizedLiftedType(t types.Type) bool {
	if packagePublicizedLiftedTypes == nil || t == nil {
		return false
	}

	if packagePublicizedLiftedTypes[t] {
		return true
	}

	return packagePublicizedLiftedTypes[types.Unalias(t)]
}

// signatureReferencesUnexportedProductionType reports whether a function/method signature references,
// in any parameter or result position (peeling pointer/slice/array/map/channel wrappers to the
// element), an unexported named type of pkg that is declared in a PRODUCTION (non-test) file.
//
// It is the MIRROR of the production publicization framework, used for test-file-declared symbols:
// production emits an unexported type as `internal` and is converted independently of (and before)
// the test files, so a test file's EXPORTED helper — Go's `func NewDecimal(uint64) *decimal` in
// strconv's internal_test.go — would be a public method whose result type is the less-accessible
// internal production `decimal` (CS0050). In the recompile test model the test assembly is self-
// contained (production + internal + external test files compile into ONE assembly, no cross-assembly
// consumer of a test symbol), so downgrading such a helper to `internal` is both correct and
// sufficient — internal is at least as restrictive as any accessibility the helper references, and
// every caller (other test files) lives in the same assembly.
//
// The PRODUCTION-file restriction is essential: an unexported type declared in a TEST file (sort's
// `multiSorter` in example_multi_test.go, returned by the exported `OrderedBy`) is publicized AND
// re-emitted `public` within this same test pass by the framework above, so its exported referrer
// compiles as public with no CS0050 — flipping it to internal would be a needless emission change.
// Only a production-declared unexported type stays internal-on-disk and forces the downgrade.
func (v *Visitor) signatureReferencesUnexportedProductionType(sig *types.Signature, pkg *types.Package) bool {
	if sig == nil {
		return false
	}

	if params := sig.Params(); params != nil {
		for i := range params.Len() {
			if v.typeReferencesUnexportedProductionNamed(params.At(i).Type(), pkg) {
				return true
			}
		}
	}

	if results := sig.Results(); results != nil {
		for i := range results.Len() {
			if v.typeReferencesUnexportedProductionNamed(results.At(i).Type(), pkg) {
				return true
			}
		}
	}

	return false
}

// typeReferencesUnexportedProductionNamed reports whether t is, or wraps (through pointer/slice/array/
// map/channel), an unexported named type of pkg declared in a production (non-test) file — the read-
// only counterpart of collectUnexportedNamedTypes, with the production-file gate described on
// signatureReferencesUnexportedProductionType.
func (v *Visitor) typeReferencesUnexportedProductionNamed(t types.Type, pkg *types.Package) bool {
	switch t := t.(type) {
	case *types.Named:
		obj := t.Obj()
		return obj.Pkg() == pkg && !obj.Exported() && !v.isTestFileDecl(obj.Pos())
	case *types.Pointer:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Slice:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Array:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Map:
		return v.typeReferencesUnexportedProductionNamed(t.Key(), pkg) || v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Chan:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	}

	return false
}

// isTestFileDecl reports whether the declaration at pos originates from a Go test file (`*_test.go`).
// Used to gate test-only accessibility handling; resolves the source filename through the visitor's
// FileSet so it is independent of which file the visitor currently has open.
func (v *Visitor) isTestFileDecl(pos token.Pos) bool {
	if v.fset == nil || !pos.IsValid() {
		return false
	}

	return strings.HasSuffix(strings.ToLower(v.fset.Position(pos).Filename), "_test.go")
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
