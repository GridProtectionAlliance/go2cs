package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"path/filepath"
	"strings"
)

// csharpKeywordCastTypes are the C# keyword primitive type names for which `(T)-value` is
// unambiguously a cast. Every other cast target the converter emits is either a using-ALIAS
// (int64=long, uint64=ulong, rune=int, …) or a `[GoType]` NAMED type (level, Class) — for those,
// `(T)-1` parses as `T MINUS 1` (CS0075 "to cast a negative value … enclose in parentheses" /
// CS0119 "T is a type, not valid in the given context"), so the operand must be parenthesized.
var csharpKeywordCastTypes = NewHashSet([]string{
	"int", "uint", "long", "ulong", "short", "ushort", "byte", "sbyte",
	"nint", "nuint", "float", "double", "decimal", "bool", "char",
})

// castOperandNeedsParens reports whether a cast operand must be wrapped in parentheses — it leads
// with a unary `+`/`-` AND the cast target is not a C# keyword type (see csharpKeywordCastTypes), so
// `(T)-value` would otherwise mis-parse as a subtraction rather than a cast (x/text/unicode/bidi's
// `level(-1)`, archive/tar's `int64(-1)`).
func castOperandNeedsParens(typeName, expr string) bool {
	if !strings.HasPrefix(expr, "-") && !strings.HasPrefix(expr, "+") {
		return false
	}

	return !csharpKeywordCastTypes.Contains(typeName)
}

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

// identIsUniverseBuiltin reports whether an identifier in call position resolves to a Go universe
// built-in rather than to a same-named declaration that SHADOWS it. Go permits shadowing a built-in
// at ANY scope — `make := func(z *Int) *Int {…}` as a function-local in math/big's own tests, a
// parameter named `len`, a package-level `var new …` — after which `make(x)` is an ordinary call to
// that declaration, NOT the built-in. The converter's built-in arms are keyed on the identifier's
// NAME, so without this check a shadowed call is emitted with built-in semantics (`make(test.z)` →
// `new nint()`, CS1503/CS1929). go/types has already resolved the name for us: a genuine built-in
// is the universe *types.Builtin object, anything else is a shadowing declaration and must fall
// through to the ordinary call path.
//
// This is the opposite direction from packageBuiltinShadows, which handles a package method whose
// name collides with a built-in the call still MEANS (there the call is a real built-in and gets
// QUALIFIED as `builtin.<name>`); here the call is not a built-in at all.
func (v *Visitor) identIsUniverseBuiltin(ident *ast.Ident) bool {
	_, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin)

	return isBuiltin
}

