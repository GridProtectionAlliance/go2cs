package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
)

// isNarrowIntegerKind reports whether the basic kind is a sub-`int`-width integer (int8/uint8/
// int16/uint16). C# promotes arithmetic on these to `int`, whereas Go evaluates them at their own
// width (with overflow wrapping), so a narrow-typed result needs an explicit cast back.
func isNarrowIntegerKind(kind types.BasicKind) bool {
	switch kind {
	case types.Int8, types.Uint8, types.Int16, types.Uint16:
		return true
	}

	return false
}

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr, context LambdaContext) string {
	// Immediately-invoked, no-argument function literal (IIFE): `func(){ … }()`. A bare C#
	// lambda cannot be invoked directly (CS0149), and the literal may use defer/recover that
	// must be scoped to itself. Emit it as a `func((defer, recover) => body)` execution-context
	// call, which both wraps (its own defer/recover scope) and runs immediately — so no trailing
	// call `()` is appended. (Argument-taking IIFEs are handled by the normal call path.)
	if funcLit, ok := callExpr.Fun.(*ast.FuncLit); ok && !context.deferOrGoCall {
		// A C# lambda cannot be invoked directly, so an IIFE is cast to a delegate and then
		// called: `((Action)(() => …))()`, `((Func<nint>)(() => …))()`, `((Action<nint>)(n =>
		// …))(7)`, etc. The literal's body picks up its own `func((defer, recover) => …)`
		// execution context only when it actually uses defer/recover (see convFuncLit). Variadic
		// literals fall through to the normal path (delegate type would need a params array).
		if sig, ok := v.info.TypeOf(funcLit).(*types.Signature); ok && !sig.Variadic() {
			iifeContext := DefaultLambdaContext()
			iifeContext.isIIFE = true

			lambda := v.convFuncLit(funcLit, iifeContext)
			args := v.convExprList(callExpr.Args, callExpr.Lparen, DefaultCallExprContext())

			return fmt.Sprintf("((%s)(%s))(%s)", v.iifeDelegateType(sig), lambda, args)
		}
	}

	funcType := v.getType(callExpr.Fun, false)

	// Check if the call is a type conversion
	if ok, targetTypeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]

		// `(*Base)(p)` reinterpreting a pointer to a DEFINED type as a pointer to its underlying
		// named type — `(*atomic.Uint32)(c)` where `c` is `*counter` and `type counter atomic.Uint32`
		// (runtime/mprof goroutineProfileStateHolder). C# has no `ж<counter> → ж<atomic.Uint32>`
		// conversion (distinct generic instantiations); the inherited [GoType] wrapper only provides a
		// VALUE conversion (counter → atomic.Uint32). Box that converted value so the pointer-receiver
		// methods (Load/Store/…) resolve: `Ꮡ((atomic.Uint32)(c))`. Done before checkForImplicitConversion
		// so it does not record a spurious counter→ж<atomic.Uint32> indirect conversion. (The address is
		// of a copy — the atomic intrinsics behind it are asm stubs, so this is about compilable C#.)
		if resultPtr, ok := v.info.TypeOf(callExpr).(*types.Pointer); ok {
			if argPtr, ok := v.info.TypeOf(arg).(*types.Pointer); ok {
				resultElem := resultPtr.Elem()
				argElem := argPtr.Elem()

				baseNamed, okBaseNamed := resultElem.(*types.Named)
				defNamed, okDefNamed := argElem.(*types.Named)

				// A pointer reinterpret `(*Base)(p)` between two types with an IDENTICAL underlying, boxed
				// as a COPY of the [GoType] VALUE conversion. `ж<Base>` and `ж<Def>` are distinct generic
				// instantiations with no C# conversion between them, and the atomic intrinsics behind these
				// are asm stubs, so this is about producing compilable C#, not write-through. Two shapes:
				//   - Named ↔ Named: `(*pinnerBits)(*gcBits)` (type pinnerBits gcBits).
				//   - Named → its underlying BASIC: `(*uint64)(*lfstack)` — an atomic op on a named numeric
				//     `type lfstack uint64` (also sweepClass/profAtomic/sysMemStat), where the address of
				//     the named field flows to `atomic.Load64((*uint64)(head))`. `ж<lfstack> → ж<uint64>`
				//     has no conversion (CS0030); box a copy of the value conversion `(uint64)(head)`.
				namedToNamed := okBaseNamed && okDefNamed && baseNamed != defNamed &&
					types.Identical(baseNamed.Underlying(), defNamed.Underlying())

				_, resultIsBasic := resultElem.(*types.Basic)
				namedToBasic := resultIsBasic && okDefNamed && types.Identical(resultElem, defNamed.Underlying())

				// `(*[4]uint64)(&s.s)` / `(*fiatScalarNonMontgomeryDomainFieldElement)(&s.s)` — pointer
				// reinterprets of an array-backed DEFINED type (crypto/internal/edwards25519 scalar.go):
				// to its unnamed underlying array, or to a SIBLING defined type written over the SAME
				// unnamed array. `ж<Def>` has no conversion to `ж<array<E>>`/`ж<Sibling>` (distinct
				// generic instantiations — CS0030), and the sibling VALUE conversion the namedToNamed
				// route emits does not exist either (each [GoType("[N]elem")] wrapper converts only
				// to array<E>, and C# never chains two user-defined conversions). Unlike the copy-boxed
				// atomic shapes below, these fiat sites WRITE through the reinterpreted pointer
				// (fiatScalarFromBytes parses the scalar INTO &s.s on a virgin receiver), so the route
				// must share element storage: the wrapper's `Value` property invoked THROUGH THE REF
				// (ж<T>.Value is ref-returning) materializes the lazy array<E> backing on the ORIGINAL
				// storage and returns an array<E> struct sharing its T[]; boxing that (Ꮡ) yields a
				// pointer whose element reads AND writes flow through. Whole-value writes (`*p = q`)
				// rebind only the boxed copy — acceptable, matching the documented array<T> model.
				// The written-RHS gate keeps chain-defined types (`type pallocBits pageBits`, whose
				// wrappers DO convert to each other) on the namedToNamed route below, byte-identical.
				_, resultIsArray := resultElem.(*types.Array)
				namedToArray := resultIsArray && okDefNamed &&
					types.Identical(resultElem, defNamed.Underlying()) &&
					writtenRHSIsUnnamedArray(defNamed)

				namedSiblingArrays := namedToNamed &&
					writtenRHSIsUnnamedArray(defNamed) && writtenRHSIsUnnamedArray(baseNamed)

				if namedToArray || namedSiblingArrays {
					argExpr := v.convExpr(arg, nil)

					if v.needsParentheses(arg) {
						argExpr = fmt.Sprintf("(%s)", argExpr)
					}

					// A deref-aliased pointer param/receiver renders as the wrapper VALUE — a
					// ref-local alias of the real storage, so its `Value` property already runs in
					// place. A genuine box (field box `Ꮡs.of(…)`, heap box `Ꮡss`) derefs through the
					// ref-returning ж<T>.Value first — NOT `~` (operator ~ returns a COPY, which would
					// materialize a virgin backing on the temp and orphan every write — the
					// SetCanonicalBytes-on-a-fresh-Scalar shape).
					if !v.exprIsDerefAliasedPointer(arg) {
						argExpr = fmt.Sprintf("%s.Value", argExpr)
					}

					if namedToArray {
						return fmt.Sprintf("%s(%s.Value)", AddressPrefix, argExpr)
					}

					baseName := convertToCSTypeName(v.getTypeName(resultElem, false))

					return fmt.Sprintf("%s((%s)(%s.Value))", AddressPrefix, baseName, argExpr)
				}

				if namedToNamed || namedToBasic {
					baseName := convertToCSTypeName(v.getTypeName(resultElem, false))
					argExpr := v.convExpr(arg, nil)

					// The [GoType] VALUE conversion `(Base)(Def)` operates on the underlying VALUE. A
					// deref-aliased pointer param/receiver already renders as that value (`Δp`/`head`), so
					// it casts directly. But a genuine pointer expression — a call result (`newMarkBits(…)`
					// returning `*gcBits`), a local box, a pointer field — renders as the box `ж<Def>`,
					// which has no conversion to the Base VALUE (CS0030, runtime/pinner
					// `(*pinnerBits)(newMarkBits(…))`). Dereference the box first so the value conversion
					// binds. Both forms then box a COPY (`Ꮡ`) — the shared-underlying value has no
					// write-through to lose (the atomic intrinsics are asm stubs).
					if !v.exprIsDerefAliasedPointer(arg) {
						argExpr = fmt.Sprintf("%s%s", PointerDerefOp, argExpr)
					}

					return fmt.Sprintf("%s((%s)(%s))", AddressPrefix, baseName, argExpr)
				}
			}
		}

		targetTypeName = convertToCSTypeName(targetTypeName)
		expr := v.checkForImplicitConversion(funcType, arg, targetTypeName)

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency. Although C# allows circular dependencies, NuGet does not. If the
		// target happens to not be an unsafe pointer, the cast is still safe since all pointer
		// types support this cast operation.
		//
		// The same routing is required for a pointer-type conversion `(*T)(p)` whose SOURCE is a raw
		// address — an unsafe.Pointer or uintptr — even when the deref path did not set isPointerCast.
		// `unsafe.Pointer` is the golib `Pointer : ж<uintptr>`, so `(ж<T>)p` needs the two user-defined
		// conversions Pointer→uintptr→ж<T>, which C# will not chain in a single cast (CS0030). Routing
		// through uintptr — `(ж<T>)(uintptr)(p)` — reads the T at p's address via golib's
		// `implicit operator ж<T>(uintptr) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => val`), which
		// IS Go's `*(*T)(p)` semantics for native memory. Two shapes miss isPointerCast and need this: the
		// bare-argument form `atomicwb((*unsafe.Pointer)(ptr), …)` and the extra-paren deref
		// `*((*unsafe.Pointer)(k))` (convStarExpr's CallExpr branch sees a ParenExpr, not the CallExpr).
		// The pointer-to-NAMED-type value conversion `(*atomic.Uint32)(counterPtr)` is handled and returned
		// above (its arg is a *types.Pointer, not a raw address), so it never reaches here.
		if context.isPointerCast || v.isRawAddressPointerConversion(callExpr, arg) {
			return fmt.Sprintf("(%s)(uintptr)(%s)", targetTypeName, expr)
		}

		// unsafe.Pointer(ptr) where ptr is a Go pointer (`*T`, emitted as the managed box `ж<T>`).
		// A managed box has no conversion to the numeric Pointer (`ж<uintptr>`), so a plain cast is
		// CS0030 — e.g. `unsafe.Pointer(&u.value)` → `(@unsafe.Pointer)(Ꮡu.of(…))`. Pin the
		// pointed-to storage instead via the golib helper: `@unsafe.Pointer.FromRef(ref (box).Value)`.
		// (`uintptr`/`unsafe.Pointer` args are not boxes and keep the implicit-cast path below; only
		// a genuine pointer arg needs this.) The numeric address is not GC-stable — the same caveat
		// that applies to every unsafe.Pointer-as-uintptr use; the atomic intrinsics consuming it are
		// asm stubs, so this is purely about producing compilable C#.
		if targetTypeName == "@unsafe.Pointer" {
			// A NAMED-numeric arg whose underlying is uintptr — `unsafe.Pointer(v)` where v is
			// `type gclinkptr uintptr` (runtime malloc/mcache/stack: the allocator's span-address
			// arithmetic type). The [GoType] wrapper only converts named↔underlying, and golib's
			// Pointer only converts from uintptr, so the direct cast needs a two-op chain C# will
			// not build (CS0030); hop through the underlying — `((@unsafe.Pointer)(uintptr)v)`.
			// (Go permits exactly uintptr-underlying types here, so the gate matches the language.)
			if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok {
				if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Kind() == types.Uintptr {
					if v.needsParentheses(arg) {
						return fmt.Sprintf("((@unsafe.Pointer)(uintptr)(%s))", expr)
					}

					return fmt.Sprintf("((@unsafe.Pointer)(uintptr)%s)", expr)
				}
			}

			if _, isPtr := v.info.TypeOf(arg).(*types.Pointer); isPtr {
				// A deref-aliased pointer PARAMETER or RECEIVER renders as the pointed-to VALUE alias
				// (`ref var pc0 = ref Ꮡpc0.Value`), not a box — `.Value` on it is CS1061 (`nuint` has no
				// `val`; runtime select.go `unsafe.Pointer(pc0)` / heapdump.go `unsafe.Pointer(pstk)`,
				// both `*uintptr` params). The alias is itself a ref-local into the boxed storage, so
				// take its ref directly. A genuine box (a local, field, call result) keeps `.Value`.
				if v.exprIsDerefAliasedPointer(arg) {
					return fmt.Sprintf("@unsafe.Pointer.FromRef(ref %s)", expr)
				}

				return fmt.Sprintf("@unsafe.Pointer.FromRef(ref (%s).Value)", expr)
			}
		}

		if targetTypeName == "@string" {
			// Check if it is a generic type parameter - Go will have already
			// validated constraint, so we can just cast to string directly
			if _, ok := v.getType(arg, false).(*types.TypeParam); ok {
				return fmt.Sprintf("new %s(%s)", targetTypeName, expr)
			}
		}

		// A conversion to a NAMED FUNC type — `metricReader(read)` where `type metricReader
		// func() uint64` (runtime metrics.go) — targets a C# DELEGATE declaration
		// (`internal delegate uint64 metricReader();`). Distinct delegate types have no cast
		// conversion (CS0030 from `Func<ulong>`); C# converts via DELEGATE CREATION:
		// `new metricReader(read)` (accepts a compatible delegate or method group).
		if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				return fmt.Sprintf("new %s(%s)", targetTypeName, expr)
			}
		}

		// A conversion of a string LITERAL to a named type whose underlying is `string`
		// (e.g. `errorString("makeslice: len out of range")`, `type errorString string`): the
		// literal renders as a `u8` ReadOnlySpan<byte>, which has no conversion to the named type
		// (CS0030). Route it through `@string` — which has an implicit conversion FROM the u8 span
		// and TO which the named type converts — `((errorString)(@string)"…"u8)`.
		if basicLit, ok := arg.(*ast.BasicLit); ok && basicLit.Kind == token.STRING && targetTypeName != "@string" {
			if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
				if basic, ok := named.Underlying().(*types.Basic); ok && basic.Kind() == types.String {
					return fmt.Sprintf("((%s)(@string)%s)", targetTypeName, expr)
				}
			}
		}

		// A conversion to a MANUALLY-converted type (see manualTypeOperations.go) from an
		// unsafe.Pointer — `guintptr(unsafe.Pointer(newg))` (runtime proc.go). The manual type
		// stores the managed referent DIRECTLY, so the numeric cast chain
		// `(Δguintptr)(uintptr)new @unsafe.Pointer(newg)` would lose it; unwrap the
		// unsafe.Pointer conversion and construct from its operand — `new Δguintptr(newg)` —
		// the referent-carrying box expression. (Every current call site passes a pointer LOCAL
		// or an explicit box; a deref-aliased param operand would need its box form and will
		// surface as a loud compile error, not a silent number.) Non-pointer args (e.g. a zero
		// literal) fall through to the named-numeric route against the manual type's operators.
		if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
			if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg && v.isManualType(obj.Name()) {
				if argCall, ok := arg.(*ast.CallExpr); ok && v.callExprIsTypeConversion(argCall) && len(argCall.Args) == 1 {
					if argBasic, ok := v.info.TypeOf(argCall).(*types.Basic); ok && argBasic.Kind() == types.UnsafePointer {
						return fmt.Sprintf("new %s(%s)", targetTypeName, v.convExpr(argCall.Args[0], nil))
					}
				}
			}
		}

		// A conversion to a NAMED numeric type (`type arenaIdx uint`, `type traceArg uint64`) whose
		// arg is not already exactly its underlying basic — `arenaIdx(1 << b)` (int arg),
		// `traceArg(procs)` (int32 arg). The `[GoType]` conversion operator only converts between the
		// named type and its EXACT underlying (`nuint` / `uint64`), so a plain `(arenaIdx)(intExpr)`
		// is CS0030. Coerce through the underlying first — `((arenaIdx)(nuint)(1 << b))` — a numeric
		// C# cast that is exactly Go's conversion semantics. Skipped when the arg is already the
		// underlying basic (the existing cast already binds → no churn).
		if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
			if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
				argType := v.info.TypeOf(arg)

				// An IDENTITY conversion to the same named numeric type — `arenaIdx(x)` where x is
				// already `arenaIdx` — is a Go no-op. This happens for an untyped-constant shift that
				// adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types
				// already types as arenaIdx, so convExpr(arg) has emitted `((arenaIdx)((nuint)1 << b))`),
				// and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expr in another
				// `((arenaIdx)…)` cast just doubles it; return the converted arg as-is.
				if argType != nil && types.Identical(argType, named) {
					return expr
				}

				// A NAMED numeric arg needs the underlying hop even when its underlying is
				// IDENTICAL to the target's — `hex(work.full)` where full is `type lfstack uint64`
				// and hex is uint64 (runtime mgc.go): the direct `(Δhex)full` is still a two-op
				// user-defined chain (lfstack→uint64, uint64→Δhex) that C# won't build (CS0030).
				// The identical-underlying skip below is right only for a BASIC arg, where the
				// exact [GoType] operator binds directly.
				argIsDistinctNamedNumeric := false

				if argNamed, ok := types.Unalias(argType).(*types.Named); ok && argType != nil {
					if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 {
						argIsDistinctNamedNumeric = true
					}
				}

				if argType == nil || argIsDistinctNamedNumeric || !types.Identical(argType.Underlying(), basic) {
					underlyingCS := v.getCSTypeName(basic)
					inner := expr

					// When the arg is ITSELF a named numeric whose underlying differs from the target's
					// underlying — `hex(off)` where `off` is `abi.NameOff` (int32) and `hex` is uint64 —
					// the intermediate `(uint64)NameOff` cast is itself CS0030 (the [GoType] wrapper only
					// converts NameOff↔int32). Unwrap the arg through ITS underlying first, so the chain
					// is named→argUnderlying (operator) → targetUnderlying (numeric) → target (operator):
					// `((hex)(uint64)(int32)off)`. (types.Unalias resolves the runtime's aliased offsets.)
					if argNamed, ok := types.Unalias(argType).(*types.Named); ok {
						if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 && !types.Identical(argBasic, basic) {
							if v.needsParentheses(arg) {
								inner = fmt.Sprintf("(%s)(%s)", v.getCSTypeName(argBasic), expr)
							} else {
								inner = fmt.Sprintf("(%s)%s", v.getCSTypeName(argBasic), expr)
							}
						}
					}

					if v.needsParentheses(arg) {
						return fmt.Sprintf("((%s)(%s)(%s))", targetTypeName, underlyingCS, inner)
					}

					return fmt.Sprintf("((%s)(%s)%s)", targetTypeName, underlyingCS, inner)
				}
			}
		}

		// The MIRROR of the branch above: a conversion FROM a NAMED numeric type TO a DIFFERENT basic
		// numeric type — `uint64(t.Str)` where `Str` is `abi.NameOff` (named int32), `int(idx)` where
		// `idx` is a `num:nuint` named type. The named type's [GoType] wrapper only converts between it
		// and its EXACT underlying (`NameOff ↔ int32`), so a plain `(ulong)NameOff` / `(nint)idx` is
		// CS0030. Route through the underlying first — `((ulong)(int)t.Str)` / `((nint)(nuint)idx)` — the
		// named→basic [GoType] operator followed by a numeric C# cast (exactly Go's conversion semantics).
		// Skipped when the target basic IS the named type's underlying (the exact operator binds → no churn)
		// and when the arg is not a named numeric (a plain basic→basic cast already works).
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok && targetBasic.Info()&types.IsNumeric != 0 {
			// types.Unalias resolves a Go type alias (`type nameOff = abi.NameOff`) to the named type it
			// aliases — in Go 1.23 `TypeOf` returns a `*types.Alias`, so a bare `.(*types.Named)` would
			// miss the runtime's aliased `abi` offset types (`nameOff`/`typeOff`/`textOff`).
			if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok {
				if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 {
					if !types.Identical(argBasic, targetBasic) {
						underlyingCS := v.getCSTypeName(argBasic)

						// No outer parentheses: the conversion target is a BASIC C# type, whose result
						// can never be the receiver of a postfix `.`/`[]`/invocation (basic types expose
						// no Go-callable members and the converter emits none), so C# cast precedence —
						// higher than every binary operator — already binds correctly in any surrounding
						// context. `(ulong)(int)t.Str` parses as `(ulong)((int)(t.Str))` (postfix `.`
						// binds before the cast). See the basic-target note at the general return below.
						if v.needsParentheses(arg) {
							return fmt.Sprintf("(%s)(%s)(%s)", targetTypeName, underlyingCS, expr)
						}

						return fmt.Sprintf("(%s)(%s)%s", targetTypeName, underlyingCS, expr)
					}
				}
			}
		}

		// A conversion to a BASIC C# type omits the outer parentheses around the whole cast: the
		// result of `(uint64)x` / `(nint)y` can never be the receiver of a postfix `.`/`[]`/invocation
		// (Go basic types expose no callable members and the converter emits none on them), and the C#
		// cast operator outranks every binary operator, so `(uint64)x` binds correctly unparenthesized
		// in any surrounding context (`f((uint64)a)`, `return (uint64)a;`, `(uint64)a << n`). This keeps
		// the common `uint64(a)` → `(uint64)a` close to the Go source. A NAMED-type target keeps the
		// outer parens — its result CAN be member-accessed (`Named(x).Method()`), which is parent-
		// context-dependent and not decidable here, so the defensive wrap is retained.
		//
		// `string` is the exception among basic types: its C# representation is the golib `@string`
		// STRUCT, which IS member-accessible — it exposes methods, an indexer, and is the receiver of
		// the variadic-string spread `string(r)...` → `((@string)(rune)r).ꓸꓸꓸ`. Dropping the outer wrap
		// there reparses `.ꓸꓸꓸ`/`[]`/`.Method()` against the cast's INNER operand (`(@string)(rune)r.ꓸꓸꓸ`
		// binds `.ꓸꓸꓸ` to `r`, CS1061). So treat a `string` target like a named type and keep the wrap.
		basicTarget, _ := v.info.TypeOf(callExpr).(*types.Basic)
		targetIsBasic := basicTarget != nil && basicTarget.Kind() != types.String

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
			if targetIsBasic {
				return fmt.Sprintf("(%s)(%s)", targetTypeName, expr)
			}

			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
		}

		if targetIsBasic {
			return fmt.Sprintf("(%s)%s", targetTypeName, expr)
		}

		return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
	}

	constructType := ""

	if v.isConstructorCall(callExpr) {
		constructType = "new "
	}

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	callExprContext := DefaultCallExprContext()
	callExprContext.callArgs = context.callArgs
	// Hoist a func-literal argument's capture declarations to the enclosing statement (a
	// `var mʗ1 = m;` statement is invalid inside an argument list). convExprList builds a
	// LambdaContext from this builder for each argument so the func-literal arg's convFuncLit
	// writes its decls here instead of inline.
	callExprContext.deferredDecls = context.deferredDecls

	// Check if the call is using the spread operator "..."
	if callExpr.Ellipsis.IsValid() {
		callExprContext.hasSpreadOperator = true
	}

	var replacementArgs []string
	funcSignature := v.getFunctionSignature(callExpr)

	if funcSignature != nil {
		// Check if any parameters of callExpr.Fun are interface or pointer types
		params := funcSignature.Params()

		for i := range params.Len() {
			var paramType types.Type
			paramHasArg := callExpr.Args != nil && i < len(callExpr.Args)

			if paramHasArg {
				// Check if the parameter type is an anonymous struct
				if structType, exprType := v.extractStructType(callExpr.Args[i]); structType != nil && !v.liftedTypeExists(structType) {
					v.indentLevel++
					v.visitStructType(structType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(callExpr.Args[i]); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
				}

				// A deferred/go method-value call routes its args through deferǃ(Action<T>, T arg, …),
				// where T must unify from BOTH the method parameter and the argument. An untyped numeric
				// constant (e.g. `0`) renders as C# int and won't unify with a wider concrete parameter
				// (e.g. atomic.Uint64.Store(ulong)) — cast it to the parameter type. Only the method-value
				// form (no rendered lambda params) is affected; the lambda form passes args into a lambda.
				if context.callArgs != nil && !context.renderParams {
					deferParamType := params.At(i).Type()
					if basic, ok := deferParamType.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
						if v.isUntypedNumericConstArg(callExpr.Args[i]) {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}
							callExprContext.castArgToType[i] = convertToCSTypeName(v.getTypeName(deferParamType, false))
						}
					}

					// An untyped-nil argument renders as golib `nil` (NilType), which unifies the
					// deferǃ/goǃ type parameter as NilType and breaks the method-group match
					// (CS0123 — exec_windows DuplicateHandle). Cast it to the parameter's C# type:
					// ж<T>/interface/slice/map/channel all carry an implicit NilType conversion,
					// so `(ж<ΔHandle>)(nil)` binds and keeps the visible `nil`.
					if tv, ok := v.info.Types[callExpr.Args[i]]; ok && tv.IsNil() {
						if callExprContext.castArgToType == nil {
							callExprContext.castArgToType = make(map[int]string)
						}
						callExprContext.castArgToType[i] = v.getCSTypeName(deferParamType)
					}
				}
			}

			callExprContext.u8StringArgOK[i] = true
			funcName := v.convExpr(callExpr.Fun, nil)

			// Handle builtin functions that take `...Type` parameters, treat as `interface{}`
			var ok bool

			if funcName == "print" || funcName == "println" {
				paramType = types.NewInterfaceType(nil, nil)
			} else if paramType, ok = getParameterType(funcSignature, i); !ok {
				continue
			}

			if paramHasArg {
				argType := v.getType(callExpr.Args[i], false)
				targetType := paramType
				replacementArg := v.checkForDynamicStructs(argType, targetType)

				// If a replacement argument is found, add it to the replacementArgs slice,
				// creating the slice if it doesn't exist yet
				if len(replacementArg) > 0 {
					if replacementArgs == nil {
						replacementArgs = make([]string, params.Len())
					}

					replacementArgs[i] = replacementArg
				}
			}

			// A CONSTRAINED SLICE TYPE PARAMETER passed where a concrete slice<E> parameter is
			// expected — Go assignability (S ~[]E is assignable to []E; the slices package's
			// rotateRight(s[m:i], …)/pdqsortOrdered(x, …) helper chain) — materializes through
			// the SHARING slice<T>(ISlice<T>) constructor: `new slice<E>(arg)`. A cast cannot
			// apply (the source is interface-constrained; C# forbids user-defined conversions
			// from interfaces), and the constructor shares backing, preserving Go aliasing.
			if paramHasArg && i < len(callExpr.Args) {
				if paramSlice, ok := types.Unalias(paramType).(*types.Slice); ok {
					if argTP, ok := types.Unalias(v.info.TypeOf(callExpr.Args[i])).(*types.TypeParam); ok && typeParamSliceCore(argTP) != nil {
						if callExprContext.wrapArgWithNew == nil {
							callExprContext.wrapArgWithNew = make(map[int]string)
						}

						callExprContext.wrapArgWithNew[i] = fmt.Sprintf("slice<%s>", convertToCSTypeName(v.getTypeName(paramSlice.Elem(), false)))
					}
				}
			}

			// A Go string passed to a generic type-parameter parameter must be cast to
			// golib's `@string` (a struct). Without a target type, a bare string literal
			// converts to a .NET `System.String`, so C# infers the type argument as
			// `string` — which fails the `new()` constraint go2cs adds to type parameters
			// (and mismatches `@string` args). e.g. `First("A", "B")` for `func First[T any](v ...T)`.
			if paramHasArg {
				if _, isTypeParam := paramType.(*types.TypeParam); isTypeParam {
					// A variadic type parameter receives every trailing argument, so flag
					// all of them (the loop only iterates the declared parameters).
					lastArg := i

					if funcSignature.Variadic() && i == params.Len()-1 {
						lastArg = len(callExpr.Args) - 1
					}

					for j := i; j <= lastArg; j++ {
						if v.isStringType(callExpr.Args[j]) {
							callExprContext.useGoStringArg[j] = true
						}
					}
				}
			}

			// A narrow-integer parameter (int8/uint8/int16/uint16) receiving a binary/unary arithmetic
			// argument: Go evaluates `a+b`/`^a` at the operand's narrow width (with overflow wrapping),
			// but C# promotes sub-int integer arithmetic to `int`, so the result needs an explicit cast
			// back to the parameter type — both to compile (the implicit int→narrow conversion is
			// rejected, CS1503) and to preserve Go's wrap semantics. Gated on the argument's Go type
			// already matching the parameter (so Go accepts it without a conversion) and on it being an
			// arithmetic expression (a bare ident/selector is already the narrow type).
			if paramHasArg {
				if paramBasic, ok := paramType.Underlying().(*types.Basic); ok && isNarrowIntegerKind(paramBasic.Kind()) {
					switch callExpr.Args[i].(type) {
					case *ast.BinaryExpr, *ast.UnaryExpr:
						if argType := v.getType(callExpr.Args[i], false); argType != nil && types.Identical(argType, paramType) {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							callExprContext.castArgToType[i] = convertToCSTypeName(v.getTypeName(paramType, false))
						}
					}
				}
			}

			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast {
				callExprContext.u8StringArgOK[i] = false

				if !isEmpty {
					callExprContext.interfaceTypes[i] = paramType
				}
			} else if paramHasArg && (isPointer(paramType) || v.instantiatedParamIsPointer(callExpr, paramType, i)) && !(callExprContext.hasSpreadOperator && i == params.Len()-1) {
				// paramHasArg guards Args[i]: a variadic pointer parameter called with no
				// trailing arguments (e.g. `In(r)` for `In(r rune, ...*RangeTable)`) has no
				// arg at the variadic index, so indexing Args[i] would panic.
				//
				// getParameterType returns the variadic *element* type, so isPointer is true
				// for `...*T`. When the call spreads a slice into the variadic (`f(s...)`), the
				// argument is the whole slice, not a single element pointer, so the element
				// address-of treatment (which would emit `Ꮡs`) must be skipped.
				// An `unsafe.Pointer` argument passed to an `unsafe.Pointer` parameter is passed as the
				// `@unsafe.Pointer` struct directly — NOT reduced to its inner `uintptr` via `.Value`.
				// `.Value` (a uintptr) converts implicitly to BOTH the `@unsafe.Pointer` parameter AND any
				// same-named method's `ж<T>` overload (golib has uintptr↔both), so the call goes
				// ambiguous — e.g. `add(p, x)` between the free `add(@unsafe.Pointer,…)` and a
				// `notInHeap.add(ж<…>,…)` extension (CS0121). The struct is an exact match for the
				// parameter, so it disambiguates.
				paramIsUnsafePtr := false

				if basic, ok := paramType.Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
					paramIsUnsafePtr = true
				}

				argIsUnsafePtr := false

				if argType := v.getType(callExpr.Args[i], false); argType != nil {
					if basic, ok := argType.Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
						argIsUnsafePtr = true
					}
				}

				if paramIsUnsafePtr && argIsUnsafePtr {
					// pass the @unsafe.Pointer struct directly; no argTypeIsPtr / `.Value`
				} else {
					ident := getIdentifier(callExpr.Args[i])

					// A deref-aliased pointer (a parameter, or the current method's direct-ж receiver)
					// passed WHOLE as a pointer argument must be emitted as its box `Ꮡc`, not the value
					// alias `c` (a value cannot bind a `ж<T>` parameter → CS1503). A direct-ж receiver
					// is not an `identIsParameter`, so it needs the explicit receiver check — e.g.
					// `func (c *mcache) prepareForSweep(){ … stackcache_clear(c) }` where `c` also takes
					// a field address (making it direct-ж).
					if !v.isPointer(ident) || v.identIsParameter(ident) || v.exprIsCurrentDirectBoxReceiver(callExpr.Args[i]) {
						callExprContext.argTypeIsPtr[i] = true
					}
				}
			}
		}
	}

	callExprContext.replacementArgs = replacementArgs

	if ident, ok := callExpr.Fun.(*ast.Ident); ok {
		// Go auto-derefs `len(p)`/`cap(p)` for a pointer-to-array; a ж<named-array-wrapper>
		// argument has no golib len/cap overload (the wrapper itself implements IArray, its box
		// does not — CS1503, runtime proc.go's `len(mp.cgoCallers)` where cgoCallers is
		// `*cgoCallers`). Emit the deref explicitly: `len(mp.cgoCallers.Value)`.
		if (ident.Name == "len" || ident.Name == "cap") && len(callExpr.Args) == 1 {
			if ptr, ok := v.info.TypeOf(callExpr.Args[0]).(*types.Pointer); ok {
				if named, ok := types.Unalias(ptr.Elem()).(*types.Named); ok {
					if _, isArray := named.Underlying().(*types.Array); isArray {
						return fmt.Sprintf("%s(%s.Value)", ident.Name, v.convExpr(callExpr.Args[0], nil))
					}
				}
			}
		}

		// Handle make call as a special case
		if ident.Name == "make" {
			typeExpr := callExpr.Args[0]
			typeParam := v.info.TypeOf(typeExpr)
			typeName := convertToCSTypeName(v.getExprTypeName(typeExpr, false))
			remainingArgs := v.convExprList(callExpr.Args[1:], callExpr.Lparen, callExprContext)
			isTypeParam := false

			if typeConstraint, ok := typeParam.(*types.TypeParam); ok {
				typeParam = v.getConstraintType(typeConstraint)
				isTypeParam = typeParam != nil
			}

			if typeParam != nil {
				if _, ok := typeParam.(*types.Chan); ok && !isTypeParam {
					if len(remainingArgs) == 0 {
						remainingArgs = "1"
					}
				}

				// make(T, len[, cap]) for a slice/map/chan — the golib `slice<T>(nint length, nint
				// capacity)` / `map<K,V>(nint capacity)` / `channel<T>(nint capacity)` ctors all take nint.
				// A length/cap/hint whose Go type is an integer with NO implicit C# conversion to nint
				// (`uintptr`/`uint`/`uint32`/`uint64`/`int64` → `nuint`/`uint`/`ulong`/`long`) has no
				// applicable ctor — for a slice, overload resolution falls onto `slice<T>(T[])` (CS1503:
				// `nuint`→`byte[]`, runtime/mbitmap `make([]byte, n/goarch.PtrSize)`); for a map/chan it is
				// a direct `nuint`→`nint` CS1503. Cast such args to nint. A plain `int` (nint) or an untyped
				// constant binds directly and is left alone.
				if !isTypeParam && len(callExpr.Args) > 1 {
					switch typeParam.Underlying().(type) {
					case *types.Slice, *types.Map, *types.Chan:
						remainingArgs = v.makeLenArgs(callExpr.Args[1:])
					}
				}

				if isTypeParam {
					return fmt.Sprintf("make<%s>(%s)", typeName, remainingArgs)
				}

				if v.options.preferVarDecl {
					return fmt.Sprintf("new %s(%s)", typeName, remainingArgs)
				}

				return fmt.Sprintf("new(%s)", remainingArgs)
			}

			v.showWarning("@convCallExpr - unexpected call to 'make' method for type '%s'", typeName)
			return fmt.Sprintf("make\u01C3<%s>(%s)", typeName, remainingArgs)
		}

		// Handle new call as a special case
		if ident.Name == "new" {
			typeExpr := callExpr.Args[0]
			typeName := convertToCSTypeName(v.getExprTypeName(typeExpr, false))
			return fmt.Sprintf("@new<%s>()", typeName)
		}

		// Handle append: an untyped-constant variadic element (e.g. `append(buf, replacementChar)`)
		// makes C# overload resolution ambiguous — the `append<T>(ISlice, params T[])` overload
		// infers T from the element (the untyped wrapper) while the `slice<T>` overloads infer T
		// from the slice. Cast such elements to the slice's element type, matching Go's implicit
		// conversion and the already-working explicitly-converted element pattern (`uint16(r)`).
		if ident.Name == "append" && len(callExpr.Args) >= 2 && !callExpr.Ellipsis.IsValid() {
			if sliceType := v.info.TypeOf(callExpr.Args[0]); sliceType != nil {
				if sliceUnder, ok := sliceType.Underlying().(*types.Slice); ok {
					// Only numeric element types are affected (the wrong-element-type overload
					// selection is a numeric-conversion artifact); skip otherwise.
					if elemBasic, ok := sliceUnder.Elem().Underlying().(*types.Basic); ok && elemBasic.Info()&types.IsNumeric != 0 {
						elemCSType := convertToCSTypeName(v.getTypeName(sliceUnder.Elem(), false))

						for i := 1; i < len(callExpr.Args); i++ {
							if v.isUntypedNumericConstArg(callExpr.Args[i]) {
								if callExprContext.castArgToType == nil {
									callExprContext.castArgToType = make(map[int]string)
								}
								callExprContext.castArgToType[i] = elemCSType
							}
						}
					}
				}
			}
		}

		// Handle panic call as a special case
		if ident.Name == "panic" {
			context := DefaultBasicLitContext()
			context.u8StringOK = false
			return fmt.Sprintf("throw panic(%s)", v.convExpr(callExpr.Args[0], []ExprContext{context}))
		}
	}

	lambdaContext := DefaultLambdaContext()
	lambdaContext.isCallExpr = true
	lambdaContext.isPointerCast = context.isPointerCast
	lambdaContext.deferredDecls = context.deferredDecls
	// A deferred call target (`defer func(){…}()`) is a function literal whose recover() binds to
	// the enclosing function; pass that through so convFuncLit does not give it its own wrapper.
	lambdaContext.deferCall = context.deferCall

	var typeParamExpr string
	resultType := v.info.TypeOf(callExpr)

	if resultType != nil {
		// A call whose RESULT is a generic instantiation needs the type arguments when the
		// callee IS the type (a conversion/constructor form) or a GENERIC function (whose
		// untyped-const args would infer C# int where Go infers nint — `NewOption<nint>(42)`).
		// A plain NON-generic function returning a generic named type
		// (`func countdown(n int) Seq[int]`) must not have them appended (`countdown<nint>(5)`
		// — CS0308 on a non-generic method).
		funIsType := false
		calleeIsGeneric := false

		if tv, ok := v.info.Types[callExpr.Fun]; ok {
			funIsType = tv.IsType()
		}

		if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
			if funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func); ok {
				if sig, ok := funcObj.Type().(*types.Signature); ok && sig.TypeParams() != nil && sig.TypeParams().Len() > 0 {
					calleeIsGeneric = true
				}
			}
		}

		if named, ok := resultType.(*types.Named); ok && (funIsType || calleeIsGeneric) {
			if named.TypeArgs().Len() > 0 {
				var typeParams []string

				for i := range named.TypeArgs().Len() {
					typeParams = append(typeParams, v.getCSTypeName(named.TypeArgs().At(i)))
				}

				typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
			}
		}

		// A call to a GENERIC FUNCTION whose resolved type arguments involve a TYPE PARAMETER —
		// slices.Sort's helper chain (`Sort[S ~[]E, …]` calling `pdqsortOrdered(x, …)`), pdqsort's
		// recursion — must render its type arguments EXPLICITLY: Go infers them through core
		// types, but C# never infers a type parameter that appears only in constraints (CS0411,
		// 14 sites in the slices/maps wave). go/types already resolved every instantiation
		// (info.Instances); a concrete instantiation still infers fine in C# and stays bare
		// (no churn).
		if len(typeParamExpr) == 0 {
			if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
				if instance, ok := v.info.Instances[funIdent]; ok && instance.TypeArgs != nil &&
					v.calleeHasConstraintOnlyTypeParam(funIdent) {
					var typeParams []string

					for i := range instance.TypeArgs.Len() {
						typeParams = append(typeParams, v.getCSTypeName(instance.TypeArgs.At(i)))
					}

					typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
				}
			}
		}

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency.
		if resultType.String() == "unsafe.Pointer" {
			if len(constructType) == 0 {
				constructType = "(uintptr)"
			} else if len(callExpr.Args) == 1 {
				// Check if current function is a receiver function
				if v.currentFuncSignature.Recv() != nil {
					// Get the receiver type
					recvType := v.currentFuncSignature.Recv().Type()

					// Check if receiver is a pointer type
					isRecvPointer := false

					if ptrType, ok := recvType.(*types.Pointer); ok {
						recvType = ptrType.Elem()
						isRecvPointer = true
					}

					// Get the unsafe.Pointer call argument type
					argType := v.info.TypeOf(callExpr.Args[0])
					isArgPointer := false

					// Check if the argument is a pointer
					if ptrType, ok := argType.(*types.Pointer); ok {
						argType = ptrType.Elem()
						isArgPointer = true
					}

					// Check if the receiver type is pointer and call argument matches
					if isRecvPointer && isArgPointer && types.Identical(recvType, argType) {
						// Since pointer-based receiver functions are converted to C# as ref-based
						// extension functions, we need to convert the pointer from a reference type
						return fmt.Sprintf("(uintptr)@unsafe.Pointer.FromRef(ref %s)", v.convExpr(callExpr.Args[0], nil))
					}
				}
			}
		}
	}

	funcTypeName := v.getTypeName(funcType, true)
	// A string-source element-decoding conversion — `[]rune("lit")` or `[]byte("lit")` — must cast
	// the literal to golib's `@string` so the existing `@string`→`slice<rune>`/`slice<byte>`
	// conversion applies. A bare string literal is a System.String, which has no such conversion
	// (CS1503/CS1929). A string *variable* is already `@string`, so this only matters for literals;
	// the cast fires only on STRING basic-literal args (see convBasicLit). The flag name predates
	// the `[]byte` case.
	callExprContext.sourceIsRuneArray = funcTypeName == "[]rune" || funcTypeName == "[]byte"

	if len(callExpr.Args) == 1 {
		argTypeName := v.getExprTypeName(callExpr.Args[0], true)

		if argTypeName == "unsafe.Pointer" {
			lambdaContext.isPointerCast = true
		}
	}

	funcName := ""

	// A call through a dereferenced function pointer, `(*fp)(args)`. Converting the ParenExpr
	// faithfully yields `(fp.Value)(args)`, which C# parses as a CAST when the argument list is empty
	// (`(fp.Value)()` reads as "cast `()` to type `fp.Value`" → CS1525). Emit the deref WITHOUT the
	// wrapping parens (`fp.Value(args)`) so it is unambiguously an invocation. Restricted to a starred
	// VALUE operand; a starred type (`(*int)(x)`) is a conversion handled earlier.
	if paren, ok := callExpr.Fun.(*ast.ParenExpr); ok {
		if star, ok := paren.X.(*ast.StarExpr); ok {
			if tv, ok := v.info.Types[star.X]; ok && tv.IsValue() {
				funcName = v.convExpr(star, []ExprContext{lambdaContext})
			}
		}
	}

	if funcName == "" {
		funcName = v.convExpr(callExpr.Fun, []ExprContext{lambdaContext})
	}

	// A Go built-in call (`clear(s)`, `len(s)`, …) whose name the package ALSO declares as a method
	// shadows the using-static `go.builtin.<name>` (C# member lookup binds the package's own
	// `<name>(this ref T)` extension first → CS1620/CS1503). Qualify it as `builtin.<name>` so it
	// resolves to the golib built-in regardless of the same-class shadow.
	if len(packageBuiltinShadows) > 0 {
		if ident, ok := callExpr.Fun.(*ast.Ident); ok && funcName == ident.Name && packageBuiltinShadows[ident.Name] {
			if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); isBuiltin {
				funcName = "builtin." + funcName
			}
		}
	}

	// Go's min/max builtins type every argument to the call's single result type. An argument that
	// is a NAMED UNTYPED CONSTANT renders as its UntypedInt (BigInteger) static, which the golib
	// min/max `params ReadOnlySpan<T>` overloads reject (CS1503 — params-span element binding does
	// not apply the user-defined implicit conversion): runtime `min(n, maxObletBytes)` (mgcmark.go,
	// n uintptr) and `min(debug.profstackdepth, maxProfStackDepth)` (runtime1.go, int32). Cast such
	// an argument to the call's Go-resolved result type: `min(n, (uintptr)(maxObletBytes))`.
	// Literal and typed arguments are left as-is (no churn — the early return fires only when an
	// untyped-const argument is present).
	if funcName == "min" || funcName == "max" || funcName == "builtin.min" || funcName == "builtin.max" {
		if funIdent, ok := callExpr.Fun.(*ast.Ident); ok {
			if _, isBuiltin := v.info.ObjectOf(funIdent).(*types.Builtin); isBuiltin {
				if callType := v.info.TypeOf(callExpr); callType != nil {
					argIsNamedUntypedConst := func(arg ast.Expr) bool {
						ident := getIdentifier(arg)

						if ident == nil {
							return false
						}

						constObj, ok := v.info.ObjectOf(ident).(*types.Const)

						if !ok {
							return false
						}

						basic, ok := constObj.Type().(*types.Basic)

						return ok && basic.Info()&types.IsUntyped != 0
					}

					needsCast := false

					for _, arg := range callExpr.Args {
						if argIsNamedUntypedConst(arg) {
							needsCast = true
							break
						}
					}

					if needsCast {
						// Once one argument is cast, an untyped LITERAL sibling (`min(big, limit,
						// 500)` — a bare `500` is a C# int) breaks T inference against the cast
						// type, so every constant-valued argument gets the cast; typed variable
						// arguments are left as-is.
						csCallType := convertToCSTypeName(v.getTypeName(callType, false))
						args := make([]string, len(callExpr.Args))

						for i, arg := range callExpr.Args {
							args[i] = v.convExpr(arg, nil)

							if _, isLit := arg.(*ast.BasicLit); isLit || argIsNamedUntypedConst(arg) {
								args[i] = fmt.Sprintf("(%s)(%s)", csCallType, args[i])
							}
						}

						return fmt.Sprintf("%s(%s)", funcName, strings.Join(args, ", "))
					}
				}
			}
		}
	}

	// Handle unsafe.Offsetof and unsafe.AlignOf as a special cases. Gate on funcName before
	// converting the argument: this re-converts the single arg purely to reshape it for the
	// unsafe.* helpers, and doing it unconditionally would re-run a func-literal argument's capture
	// generation (a side effect), duplicating its hoisted decls for an ordinary single-arg call.
	if len(constructType) == 0 && len(callExpr.Args) == 1 &&
		(funcName == "@unsafe.Offsetof" || funcName == "@unsafe.Alignof" || funcName == "@unsafe.Sizeof") {
		argExpr := v.convExpr(callExpr.Args[0], nil)
		argParts := strings.Split(argExpr, ".")

		if funcName == "@unsafe.Offsetof" {
			v.showWarning("Go code converted to C# using 'unsafe.Offsetof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			if len(argParts) == 2 {
				// `unsafe.Offsetof(structValue.field)` to
				// `@unsafe.Offsetof(structValue.GetType(), "field")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				v.showWarning("Unexpected 'unsafe.Offsetof' argument format: %s", argExpr)
			}
		} else if funcName == "@unsafe.Alignof" {
			v.showWarning("Go code converted to C# using 'unsafe.Alignof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			if len(argParts) == 1 {
				// `unsafe.Alignof(x)` to
				// `@unsafe.Alignof(x.GetType())`
				return fmt.Sprintf("%s(%s.GetType())", funcName, argParts[0])
			} else if len(argParts) == 2 {
				// `unsafe.Alignof(s.f)` to
				// `@unsafe.Alignof(s.GetType(), "f")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				v.showWarning("Unexpected 'unsafe.Alignof' argument format: %s", argExpr)
			}
		} else if funcName == "@unsafe.Sizeof" {
			v.showWarning("Go code converted to C# using 'unsafe.Sizeof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))
		}
	}

	if len(typeParamExpr) > 0 && !strings.HasSuffix(funcName, typeParamExpr) {
		// A PARTIAL Go instantiation (`Grow[S](nil, size)` — only S written, E inferred through
		// core types) already rendered its explicit arguments into funcName; the RESOLVED full
		// list replaces them (C# needs every constraint-only parameter spelled out — appending
		// would emit `Grow<S><S, E>`).
		funcName = stripTrailingTypeArgs(funcName) + typeParamExpr
	}

	var result string

	if !context.renderParams && context.callArgs != nil {
		// Capture arguments for function literal in a defer context, but do not render
		v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)
		result = fmt.Sprintf("%s%s", constructType, funcName)
	} else {
		expr := v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)

		if strings.HasSuffix(funcName, "(uintptr)") && strings.HasPrefix(expr, "(uintptr)") {
			// Remove redundant cast to uintptr
			expr = expr[9:]
		}

		result = fmt.Sprintf("%s%s(%s)", constructType, funcName, expr)
	}

	// Check each argument for implicit conversions. This re-converts each arg purely for its
	// side-effects (recording implicit conversions); the result is discarded. Suppress capture-decl
	// hoisting during it so a func-literal arg's decls (already emitted by the real conversion
	// above) are not written a second time into the hoist buffer.
	savedHoist := v.hoistedDecls
	v.hoistedDecls = nil

	for _, arg := range callExpr.Args {
		argType := v.getType(arg, false)
		argTypeName := convertToCSTypeName(v.getTypeName(argType, false))

		v.checkForImplicitConversion(funcType, arg, argTypeName)
	}

	v.hoistedDecls = savedHoist

	return result
}

