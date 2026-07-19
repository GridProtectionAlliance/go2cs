package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convTypeAssertExpr(typeAssertExpr *ast.TypeAssertExpr) string {
	if typeAssertExpr.Type == nil {
		return fmt.Sprintf("%s.type()", v.convExpr(typeAssertExpr.X, nil))
	}

	// A type assertion to a NAMED interface resolves at run time only through a compile-time
	// GoImplement adapter — builtin.cs TryTypeAssert has no duck-typing path for named interfaces
	// (that fallback is anonymous-interface-only), so without a recorded implementation
	// `h.(encoding.BinaryMarshaler)` panics ("interface conversion: … not encoding.BinaryMarshaler").
	// Record the concrete type(s) that implement the target interface for both source shapes.
	exprType := v.getExprType(typeAssertExpr.X)

	if sourceIsInterface, sourceIsEmpty := isInterface(exprType); sourceIsInterface {
		targetType := v.getExprType(typeAssertExpr.Type)

		if needsInterfaceCast, isEmpty := isInterface(targetType); needsInterfaceCast && !isEmpty {
			if sourceIsEmpty {
				// EMPTY source (`any`): the expression may still carry a resolvable concrete type.
				concreteType := v.getUnderlyingType(typeAssertExpr.X)

				if concreteType != nil {
					v.convertToInterfaceType(targetType, concreteType, "")
				}
			} else if named, ok := types.Unalias(targetType).(*types.Named); ok {
				if _, targetIsInterface := named.Underlying().(*types.Interface); targetIsInterface {
					// NON-EMPTY source (`h.(T)` with h a `hash.Hash32`) asserting to a NAMED target
					// interface: getUnderlyingType yields the interface itself, not the concrete
					// dynamic type, so enumerate the package's concrete types that implement BOTH the
					// source and target interfaces — exactly the dynamic types this assertion can
					// succeed on — and record each. Restricted to NAMED targets: an anonymous target
					// interface resolves through its generated ᴛAs conversion method at run time and
					// needs no recorded adapter (builtin.cs TryTypeAssert), so recording one would be
					// dead machinery.
					v.recordAssertConcreteImplementers(exprType, targetType)
				}
			}
		}
	}

	// Get the type information for this expression
	typesInfo := v.info.TypeOf(typeAssertExpr)
	var safeAssertDescriminator string

	// Check if this is used in a multi-value context, thus making it a safe assertion
	if tuple, isTuple := typesInfo.(*types.Tuple); isTuple && tuple.Len() == 2 {
		safeAssertDescriminator = TrueMarker
	}

	context := DefaultIdentContext()
	context.isType = true

	// convExpr on the TYPE expression already yields the C# form — do NOT run it through
	// convertToCSTypeName again: the conversion is not idempotent, re-sanitizing machinery
	// names inside generic args (`d.(chan struct{})` → `channel<EmptyStruct>` whose inner
	// arg re-sanitized to the reserved-Δ `ΔEmptyStruct` — context CS0246 ×4/CS0019).
	typeExpr := v.convExpr(typeAssertExpr.Type, []ExprContext{context})

	// A methodless named func type collapses to its base C# delegate everywhere it is REFERENCED
	// (getTypeName), so its NAME is never emitted. A type-assertion target `ci.(Compressor)` where
	// `type Compressor func(io.Writer) (io.WriteCloser, error)` must therefore assert against the
	// collapsed delegate `Func<…>`, not the bare (undefined) name `Compressor` — convExpr on the type
	// ident renders the name (CS0246, archive/zip's compressor/decompressor registries). Render the
	// collapsed form for such a target.
	// An ANONYMOUS func type target takes the same route: the AST render above goes through the
	// string-based type-name path, which skips the VARIADIC params-Span delegate lowering —
	// `cw.(func(string, ...any))` emitted `._<Action<@string, .any>>` with a literal `.any`
	// (CS1001, net/http transport.go logf). getCSTypeName builds the delegate structurally via
	// iifeDelegateType (`Actionꓸꓸꓸ<@string, any>`); non-variadic signatures render identically
	// on both paths.
	if targetType := v.getExprType(typeAssertExpr.Type); targetType != nil {
		_, isMethodlessNamed := methodlessNamedFuncSignature(targetType)
		_, isAnonSig := targetType.(*types.Signature)

		if isMethodlessNamed || isAnonSig {
			typeExpr = v.getCSTypeName(targetType)
		}
	}

	return fmt.Sprintf("%s._<%s>(%s)", v.convExpr(typeAssertExpr.X, nil), typeExpr, safeAssertDescriminator)
}

