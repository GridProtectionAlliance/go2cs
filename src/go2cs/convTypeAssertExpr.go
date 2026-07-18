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

		// Value form first, then the pointer form for pointer-receiver method sets.
		if types.Implements(named, sourceInterface) && types.Implements(named, targetInterface) {
			v.convertToInterfaceType(targetType, named, "")
			continue
		}

		pointerType := types.NewPointer(named)

		if types.Implements(pointerType, sourceInterface) && types.Implements(pointerType, targetInterface) {
			v.convertToInterfaceType(targetType, pointerType, "")
		}
	}
}