func (v *Visitor) checkForImplicitConversion(funcType types.Type, arg ast.Expr, targetTypeName string) string {
	expr := v.convExpr(arg, nil)
	argType := v.getType(arg, false)

	var targetTypeIsPointer bool

	// Check if function type is a signature, i.e., an anonymous struct
	if sigType, ok := funcType.(*types.Signature); ok && sigType.Params().Len() > 0 {
		funcType = sigType.Params().At(0).Type()
	}

	// Check if function type is a struct or a pointer to a struct
	if ptrType, ok := funcType.(*types.Pointer); ok {
		funcType = ptrType.Elem()
		targetTypeIsPointer = true
	}

	if _, ok := funcType.Underlying().(*types.Struct); ok {
		// Check if argType is a struct or a pointer to a struct
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			if _, ok := argType.Underlying().(*types.Struct); ok {
				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.Value ?? default!)", expr)
				}

				argTypeName := v.getCSTypeName(argType)

				if targetTypeName != argTypeName {
					// The recorded conversion type names use cross-package import aliases (e.g.
					// `abi.Type`); register them so package_info.cs can emit a resolving `global using`.
					v.recordConversionPackageUsing(argType)
					v.recordConversionPackageUsing(funcType)

					// If both funcType and argType are distinct structs, track implicit conversions
					packageLock.Lock()

					var targetConversionsMap map[string]HashSet[string]

					if targetTypeIsPointer {
						targetConversionsMap = indirectImplicitConversions
					} else {
						targetConversionsMap = implicitConversions
					}

					var conversions HashSet[string]
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions.Add(targetTypeName)
					} else {
						conversions = NewHashSet([]string{targetTypeName})
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()

					v.addImplicitSubStructConversions(argType, targetTypeName, targetTypeIsPointer)
				}
			}
		}
	}

	// Check if the function type is an aliased numeric type
	if ok := isAliasedNumericType(funcType); ok {
		// Check if argType is a pointer type
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			// Check if the arg type is an aliased numeric type
			if ok := isAliasedNumericType(argType); ok {
				valueTypeName := convertToCSTypeName(v.getTypeName(argType, true))

				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.Value ?? default!)", expr)
				}

				argTypeName := v.getCSTypeName(argType)

				if targetTypeName != argTypeName {
					if strings.Contains(argTypeName, ".") || strings.Contains(argTypeName, TypeAliasDot) {
						valueTypeName = fmt.Sprintf("imported:%s", valueTypeName)
						targetTypeName, argTypeName = argTypeName, targetTypeName
					}

					// If both funcType and argType are both aliased numeric types, track value conversions
					packageLock.Lock()

					var targetConversionsMap map[string]map[string]string

					if targetTypeIsPointer {
						targetConversionsMap = indirectNumericConversions
					} else {
						targetConversionsMap = numericConversions
					}

					var conversions map[string]string
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions[targetTypeName] = valueTypeName
					} else {
						conversions = make(map[string]string)
						conversions[targetTypeName] = valueTypeName
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()
				}
			}
		}
	}

	return expr
}

