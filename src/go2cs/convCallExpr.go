package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
)

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
				baseNamed, okBase := resultPtr.Elem().(*types.Named)
				defNamed, okDef := argPtr.Elem().(*types.Named)

				if okBase && okDef && baseNamed != defNamed && types.Identical(baseNamed.Underlying(), defNamed.Underlying()) {
					baseName := convertToCSTypeName(v.getTypeName(baseNamed, false))
					return fmt.Sprintf("%s((%s)(%s))", AddressPrefix, baseName, v.convExpr(arg, nil))
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
		if context.isPointerCast {
			return fmt.Sprintf("(%s)(uintptr)(%s)", targetTypeName, expr)
		}

		// unsafe.Pointer(ptr) where ptr is a Go pointer (`*T`, emitted as the managed box `ж<T>`).
		// A managed box has no conversion to the numeric Pointer (`ж<uintptr>`), so a plain cast is
		// CS0030 — e.g. `unsafe.Pointer(&u.value)` → `(@unsafe.Pointer)(Ꮡu.of(…))`. Pin the
		// pointed-to storage instead via the golib helper: `@unsafe.Pointer.FromRef(ref (box).val)`.
		// (`uintptr`/`unsafe.Pointer` args are not boxes and keep the implicit-cast path below; only
		// a genuine pointer arg needs this.) The numeric address is not GC-stable — the same caveat
		// that applies to every unsafe.Pointer-as-uintptr use; the atomic intrinsics consuming it are
		// asm stubs, so this is purely about producing compilable C#.
		if targetTypeName == "@unsafe.Pointer" {
			if _, isPtr := v.info.TypeOf(arg).(*types.Pointer); isPtr {
				return fmt.Sprintf("@unsafe.Pointer.FromRef(ref (%s).val)", expr)
			}
		}

		if targetTypeName == "@string" {
			// Check if it is a generic type parameter - Go will have already
			// validated constraint, so we can just cast to string directly
			if _, ok := v.getType(arg, false).(*types.TypeParam); ok {
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

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
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

			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast {
				callExprContext.u8StringArgOK[i] = false

				if !isEmpty {
					callExprContext.interfaceTypes[i] = paramType
				}
			} else if paramHasArg && isPointer(paramType) && !(callExprContext.hasSpreadOperator && i == params.Len()-1) {
				// paramHasArg guards Args[i]: a variadic pointer parameter called with no
				// trailing arguments (e.g. `In(r)` for `In(r rune, ...*RangeTable)`) has no
				// arg at the variadic index, so indexing Args[i] would panic.
				//
				// getParameterType returns the variadic *element* type, so isPointer is true
				// for `...*T`. When the call spreads a slice into the variadic (`f(s...)`), the
				// argument is the whole slice, not a single element pointer, so the element
				// address-of treatment (which would emit `Ꮡs`) must be skipped.
				ident := getIdentifier(callExpr.Args[i])

				if !v.isPointer(ident) || v.identIsParameter(ident) {
					callExprContext.argTypeIsPtr[i] = true
				}
			}
		}
	}

	callExprContext.replacementArgs = replacementArgs

	if ident, ok := callExpr.Fun.(*ast.Ident); ok {
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
		if named, ok := resultType.(*types.Named); ok {
			if named.TypeArgs().Len() > 0 {
				var typeParams []string

				for i := range named.TypeArgs().Len() {
					typeParams = append(typeParams, v.getCSTypeName(named.TypeArgs().At(i)))
				}

				typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
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
	// faithfully yields `(fp.val)(args)`, which C# parses as a CAST when the argument list is empty
	// (`(fp.val)()` reads as "cast `()` to type `fp.val`" → CS1525). Emit the deref WITHOUT the
	// wrapping parens (`fp.val(args)`) so it is unambiguously an invocation. Restricted to a starred
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
		funcName += typeParamExpr
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
					expr = fmt.Sprintf("(%s?.val ?? default!)", expr)
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
					expr = fmt.Sprintf("(%s?.val ?? default!)", expr)
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
		case *ast.StarExpr:
			if ident, ok := funExpr.X.(*ast.Ident); ok {
				obj = v.info.ObjectOf(ident)
				isPointer = true
			} else if sel, ok := funExpr.X.(*ast.SelectorExpr); ok {
				// A pointer conversion to a cross-package type: `(*atomic.Uint32)(p)`.
				obj = v.info.ObjectOf(sel.Sel)
				isPointer = true
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