// recordAssertConcreteImplementers records a GoImplement for every concrete package type that
// implements BOTH the (non-empty) source interface and the named target interface of an
// interface-to-interface type assertion `x.(T)`. A named-interface assertion has no run-time
// duck-typing path (builtin.cs TryTypeAssert resolves named interfaces only through the adapter
// registry), so the concrete dynamic type must carry a compile-time adapter or the assertion
// panics. The dynamic type of `x` necessarily implements the source interface (that is x's static
// type), so filtering the package's concrete types on the source interface as well as the target
// keeps the recorded set to exactly the types the assertion can succeed on — for a package like
// hash/adler32 that is just its `digest`, whose `*digest` implements both hash.Hash32 (the source)
// and encoding.BinaryMarshaler (the target). Both the value and pointer forms are probed so a
// pointer-receiver implementation (`func (d *digest) MarshalBinary()`) is recorded against `*digest`
// (Pointer = true). Known limitation: only the current package's scope is scanned, so an external
// (`package X_test`) assertion whose concrete type lives in the package under test, or a dynamic
// type imported from a third package, is not covered here — those remain follow-ups.
func (v *Visitor) recordAssertConcreteImplementers(sourceType, targetType types.Type) {
	if v.pkg == nil {
		return
	}

	sourceInterface, ok := sourceType.Underlying().(*types.Interface)
	if !ok {
		return
	}

	targetInterface, ok := targetType.Underlying().(*types.Interface)
	if !ok {
		return
	}

	scope := v.pkg.Scope()

	for _, name := range scope.Names() {
		named, ok := scope.Lookup(name).Type().(*types.Named)
		if !ok {
			continue
		}

		// Concrete (non-interface), non-generic types only — those can be an assertion's dynamic value.
		if _, isInterfaceType := named.Underlying().(*types.Interface); isInterfaceType {
			continue
		}

		if named.TypeParams() != nil {
			continue
		}

		// Filter on the SOURCE interface: only a type that can be the assertion's dynamic value
		// (its value OR pointer form implements the source interface) is recordable — this keeps
		// the recorded set to exactly the types the assertion can succeed on. The target probe and
		// the value-then-pointer record are delegated to the shared recordIfImplements helper.
		if !types.Implements(named, sourceInterface) && !types.Implements(types.NewPointer(named), sourceInterface) {
			continue
		}

		v.recordIfImplements(named, targetInterface, targetType)
	}
}

// recordIfImplements records a GoImplement pairing `named` (or its pointer form) with the named
// interface identified by recordTarget when the type's method set satisfies iface. The VALUE form
// is probed first; if it does not satisfy iface the POINTER form is tried, so a pointer-receiver
// implementation records against *named (the ж<T> shape the attribute writer emits as
// `Pointer = true`). Recording runs through convertToInterfaceType in record-only mode (empty
// exprResult), reusing its recordableBase/recordable gates and the writePackageInfoFile HashSet
// dedup unchanged. Shared by the assertion-site recorder (recordAssertConcreteImplementers) and
// the declaration-site recorder (recordLocalConcreteImplementers in visitInterfaceType.go).
func (v *Visitor) recordIfImplements(named *types.Named, iface *types.Interface, recordTarget types.Type) {
	// The go2cs-gen interface adapter forwards each interface method to a DIRECT method of the
	// concrete or to a SINGLE embedded-field hop (`this.Field.M()`). A method the concrete provides
	// only by a deeper promotion has no such target, so the generated adapter references a
	// non-existent member: an EMBEDDED-INTERFACE-field method (CS1929 — context.afterFuncCtx, whose
	// Deadline promotes from cancelCtx's embedded Context interface), or a method reached through
	// TWO OR MORE embedded structs (CS1503 — a rig{Device{Sensor}} whose Label lives on the
	// twice-embedded Sensor is forwarded as `Label(this.Device)`, one hop short). Recording such a
	// pair yields a broken adapter, so skip it — the concrete already carries the interface through
	// embedding and no site needs the nominal adapter (base records none, so this drops zero
	// pre-existing records and only ever shrinks the recorded set, keeping the corpus green).
	if v.adapterCannotForward(named, iface) {
		return
	}

	if types.Implements(named, iface) {
		v.convertToInterfaceType(recordTarget, named, "")
		return
	}

	pointerType := types.NewPointer(named)

	if types.Implements(pointerType, iface) {
		v.convertToInterfaceType(recordTarget, pointerType, "")
	}
}