func isAliasedNumericType(targetType types.Type) bool {
	if aliasedType, ok := targetType.(*types.Alias); ok {
		underlyingType := aliasedType.Underlying()
		return isNumericType(underlyingType)
	} else if namedType, ok := targetType.(*types.Named); ok {
		underlyingType := namedType.Underlying()
		return isNumericType(underlyingType)
	}

	return false
}

// isUntypedNumericConstArg reports whether the argument is an untyped numeric constant — a
// numeric literal, or a named const declared without a type. In C# these render either as a
// bare `int`/`double` literal or a golib `Untyped*` wrapper, neither of which is the slice's
// element type; that is what makes `append`'s overload resolution pick the wrong element type
// (the `ISlice` overload infers T from the element, yielding e.g. `slice<int>`).
func (v *Visitor) isUntypedNumericConstArg(arg ast.Expr) bool {
	switch a := arg.(type) {
	case *ast.BasicLit:
		// Literals are untyped constants; numeric kinds only (avoid string/append-string).
		return a.Kind == token.INT || a.Kind == token.FLOAT || a.Kind == token.CHAR || a.Kind == token.IMAG
	case *ast.Ident:
		if constObj, ok := v.info.Uses[a].(*types.Const); ok {
			if basic, ok := constObj.Type().(*types.Basic); ok {
				return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
			}
		}
	}

	return false
}