// callFunIsUniverseBuiltin reports whether a call's callee is an unshadowed universe built-in —
// identIsUniverseBuiltin for a call whose callee has not already been narrowed to an identifier.
func (v *Visitor) callFunIsUniverseBuiltin(callExpr *ast.CallExpr) bool {
	ident, ok := callExpr.Fun.(*ast.Ident)

	return ok && v.identIsUniverseBuiltin(ident)
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

	// A `(*T)(…)` conversion whose source is an IDENTITY reinterpret of a same-typed pointer —
	// `(*Builder)(abi.NoEscape(unsafe.Pointer(b)))` with b already `*Builder` — is Go's
	// escape-analysis idiom for `b.addr = b` (strings.Builder copyCheck; the type's own TODO says to
	// revert it to exactly that once escape analysis improves). Every other rendering of it — whether
	// the conversion renderer or, because `(*T)(unsafe.Pointer)` mis-classifies as a non-conversion,
	// the regular-call path — round-trips through uintptr, which golib's
	// `(ж<T>)(uintptr) => new ж<T>(*(T*)value)` resolves by DEREFERENCING-and-COPYING: the result box
	// is not reference-equal to the source, so the type's copy-by-value self-check (`b.addr != b`)
	// false-panics at runtime. Intercept ONLY this exact identity shape (leaving all other pointer
	// conversions on their existing path) and emit the source pointer's BOX form directly. The
	// isPointer context renders a deref-aliased pointer param/receiver as `Ꮡb`, not its value alias
	// `b` (a `Builder` value cannot assign to the `ж<Builder>` addr field).
	if len(callExpr.Args) == 1 {
		if identSrc := v.pointerReinterpretIdentitySource(callExpr, callExpr.Args[0]); identSrc != nil {
			identContext := DefaultIdentContext()
			identContext.isPointer = true
			return v.convExpr(identSrc, []ExprContext{identContext})
		}
	}

	funcType := v.getType(callExpr.Fun, false)

	// Check if the call is a type conversion
	if ok, targetTypeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]

		// A compile-time FLOAT constant CONVERSION whose operand references a named untyped-float
		// const — `float64(100000 * Pi)` / `float32(...)` — FOLDS to a single-rounded literal at the
		// TARGET width, the conversion-operand counterpart of the package-level const fold in
		// visitValueSpec. The runtime form `(float64)(100000D * Pi)` rounds a SECOND time
		// (314159.2653589793, −1 ULP), whereas Go folds `100000*Pi` in arbitrary precision and rounds
		// ONCE to 314159.26535897935. Restricted to a BASIC float64/float32 target (a named float type
		// keeps its [GoType]-wrapper conversion path below), and short-circuits so the operand is NOT
		// separately converted — re-introducing the double round. (Bare refs / pure-literal / int /
		// complex operands are rejected inside the helper.)
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok &&
			(targetBasic.Kind() == types.Float64 || targetBasic.Kind() == types.Float32) {
			if folded := v.foldedNamedFloatConstLiteral(arg, v.getCSTypeName(targetBasic)); folded != "" {
				return folded
			}
		}

		// A `string(x)` conversion the hoisting pre-pass elected to lift to a single function-scope
		// `sstring` temp (see planSStringHoists) emits just the temp NAME at every use, instead of
		// re-materializing `((sstring)x)` here. suppressSStringHoist is set only while the hoisted
		// decl's OWN initializer is being rendered, so that one keeps emitting the real view.
		if tempName, ok := v.sstringHoistedConvExprs[callExpr]; ok && !v.suppressSStringHoist {
			return tempName
		}

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

				// The third direction — BASIC → Named over that basic: `(*stringReader)(&str)`
				// where `type stringReader string` (fmt's Sscan family, CS0030 x3). Same
				// value-convert-and-re-box; writes through the box hit the copy, which is
				// faithful for this pattern (the source string is never re-read).
				_, argIsBasic := argElem.(*types.Basic)
				basicToNamed := okBaseNamed && argIsBasic && types.Identical(argElem, baseNamed.Underlying())

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
				// types.Unalias: the target may be an ALIAS to the unnamed array — fiat's
				// `(*p224UntypedFieldElement)(&x)` where `type p224UntypedFieldElement =
				// [4]uint64` renders as `ж<array<ulong>>` via its global using, so the same
				// storage-sharing route applies (CS0030 ×20, all four fiat curves).
				_, resultIsArray := types.Unalias(resultElem).(*types.Array)
				namedToArray := resultIsArray && okDefNamed &&
					types.Identical(types.Unalias(resultElem), defNamed.Underlying()) &&
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

				if namedToNamed || namedToBasic || basicToNamed {
					baseName := convertToCSTypeName(v.getTypeName(resultElem, false))
					var argExpr string

					if unary, ok := arg.(*ast.UnaryExpr); ok && unary.Op == token.AND && basicToNamed {
						// `(*stringReader)(&str)` — the address-of collapses with the value
						// deref: the conversion operates on `str` directly, then re-boxes.
						// (Restricted to the new basicToNamed arm so the long-guarded
						// namedToNamed/namedToBasic emissions stay byte-identical.)
						argExpr = v.convExpr(unary.X, nil)
					} else {
						argExpr = v.convExpr(arg, nil)

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
					}

					return fmt.Sprintf("%s((%s)(%s))", AddressPrefix, baseName, argExpr)
				}
			}
		}

		// Go's slice-to-ARRAY conversions route through golib's array<T>(slice<T>, nint)
		// COPY ctor (which panics Go-style on a short slice): the 1.20 VALUE form
		// `[4]byte(slice)` copies exactly as Go does (netip AddrFromSlice, CS1955 ×5); the
		// 1.17 POINTER form `(*[32]byte)(slice)` boxes the same copy (edwards25519
		// fiatScalarFromBytes' input, CS0030) — reads back through the pointer stay
		// faithful. A NAMED-over-array target falls through unchanged (none in the corpus).
		if _, argIsSlice := v.getType(arg, false).Underlying().(*types.Slice); argIsSlice {
			if resultArr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Array); ok {
				elemName := convertToCSTypeName(v.getTypeName(resultArr.Elem(), false))
				return fmt.Sprintf("new array<%s>(%s, %d)", elemName, v.convExpr(arg, nil), resultArr.Len())
			}

			if resultPtr, ok := v.info.TypeOf(callExpr).(*types.Pointer); ok {
				if resultArr, ok := types.Unalias(resultPtr.Elem()).(*types.Array); ok {
					elemName := convertToCSTypeName(v.getTypeName(resultArr.Elem(), false))
					return fmt.Sprintf("%s(new array<%s>(%s, %d))", AddressPrefix, elemName, v.convExpr(arg, nil), resultArr.Len())
				}
			}
		}

		targetTypeName = convertToCSTypeName(targetTypeName)

		// A `string(x)` conversion the escape pass marked emits the stack-only `sstring` — a zero-copy
		// view over x's bytes — instead of the heap `@string` copy. Two sources feed this: an eligible
		// `s := string(x)` local (visitAssignStmt sets the transient flag around its one RHS conversion),
		// and an unnamed `string(x) == "…"` comparison operand (keyed per-CallExpr in sstringConvExprs).
		// Done after the Go-name → C#-name mapping above, where the target is the `@string` C# name.
		if targetTypeName == "@string" && (v.emitStringConvAsSString || v.sstringConvExprs[callExpr]) {
			targetTypeName = "sstring"
			v.emitStringConvAsSString = false
		}

		// A conversion TARGET that is a foreign RENAMED type routes through the recorded
		// alias (`(syscallꓸHandle)fd`, not the nonexistent `(Δsyscall.Handle)fd` -
		// CS0426, internal/poll DupCloseOnExec).
		if aliased, ok := v.foreignAliasedTypeName(v.info.TypeOf(callExpr)); ok {
			targetTypeName = aliased
		}
		expr := v.checkForImplicitConversion(funcType, arg, targetTypeName)

		// A conversion whose TARGET is a non-empty INTERFACE and whose SOURCE is a POINTER —
		// `image.Image(dst)` with `dst *image.RGBA` (image/draw) — is Go's ordinary
		// pointer-to-interface conversion in explicit clothing: route it through the same
		// machinery as implicit interface casts (the exported/local ж-adapter), re-rendering
		// the argument in its BOX form (the adapter wraps the box). The constructor fallback
		// instantiated the interface (`new image.Image(dst)`, CS0144 ×2). Interface and value
		// sources keep their existing plain-cast/partial-impl routes (no churn).
		if callTargetType := v.info.TypeOf(callExpr); callTargetType != nil {
			if targetIsIface, isEmpty := isInterface(callTargetType); targetIsIface && !isEmpty {
				if argPtrType, srcIsPtr := v.getType(arg, false).(*types.Pointer); srcIsPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true

					// The result is CAST to the interface: the adapter implements its members
					// EXPLICITLY, so a chained member access on the conversion result
					// (`CrossPkgLib.Labeled(sp).Label()`) cannot bind on the adapter class
					// itself (CS1929) — the cast is an implicit reference conversion that
					// exposes the interface view.
					return fmt.Sprintf("((%s)%s)", targetTypeName, v.convertToInterfaceType(callTargetType, argPtrType, v.convExpr(arg, []ExprContext{identContext})))
				}

				// The VALUE mirror: a FOREIGN named VALUE source — `crypto.SignerOpts(sigHash)`
				// with `sigHash crypto.Hash` (crypto/tls, CS0030 ×4). The plain cast cannot bind:
				// a foreign value type implements its interfaces via extension methods (never
				// structurally), and this assembly cannot partial it — route through
				// convertToInterfaceType, which records + references the LOCAL value adapter
				// (the both-foreign arm; syscall.Signal→os.Signal precedent) or no-ops into the
				// plain spelling when the defining assembly already implements the pair. LOCAL
				// value sources keep the plain-cast/partial-impl route (no churn). The outer
				// interface cast stays load-bearing exactly like the pointer arm: `var signOpts
				// = …` must type as the INTERFACE, not the adapter class — each tls site
				// reassigns signOpts to a different adapter two lines later (CS0029 hazard).
				if argType := v.getType(arg, false); argType != nil {
					if named, ok := types.Unalias(argType).(*types.Named); ok && !types.IsInterface(named) {
						if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
							return fmt.Sprintf("((%s)%s)", targetTypeName, v.convertToInterfaceType(callTargetType, argType, v.convExpr(arg, nil)))
						}
					}
				}
			}
		}

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
		//
		// NOTE isPointerCast means only "this conversion is the operand of a deref" — it does NOT imply the
		// SOURCE is an address, and this bridge is only ever correct for one that is. The IDENTITY reinterpret
		// `*(*T)(p)` with p already `*T` is the shape that made the difference visible: its source is a managed
		// box (or, for a deref-aliased parameter, that box's VALUE alias), for which the `(uintptr)` leg has no
		// conversion at all (CS0030). It is intercepted upstream by pointerReinterpretIdentitySource and never
		// reaches here. Non-identity typed-pointer sources whose element types differ but share an underlying
		// (a tag-differing struct, a named/unnamed array or struct pair) are NOT all covered by the re-box
		// routes above and still land here; see ConversionStrategies-Reference.
		if context.isPointerCast || v.isRawAddressPointerConversion(callExpr, arg) {
			return fmt.Sprintf("(%s)(uintptr)(%s)", targetTypeName, expr)
		}

		// A NAMED-over-pointer target from a RAW-ADDRESS source — `syscall.Pointer(unsafe.
		// Pointer(x))`, where `type Pointer *struct{}` emits as a ж<EmptyStruct>-wrapping
		// class — needs the same uintptr reinterpret hop as the `(*T)(p)` form above, then
		// the named type's own operator from its underlying box:
		// `((Pointer)(ж<EmptyStruct>)(uintptr)(x))`. The direct cast chained two
		// user-defined conversions (CS0030 ×6, internal/poll's WSAMsg.Name assignments).
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if ptrUnder, ok := named.Underlying().(*types.Pointer); ok {
				if argType := v.info.TypeOf(arg); argType != nil {
					if basic, ok := argType.Underlying().(*types.Basic); ok && (basic.Kind() == types.UnsafePointer || basic.Kind() == types.Uintptr) {
						elemCS := convertToCSTypeName(v.getTypeName(ptrUnder.Elem(), false))
						return fmt.Sprintf("((%s)(%s<%s>)(uintptr)(%s))", targetTypeName, PointerPrefix, elemCS, expr)
					}
				}
			}
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

				// A NAMED-over-POINTER arg — `unsafe.Pointer(result.Addr)` where Addr is
				// syscall.Pointer (`type Pointer *struct{}`, net lookup_windows CS0030 ×2):
				// the named wrapper's uintptr bridge provides the first leg, golib Pointer
				// converts from uintptr — same two-leg hop as the named-numeric arm above.
				if _, isPtrUnder := argNamed.Underlying().(*types.Pointer); isPtrUnder {
					return fmt.Sprintf("((@unsafe.Pointer)(uintptr)(%s))", expr)
				}
			}

			if _, isPtr := v.info.TypeOf(arg).(*types.Pointer); isPtr {
				// A pointer PARAMETER carries its BOX (`Ꮡp`) alongside the value alias, and golib's
				// `implicit operator uintptr(ж<T>)` already yields exactly the address Go wants: 0 for
				// a nil box, the aliased address for a reinterpreted native pointer, the pinned
				// storage otherwise. Build the Pointer from the box so `unsafe.Pointer(p)` never
				// DEREFERENCES p — Go's `unsafe.Pointer(nil)` is the 0 address, and a nil out-pointer
				// is idiomatic in the syscall wrappers (`DuplicateHandle(…, nil, …,
				// DUPLICATE_CLOSE_SOURCE)` closes a handle without returning one). The FromRef form
				// instead read the value alias, whose entry-time `ref var p = ref Ꮡp.Value` threw a
				// nil-pointer panic before the call — the hang/crash behind os/exec child creation.
				// Rendering the box also removes the bare VALUE reference from the body, so an
				// otherwise-unused alias is then skipped as dead (visitFuncDecl's
				// bodyReferencesIdentAsValue) and the entry deref disappears with it. Pointer params
				// whose pointee is a basic or struct type already emitted this box form.
				if v.exprIsDerefdPointerParam(arg) {
					identContext := DefaultIdentContext()
					identContext.isPointer = true

					return fmt.Sprintf("new @unsafe.Pointer(%s)", v.convExpr(arg, []ExprContext{identContext}))
				}

				// A deref-aliased pointer RECEIVER renders as the pointed-to VALUE alias
				// (`ref var pc0 = ref Ꮡpc0.Value`) and has NO box to address — a value-ref receiver is
				// `this ref T r` — so its address must come from the alias itself.
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

			// A NAMED-slice arg converting to string — `string(buf)` where `type
			// appendSliceWriter []byte` (strings replace.go) — hops through the written
			// underlying: the [GoType] wrapper converts only to slice<byte>, and
			// slice<byte>→@string is a SECOND user conversion C# won't chain (CS0030).
			if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok {
				if sliceType, ok := argNamed.Underlying().(*types.Slice); ok {
					if basic, ok := sliceType.Elem().Underlying().(*types.Basic); ok && (basic.Kind() == types.Byte || basic.Kind() == types.Rune) {
						return fmt.Sprintf("((@string)(%s)%s)", v.getCSTypeName(sliceType), expr)
					}
				}
			}
		}

		// A conversion whose TARGET is an integer-constrained TYPE PARAMETER — `Int(x)` in
		// rand.N[Int intType] (the banked E(100) family; also reflect rangeNum's `T(0)`/`T(v)`)
		// — cannot be a C# cast (CS0030). Route through golib's runtime-typed ConvertToType<T>.
		// An argument that is ITSELF a type parameter first drops to uint64 (no overload binds
		// a bare type parameter).
		if targetTP, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.TypeParam); ok && typeParamIsInteger(targetTP) {
			if argTP, ok := types.Unalias(v.info.TypeOf(arg)).(*types.TypeParam); ok && typeParamIsInteger(argTP) {
				return fmt.Sprintf("ConvertToType<%s>(ConvertToUInt64<%s>(%s))", targetTypeName, v.getCSTypeName(argTP), expr)
			}

			return fmt.Sprintf("ConvertToType<%s>(%s)", targetTypeName, expr)
		}

		// The MIRROR: a conversion FROM an integer-constrained type parameter TO a basic integer
		// — `uint64(n)` in rand.N[Int] (CS0030). Drop through golib's ConvertToUInt64 (sign-/
		// zero-extension through the instantiated type — Go's exact widening), then a plain
		// numeric cast when the target is not uint64 itself.
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok && targetBasic.Info()&types.IsInteger != 0 {
			if argTP, ok := types.Unalias(v.info.TypeOf(arg)).(*types.TypeParam); ok && typeParamIsInteger(argTP) {
				inner := fmt.Sprintf("ConvertToUInt64<%s>(%s)", v.getCSTypeName(argTP), expr)

				if targetBasic.Kind() == types.Uint64 {
					return inner
				}

				return fmt.Sprintf("(%s)%s", targetTypeName, inner)
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

				// The BYTE/RUNE-slice sibling: a string literal converting to a NAMED type whose
				// underlying is `[]byte`/`[]rune` — `htmlSig("<!DOCTYPE HTML")`, `type htmlSig
				// []byte` (net/http sniff.go's signature table, CS0030 ×17). The u8 span converts
				// to neither the wrapper (whose [GoType] operator takes exactly its underlying
				// slice) nor through @string in one hop (C# chains at most one user-defined
				// conversion). Materialize the underlying slice the way the plain `[]byte("…")`
				// conversion does — the slice<T>(T[]) builtin over the literal's @string — and the
				// wrapper's own operator applies: `((htmlSig)slice<byte>((@string)"…"u8))`.
				if sliceType, ok := named.Underlying().(*types.Slice); ok {
					if basic, ok := sliceType.Elem().Underlying().(*types.Basic); ok && (basic.Kind() == types.Byte || basic.Kind() == types.Rune) {
						return fmt.Sprintf("((%s)%s((@string)%s))", targetTypeName, v.getCSTypeName(sliceType), expr)
					}
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
		// A POINTER reinterpret from a named-slice pointer to its underlying-slice pointer —
		// net fd_windows.go's `fd.pfd.Writev((*[][]byte)(buf))` with `buf *Buffers` (`type
		// Buffers [][]byte`): ж<Buffers> and ж<slice<slice<byte>>> are unrelated
		// instantiations (CS0030). Project a ж-view over the wrapper's backing field —
		// `Ꮡbuf.of(Buffers.Ꮡm_value)` — TRUE aliasing: header writes through the view
		// (Writev's consume reslicing) land on the original wrapper.
		if targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer); ok {
			if _, targetElemIsNamed := types.Unalias(targetPtr.Elem()).(*types.Named); !targetElemIsNamed {
				if targetSlice, ok := targetPtr.Elem().Underlying().(*types.Slice); ok {
					if argPtr, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); ok {
						if argNamed, ok := types.Unalias(argPtr.Elem()).(*types.Named); ok {
							if argUnderSlice, ok := argNamed.Underlying().(*types.Slice); ok && types.Identical(argUnderSlice, targetSlice) {
								ptrExpr := expr

								if identArg, ok := arg.(*ast.Ident); ok {
									ptrContext := DefaultIdentContext()
									ptrContext.isPointer = true
									ptrExpr = v.convIdent(identArg, ptrContext)
								}

								namedCS := convertToCSTypeName(v.getTypeName(argNamed, false))
								return fmt.Sprintf("%s.of(%s.Ꮡm_value)", ptrExpr, namedCS)
							}
						}
					}
				}
			}
		}

		// The MIRROR reinterpret: an underlying-slice pointer to a NAMED-slice pointer —
		// log/slog/internal/buffer's `(*Buffer)(&b)` with `type Buffer []byte` and a local
		// `b []byte`. A named-wrapper box CONTAINS the underlying (the forward arm above projects
		// it out via `.of(Named.Ꮡm_value)`), but a bare-slice box does NOT contain a wrapper, so
		// the reverse cannot project — it must CONSTRUCT a wrapper box over the addressed slice:
		// `Ꮡ(new Buffer(b))`. A bare `(ж<Buffer>)(Ꮡ(b))` cast is CS0030 (unrelated ж
		// instantiations). Aliasing with the original `b` is not preserved — `Ꮡ(value)` already
		// copies — but the reinterpret is used through the returned pointer, which is the pattern.
		if targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer); ok {
			if targetNamed, ok := types.Unalias(targetPtr.Elem()).(*types.Named); ok {
				if _, targetIsSlice := targetNamed.Underlying().(*types.Slice); targetIsSlice {
					if argPtr, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); ok {
						if _, argElemIsNamed := types.Unalias(argPtr.Elem()).(*types.Named); !argElemIsNamed {
							if argSlice, ok := argPtr.Elem().Underlying().(*types.Slice); ok && types.Identical(argSlice, targetNamed.Underlying()) {
								namedCS := convertToCSTypeName(v.getTypeName(targetNamed, false))

								var inner string

								if u, ok := arg.(*ast.UnaryExpr); ok && u.Op == token.AND {
									inner = v.convExpr(u.X, nil)
								} else {
									// The arg is an existing POINTER expression (not `&x`) — cryptobyte's
									// `(*String)(out)` with `out *[]byte`. Render its POINTEE slice via the
									// deref: a pointer PARAMETER yields its deref-aliased slice (`@out`), a
									// box yields `Ꮡx.Value`. The prior `(expr).Value` wrongly added `.Value`
									// to the already-deref'd `@out` (a `slice<byte>` has no `.Value` — CS1061).
									inner = v.convExpr(&ast.StarExpr{Star: arg.Pos(), X: arg}, nil)
								}

								return fmt.Sprintf("%s(new %s(%s))", AddressPrefix, namedCS, inner)
							}
						}
					}
				}
			}
		}

		// A conversion between two NAMED SLICE types sharing an identical underlying — tar's
		// `sparseElem(s[i*24:])` where s is sparseArray (both `[]byte`): the named-slice
		// slicing wrapper-return makes the arg the NAMED wrapper, and a direct
		// `(sparseElem)(sparseArray)` cast chains two user-defined operators (CS0030). Hop
		// through the shared underlying slice: `((sparseElem)(slice<byte>)(…))`.
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if sliceUnder, ok := named.Underlying().(*types.Slice); ok {
				if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok && argNamed != named {
					if _, argIsSlice := argNamed.Underlying().(*types.Slice); argIsSlice && types.Identical(argNamed.Underlying(), named.Underlying()) {
						underlyingCS := convertToCSTypeName(v.getTypeName(sliceUnder, false))
						return fmt.Sprintf("((%s)(%s)(%s))", targetTypeName, underlyingCS, expr)
					}
				}
			}
		}

		// underlying basic (the existing cast already binds → no churn). types.Unalias: os's
		// `type FileMode = fs.FileMode` arrives as a *types.Alias, and the bare assertion
		// skipped the hop (direct nint cast into the foreign wrapper, CS0030 removeall_noat).
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
				argType := v.info.TypeOf(arg)

				// An IDENTITY conversion to the same named numeric type — `arenaIdx(x)` where x is
				// already `arenaIdx` — is a Go no-op. This happens for an untyped-constant shift that
				// adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types
				// already types as arenaIdx, so convExpr(arg) has emitted `((arenaIdx)((nuint)1 << b))`),
				// and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expr in another
				// `((arenaIdx)…)` cast just doubles it; return the converted arg as-is.
				if argType != nil && types.Identical(argType, named) {
					// EXCEPT a plain CONSTANT arg: go/types types the constant AS the target
					// (identity), but convExpr rendered the bare literal (`Word(1)` → `1`),
					// which under a binary operator resolves as int and degrades the whole
					// expression (math/big's `mask := Word(1)<<s - 1`, CS0029 ×3). Re-impose
					// the named cast — the := declaration patch in visitAssignStmt covered
					// only the direct-RHS position.
					if tv, ok := v.info.Types[callExpr]; ok && tv.Value != nil {
						namedCS := convertToCSTypeName(v.getTypeName(named, false))

						if !strings.HasPrefix(expr, "(("+namedCS+")") && !strings.HasPrefix(expr, "("+namedCS+")") && !strings.HasPrefix(expr, "new ") {
							if castOperandNeedsParens(namedCS, expr) {
								return fmt.Sprintf("((%s)(%s))", namedCS, expr)
							}

							return fmt.Sprintf("((%s)%s)", namedCS, expr)
						}
					}

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

					// EXCEPTION to the hop: the conversion target IS the arg's WRITTEN base named
					// type — `syscall.Handle(k)` where `type Key syscall.Handle` (registry value.go).
					// The arg's [GoType] wrapper declares the one-step operator to exactly that type
					// (Key → ΔHandle), so the direct cast binds; the underlying hop's first leg
					// `(uintptr)k` is itself the illegal two-op chain (Key→ΔHandle→uintptr, CS0030).
					// Gated to a CROSS-PACKAGE base: only there does the [GoType] emission keep the
					// NAMED base (`[GoType("syscall_package.ΔHandle")]`); a same-package chain
					// resolves to the basic underlying (`[GoType("num:uintptr")]` — no named-base
					// operator exists) and the underlying hop already binds one-op-per-leg.
					if rhs, okRHS := packageTypeSpecRHS[argNamed.Obj()]; okRHS && rhs != nil {
						if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed == named &&
							named.Obj().Pkg() != argNamed.Obj().Pkg() {
							return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
						}
					}

					// The FORWARD mirror: the conversion TARGET's written base IS the arg's named
					// type (`Key(handle)` / `reading(celsius)` where `type reading lib.Celsius`) —
					// the target's wrapper declares the one-step operator FROM exactly that type;
					// the hop's second leg `(reading)(double)…` has no operator (CS0030).
					if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil {
						if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed == argNamed &&
							named.Obj().Pkg() != argNamed.Obj().Pkg() {
							return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
						}
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

		// A conversion to STRING whose arg is an UNTYPED constant REFERENCE
		// (`string(utf8.RuneError)`, time format.cs) renders the arg as its cross-package
		// `static readonly` Untyped* wrapper, from which @string has no conversion (CS0030).
		// Hop through the constant's DEFAULT Go type first (`((@string)(rune)runeError)`) —
		// exactly Go's conversion semantics. A plain literal is already a C# constant and
		// keeps its direct form (no churn).
		if basicTarget != nil && basicTarget.Kind() == types.String {
			if tvArg, ok := v.info.Types[arg]; ok && tvArg.Value != nil {
				if argBasic, ok := tvArg.Type.(*types.Basic); ok && argBasic.Info()&types.IsUntyped != 0 && !v.isCSharpConstantExpr(arg) {
					expr = fmt.Sprintf("(%s)%s", v.getCSTypeName(types.Default(argBasic).(*types.Basic)), expr)
				}
			} else if argType := v.info.TypeOf(arg); argType != nil {
				// A NAMED integer type (`type Delim rune`, encoding/json's `string(d)`) renders as a
				// [GoType] wrapper struct with no direct @string conversion (CS0030). Go's string(intType)
				// is a code-point → UTF-8 conversion; hop through the underlying integer so the wrapper's
				// implicit operator yields it first, then @string converts the code point.
				if named, ok := types.Unalias(argType).(*types.Named); ok {
					if u, ok := named.Underlying().(*types.Basic); ok && u.Info()&types.IsInteger != 0 {
						expr = fmt.Sprintf("(%s)%s", v.getCSTypeName(u), expr)
					}
				}
			}
		}

		// A conversion between two DIFFERENT NAMED types that share a COMPOSITE underlying (net/mail
		// textproto.MIMEHeader(h) where Header and MIMEHeader both wrap map[string][]string): each is a
		// [GoType] wrapper with implicit conversions only to/from its OWN underlying, and C# will not
		// chain Header->map->MIMEHeader in one cast (CS0030). Hop through the shared underlying so each
		// wrapper implicit operator applies: (MIMEHeader)(map<@string, slice<@string>>)h.
		if targetNamed, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if convArgType := v.info.TypeOf(arg); convArgType != nil {
				if argNamed, ok := types.Unalias(convArgType).(*types.Named); ok && !types.Identical(targetNamed, argNamed) && types.Identical(targetNamed.Underlying(), argNamed.Underlying()) {
					// Map/slice/array underlyings have a nameable C# cast target (map<K,V>, slice<T>,
					// array<T>). A STRUCT underlying is anonymous (struct{...} — not a valid cast term,
					// CS1525) AND is the reverse *underlying->*named REINTERPRET case (NamedPointerReinterpret),
					// so it is excluded.
					switch targetNamed.Underlying().(type) {
					case *types.Map, *types.Slice, *types.Array:
						expr = fmt.Sprintf("(%s)%s", convertToCSTypeName(v.getTypeName(targetNamed.Underlying(), false)), expr)
					}
				}
			}
		}

		// A widened constant operand already carries this exact narrowing cast in its own emission
		// (widenedConstExprCastType) — `int32(1<<31 - 1)` converts an operand that rendered as
		// `(int32)(2147483648L - 1)`. Re-casting would only double it, so return it as it stands;
		// the same wholeExprIsCastOfType check the assignment path uses for the sibling fold.
		if operand, ok := arg.(*ast.BinaryExpr); ok && targetIsBasic &&
			v.widenedConstExprCastType(operand) == targetTypeName && wholeExprIsCastOfType(expr, targetTypeName) {
			return expr
		}

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
			if targetIsBasic {
				return fmt.Sprintf("(%s)(%s)", targetTypeName, expr)
			}

			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
		}

		if targetIsBasic {
			if castOperandNeedsParens(targetTypeName, expr) {
				return fmt.Sprintf("(%s)(%s)", targetTypeName, expr)
			}

			return fmt.Sprintf("(%s)%s", targetTypeName, expr)
		}

		if castOperandNeedsParens(targetTypeName, expr) {
			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
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

			if (funcName == "print" || funcName == "println") && v.callFunIsUniverseBuiltin(callExpr) {
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

			// A GENERIC parameter declared as a SLICE of a type parameter whose constraint is a
			// METHOD-SET interface — `walkList[N Node](v Visitor, list []N)` — instantiated with
			// a POINTER type argument (`walkList(v, n.Names)`, N=*Ident): the emitted constraint
			// is `where N : Node` (see getGenericDefinition), which the `ж<Ident>` box cannot
			// satisfy — the box does not implement the interface, its generated pointer adapter
			// does. Project the slice element-wise through the adapter (`widen<ж<Ident>, Node>(
			// (~n).Names, elemᴛ0 => new IdentжNode(elemᴛ0))`), instantiating N as the interface
			// itself. The projection copies the slice HEADER only (elements alias through the
			// shared box, so method calls mutate the original objects); a callee that reassigns
			// `list[i]` itself would not write back — acceptable for the read/widen shape this
			// targets. convertToInterfaceType supplies the adapter reference AND the GoImplement
			// recording, exactly as at scalar *T→iface call sites.
			if paramHasArg && (replacementArgs == nil || len(replacementArgs[i]) == 0) {
				if declSlice, ok := paramType.Underlying().(*types.Slice); ok {
					if tp, ok := types.Unalias(declSlice.Elem()).(*types.TypeParam); ok {
						if iface, ok := tp.Constraint().Underlying().(*types.Interface); ok && iface.NumMethods() > 0 && iface.IsMethodSet() {
							if instParam := v.instantiatedParamType(callExpr, i); instParam != nil {
								if instSlice, ok := instParam.Underlying().(*types.Slice); ok {
									if ptrElem, ok := instSlice.Elem().(*types.Pointer); ok && types.Implements(ptrElem, iface) {
										elemVar := fmt.Sprintf("elem%s%d", TempVarMarker, i)
										wrapped := v.convertToInterfaceType(tp.Constraint(), ptrElem, elemVar)

										if strings.HasPrefix(wrapped, "new ") {
											if replacementArgs == nil {
												replacementArgs = make([]string, params.Len())
											}

											replacementArgs[i] = fmt.Sprintf("widen<%s, %s>(%s, %s => %s)",
												v.getCSTypeName(ptrElem), v.getCSTypeName(tp.Constraint()), DynamicCastArgMarker, elemVar, wrapped)
										}
									}
								}
							}
						}
					}
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

			// An untyped INTEGER constant passed to a GENERIC parameter whose declared type IS the
			// ELEMENT type parameter of a sibling `~[]E`-constrained type parameter — the
			// `Index[S ~[]E, E comparable](s S, v E)` / `Insert[S ~[]E, E any](s S, i int, v ...E)`
			// shape (slices) — drives C# generic inference from the LITERAL's OWN C# type. Go infers
			// E from S's core type (`[]int` → E = Go `int` → `nint`), but C# has no analogue for
			// `~[]E` core-type inference: `where S : ISlice<E>` does NOT flow S's concrete element to
			// E, so C# infers E SOLELY from the value literal. A bare int literal is C# `int`
			// (System.Int32), so `Index(s, 2)` makes C# infer E=int and `slice<nint>` then fails the
			// `~[]int` constraint (CS0315/CS0411/CS1503). go/types has already resolved the literal to
			// E's instantiation (int→nint, byte, int64→long, …); emit it AT that C# type so inference
			// binds the intended argument. A resolved `int32`/`rune` (already System.Int32) and a
			// value convBasicLit already casts (`(nint)…L`) are left untouched — no cast, no noise (the
			// wholeExprIsCastOfType skip in convExprList drops the latter). The sibling-lock gate is
			// deliberately narrow: a FREELY-inferred type parameter (`First[T any](v ...T)`), one
			// determined DIRECTLY by another argument (`setThrough[P *T, T any](p P, v T)` — C# infers
			// T from the pointer), and an EXPLICITLY-instantiated call (`NewOption[nint](42)`) all
			// infer correctly already, so they keep their bare literal. A variadic slot flags every
			// trailing element; an existing per-arg cast (append/narrow-int/defer) is not overwritten.
			if paramHasArg {
				if pTP, ok := types.Unalias(paramType).(*types.TypeParam); ok && typeParamIsSliceElementOfSibling(funcSignature, pTP) {
					lastArg := i

					if funcSignature.Variadic() && i == params.Len()-1 {
						lastArg = len(callExpr.Args) - 1
					}

					for j := i; j <= lastArg; j++ {
						if castType := v.untypedIntGenericArgCastType(callExpr.Args[j]); castType != "" {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							if _, exists := callExprContext.castArgToType[j]; !exists {
								callExprContext.castArgToType[j] = castType
							}
						}
					}
				}
			}

			// A TYPE PARAMETER reads as an interface (its underlying is the constraint), which
			// routed a pointer-instantiated generic param into the interface arm and away from
			// the box treatment (`ptr = abi.Escape(ptr)` instantiating T = *T passed the
			// deref'd value alias - internal/weak Make, CS0029). When the instantiation is a
			// pointer, fall through to the pointer arm below.
			// A NAMED FUNC-type parameter receiving a value of a DIFFERENT delegate type
			// wraps in the target delegate's constructor — C# has no implicit conversion
			// between distinct delegate types (mirrors the composite-literal field rule;
			// archive/tar templateV7Plus's stringFormatter args, CS1503). Method groups and
			// func literals convert natively and are left bare.
			if paramHasArg {
				if paramNamed, ok := types.Unalias(paramType).(*types.Named); ok {
					if _, isSig := paramNamed.Underlying().(*types.Signature); isSig {
						if argType := v.getType(callExpr.Args[i], false); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(paramType)) {
							if _, argIsSig := argType.Underlying().(*types.Signature); argIsSig {
								// A GENERIC named delegate param renders unsubstituted type
								// params at the call site — leave it to native conversion.
								if _, isLit := callExpr.Args[i].(*ast.FuncLit); !isLit && !typeContainsTypeParams(paramType) {
									if callExprContext.wrapArgWithNew == nil {
										callExprContext.wrapArgWithNew = make(map[int]string)
									}

									callExprContext.wrapArgWithNew[i] = v.getCSTypeName(paramType)
								}
							}
						}
					}
				}

				// The MIRROR: a STRUCTURAL (anonymous) func-type parameter receiving a value that
				// RENDERS as a named delegate — h2's `sc.scheduleHandler(…, handler)` where the
				// parameter is the written `func(ResponseWriter, *Request)` (CS1503). Go converts
				// named→structural implicitly; C# needs the same delegate re-wrap, targeting the
				// synthesized structural delegate: `new Action<ResponseWriter, ж<Request>>(handler)`.
				// Two argument shapes render named: a value whose GO type is a named func type
				// (with methods — a METHODLESS named func type already renders as the structural
				// delegate itself and stays bare), and a `:=` local DECLARED from a method group,
				// which the declaration emission types with the matching package named delegate
				// (`HandlerFunc handler = Ꮡsc.Value.handler.ServeHTTP;` — see visitAssignStmt's
				// methodGroupDelegateType) even though go/types keeps it structural. Method
				// groups and func literals themselves convert natively and are excluded by both
				// gates.
				if _, paramIsSig := types.Unalias(paramType).(*types.Signature); paramIsSig && paramHasArg && !typeContainsTypeParams(paramType) {
					if argType := v.getType(callExpr.Args[i], false); argType != nil {
						argRendersNamedDelegate := false

						if argNamed, ok := types.Unalias(argType).(*types.Named); ok {
							if _, argIsSig := argNamed.Underlying().(*types.Signature); argIsSig {
								if _, collapses := methodlessNamedFuncSignature(argNamed); !collapses {
									argRendersNamedDelegate = true
								}
							}
						} else if argSig, ok := types.Unalias(argType).(*types.Signature); ok {
							if argIdent, ok := callExpr.Args[i].(*ast.Ident); ok &&
								v.namedFuncTypeNameForSignature(argSig) != "" && v.identDeclaredFromMethodGroup(argIdent) {
								argRendersNamedDelegate = true
							}
						}

						if argRendersNamedDelegate {
							if callExprContext.wrapArgWithNew == nil {
								callExprContext.wrapArgWithNew = make(map[int]string)
							}

							callExprContext.wrapArgWithNew[i] = v.getCSTypeName(paramType)
						}
					}
				}
			}

			// An ERASED pointer-core parameter ([P *T]) reads as an interface too (same
			// type-parameter shape as the instantiated-pointer carve-out above) — route it to
			// the pointer arm so a P-typed argument supplies its box (`clone(p)` inside another
			// erased generic must pass `Ꮡp`, not the deref'd value alias).
			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast && !v.instantiatedParamIsPointer(callExpr, paramType, i) && !signatureErasedParamPointerOk(funcSignature, paramType) {
				callExprContext.u8StringArgOK[i] = false

				// A variadic interface parameter (`...Type` / `...any`) receives EVERY trailing
				// argument (getParameterType already yields the variadic ELEMENT type), so the
				// non-empty *T→iface adapter treatment (`makeSig(S, S, NewSlice(T))` left the
				// ж<Slice> result unwrapped otherwise — go/types builtins CS1503) must fan out to
				// all of them; a spread arg is excluded at consumption (convExprList's spreadArg
				// guard), and a non-variadic parameter degenerates to the single index (lastArg == i),
				// byte-identical to before.
				lastArg := i

				if funcSignature.Variadic() && i == params.Len()-1 {
					lastArg = len(callExpr.Args) - 1
				}

				// The untyped-int→nint box cast is DELIBERATELY skipped for a variadic `...any`
				// argument (the fmt/print/log family): a boxed System.Int32 formats identically to
				// nint and its %T / type-switch dynamic type is already resolved as `int`, so the
				// cast would be redundant noise on the most common call pattern — matching how the
				// string→@string boxing family also leaves the variadic fmt call position untouched.
				// A non-variadic `any` parameter (`atomic.Value.Store`, `context.WithValue`, …) is a
				// value the caller stores and later type-asserts, so it DOES take the cast.
				variadicSlot := funcSignature.Variadic() && i == params.Len()-1

				for j := i; j <= lastArg; j++ {
					if !isEmpty {
						callExprContext.interfaceTypes[j] = paramType
					}

					// An untyped `int` constant boxed into the interface must be cast to nint so its
					// C# box matches Go's boxed `int` dynamic type and a later `.(int)` (`._<nint>()`)
					// assertion succeeds — see argBoxesAsInt32ButNeedsNint. Reuses the per-argument
					// castArgToType plumbing convExprList already applies as `(nint)(value)`.
					// isEmptyInterfaceTarget (not the outer isInterface) gates this to a REAL `any`
					// parameter: a type parameter constrained by `any` also reads as an empty interface
					// here, but its instantiation binds the argument to a concrete type (`T`=int → the
					// nint parameter), where a bare int literal already converts implicitly — unlike
					// the u8-span→@string case, no cast is needed and one would be spurious.
					if !variadicSlot && isEmptyInterfaceTarget(paramType) && j < len(callExpr.Args) && v.argBoxesAsInt32ButNeedsNint(callExpr.Args[j]) {
						if callExprContext.castArgToType == nil {
							callExprContext.castArgToType = make(map[int]string)
						}

						callExprContext.castArgToType[j] = "nint"
					}

					// The EMPTY interface (`any`/`interface{}`) needs no wrapping adapter, but a
					// POINTER argument must still render as the pointer VALUE — the box `Ꮡp`, not the
					// deref'd value alias `p` — because Go boxes the *pointer* into the interface.
					// A deref-aliased pointer (a `*T` parameter, or the current method's direct-ж
					// receiver) whose box is dropped loses pointer identity: fmt's
					// `func (p *pp) free() { … ppFree.Put(p) }` put the `pp` VALUE into the sync.Pool,
					// so the next `Get().(*pp)` (rendered `._<ж<pp>>()`) failed the cast and panicked
					// on the 2nd pool round-trip. This mirrors the pointer-parameter branch below: a
					// pointer LOCAL already holds its box directly (`!v.isPointer`/param/direct-ж
					// guard excludes it), and a variadic `...any` fans the treatment across every
					// trailing argument (so it is NOT gated by variadicSlot, unlike the nint cast).
					// Since the empty interface leaves `interfaceTypes` unset, the argument takes the
					// identical convExpr path a `*T`-parameter argument does.
					if isEmpty && j < len(callExpr.Args) {
						if argType := v.getType(callExpr.Args[j], false); argType != nil {
							if _, argIsPtr := argType.(*types.Pointer); argIsPtr {
								ident := getIdentifier(callExpr.Args[j])

								if !v.isPointer(ident) || v.identIsParameter(ident) || v.exprIsCurrentDirectBoxReceiver(callExpr.Args[j]) {
									callExprContext.argTypeIsPtr[j] = true
								}
							}
						}
					}
				}
			} else if paramHasArg && (isPointer(paramType) || signatureErasedParamPointerOk(funcSignature, paramType) || v.instantiatedParamIsPointer(callExpr, paramType, i)) && !(callExprContext.hasSpreadOperator && i == params.Len()-1) {
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

				// A variadic pointer parameter (`...*T`) receives EVERY trailing argument, so the
				// per-argument box treatment below must apply to all of them — this loop only visits
				// declared parameters, and argTypeIsPtr's false default left args after the first as
				// the deref'd value alias (`checkInitialized(Ꮡp, q)`, edwards25519 CS1503). Mirrors
				// the variadic fan-out of the type-parameter @string treatment above; the spread form
				// (`f(s...)`) is already excluded by this branch's guard. A non-variadic parameter
				// degenerates to the single index (lastArg == i), byte-identical to before.
				lastArg := i

				if funcSignature.Variadic() && i == params.Len()-1 {
					lastArg = len(callExpr.Args) - 1
				}

				for j := i; j <= lastArg; j++ {
					argIsUnsafePtr := false

					if argType := v.getType(callExpr.Args[j], false); argType != nil {
						if basic, ok := argType.Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
							argIsUnsafePtr = true
						}
					}

					if paramIsUnsafePtr && argIsUnsafePtr {
						// pass the @unsafe.Pointer struct directly; no argTypeIsPtr / `.Value`
						continue
					}

					ident := getIdentifier(callExpr.Args[j])

					// A deref-aliased pointer (a parameter, or the current method's direct-ж receiver)
					// passed WHOLE as a pointer argument must be emitted as its box `Ꮡc`, not the value
					// alias `c` (a value cannot bind a `ж<T>` parameter → CS1503). A direct-ж receiver
					// is not an `identIsParameter`, so it needs the explicit receiver check — e.g.
					// `func (c *mcache) prepareForSweep(){ … stackcache_clear(c) }` where `c` also takes
					// a field address (making it direct-ж).
					if !v.isPointer(ident) || v.identIsParameter(ident) || v.exprIsCurrentDirectBoxReceiver(callExpr.Args[j]) {
						callExprContext.argTypeIsPtr[j] = true
					}
				}
			}
		}
	}

	callExprContext.replacementArgs = replacementArgs

	// Every arm below is keyed on a built-in's NAME; a shadowing declaration of that name makes the
	// call an ordinary one, so the whole group is gated on the identifier actually resolving to the
	// universe built-in (see identIsUniverseBuiltin).
	if ident, ok := callExpr.Fun.(*ast.Ident); ok && v.identIsUniverseBuiltin(ident) {
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

		// close(ch) on a NAMED channel type cannot infer `close<T>(in channel<T>)`'s element
		// from the [GoType("chan T")] wrapper (a user-defined conversion is invisible to
		// generic inference, CS0411) — name the element type explicitly so the wrapper's
		// implicit conversion applies at the argument (see namedChanElemTypeArg). A package
		// that ALSO declares a `close` method shadows the using-static builtin (C# member
		// lookup stops at the class's own group — net/http's `(*conn).close`), so the same
		// `builtin.` qualification the general builtin path applies is needed here too.
		if ident.Name == "close" && len(callExpr.Args) == 1 {
			if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); isBuiltin {
				if typeArgs := v.namedChanElemTypeArg(callExpr.Args[0]); typeArgs != "" {
					funcName := ident.Name

					if packageBuiltinShadows[ident.Name] {
						funcName = "builtin." + funcName
					}

					return fmt.Sprintf("%s%s(%s)", funcName, typeArgs, v.convExpr(callExpr.Args[0], nil))
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
				// Underlying: `make(closeWaiter)` of a NAMED channel type (`type closeWaiter
				// chan struct{}`) takes the same unbuffered default as a plain `make(chan T)` —
				// the wrapper's `(nint size)` constructor forwards to `channel<T>(size)`.
				if _, ok := typeParam.Underlying().(*types.Chan); ok && !isTypeParam {
					if len(remainingArgs) == 0 {
						remainingArgs = "1"
					}
				}

				// `make(StrIntMap)` of a DEFINED map type (`type StrIntMap map[string]int`) with NO
				// size argument: the generated wrapper struct has an allocating `(nint size)` ctor
				// but NO parameterless one, so a bare `new StrIntMap()` is default(StrIntMap) — a
				// NIL map (its backing store is null), making `m == nil` TRUE. Go's `make` yields a
				// NON-nil empty map (`m == nil` is false), so default the size to 0 to run the
				// allocating ctor. Scoped to *types.Named defined types: the UNNAMED `map<K,V>`
				// builtin already has an allocating parameterless ctor and is left as `new map<K,V>()`.
				if _, isNamed := typeParam.(*types.Named); isNamed && !isTypeParam {
					if _, ok := typeParam.Underlying().(*types.Map); ok && len(remainingArgs) == 0 {
						remainingArgs = "0"
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
			// A Go array VALUE appended as a slice ELEMENT (`append(rows, arr)` with rows of
			// type [][3]int) is copied into the new slot; an existing-storage array element
			// clones so the stored element does not alias the source's backing (see
			// exprReadsArrayValueFromStorage). A spread `append(dst, src...)` is untouched —
			// its element-wise copy happens inside golib (a documented remaining gap for
			// nested-array elements, alongside copy()).
			for i := 1; i < len(callExpr.Args); i++ {
				if v.exprReadsArrayValueFromStorage(callExpr.Args[i]) {
					if callExprContext.cloneArrayArg == nil {
						callExprContext.cloneArrayArg = make(map[int]bool)
					}

					callExprContext.cloneArrayArg[i] = true
				}
			}

			if sliceType := v.info.TypeOf(callExpr.Args[0]); sliceType != nil {
				if sliceUnder, ok := sliceType.Underlying().(*types.Slice); ok {
					// A bare `nil` element (untyped nil → `default!`) binds to append's `params`
					// parameter as the WHOLE null array — appending ZERO elements — instead of as a
					// single nil element: `append(b.lines, nil)` on `[][]cell` never grew the slice
					// (tabwriter addLine, which left b.lines empty and every terminateCell indexing
					// `b.lines[len-1]` panicking [-1]). Cast the nil to the element type so it binds as
					// ONE element. The interface and named-composite branches below already do this for
					// their element kinds (both differ from the untyped nil); this covers the remaining
					// nillable element types — unnamed slice/map/pointer/chan/func — for which no other
					// branch fires. A nil element is only ever valid when the element type is nillable,
					// so the cast target always exists.
					for i := 1; i < len(callExpr.Args); i++ {
						if tv, ok := v.info.Types[callExpr.Args[i]]; ok && tv.IsNil() {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							callExprContext.castArgToType[i] = convertToCSTypeName(v.getTypeName(sliceUnder.Elem(), false))
						}
					}

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

					// An INTERFACE element appended from a value of a DIFFERENT type — a pointer
					// rendering as the *T→iface ADAPTER ctor (`new rtypeᴵΔType(Ꮡt)`) or a raw
					// struct value (`new Dog(nil)`) — leaves both append overloads applicable
					// (`append<T>(ISlice, params T[])` infers the concrete/adapter type,
					// `append<T>(slice<T>, params Span<T>)` infers the interface — CS0121 ×3,
					// reflect Method construction). Cast the element to the interface type so the
					// slice<T> overload binds; an already-interface-typed element stays bare. The
					// EMPTY interface (`any`) is affected identically: `append(args[:len:len], c.output)`
					// with `args []any` and `c.output []byte` infers T=slice<byte> on the ISlice
					// overload but T=any on the slice<T> overload (testing flushToParent, CS0121) —
					// cast to `any` so both agree.
					if needsCast, _ := isInterface(sliceUnder.Elem()); needsCast {
						elemCSType := convertToCSTypeName(v.getTypeName(sliceUnder.Elem(), false))

						for i := 1; i < len(callExpr.Args); i++ {
							if argType := v.info.TypeOf(callExpr.Args[i]); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(sliceUnder.Elem())) {
								if callExprContext.castArgToType == nil {
									callExprContext.castArgToType = make(map[int]string)
								}
								callExprContext.castArgToType[i] = elemCSType
							}
						}
					}

					// A NAMED-COMPOSITE element type (slice/struct/map — NOT interface or basic, those are
					// handled by the branches above) appended from a value of a DIFFERENT type: crypto/x509/pkix
					// append(rdns, s) where rdns is RDNSequence ([]RelativeDistinguishedNameSET) and s is
					// []AttributeTypeAndValue (the element UNDERLYING). Go implicitly converts s to the element
					// type; C# append otherwise infers the element as slice<ATV>, so the result slice<slice<ATV>>
					// does not bind RDNSequence (CS0029). Cast the differing arg to the named element type.
					if named, isNamed := sliceUnder.Elem().(*types.Named); isNamed {
						switch named.Underlying().(type) {
						case *types.Basic, *types.Interface:
							// numeric / interface element casts are handled above
						default:
							elemCSType := convertToCSTypeName(v.getTypeName(sliceUnder.Elem(), false))

							for i := 1; i < len(callExpr.Args); i++ {
								if argType := v.info.TypeOf(callExpr.Args[i]); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(sliceUnder.Elem())) {
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
				typeArgs := named.TypeArgs()

				// A GENERIC FUNCTION's explicit arguments must come from the CALLEE's resolved
				// instantiation (info.Instances), not the RESULT type's arguments — the lists
				// differ whenever the callee has more type parameters than the result names
				// (reflect's `rangeNum[T, N](num N) iter.Seq[T]` called `rangeNum[int8](v)`: the
				// result Seq[T] carries ONE argument where the method needs TWO — CS0305 ×11).
				// The result's OWN arguments still gate WHETHER to emit (a generic callee
				// returning a plain named type keeps C# inference — no churn); the conversion/
				// constructor form (funIsType) keeps the result's arguments outright.
				var typeParams []string

				if calleeIsGeneric && !funIsType {
					if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
						if instance, ok := v.info.Instances[funIdent]; ok && instance.TypeArgs != nil {
							// Instance-derived positions align with the callee's declared type
							// parameters, so erased (pointer-core) positions leave the emitted
							// list (see renderedTypeArgs) — identical output when nothing erases.
							typeParams = v.renderedTypeArgs(funIdent, instance.TypeArgs)
						}
					}
				}

				if typeParams == nil {
					for i := range typeArgs.Len() {
						typeParams = append(typeParams, v.getCSTypeName(typeArgs.At(i)))
					}
				}

				if len(typeParams) > 0 {
					typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
				}
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
					(v.calleeHasConstraintOnlyTypeParam(funIdent) || v.callHasMethodGroupArg(callExpr)) {
					// Erased (pointer-core) callee positions leave the emitted list — `clone[P *T,
					// T any]` emits `clone<ΔSignature>(…)` (see renderedTypeArgs); a list that
					// erases to empty stays bare.
					if typeParams := v.renderedTypeArgs(funIdent, instance.TypeArgs); len(typeParams) > 0 {
						typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
					}
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
			} else if len(callExpr.Args) == 1 && v.currentFuncSignature != nil {
				// The ref-based extension-function rewrite below only applies to a pointer-receiver
				// METHOD whose argument aliases the receiver; a package-scope initializer (e.g.
				// `var p uintptr = uintptr(unsafe.Pointer(&x))` in cmp_test.go) has no enclosing
				// function, so currentFuncSignature is nil — skip the receiver handling and fall
				// through to the normal `(uintptr)new @unsafe.Pointer(...)` emission.
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

	// A `[]byte(s)` conversion of a `string | []byte` union-constrained value (rendered
	// IByteSeq<byte> at lambda params — time format_rfc3339's parseUint): the usual route
	// binds golib's slice<T>(T[])/slice(@string) builtins, neither of which accepts the
	// interface — and ADDING a builtin IByteSeq overload makes @string args ambiguous
	// (CS0121, both conversions applicable). Emit the slice<byte>(IByteSeq<byte>)
	// CONSTRUCTOR directly: for the interface-typed arg only it is applicable, sharing a
	// boxed slice's backing and copying a string — Go's per-instantiation semantics.
	if funcTypeName == "[]byte" && len(callExpr.Args) == 1 {
		if tp, ok := types.Unalias(v.getType(callExpr.Args[0], false)).(*types.TypeParam); ok && typeParamIsStringByteUnion(tp) {
			return fmt.Sprintf("new slice<byte>(%s)", v.convExpr(callExpr.Args[0], nil))
		}
	}

	// A `[]byte("literal")` over a plain-text string LITERAL feeds the `u8` ROM span straight into the
	// slice — `slice<byte>("literal"u8)` — instead of routing through the heap `@string` the general
	// `[]byte`/`[]rune` path emits (`slice<byte>((@string)"literal")`, via sourceIsRuneArray below). The
	// `u8` literal is zero-allocation static ROM; golib's `slice<T>(ReadOnlySpan<T>)` copies it into the
	// slice's backing array — one fewer allocation and a cleaner rendering. Gated to what `convBasicLit`
	// renders as a `u8` span: a high-`\xHH`-byte literal stays the byte-array-backed `@string` (its bytes
	// do not round-trip through `u8`), and a `[]rune` conversion needs `@string`'s rune decoding, so only
	// `[]byte` qualifies. A string VARIABLE is already an `@string` and keeps the general path.
	if funcTypeName == "[]byte" && len(callExpr.Args) == 1 {
		if basicLit, ok := callExpr.Args[0].(*ast.BasicLit); ok && basicLit.Kind == token.STRING {
			if strings.HasPrefix(basicLit.Value, "`") || !stringLiteralNeedsByteArray(basicLit.Value) {
				u8Context := DefaultBasicLitContext()
				u8Context.u8StringOK = true
				return fmt.Sprintf("slice<byte>(%s)", v.convExpr(basicLit, []ExprContext{u8Context}))
			}
		}
	}

	// An explicit slice conversion `[]E(x)` whose SOURCE is a ~[]E-constrained type
	// parameter S is the EXPLICIT twin of the implicit wrapArgWithNew assignability path
	// above (a value of S passed where a concrete []E is expected). The general call path
	// renders `slice<E>(x)`, binding golib's array-only builtin.slice<T>(T[]) — but S is
	// an ISlice<E>, not E[], so that fails to compile (CS1503). Emit the SHARING
	// slice<T>(ISlice<T>) constructor instead, so an explicit conversion aliases the
	// caller's backing exactly as the implicit form does (Go's `[]E(x)` shares storage).
	// Cleanly disjoint from surrounding conversions: named-slice casts (`[]Named(x)`) take
	// the isTypeConversion cast path and never reach here; string/nil sources are not
	// *types.TypeParam; and the string|[]byte union `[]byte(x)` is handled by the block
	// just above (typeParamSliceCore returns nil when a constraint term is not a slice).
	if strings.HasPrefix(funcTypeName, "[]") && len(callExpr.Args) == 1 {
		if tp, ok := types.Unalias(v.getType(callExpr.Args[0], false)).(*types.TypeParam); ok && typeParamSliceCore(tp) != nil {
			if targetSlice, ok := funcType.(*types.Slice); ok {
				elemName := convertToCSTypeName(v.getTypeName(targetSlice.Elem(), false))
				return fmt.Sprintf("new slice<%s>(%s)", elemName, v.convExpr(callExpr.Args[0], nil))
			}
		}
	}

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

	// sync/atomic.LoadPointer/StorePointer on a MANAGED pointer field — the lock-free
	// `atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))` / `StorePointer(…,
	// unsafe.Pointer(v))` idiom where `x.field` is a `*T` (a `ж<T>` reference). The literal
	// conversion round-trips the managed reference through a transient `uintptr` address and NREs
	// (x/sys/windows's LazyDLL/LazyProc caches). Emit the golib managed-referent overloads on the
	// field box (`ж<ж<T>>`) instead, so the atomic read/write operates on the reference directly.
	// A LOAD result stays `unsafe.Pointer`-typed to Go, so the caller's nil-compare still wraps it
	// `(uintptr)… == nil`; the `ж<T> → uintptr` operator yields 0 for a nil box (see golib), so the
	// comparison is correct without changing the surrounding emission.
	if addrExpr, storeVal, isLoad, ok := v.managedAtomicPointerIdiom(callExpr); ok {
		box := v.convExpr(addrExpr, nil)

		if isLoad {
			return fmt.Sprintf("%s(%s)", funcName, box)
		}

		return fmt.Sprintf("%s(%s, %s)", funcName, box, v.convExpr(storeVal, nil))
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

// identDeclaredFromMethodGroup reports whether ident resolves to a local whose `:=` declaration
// initialized it from a bare function/method reference (a C# method group). Such a declaration is
// emitted with the package named delegate matching its signature when one exists (visitAssignStmt's
// methodGroupDelegateType via namedFuncTypeNameForSignature), so at a call site the local's C# type
// is that NAMED delegate even though go/types reports the unnamed structural signature — the shape
// that needs the delegate re-wrap at a structural func parameter. Scans only the current function's
// body (the `:=` declaration and its uses share the function scope).
func (v *Visitor) identDeclaredFromMethodGroup(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return false
	}

	found := false

	ast.Inspect(v.currentFuncDecl.Body, func(node ast.Node) bool {
		if found {
			return false
		}

		assign, ok := node.(*ast.AssignStmt)

		if !ok || assign.Tok != token.DEFINE || len(assign.Lhs) != len(assign.Rhs) {
			return true
		}

		for i, lhs := range assign.Lhs {
			lhsIdent, ok := lhs.(*ast.Ident)

			if !ok || v.info.Defs[lhsIdent] != obj {
				continue
			}

			found = v.exprIsMethodGroup(assign.Rhs[i])

			return false
		}

		return true
	})

	return found
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

				// SKIP a conversion the [GoType] wrapper already provides: `type bitStringEncoder
				// BitString` makes the TypeGenerator emit BOTH bitStringEncoder<->BitString
				// implicit operators, so a recorded GoImplicitConv<BitString, bitStringEncoder>
				// (or its reverse) is a DUPLICATE user-defined conversion (CS0557, encoding/asn1).
				// packageTypeSpecRHS marks a defined type's written RHS (the wrapper relationship).
				wrapperConversion := false

				if named, ok := funcType.(*types.Named); ok {
					if rhs, has := packageTypeSpecRHS[named.Obj()]; has && rhs != nil && types.Identical(rhs, argType) {
						wrapperConversion = true
					}
				}

				if named, ok := argType.(*types.Named); ok {
					if rhs, has := packageTypeSpecRHS[named.Obj()]; has && rhs != nil && types.Identical(rhs, funcType) {
						wrapperConversion = true
					}
				}

				// A pairing carrying UNBOUND type parameters cannot be recorded: an assembly
				// attribute cannot name K/V (internal/concurrent's indirect[K,V] GoImplicitConv
				// record emitted GoImplicitConv<Δindirect<K, V>, ж<Δindirect<K, V>>>, CS0246 x4
				// - and one bad record kills the whole generator run).
				if targetTypeName != argTypeName && !wrapperConversion && !typeContainsTypeParams(argType) && !typeContainsTypeParams(funcType) {
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
			// A conversion between a defined type and its WRITTEN base named type — `Key(handle)`
			// or `Handle(key)` where `type Key syscall.Handle` — is already provided by the
			// [GoType] wrapper itself (TypeGenerator emits the implicit operators both ways);
			// recording it again makes ImplicitConvGenerator emit the identical operator into the
			// same type (CS0557 — internal/syscall/windows/registry's Key, gating os/fmt).
			if named, ok := funcType.(*types.Named); ok {
				if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil && types.Identical(rhs, argType) {
					return expr
				}
			}

			if named, ok := argType.(*types.Named); ok {
				if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil && types.Identical(rhs, funcType) {
					return expr
				}
			}

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
					// The recorded conversion type names use cross-package import aliases (e.g.
					// `driver.IsolationLevel`); register them so package_info.cs emits a resolving
					// `global using` — the STRUCT-conversion branch above already does this, but the
					// aliased-NUMERIC branch omitted it, so a cross-package named-numeric conversion
					// (database/sql's `driver.IsolationLevel(opts.Isolation)`) left `driver` unresolved
					// in both package_info.cs and the ImplicitConvGenerator .g.cs (CS0246).
					v.recordConversionPackageUsing(argType)
					v.recordConversionPackageUsing(funcType)

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
			// A TIGHTENED local const is declared at its concrete type — element/parameter
			// inference sees that type directly, so the cast machinery does not apply
			// (see performUntypedConstAnalysis).
			if _, tightened := v.tightenedConsts[constObj]; tightened {
				return false
			}

			if basic, ok := constObj.Type().(*types.Basic); ok {
				return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
			}
		}
	case *ast.SelectorExpr:
		// A CROSS-PACKAGE untyped numeric const reached through a qualified selector
		// (`tabwriter.Escape`, whose `const Escape = '\xff'` renders as a golib `UntypedInt`)
		// is an untyped constant just like a bare ident, but arrives here as a SelectorExpr —
		// the const object hangs off the Sel ident. `append([]byte, tabwriter.Escape)` otherwise
		// leaves the two append overloads ambiguous (the ISlice overload infers T from the
		// UntypedInt element while slice<T> infers byte — CS0121 ×6, go/printer). Cast it to the
		// element type exactly as the bare-ident case does.
		if constObj, ok := v.info.Uses[a.Sel].(*types.Const); ok {
			if basic, ok := constObj.Type().(*types.Basic); ok {
				return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
			}
		}
	case *ast.UnaryExpr:
		// A numeric unary operator over an untyped numeric constant (`-1`, `+2`, `^0`) is itself an
		// untyped numeric constant but not a bare BasicLit/Ident (regexp `append(a, -1)` left the two
		// append overloads ambiguous, CS0121). Recurse on the operand.
		switch a.Op {
		case token.SUB, token.ADD, token.XOR:
			return v.isUntypedNumericConstArg(a.X)
		}
	}

	return false
}

// untypedIntGenericArgCastType returns the C# type an untyped INTEGER constant argument must be
// cast to when it feeds a bare type-parameter slot of a generic call — or "" when no cast is
// needed. C# infers the type argument from the literal's OWN type, and a bare int literal is
// `int` (System.Int32); go/types has already resolved the literal to the type parameter's
// concrete instantiation, so the cast retypes it to bind the intended argument (`nint` for Go
// `int`, `byte`, `long` for `int64`, `nuint` for `uint`/`uintptr`, …). Returns "" for a
// non-constant, a non-integer resolved type, or a resolved `int32`/`rune` kind — a bare int
// literal already IS System.Int32, so those need no retype (an int literal in an int32 context
// gains no noise). Reuses isUntypedNumericConstArg's constant test — the same one the append and
// narrow-int element casts key off — so a tightened local const (already declared at its concrete
// type) is correctly excluded.
func (v *Visitor) untypedIntGenericArgCastType(arg ast.Expr) string {
	if !v.isUntypedNumericConstArg(arg) {
		return ""
	}

	tv, ok := v.info.Types[arg]

	if !ok || tv.Value == nil {
		return ""
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 || basic.Info()&types.IsUntyped != 0 {
		return ""
	}

	// int32/rune already renders as — and C# infers as — System.Int32 from a bare literal.
	if basic.Kind() == types.Int32 {
		return ""
	}

	return convertToCSTypeName(v.getTypeName(basic, false))
}

// typeParamIsSliceElementOfSibling reports whether type parameter tp is the ELEMENT type of some
// OTHER type parameter's `~[]E` slice-core constraint in the same signature — the `S ~[]E, E …`
// shape (slices.Index/Insert/Replace). Go's core-type inference fixes E from S's concrete element,
// but C#'s `where S : ISlice<E>` constraint carries no such flow — C# infers E purely from the
// value argument — so an untyped-int literal in the E slot mis-infers E and violates the S
// constraint unless it is retyped to E's instantiation (see the call-site cast). When tp is NOT
// thus locked — E free (`[T any](v ...T)`), or determined directly by another parameter's type
// (`[P *T, T any](p P, v T)`, where C# infers T from the pointer arg) — C# already infers it
// correctly, so no retype is wanted. Mirrors typeParamSliceCore's constraint walk.
func typeParamIsSliceElementOfSibling(sig *types.Signature, tp *types.TypeParam) bool {
	tparams := sig.TypeParams()

	if tparams == nil {
		return false
	}

	for k := range tparams.Len() {
		sibling := tparams.At(k)

		if sibling == tp {
			continue
		}

		if core := typeParamSliceCore(sibling); core != nil {
			if elem, ok := types.Unalias(core.Elem()).(*types.TypeParam); ok && elem == tp {
				return true
			}
		}
	}

	return false
}

// argBoxesAsInt32ButNeedsNint reports whether arg is a CONSTANT expression whose Go type is the
// untyped-constant default `int` with a value in the int32 range. convBasicLit renders such a value as
// a bare C# integer literal, which is `System.Int32`; but when the value is implicitly converted to an
// interface, Go boxes it as its dynamic type `int` — go2cs `nint` (an IntPtr). Without an explicit
// `(nint)` cast the C# box is `System.Int32`, so a later `.(int)` type assertion — emitted as
// `._<nint>()` — finds a boxed Int32 and panics (both print "int", but one is Int32, one is nint).
// Larger int constants already render as `(nint)…L` (correctly boxed); untyped float/rune/string
// defaults box to the matching C# type (double / int32 / — @string via golib's assertion
// normalization); so only the int32-range default-`int` constant needs the cast. Keying off
// info.Types[arg] (rather than the AST shape) uniformly catches a literal (`42`), a unary
// (`-5`), a binary (`1 + 2`), and a named untyped-int const, since go/types constant-folds them to a
// single `int` value.
func (v *Visitor) argBoxesAsInt32ButNeedsNint(arg ast.Expr) bool {
	// A type-conversion CallExpr (`int(x)`) is itself a constant of type int but already renders with
	// its own `(nint)…` cast — skip it so the box is not double-wrapped.
	if _, isCall := arg.(*ast.CallExpr); isCall {
		return false
	}

	tv, ok := v.info.Types[arg]

	if !ok || tv.Value == nil {
		return false
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok || basic.Kind() != types.Int {
		return false
	}

	iv, exact := constant.Int64Val(tv.Value)

	return exact && iv >= math.MinInt32 && iv <= math.MaxInt32
}

// boxUntypedIntAsNint wraps an already-rendered value expression in a `(nint)` cast when `target` is
// the empty interface and `value` is an untyped `int` constant that would otherwise box as
// System.Int32 (see argBoxesAsInt32ButNeedsNint). It is the non-call-argument twin of the
// castArgToType["nint"] treatment convCallExpr applies at interface call sites — assignment, var-spec,
// return, channel send, and keyed composite/struct/map positions render a value against a known
// empty-interface slot and route through here so a later `.(int)` / `case int:` observes Go's boxed
// `int` dynamic type. A non-empty-interface slot, a type-parameter slot, or a non-int-constant value
// passes through unchanged. Mirrors the string→@string family's per-position boxing (castToGoString),
// which the empty interface likewise handles outside convertToInterfaceType.
func (v *Visitor) boxUntypedIntAsNint(target types.Type, value ast.Expr, rendered string) string {
	if isEmptyInterfaceTarget(target) && v.argBoxesAsInt32ButNeedsNint(value) {
		return fmt.Sprintf("(nint)(%s)", rendered)
	}

	return rendered
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
				// Register under the same qualifier the recorded conversion STRINGS carry:
				// a Δ-renamed import (`Δsyscall`, internal/poll) records `Δsyscall.Sockaddr…`
				// attribute type names, so the resolving using must declare that exact alias
				// (`using Δsyscall = go.syscall_package;`) — the plain-name using left the
				// attributes unresolvable (CS0246 ×4). Unrenamed imports are unchanged
				// (importQualifier is the identity for them).
				packageLock.Lock()
				conversionPackageUsings[importQualifier(pkg.Name())] = convertImportPathToNamespace(pkg.Path(), PackageSuffix)
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
			// A NAMED numeric type (`type Hash uint`, crypto's maxHash in `make([]T, maxHash)`) renders
			// as a [GoType] wrapper struct with implicit conversions only to/from its UNDERLYING, so a
			// direct `(nint)(Hash)` has no conversion (CS0030). Route the cast through the underlying
			// numeric so the wrapper's implicit operator applies first: `(nint)(nuint)(maxHash)`.
			if argType := v.info.TypeOf(arg); argType != nil {
				if _, isNamed := types.Unalias(argType).(*types.Named); isNamed {
					underlyingCS := convertToCSTypeName(v.getTypeName(argType.Underlying(), false))
					argStr = fmt.Sprintf("(nint)(%s)(%s)", underlyingCS, argStr)
				} else {
					argStr = "(nint)(" + argStr + ")"
				}
			} else {
				argStr = "(nint)(" + argStr + ")"
			}
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

// pointerReinterpretIdentitySource reports the underlying pointer expression when a `(*T)(…)`
// conversion is a semantic IDENTITY — its source, after peeling an optional escape-analysis
// identity wrapper, is a pointer that ALREADY has the same pointer type `*T`, reached either
// DIRECTLY (`(*T)(p)`) or through an `unsafe.Pointer(p)` round trip.
//
// Go's strings.Builder/bytes.Buffer copyCheck writes `b.addr = (*Builder)(abi.NoEscape(unsafe.
// Pointer(b)))`, which the language spec makes exactly `b.addr = b` (see the type's own TODO to
// revert it once escape analysis improves). Emitting the uintptr round-trip instead
// DEREFERENCES-and-COPIES through golib's `(ж<T>)(uintptr) => new ж<T>(*(T*)value)`, producing a
// box that is NOT reference-equal to the source — so the type's copy-by-value self-check
// (`b.addr != b`) false-fires at runtime. Returning p lets the caller emit the box directly and
// preserve managed-pointer identity.
//
// The DIRECT form — `*(*T)(p)` with p already `*T`, the shape reflect/runtime use to re-read a
// pointer at a fixed type — is the same no-op, but its source is a MANAGED BOX (`ж<T>`), never an
// address. Routing it through the raw-address uintptr bridge emitted `(ж<T>)(uintptr)(p)`, and a
// deref-aliased pointer parameter renders as its VALUE alias, so the leg had no conversion at all
// (CS0030 — `Cannot convert type 'Pt' to 'uintptr'`). Returning p emits the box, which the caller's
// deref then reads in place: correct for a value read, an lvalue write, and pointer identity alike.
// Returns nil when the pattern does not apply (a DIFFERENT element type is a genuine reinterpret and
// keeps the round-trip).
func (v *Visitor) pointerReinterpretIdentitySource(callExpr *ast.CallExpr, arg ast.Expr) ast.Expr {
	targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer)
	if !ok {
		return nil
	}

	// callExpr must be the CONVERSION `(*T)(…)` — its Fun DENOTES the pointer type. Without this the
	// direct-source form below matches any one-argument CALL that happens to take and return the same
	// pointer type, and elides it: `advance(a)` → `a`, `Ꮡp.Swap(Ꮡa)` → `Ꮡa`. (The unsafe.Pointer form
	// was implicitly guarded by requiring its ARGUMENT to be a type conversion; the direct form has no
	// such wrapper and must check the call itself.)
	if tv, isType := v.info.Types[callExpr.Fun]; !isType || !tv.IsType() {
		return nil
	}

	// Peel a single escape-analysis identity wrapper (`abi.NoEscape`/local `noescape`).
	inner := arg
	if call, ok := inner.(*ast.CallExpr); ok && len(call.Args) == 1 && v.isNoEscapeIdentityCall(call) {
		inner = call.Args[0]
	}

	// The source pointer, either reached directly or unwrapped from an `unsafe.Pointer(p)`
	// CONVERSION — a call whose Fun DENOTES the unsafe.Pointer type, not merely a function that
	// happens to return unsafe.Pointer (e.g. runtime `mallocgc(…)`), which is a genuine raw-address
	// reinterpret and keeps its path.
	p := inner

	if call, ok := inner.(*ast.CallExpr); ok && len(call.Args) == 1 {
		if tv, isType := v.info.Types[call.Fun]; isType && tv.IsType() {
			if basic, ok := v.info.TypeOf(call).Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
				p = call.Args[0]
			}
		}
	}

	// p must already be `*T` with the SAME element type as the target — only then is the whole
	// conversion a no-op identity.
	srcPtr, ok := types.Unalias(v.info.TypeOf(p)).(*types.Pointer)
	if !ok {
		return nil
	}

	if !types.Identical(srcPtr.Elem(), targetPtr.Elem()) {
		return nil
	}

	return p
}

// isNoEscapeIdentityCall reports whether call is to an escape-analysis identity helper —
// `abi.NoEscape(…)` (internal/abi) or a package-local `noescape(…)` — each an
// unsafe.Pointer→unsafe.Pointer function that returns its argument unchanged. Matched by name
// AND signature shape so an unrelated one-arg call cannot be mistaken for it.
func (v *Visitor) isNoEscapeIdentityCall(call *ast.CallExpr) bool {
	var name string

	switch fun := call.Fun.(type) {
	case *ast.SelectorExpr:
		name = fun.Sel.Name
	case *ast.Ident:
		name = fun.Name
	default:
		return false
	}

	if name != "NoEscape" && name != "noescape" {
		return false
	}

	sig, ok := v.info.TypeOf(call.Fun).(*types.Signature)
	if !ok || sig.Params().Len() != 1 || sig.Results().Len() != 1 {
		return false
	}

	pIn, ok1 := sig.Params().At(0).Type().Underlying().(*types.Basic)
	pOut, ok2 := sig.Results().At(0).Type().Underlying().(*types.Basic)

	return ok1 && ok2 && pIn.Kind() == types.UnsafePointer && pOut.Kind() == types.UnsafePointer
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

					// A pointer reinterpret from a NAMED-SLICE pointer to its underlying-slice
					// pointer — `(*[][]byte)(buf)` with `buf *Buffers` (net fd_windows.go) —
					// is claimed so the conversion renderer's of-projection arm emits the
					// backing-field VIEW (`Ꮡbuf.of(Buffers.Ꮡm_value)`); unclaimed, the callee
					// rendered as a bare cast between unrelated ж<> instantiations (CS0030).
					if named, ok := types.Unalias(argPtr.Elem()).(*types.Named); ok {
						if underSlice, ok := named.Underlying().(*types.Slice); ok {
							if elemSlice, ok := elemType.Underlying().(*types.Slice); ok && types.Identical(underSlice, elemSlice) {
								if _, elemIsNamed := types.Unalias(elemType).(*types.Named); !elemIsNamed {
									return true, "*" + v.getTypeName(elemType, false)
								}
							}
						}
					}
				}

				// Go 1.17 slice-to-array-POINTER — `(*[32]byte)(x)` with a SLICE argument
				// (edwards25519 fiatScalarFromBytes' input, CS0030): claimed so the
				// slice-to-array conversion arm boxes the golib copy.
				if slc, ok := argType.Underlying().(*types.Slice); ok {
					if arrType, ok := types.Unalias(elemType).(*types.Array); ok && types.Identical(arrType.Elem(), slc.Elem()) {
						return true, "*" + v.getTypeName(elemType, false)
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

			// Go 1.20 slice-to-ARRAY value conversion — `[4]byte(slice)` (netip
			// AddrFromSlice, CS1955): claimed so the slice-to-array conversion arm emits
			// the golib copy ctor.
			if arrType, ok := types.Unalias(targetType).(*types.Array); ok {
				if slc, ok := argType.Underlying().(*types.Slice); ok && types.Identical(arrType.Elem(), slc.Elem()) {
					return true, v.getTypeName(targetType, false)
				}
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
	originalArgType := argType

	// Check if the argument is a pointer
	if pointer, ok := argType.(*types.Pointer); ok {
		argType = pointer.Elem()
	}

	typeName := v.getTypeName(targetType, false)

	if isPointer {
		typeName = "*" + typeName
	}

	// Check if the argument type is convertible to the target type. For an INTERFACE target
	// the ORIGINAL (pointer) argument type is probed too: a pointer-to-interface conversion —
	// `image.Image(dst)` with `dst *image.RGBA` (image/draw) — converts through the POINTER's
	// method set (the value type alone does not implement the interface), so the elem-only
	// probe misread the conversion as a constructor call (`new image.Image(dst)`, CS0144 ×2).
	// Gated to interface targets: widening every target reroutes `unsafe.Pointer(ptr)`
	// constructor forms into the conversion arm (13-project churn).
	convertible := types.ConvertibleTo(argType, targetType)

	if !convertible && originalArgType != nil {
		if targetIsIface, _ := isInterface(targetType); targetIsIface {
			convertible = types.ConvertibleTo(originalArgType, targetType)
		}
	}

	return convertible, typeName
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

// typeContainsTypeParams reports whether t contains any unbound type parameter, walking
// pointers, containers, and generic instantiations' type arguments.
func typeContainsTypeParams(t types.Type) bool {
	switch typ := t.(type) {
	case *types.TypeParam:
		return true
	case *types.Alias:
		return typeContainsTypeParams(types.Unalias(typ))
	case *types.Pointer:
		return typeContainsTypeParams(typ.Elem())
	case *types.Slice:
		return typeContainsTypeParams(typ.Elem())
	case *types.Array:
		return typeContainsTypeParams(typ.Elem())
	case *types.Chan:
		return typeContainsTypeParams(typ.Elem())
	case *types.Map:
		return typeContainsTypeParams(typ.Key()) || typeContainsTypeParams(typ.Elem())
	case *types.Named:
		if args := typ.TypeArgs(); args != nil {
			for i := range args.Len() {
				if typeContainsTypeParams(args.At(i)) {
					return true
				}
			}
		}
	}

	return false
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

			// A FUNC-typed FIELD callee — `d.fill(d, b)` on flate compressor's
			// method-expression fields: its signature drives the same per-argument
			// treatment (the receiver arg must render as the box for a ж<T> slot,
			// CS1503 ×5). Underlying looks through a NAMED func type.
			if vr, ok := sel.Obj().(*types.Var); ok {
				if sig, ok := vr.Type().Underlying().(*types.Signature); ok {
					return sig
				}
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
		// Functions returned by other functions. Underlying() looks through a NAMED func-type
		// result — encoding/json's `valueEncoder(v)` returns `encoderFunc` (a methodless named
		// func type), so a bare `t.(*types.Signature)` failed and the per-argument pointer
		// treatment never fired: `valueEncoder(v)(e, …)` passed the receiver value alias `e`
		// where the `ж<encodeState>` slot wanted the box `Ꮡe` (CS1503).
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.Underlying().(*types.Signature); ok {
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
// callHasMethodGroupArg reports whether any argument of the call is a bare FUNCTION reference
// (a method group in C#) rather than a func-typed value or a lambda. C# cannot infer a
// generic method's type arguments through a method-group argument (the group has no single
// type until the target delegate is known), so a generic call carrying one must spell its
// type arguments out — `slices.SortFunc(l, bytes.Compare)` was CS0411 (encoding/asn1). A
// func-typed VARIABLE (a *types.Var) is a real delegate value and infers fine, so it is
// excluded; only a *types.Func (package function or method) counts.
func (v *Visitor) callHasMethodGroupArg(callExpr *ast.CallExpr) bool {
	for _, arg := range callExpr.Args {
		switch a := arg.(type) {
		case *ast.Ident:
			if _, ok := v.info.Uses[a].(*types.Func); ok {
				return true
			}
		case *ast.SelectorExpr:
			if _, ok := v.info.Uses[a.Sel].(*types.Func); ok {
				return true
			}
		}
	}

	return false
}

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

	// The Fun expression's recorded type can be the UNINSTANTIATED signature — its param
	// still the bare type parameter — with the instantiation living in info.Instances,
	// keyed by the Fun's identifier (`ptr = abi.Escape(ptr)` instantiating T = *T left the
	// arg as the deref'd value alias, internal/weak Make CS0029).
	funIdent, _ := callExpr.Fun.(*ast.Ident)

	if sel, ok := callExpr.Fun.(*ast.SelectorExpr); ok {
		funIdent = sel.Sel
	} else if idx, ok := callExpr.Fun.(*ast.IndexExpr); ok {
		// Explicitly instantiated form: escape[*int](p) / pkg.Escape[*int](p)
		if id, ok := idx.X.(*ast.Ident); ok {
			funIdent = id
		} else if sel, ok := idx.X.(*ast.SelectorExpr); ok {
			funIdent = sel.Sel
		}
	}

	if funIdent != nil {
		if inst, ok := v.info.Instances[funIdent]; ok {
			if sig, ok := inst.Type.(*types.Signature); ok {
				if paramType, ok := getParameterType(sig, i); ok {
					return isPointer(paramType)
				}
			}
		}
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

// instantiatedParamType returns the INSTANTIATED type of parameter i at this call site, or nil
// when unavailable. getFunctionSignature returns the DECLARED generic signature (its params are
// still bare type parameters); the instantiation lives in info.Instances keyed by the Fun's
// identifier — the same resolution instantiatedParamIsPointer performs (see its notes), factored
// for callers that need the full type rather than a pointer check.
func (v *Visitor) instantiatedParamType(callExpr *ast.CallExpr, i int) types.Type {
	funIdent, _ := callExpr.Fun.(*ast.Ident)

	if sel, ok := callExpr.Fun.(*ast.SelectorExpr); ok {
		funIdent = sel.Sel
	} else if idx, ok := callExpr.Fun.(*ast.IndexExpr); ok {
		// Explicitly instantiated form: walkList[*Ident](v, list) / pkg.F[*T](x)
		if id, ok := idx.X.(*ast.Ident); ok {
			funIdent = id
		} else if sel, ok := idx.X.(*ast.SelectorExpr); ok {
			funIdent = sel.Sel
		}
	}

	if funIdent != nil {
		if inst, ok := v.info.Instances[funIdent]; ok {
			if sig, ok := inst.Type.(*types.Signature); ok {
				if paramType, ok := getParameterType(sig, i); ok {
					return paramType
				}
			}
		}
	}

	if instSig, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		if paramType, ok := getParameterType(instSig, i); ok {
			return paramType
		}
	}

	return nil
}

// managedAtomicPointerIdiom recognizes Go's lock-free managed-pointer-field atomics —
//
//	atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))
//	atomic.StorePointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)), unsafe.Pointer(v))
//
// where `x.field` has a pointer type `*T` and so, in the managed conversion, holds a `ж<T>`
// reference. A managed reference cannot survive the `uintptr` round-trip the literal conversion
// emits (the pinned address is transient and reinterpreting it loses GC identity), so the
// converter routes these to the golib overloads that operate on the field BOX (`ж<ж<T>>`)
// directly. Returns the `&x.field` address expression (which renders as that box), and, for a
// store, the stored value expression `v` (unwrapped from its `unsafe.Pointer(...)` conversion so
// it renders as the plain `ж<T>` the overload takes).
func (v *Visitor) managedAtomicPointerIdiom(callExpr *ast.CallExpr) (addrExpr ast.Expr, storeVal ast.Expr, isLoad bool, ok bool) {
	sel, isSel := callExpr.Fun.(*ast.SelectorExpr)

	if !isSel {
		return nil, nil, false, false
	}

	fn, isFn := v.info.ObjectOf(sel.Sel).(*types.Func)

	if !isFn || fn.Pkg() == nil || fn.Pkg().Path() != "sync/atomic" {
		return nil, nil, false, false
	}

	switch fn.Name() {
	case "LoadPointer":
		isLoad = true
	case "StorePointer":
		isLoad = false
	default:
		return nil, nil, false, false
	}

	// arg[0] must be `(*unsafe.Pointer)(unsafe.Pointer(&x.field))` whose `x.field` is `*T`.
	if len(callExpr.Args) < 1 {
		return nil, nil, false, false
	}

	addrExpr = v.unwrapManagedPtrFieldAddress(callExpr.Args[0])

	if addrExpr == nil {
		return nil, nil, false, false
	}

	if isLoad {
		return addrExpr, nil, true, len(callExpr.Args) == 1
	}

	// StorePointer's value arg is `unsafe.Pointer(v)`; the overload takes the plain `ж<T>` value.
	if len(callExpr.Args) != 2 {
		return nil, nil, false, false
	}

	storeVal = v.unwrapUnsafePointerConversion(callExpr.Args[1])

	if storeVal == nil {
		return nil, nil, false, false
	}

	return addrExpr, storeVal, false, true
}

// unwrapManagedPtrFieldAddress returns the `&Z` expression inside
// `(*unsafe.Pointer)(unsafe.Pointer(&Z))` when `Z` has a pointer type `*T` (a managed `ж<T>`
// field); nil otherwise.
func (v *Visitor) unwrapManagedPtrFieldAddress(arg ast.Expr) ast.Expr {
	// (*unsafe.Pointer)(inner)
	outer, isCall := ast.Unparen(arg).(*ast.CallExpr)

	if !isCall || len(outer.Args) != 1 || !v.isPointerToUnsafePointerType(outer.Fun) {
		return nil
	}

	// unsafe.Pointer(&Z)
	inner := v.unwrapUnsafePointerConversion(outer.Args[0])

	if inner == nil {
		return nil
	}

	// &Z
	unary, isUnary := ast.Unparen(inner).(*ast.UnaryExpr)

	if !isUnary || unary.Op != token.AND {
		return nil
	}

	// Z must have a pointer type — its address `&Z` is then `**T`, i.e. the box of a `ж<T>` field.
	operandType := v.info.TypeOf(unary.X)

	if operandType == nil {
		return nil
	}

	if _, isPointer := operandType.Underlying().(*types.Pointer); !isPointer {
		return nil
	}

	return unary
}

// unwrapUnsafePointerConversion returns the argument of an `unsafe.Pointer(x)` conversion, or nil
// when expr is not such a conversion.
func (v *Visitor) unwrapUnsafePointerConversion(expr ast.Expr) ast.Expr {
	call, isCall := ast.Unparen(expr).(*ast.CallExpr)

	if !isCall || len(call.Args) != 1 || !v.isUnsafePointerType(call.Fun) {
		return nil
	}

	return call.Args[0]
}

// isUnsafePointerType reports whether expr denotes the `unsafe.Pointer` type (used as a conversion
// target).
func (v *Visitor) isUnsafePointerType(expr ast.Expr) bool {
	t := v.info.TypeOf(expr)

	if t == nil {
		return false
	}

	basic, isBasic := t.Underlying().(*types.Basic)

	return isBasic && basic.Kind() == types.UnsafePointer
}

// isPointerToUnsafePointerType reports whether expr denotes `*unsafe.Pointer` (the conversion
// target of the idiom's outer cast).
func (v *Visitor) isPointerToUnsafePointerType(expr ast.Expr) bool {
	star, isStar := ast.Unparen(expr).(*ast.StarExpr)

	return isStar && v.isUnsafePointerType(star.X)
}