// adapterCannotForward reports whether the go2cs-gen interface adapter for `named` -> `iface` would
// reference a non-existent member for ANY of iface's methods. The generator forwards a method only
// to a target it can name in ONE step: a method whose receiver IS `named` (a direct method), or a
// method reachable through a SINGLE embedded field (`this.Field.M()`, so the receiver is the type
// of one of named's DIRECT fields). Anything deeper is not forwardable:
//   - a method PROMOTED FROM AN EMBEDDED INTERFACE field carries an INTERFACE receiver (go/types
//     sets it to the embedded interface), which is never a direct concrete method — CS1929.
//   - a method promoted through TWO OR MORE embedded structs has a receiver that is neither `named`
//     nor any direct-field type; the generator hops to the first field and calls the method there,
//     one level too shallow — CS1503.
// Addressable lookup (the `true` arg) folds in pointer-receiver methods so the value and pointer
// record forms are judged consistently.
func (v *Visitor) adapterCannotForward(named *types.Named, iface *types.Interface) bool {
	var directFieldTypes []types.Type

	if structType, ok := named.Underlying().(*types.Struct); ok {
		for i := 0; i < structType.NumFields(); i++ {
			directFieldTypes = append(directFieldTypes, stripPointerType(structType.Field(i).Type()))
		}
	}

	for i := 0; i < iface.NumMethods(); i++ {
		method := iface.Method(i)

		obj, _, _ := types.LookupFieldOrMethod(named, true, v.pkg, method.Name())

		fn, ok := obj.(*types.Func)

		if !ok {
			continue
		}

		recv := fn.Signature().Recv()

		if recv == nil {
			continue
		}

		recvType := stripPointerType(recv.Type())

		// A method PROMOTED FROM AN EMBEDDED INTERFACE field carries an interface receiver; the
		// nominal adapter cannot forward it (CS1929), so skip the pair regardless of the field
		// depth — the concrete already carries the interface through the embed.
		if _, isInterface := recvType.Underlying().(*types.Interface); isInterface {
			return true
		}

		// A DIRECT method of named forwards as `this.M()` — always fine.
		if types.Identical(recvType, named) {
			continue
		}

		// A method reached through exactly ONE embedded field forwards as `this.Field.M()`; the
		// receiver is that field's own type. A method two-or-more embeds deep has a receiver that
		// is neither named nor any direct-field type — the generator hops only to the first field
		// and calls the method there, one level too shallow (CS1503) — so skip the pair.
		forwardable := false

		for _, fieldType := range directFieldTypes {
			if types.Identical(fieldType, recvType) {
				forwardable = true
				break
			}
		}

		if !forwardable {
			return true
		}
	}

	return false
}

// stripPointerType unwraps a single pointer indirection, yielding the pointee for a *T and the type
// itself otherwise — used to compare a method receiver / embedded field against the underlying
// named type regardless of pointer form.
func stripPointerType(t types.Type) types.Type {
	if pointer, ok := t.(*types.Pointer); ok {
		return pointer.Elem()
	}

	return t
}