// recordConversionPackageUsing registers the import alias → C# namespace for any cross-package named
// type referenced (directly, or through a pointer/slice/array/map/channel wrapper or a generic type
// argument) by a recorded implicit conversion. The generated `[assembly: GoImplicitConv<…>]` lines in
// package_info.cs use the alias form (e.g. `abi.Type`), but that file has no file-local `using abi =
// …`; conversionPackageUsings drives a resolving `global using` there.
func (v *Visitor) recordConversionPackageUsing(t types.Type) {
	switch t := t.(type) {
	case *types.Pointer:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Slice:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Array:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Map:
		v.recordConversionPackageUsing(t.Key())
		v.recordConversionPackageUsing(t.Elem())
	case *types.Chan:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Named:
		if obj := t.Obj(); obj != nil {
			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				packageLock.Lock()
				conversionPackageUsings[pkg.Name()] = convertImportPathToNamespace(pkg.Path(), PackageSuffix)
				packageLock.Unlock()
			}
		}

		if typeArgs := t.TypeArgs(); typeArgs != nil {
			for i := 0; i < typeArgs.Len(); i++ {
				v.recordConversionPackageUsing(typeArgs.At(i))
			}
		}
	}
}

// isRawAddressPointerConversion reports whether callExpr is a pointer-type conversion `(*T)(p)` whose
// RESULT is a pointer type and whose SOURCE is a raw address — an unsafe.Pointer or a uintptr. Such a
// conversion reinterprets the raw address as a `*T` (golib `ж<T>`); because `unsafe.Pointer` is the golib
// `Pointer : ж<uintptr>`, a direct `(ж<T>)p` needs two chained user-defined conversions (Pointer→uintptr→
// ж<T>) that C# rejects (CS0030), so the caller routes it through uintptr instead. Excludes the pointer-to-
// named-type value conversion (arg is a *types.Pointer, handled separately) — only a genuine raw-address
// source (Basic UnsafePointer/Uintptr) qualifies.
// makeLenArgs renders the length/capacity/size-hint arguments of a `make(T, len[, cap])` call (slice, map,
// or chan), casting any argument whose Go type is an integer with no implicit C# conversion to nint to nint
// — so it binds the golib `slice<T>(nint,nint)` / `map<K,V>(nint)` / `channel<T>(nint)` constructor rather
// than falling onto `slice<T>(T[])` or failing `nuint`→`nint` (CS1503). A plain int / untyped constant
// binds directly and is left alone (no golden churn).
func (v *Visitor) makeLenArgs(args []ast.Expr) string {
	parts := make([]string, len(args))

	for i, arg := range args {
		argStr := v.convExpr(arg, nil)

		if v.makeLenArgNeedsNintCast(arg) {
			argStr = "(nint)(" + argStr + ")"
		}

		parts[i] = argStr
	}

	return strings.Join(parts, ", ")
}

// makeLenArgNeedsNintCast reports whether a make length/cap argument's type is an integer that C# will not
// implicitly convert to nint (so `new slice<T>(arg)` must cast it): true for uintptr/uint/uint32/uint64/
// int64; false for int/int8/int16/int32/uint8/uint16 (which widen to nint) and untyped constants (which
// render as bare literals that bind to nint directly).
func (v *Visitor) makeLenArgNeedsNintCast(arg ast.Expr) bool {
	argType := v.info.TypeOf(arg)

	if argType == nil {
		return false
	}

	basic, ok := argType.Underlying().(*types.Basic)

	if !ok {
		return false
	}

	if basic.Info()&types.IsUntyped != 0 {
		return false
	}

	switch basic.Kind() {
	case types.Int64, types.Uint, types.Uint32, types.Uint64, types.Uintptr:
		return true
	}

	return false
}

func (v *Visitor) isRawAddressPointerConversion(callExpr *ast.CallExpr, arg ast.Expr) bool {
	if _, ok := v.info.TypeOf(callExpr).(*types.Pointer); !ok {
		return false
	}

	argType := v.info.TypeOf(arg)

	if argType == nil {
		return false
	}

	if basic, ok := argType.Underlying().(*types.Basic); ok {
		return basic.Kind() == types.UnsafePointer || basic.Kind() == types.Uintptr
	}

	return false
}

func (v *Visitor) isTypeConversion(callExpr *ast.CallExpr) (bool, string) {
	// Get the object associated with the function being called
	var obj types.Object
	var isPointer bool

	targetExpr := callExpr.Fun

	for targetExpr != nil {
		switch funExpr := targetExpr.(type) {
		case *ast.ParenExpr:
			targetExpr = funExpr.X
			continue
		case *ast.IndexExpr:
			targetExpr = funExpr.X
			continue
		case *ast.IndexListExpr:
			// A conversion to a MULTI-type-parameter generic instantiation — Go 1.23 iter's
			// `Seq2Like[K, V](fn)` shape. The single-param IndexExpr case above already peels;
			// without this arm the two-param form fell through as a plain (non-invocable) call
			// (CS1955 on the emitted `Seq2Like<K, V>(…)`).
			targetExpr = funExpr.X
			continue
		case *ast.StarExpr:
			if ident, ok := funExpr.X.(*ast.Ident); ok {
				obj = v.info.ObjectOf(ident)
				isPointer = true
			} else if sel, ok := funExpr.X.(*ast.SelectorExpr); ok {
				// A pointer conversion to a cross-package type: `(*atomic.Uint32)(p)`.
				obj = v.info.ObjectOf(sel.Sel)
				isPointer = true
			} else {
				// A pointer conversion to a TYPE-LITERAL target — `(*[4]uint64)(&s.s)`
				// (edwards25519's fiat reinterprets): a composite type has no types.Object,
				// so resolve the target directly from type info. Claimed ONLY for the fiat
				// reinterpret shape — the argument is a pointer to a defined type written
				// directly over an unnamed array, converting to a pointer to that exact
				// underlying array — so every other pointer-to-type-literal conversion (the
				// pointer-cast slice form `(*[1<<20]Method)(p)[:n:n]`, internal/abi) keeps
				// its pre-existing route byte-identically.
				if len(callExpr.Args) != 1 {
					return false, ""
				}

				elemType := v.info.TypeOf(funExpr.X)
				argType := v.info.TypeOf(callExpr.Args[0])

				if elemType == nil || argType == nil {
					return false, ""
				}

				if argPtr, ok := argType.(*types.Pointer); ok {
					if named, ok := argPtr.Elem().(*types.Named); ok && writtenRHSIsUnnamedArray(named) &&
						types.Identical(elemType, named.Underlying()) {
						return types.ConvertibleTo(argType, types.NewPointer(elemType)), "*" + v.getTypeName(elemType, false)
					}
				}

				return false, ""
			}
			targetExpr = nil
		case *ast.Ident:
			obj = v.info.ObjectOf(funExpr)
			targetExpr = nil
		case *ast.SelectorExpr:
			obj = v.info.ObjectOf(funExpr.Sel)
			targetExpr = nil
		case *ast.ArrayType, *ast.MapType, *ast.ChanType:
			// A composite type literal used as a conversion target whose argument is a
			// named type with the *same* underlying shape — the `[]CaseRange(special)`
			// pattern, where `special` is `type SpecialCase []CaseRange`. This lowers to
			// a cast through the generated implicit operator: `((slice<CaseRange>)special)`.
			// These type-literal targets have no associated types.Object, so resolve the
			// target type directly from type info.
			//
			// Restricted to identical-underlying conversions so that element-decoding
			// conversions like `[]rune(s)` / `[]byte(s)` (string source) keep their
			// existing argument-rendering path (which casts the source to @string first).
			if len(callExpr.Args) != 1 {
				return false, ""
			}

			targetType := v.info.TypeOf(funExpr)
			argType := v.info.TypeOf(callExpr.Args[0])

			if targetType == nil || argType == nil {
				return false, ""
			}

			if !types.Identical(targetType.Underlying(), argType.Underlying()) {
				return false, ""
			}

			return types.ConvertibleTo(argType, targetType), v.getTypeName(targetType, false)
		default:
			return false, ""
		}
	}

	if obj == nil {
		return false, ""
	}

	// Check if the function being called is a type name
	resolvedTypeName, ok := obj.(*types.TypeName)

	if !ok {
		return false, ""
	}

	// Get the target type
	targetType := resolvedTypeName.Type()

	// A conversion to a GENERIC INSTANTIATION (`Seq2Like[K, V](fn)`, Go 1.23 iter shapes)
	// resolves through the TypeName to the UNINSTANTIATED generic, against which
	// ConvertibleTo is false and the rendered name drops its type arguments. The
	// instantiated target is the type of the Fun expression itself. Gated to exactly the
	// uninstantiated-generic case: for a POINTER conversion the Fun type is the full `*T`
	// (the `*` is re-applied via isPointer below — overriding would double it, ж<ж<T>>),
	// and a non-generic target needs no override.
	if named, ok := targetType.(*types.Named); ok && !isPointer && named.TypeParams().Len() > 0 && named.TypeArgs() == nil {
		if tv, ok := v.info.Types[callExpr.Fun]; ok && tv.IsType() && tv.Type != nil {
			targetType = tv.Type
		}
	}

	// Type conversions typically have exactly one argument
	if len(callExpr.Args) != 1 {
		return false, ""
	}

	// Get the type of the argument
	argType := v.info.TypeOf(callExpr.Args[0])

	// Check if the argument is a pointer
	if pointer, ok := argType.(*types.Pointer); ok {
		argType = pointer.Elem()
	}

	typeName := v.getTypeName(targetType, false)

	if isPointer {
		typeName = "*" + typeName
	}

	// Check if the argument type is convertible to the target type
	return types.ConvertibleTo(argType, targetType), typeName
}

func (v *Visitor) needsParentheses(expr ast.Expr) bool {
	switch expr.(type) {
	case *ast.Ident:
		return false
	case *ast.CallExpr:
		return false
	case *ast.SelectorExpr:
		return false
	case *ast.BasicLit:
		return false
	case *ast.CompositeLit:
		return false
	case *ast.ParenExpr:
		return false
	case *ast.UnaryExpr:
		// Unary expressions like -x or !x need parentheses
		return true
	case *ast.BinaryExpr:
		// Binary expressions like x + y need parentheses
		return true
	case *ast.IndexExpr:
		// Array/slice indexing like arr[i] doesn't need parentheses
		return false
	default:
		// For any other expression types, err on the side of caution
		return true
	}
}

func (v *Visitor) isConstructorCall(callExpr *ast.CallExpr) bool {
	// Get the object associated with the function being called
	var obj types.Object

	switch funExpr := callExpr.Fun.(type) {
	case *ast.Ident:
		obj = v.info.ObjectOf(funExpr)
	case *ast.SelectorExpr:
		obj = v.info.ObjectOf(funExpr.Sel)
	default:
		return false
	}

	if obj == nil {
		return false
	}

	// Determine if the object is a type name
	switch obj.(type) {
	case *types.TypeName:
		// The function being called is a type (constructor call)
		return true
	case *types.Builtin:
		// Built-in functions like len, cap, etc.
		return false
	case *types.Func, *types.Var:
		// Regular functions or variables of function type
		return false
	default:
		return false
	}
}

func (v *Visitor) addImplicitSubStructConversions(sourceType types.Type, targetTypeName string, indirect bool) {
	if subStructTypes, exists := v.subStructTypes[sourceType]; exists {
		for _, subStructType := range subStructTypes {
			// Check if subStructType is a pointer
			if ptrType, ok := subStructType.(*types.Pointer); ok {
				subStructType = ptrType.Elem()
			}

			subStructTypeName := v.getCSTypeName(subStructType)
			sourceTypeName := getRootSubStructName(subStructTypeName)

			if strings.HasSuffix(targetTypeName, ">") {
				targetTypeName = fmt.Sprintf("%s_%s>", targetTypeName[:len(targetTypeName)-1], sourceTypeName)
			} else {
				targetTypeName = fmt.Sprintf("%s_%s", targetTypeName, sourceTypeName)
			}

			// Recursively add implicit conversions for sub-structs
			v.addImplicitSubStructConversions(subStructType, targetTypeName, indirect)

			var targetConversionsMap map[string]HashSet[string]

			if indirect {
				targetConversionsMap = indirectImplicitConversions
			} else {
				targetConversionsMap = implicitConversions
			}

			var conversions HashSet[string]
			var exists bool

			packageLock.Lock()

			if conversions, exists = targetConversionsMap[subStructTypeName]; exists {
				conversions.Add(targetTypeName)
			} else {
				conversions = NewHashSet([]string{targetTypeName})
				targetConversionsMap[subStructTypeName] = conversions
			}

			packageLock.Unlock()
		}
	}
}

func getRootSubStructName(subStructName string) string {
	// Get text beyond last underscore
	lastUnderscoreIndex := strings.LastIndex(subStructName, "_")

	if lastUnderscoreIndex == -1 {
		return subStructName
	}

	return subStructName[lastUnderscoreIndex+1:]
}

func getShortFileName(fileToken *token.File) string {
	if fileToken == nil {
		return ""
	}

	return filepath.Base(fileToken.Name())
}

func (v *Visitor) getFunctionSignature(callExpr *ast.CallExpr) *types.Signature {
	switch fun := callExpr.Fun.(type) {
	case *ast.Ident:
		// Simple identifiers
		obj := v.info.Uses[fun]
		if obj == nil {
			return nil
		}

		if fn, ok := obj.(*types.Func); ok {
			return fn.Type().(*types.Signature)
		}

		if vr, ok := obj.(*types.Var); ok {
			if sig, ok := vr.Type().(*types.Signature); ok {
				return sig
			}
		}

	case *ast.SelectorExpr:
		// Qualified identifiers and method calls
		sel, ok := v.info.Selections[fun]
		if ok {
			// Method call
			if fn, ok := sel.Obj().(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}
			return nil
		}

		// Package-qualified function
		if _, ok := fun.X.(*ast.Ident); ok {
			obj := v.info.Uses[fun.Sel]
			if obj == nil {
				return nil
			}

			if fn, ok := obj.(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}

			if vr, ok := obj.(*types.Var); ok {
				if sig, ok := vr.Type().(*types.Signature); ok {
					return sig
				}
			}
		}

	case *ast.ParenExpr:
		// Handle parenthesized expressions like (pkg.Func)()
		return v.getFunctionSignature(&ast.CallExpr{Fun: fun.X})

	case *ast.CallExpr:
		// Functions returned by other functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.FuncLit:
		// Anonymous functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexExpr:
		// Generic function instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexListExpr:
		// Multiple type parameter instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.TypeAssertExpr:
		// Type assertions: (x.(T))()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.StarExpr:
		// Dereferencing a function pointer: (*fnPtr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.UnaryExpr:
		// Unary expressions like (*ptr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.BinaryExpr:
		// Binary expressions that somehow evaluate to functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.CompositeLit:
		// Composite literals that are callable
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.ArrayType, *ast.ChanType, *ast.FuncType, *ast.InterfaceType,
		*ast.MapType, *ast.StructType:
		// Type expressions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Handle type conversion cases that return callable functions
	// This covers cases like (*byte)(unsafe.Pointer(...))
	if callExpr, ok := callExpr.Fun.(*ast.CallExpr); ok {
		if fun, ok := callExpr.Fun.(*ast.ParenExpr); ok {
			if t := v.info.TypeOf(fun.X); t != nil {
				if sig, ok := t.(*types.Signature); ok {
					return sig
				}
			}
		}

		// Handle types directly
		if t := v.info.TypeOf(callExpr.Fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Final general fallback - this should catch most remaining cases
	if t := v.info.TypeOf(callExpr.Fun); t != nil {
		if sig, ok := t.(*types.Signature); ok {
			return sig
		}

		if sig, ok := t.Underlying().(*types.Signature); ok {
			return sig
		}
	}

	resultType := v.info.TypeOf(callExpr)

	if resultType != nil && strings.Contains(resultType.String(), "unsafe.Pointer") {
		pkg := types.NewPackage("unsafe", "unsafe")

		// Only concerned with making the parameter a "pointer like" type
		uintptrType := types.Typ[types.Uintptr]
		params := types.NewTuple(types.NewParam(token.NoPos, pkg, "", types.NewPointer(uintptrType)))

		return types.NewSignatureType(nil, nil, nil, params, nil, false)
	}

	return nil
}

// getCallFunIdent returns the NAME identifier of a called function — the ident itself, a
// selector's Sel, or the peeled base of an explicit instantiation — the key go/types uses
// for info.Instances.
func getCallFunIdent(fun ast.Expr) *ast.Ident {
	switch e := fun.(type) {
	case *ast.Ident:
		return e
	case *ast.SelectorExpr:
		return e.Sel
	case *ast.ParenExpr:
		return getCallFunIdent(e.X)
	case *ast.IndexExpr:
		return getCallFunIdent(e.X)
	case *ast.IndexListExpr:
		return getCallFunIdent(e.X)
	}

	return nil
}


// calleeHasConstraintOnlyTypeParam reports whether the called generic function declares a type
// parameter that appears in NO parameter type — visible only in constraints (`Twice[S ~[]E, E
// Integer](s S)`: E). Go infers such a parameter through core types; C# cannot infer it from
// arguments at ANY call site (concrete included), so those calls need explicit type arguments.
func (v *Visitor) calleeHasConstraintOnlyTypeParam(funIdent *ast.Ident) bool {
	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil {
		return false
	}

	for i := range sig.TypeParams().Len() {
		tp := sig.TypeParams().At(i)
		found := false

		for j := range sig.Params().Len() {
			if typeUsesTypeParam(sig.Params().At(j).Type(), tp) {
				found = true
				break
			}
		}

		if !found {
			return true
		}
	}

	return false
}

// typeUsesTypeParam reports whether t structurally contains the SPECIFIC type parameter tp.
func typeUsesTypeParam(t types.Type, tp *types.TypeParam) bool {
	switch tt := t.(type) {
	case *types.TypeParam:
		return tt == tp
	case *types.Slice:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Array:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Pointer:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Map:
		return typeUsesTypeParam(tt.Key(), tp) || typeUsesTypeParam(tt.Elem(), tp)
	case *types.Chan:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Signature:
		params := tt.Params()
		for i := range params.Len() {
			if typeUsesTypeParam(params.At(i).Type(), tp) {
				return true
			}
		}
		results := tt.Results()
		for i := range results.Len() {
			if typeUsesTypeParam(results.At(i).Type(), tp) {
				return true
			}
		}
	case *types.Named:
		if args := tt.TypeArgs(); args != nil {
			for i := range args.Len() {
				if typeUsesTypeParam(args.At(i), tp) {
					return true
				}
			}
		}
	}

	return false
}

// stripTrailingTypeArgs removes a trailing balanced <...> type-argument group from a rendered
// function name (`Grow<S>` -> `Grow`, `F<slice<E>>` -> `F`); a name without one is unchanged.
func stripTrailingTypeArgs(funcName string) string {
	if !strings.HasSuffix(funcName, ">") {
		return funcName
	}

	depth := 0

	for i := len(funcName) - 1; i >= 0; i-- {
		switch funcName[i] {
		case '>':
			depth++
		case '<':
			depth--
			if depth == 0 {
				return funcName[:i]
			}
		}
	}

	return funcName
}

// instantiatedParamIsPointer reports whether a parameter DECLARED as a type parameter is a
// POINTER at this call's instantiation — `abi.Escape(ptr)` where `Escape[T any](x T) T` and
// T=*T (internal/weak Make): the argument must render as its box (`Ꮡptr`), not the deref'd
// value alias (`ptr`), or the result cannot assign back to the pointer (CS0029 T → ж<T>).
// getFunctionSignature returns the DECLARED generic signature; go/types records the
// INSTANTIATED one as the type of the call's Fun expression.
func (v *Visitor) instantiatedParamIsPointer(callExpr *ast.CallExpr, declaredParam types.Type, i int) bool {
	if _, isTP := types.Unalias(declaredParam).(*types.TypeParam); !isTP {
		return false
	}

	instSig, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature)

	if !ok {
		return false
	}

	if paramType, ok := getParameterType(instSig, i); ok {
		return isPointer(paramType)
	}

	return false
}
