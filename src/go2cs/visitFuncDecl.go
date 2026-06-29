package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

const FunctionPrefixMarker = ">>MARKER:FUNC_%s_PREFIX<<"
const FunctionAccessMarker = ">>MARKER:FUNC_%s_ACCESS<<"
const FunctionUnsafeMarker = ">>MARKER:FUNC_%s_UNSAFE<<"
const FunctionPartialMarker = ">>MARKER:FUNC_%s_PARTIAL<<"
const FunctionAttributeMarker = ">>MARKER:FUNC_%s_RECEIVER<<"
const FunctionParametersMarker = ">>MARKER:FUNC_%s_PARAMETERS<<"
const FunctionExecContextMarker = ">>MARKER:FUNC_%s_EXEC_CONTEXT<<"
const FunctionBlockPrefixMarker = ">>MARKER:FUNC_%s_BLOCK_PREFIX<<"

// hasDuplicateBlankParams reports whether a parameter list has two or more blank (`_`) or unnamed
// parameters. Go permits repeated blank params, but C# forbids duplicate parameter names (CS0100), so
// such a list needs synthetic placeholder names. A LONE blank/unnamed param stays `_` (valid C# and
// visually closer to the Go source).
func hasDuplicateBlankParams(parameters *types.Tuple) bool {
	if parameters == nil {
		return false
	}

	count := 0

	for i := 0; i < parameters.Len(); i++ {
		if name := parameters.At(i).Name(); name == "" || name == "_" {
			count++

			if count >= 2 {
				return true
			}
		}
	}

	return false
}

// variadicElementType returns the C# element type name for a variadic parameter and whether it must
// be emitted inline as `Span<T>` rather than via a `using ꓸꓸꓸT = Span<T>` alias. Both the alias and
// the inline form sit at namespace scope, so a SAME-PACKAGE named element type must be qualified with
// the package class — a bare nested name like `statDep` does not resolve there (CS0246). Qualifying it
// introduces a '.', which (like a type parameter, or a generic/pointer/cross-package element whose
// name already carries '<'/'.') also forces the inline form, since a using-alias identifier cannot
// contain '<', '>' or '.'.
func (v *Visitor) variadicElementType(elem types.Type) (typeName string, inline bool) {
	typeName = v.getCSTypeName(elem)

	if _, isTypeParam := elem.(*types.TypeParam); isTypeParam {
		return typeName, true
	}

	if named, ok := elem.(*types.Named); ok {
		if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg && !strings.Contains(typeName, ".") {
			typeName = fmt.Sprintf("%s%s.%s", packageName, PackageSuffix, typeName)
		}
	}

	return typeName, strings.ContainsAny(typeName, "<.")
}

func (v *Visitor) visitFuncDecl(funcDecl *ast.FuncDecl) {
	v.inFunction = true
	v.capturedVarCount = nil
	v.tempVarCount = nil
	v.useUnsafeFunc = false

	goFunctionName := funcDecl.Name.Name
	csFunctionName := getSanitizedFunctionName(goFunctionName)

	// A Go function named `_` (blank) is a compile-time-only construct — it is never callable, and
	// a package may declare several. Emitting it literally as a method `_` makes a `_ = expr`
	// discard in its body bind to the method group (CS1656). Give it a unique generated name so
	// `_` remains a discard inside (and multiple blank funcs don't collide).
	if goFunctionName == "_" && funcDecl.Recv == nil {
		csFunctionName = getGlobalTempVarName("_")
	}

	v.currentFuncDecl = funcDecl
	v.currentFuncName = csFunctionName
	v.currentFuncPrefix = &strings.Builder{}

	v.varNames = make(map[*types.Var]string)

	currentFuncType := v.info.ObjectOf(funcDecl.Name).(*types.Func)

	if currentFuncType == nil {
		panic("@visitFuncDecl - Failed to find function \"" + goFunctionName + "\" in the type info")
	}

	signature := currentFuncType.Signature()
	v.currentFuncSignature = signature

	// A generic capture-mode method (e.g. atomic.Pointer[T]) is emitted with its heap box
	// AS the receiver (`this ж<T> Ꮡx`) so the receiver's type parameter stays in scope for
	// the field-ref form `Ꮡx.of(Type.ᏑField)`. See packageDirectBoxReceiverMethods.
	directBoxReceiver := packageDirectBoxReceiverMethods != nil && packageDirectBoxReceiverMethods[currentFuncType]

	// Analyze function variables for reassignments and redeclarations (variable shadows).
	v.performVariableAnalysis(funcDecl, signature)

	// Scope defer/recover to THIS function's own body: a `defer`/`recover` inside a nested
	// function literal (an IIFE or closure) belongs to that literal, not to this function, so it
	// must not force a func() execution context here. (performVariableAnalysis sets hasDefer/
	// hasRecover by walking everything, including nested literals.)
	v.hasDefer, v.hasRecover = v.funcBodyDeferRecover(funcDecl.Body)

	// A function with named return values that also uses defer/recover needs the named
	// returns declared outside the func() wrapper and returned after it (see the field doc on
	// namedReturnDeferMode). Determine that here, before the body is emitted, so visitReturnStmt
	// can route returns through the named result params.
	v.namedReturnDeferMode = false
	v.namedReturnNames = nil

	if funcDecl.Body != nil {
		v.namedReturnDeferMode, v.namedReturnNames = v.detectNamedReturnDefer(signature, v.hasDefer, v.hasRecover)
	}

	// Collect parameter names from the function declaration
	if v.paramNames == nil {
		v.paramNames = HashSet[string]{}
	} else {
		v.paramNames.Clear()
	}

	// Collect the parameter OBJECTS too, so identIsParameter can distinguish a real parameter from
	// a local that merely SHADOWS a parameter's name (`func f(t *T){ { var t *T; … } }`).
	v.paramObjects = map[types.Object]bool{}

	for _, param := range funcDecl.Type.Params.List {
		for _, name := range param.Names {
			v.paramNames.Add(name.Name)

			if obj := v.info.Defs[name]; obj != nil {
				v.paramObjects[obj] = true
			}
		}
	}

	// Loop through function results to check if any are structs
	if funcDecl.Type.Results != nil {
		for index, field := range funcDecl.Type.Results.List {
			var fieldName string

			if field.Names == nil {
				fieldName = fmt.Sprintf("R%d", index)
			} else {
				fieldName = field.Names[0].Name
			}

			// Check if the return type is a struct or pointer to a struct
			if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
				v.indentLevel++
				v.visitStructType(structType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}

			// Check if the return type is an anonymous interface
			if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
				v.indentLevel++
				v.visitInterfaceType(interfaceType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}
		}
	}

	// Loop through function parameters to check if any are structs
	if funcDecl.Type.Params != nil {
		for _, field := range funcDecl.Type.Params.List {
			for _, name := range field.Names {
				// Check if the parameter type is a struct or pointer to a struct
				if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
					v.indentLevel++
					v.visitStructType(structType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}
			}
		}
	}

	functionPrefixMarker := fmt.Sprintf(FunctionPrefixMarker, goFunctionName)
	functionAccessMarker := fmt.Sprintf(FunctionAccessMarker, goFunctionName)
	functionUnsafeMarker := fmt.Sprintf(FunctionUnsafeMarker, goFunctionName)
	functionPartialMarker := fmt.Sprintf(FunctionPartialMarker, goFunctionName)
	functionAttributeMarker := fmt.Sprintf(FunctionAttributeMarker, goFunctionName)
	functionParametersMarker := fmt.Sprintf(FunctionParametersMarker, goFunctionName)
	functionExecContextMarker := fmt.Sprintf(FunctionExecContextMarker, goFunctionName)
	functionBlockPrefixMarker := fmt.Sprintf(FunctionBlockPrefixMarker, goFunctionName)

	v.targetFile.WriteString(v.newline)
	v.targetFile.WriteString(functionPrefixMarker)
	v.writeDoc(funcDecl.Doc, funcDecl.Pos())

	functionAccess := getAccess(goFunctionName)
	isModuleInitializer := false

	if funcDecl.Recv == nil {
		// Handle Go "main" function as a special case, in C# this should be capitalized "Main"
		if csFunctionName == "main" {
			csFunctionName = "Main"
		} else if csFunctionName == "init" {
			isModuleInitializer = true

			// C# module initializer functions should have internal scope
			functionAccess = "internal"

			packageLock.Lock()

			if initFuncCounter > 0 {
				csFunctionName = fmt.Sprintf("init%s%d", ShadowVarMarker, initFuncCounter)
			}

			initFuncCounter++

			packageLock.Unlock()
		}
	}

	blockContext := DefaultBlockStmtContext()
	blockContext.innerPrefix = functionBlockPrefixMarker
	typeParams, constraints := v.getGenericDefinition(currentFuncType.Type())

	v.writeOutput("%s%s static%s%s %s %s%s(%s)%s%s", functionAttributeMarker, functionAccessMarker, functionUnsafeMarker, functionPartialMarker, v.generateResultSignature(signature), csFunctionName, typeParams, functionParametersMarker, constraints, functionExecContextMarker)

	if funcDecl.Body != nil {
		blockContext.format.useNewLine = len(constraints) > 0

		// In namedReturnDeferMode the func() wrapper is nested inside an extra block body, so
		// the lambda body sits one level deeper. Bump the indent across the body visit so the
		// statements (and the closing `}` that the `);` attaches to) align under `func(…`.
		if v.namedReturnDeferMode {
			v.indentLevel++
		}

		v.visitBlockStmt(funcDecl.Body, blockContext)

		if v.namedReturnDeferMode {
			v.indentLevel--
		}
	}

	signatureOnly := funcDecl.Body == nil
	useFuncExecutionContext := v.hasDefer || v.hasRecover
	parameterSignature, receiverAccess := v.generateParametersSignature(signature, true)
	blockPrefix := ""
	// In namedReturnDeferMode this holds the named-return declarations, emitted outside the
	// func() wrapper (see the exec-context assembly below).
	namedReturnDeclsStr := ""

	// If receiver access is not public, update function access to match
	if len(receiverAccess) > 0 && receiverAccess != "public" {
		functionAccess = receiverAccess
	}

	if !signatureOnly {
		resultParameters := &strings.Builder{}
		arrayClones := &strings.Builder{}
		implicitPointers := &strings.Builder{}

		// In namedReturnDeferMode the named-return declarations are emitted OUTSIDE the func()
		// wrapper (so defers/recover mutate them by closure); collect them separately. Otherwise
		// they go into the block prefix inside the wrapper as before.
		namedReturnDecls := &strings.Builder{}

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			resultParams := signature.Results()
			paramIndex := 0

			resultDeclTarget := resultParameters

			if v.namedReturnDeferMode {
				resultDeclTarget = namedReturnDecls
			}

			for _, field := range funcDecl.Type.Results.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous parameter (no name)
					paramIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							paramIndex++
							continue
						}

						param := resultParams.At(paramIndex)
						paramName := getSanitizedIdentifier(v.getIdentName(ident))

						resultDeclTarget.WriteString(v.newline)

						v.writeString(resultDeclTarget, "%s%s %s = default!;", v.indent(v.indentLevel+1), v.getCSTypeName(param.Type()), paramName)

						paramIndex++
					}
				}
			}
		}

		parameters := getParameters(signature, true)

		for i := 0; i < parameters.Len(); i++ {
			param := parameters.At(i)

			// For any array parameters, Go copies the array by value
			if _, ok := param.Type().(*types.Array); ok {
				v.writeString(arrayClones, "%s%s%s = %s.Clone();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), getSanitizedIdentifier(param.Name()))
			}

			// All pointers in Go can be implicitly dereferenced, so setup a "local ref" instance to each
			if pointerType, ok := param.Type().(*types.Pointer); ok {
				if i == 0 && funcDecl.Recv != nil && !directBoxReceiver {
					// Skip receiver parameter (direct-ж receivers get the deref below, so
					// the box parameter `Ꮡx` resolves to the value `x` in the body).
					continue
				}

				// An unnamed (`func(*T)`) or blank (`_`) pointer parameter is never referenced in the
				// body, so it gets no deref alias; it is emitted in the signature with a synthetic name
				// and no box (`Ꮡ`) convention. Emitting the deref would produce `ref var  = ref Ꮡ.val;`.
				if param.Name() == "" || param.Name() == "_" {
					continue
				}

				if v.options.preferVarDecl {
					v.writeString(implicitPointers, "%s%sref var %s = ref %s%s.val;", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), AddressPrefix, param.Name())
				} else {
					v.writeString(implicitPointers, "%s%sref %s %s = ref %s%s.val;", v.newline, v.indent(v.indentLevel+1), convertToCSTypeName(pointerType.Elem().String()), getSanitizedIdentifier(param.Name()), AddressPrefix, param.Name())
				}
			}

			// Check if parameter is variadic, in this case parameter is a C# params array that needs to be converted to a Go slice<T>
			if i == parameters.Len()-1 && signature.Variadic() {
				if v.options.preferVarDecl {
					v.writeString(resultParameters, "%s%svar %s = %s.slice();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), getVariadicParamName(param))
				} else {
					v.writeString(resultParameters, "%s%sslice<%s> %s = %s.slice();", v.newline, v.indent(v.indentLevel+1), v.getCSTypeName(param.Type().(*types.Slice).Elem()), getSanitizedIdentifier(param.Name()), getVariadicParamName(param))
				}
			}
		}

		if namedReturnDecls.Len() > 0 {
			namedReturnDeclsStr = namedReturnDecls.String()
		}

		if resultParameters.Len() > 0 {
			resultParameters.WriteString(v.newline)
			blockPrefix += resultParameters.String()
		}

		if arrayClones.Len() > 0 {
			if blockPrefix == "" {
				arrayClones.WriteString(v.newline)
			}

			blockPrefix += arrayClones.String()
		}

		if implicitPointers.Len() > 0 {
			if blockPrefix == "" {
				implicitPointers.WriteString(v.newline)
			}

			blockPrefix += implicitPointers.String()

			updatedSignature := strings.Builder{}
			dupBlankParams := hasDuplicateBlankParams(parameters)

			for i := 0; i < parameters.Len(); i++ {
				param := parameters.At(i)

				if i == 0 && funcDecl.Recv != nil {
					updatedSignature.WriteString("this ")

					// Get receiver parameter type
					recvTypeName := v.getRefParamTypeName(param.Type())

					// Method accessibility is the more restrictive of the receiver type and the method's
					// own (Go) name: an unexported method on an exported type stays package-private
					// (internal) -- otherwise a public method returning that method's own unexported
					// types is CS0050 (inconsistent accessibility).
					if getAccess(recvTypeName) == "public" && getAccess(goFunctionName) == "public" {
						functionAccess = "public"
					} else {
						functionAccess = "internal"
					}

					if directBoxReceiver {
						// Direct-ж: emit the box itself (`ж<Box<T>> Ꮡb`) as the receiver. The
						// deref `ref var b = ref Ꮡb.val;` is emitted above so the body's value
						// references still read as `b`, while `&b.field` uses the box `Ꮡb`.
						updatedSignature.WriteString(v.getCSTypeName(param.Type()))
						updatedSignature.WriteRune(' ')
						updatedSignature.WriteString(AddressPrefix + param.Name())
					} else {
						updatedSignature.WriteString(recvTypeName)
						updatedSignature.WriteRune(' ')
						updatedSignature.WriteString(getSanitizedIdentifier(param.Name()))
					}

					continue
				}

				if i > 0 {
					updatedSignature.WriteString(", ")
				}

				if i == parameters.Len()-1 && signature.Variadic() {
					updatedSignature.WriteString("params ")

					// If parameter is a slice, convert it to a Span
					if sliceType, ok := param.Type().(*types.Slice); ok {
						typeName, inline := v.variadicElementType(sliceType.Elem())

						if inline {
							updatedSignature.WriteString("Span<" + typeName + ">")
						} else {
							updatedSignature.WriteString(EllipsisOperator + typeName)
							v.addRequiredUsing(fmt.Sprintf("%s%s = Span<%s>", EllipsisOperator, typeName, typeName))
						}
					} else {
						updatedSignature.WriteString("object[]")
					}

					// Variadic parameters are passed as C# param arrays, so we use a temporary
					// parameter name that will be later converted to a Go slice<T>
					updatedSignature.WriteRune(' ')
					updatedSignature.WriteString(getVariadicParamName(param))
				} else {
					updatedSignature.WriteString(v.getCSTypeName(param.Type()))
					updatedSignature.WriteRune(' ')

					if _, ok := param.Type().(*types.Pointer); ok {
						// An unnamed or blank (`_`) pointer param is never referenced (no deref alias
						// above), so emit a plain name without the box `Ꮡ` convention — synthesized
						// unique only when blanks would collide (else a lone `_` is kept).
						if param.Name() == "" || param.Name() == "_" {
							if dupBlankParams {
								updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
							} else {
								updatedSignature.WriteString("_")
							}
						} else {
							updatedSignature.WriteString(AddressPrefix)
							updatedSignature.WriteString(param.Name())
						}
					} else if param.Name() == "" || param.Name() == "_" {
						// Unnamed or blank (`_`) non-pointer param — keep a lone `_`, but synthesize a
						// unique placeholder when blanks would collide (Go allows repeated blank params;
						// C# forbids duplicate parameter names — CS0100).
						if dupBlankParams {
							updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
						} else {
							updatedSignature.WriteString("_")
						}
					} else {
						updatedSignature.WriteString(getSanitizedIdentifier(param.Name()))
					}
				}
			}

			parameterSignature = updatedSignature.String()
		}
	}

	// Replace function markers
	v.replaceMarker(functionAccessMarker, functionAccess)

	if v.useUnsafeFunc {
		v.replaceMarker(functionUnsafeMarker, " unsafe")
		usesUnsafeCode = true
	} else {
		v.replaceMarker(functionUnsafeMarker, "")
	}

	// A nil body means the Go function is implemented externally (assembly or cgo):
	// emit a `partial` declaration. Its implementation is supplied either by a
	// hand-written companion (e.g. sync/atomic's doc_impl.cs) or, when none exists, by
	// the PartialStubGenerator (go2cs-gen), which emits a throwing default so the code
	// still compiles.
	if funcDecl.Body == nil {
		v.replaceMarker(functionPartialMarker, " partial")
	} else {
		v.replaceMarker(functionPartialMarker, "")
	}

	v.replaceMarker(functionParametersMarker, parameterSignature)

	if isModuleInitializer {
		v.replaceMarker(functionAttributeMarker, "[GoInit] ")
	} else if strings.HasPrefix(parameterSignature, "this ref ") {
		v.replaceMarker(functionAttributeMarker, "[GoRecv] ")
	} else {
		v.replaceMarker(functionAttributeMarker, "")
	}

	var funcExecutionContext string

	if useFuncExecutionContext {
		// Always name the wrapper's two parameters by their roles, even when one is unused.
		// Using `_` for an unused parameter is unsafe: a single `_` is a *named* C# lambda
		// parameter (not a discard), so a `_ = expr` discard in the body would bind to it
		// rather than discarding (e.g. `_ = call()` → CS0029). Unused-parameter warnings are
		// already suppressed (IDE0060) for the converted projects.
		deferParam := "defer"
		recoverParam := "recover"

		if v.namedReturnDeferMode {
			// Open a block body, declare the named returns outside the wrapper (so defers/recover
			// mutate them by closure), then open the void func() wrapper. The trailing
			// `return <named>;` and block close are emitted after the body below.
			funcExecutionContext = fmt.Sprintf(" {%s%s%sfunc((%s, %s) =>", namedReturnDeclsStr, v.newline, v.indent(v.indentLevel+1), deferParam, recoverParam)
		} else {
			funcExecutionContext = fmt.Sprintf(" => func((%s, %s) =>", deferParam, recoverParam)
		}
	} else {
		funcExecutionContext = ""
	}

	v.replaceMarker(functionExecContextMarker, funcExecutionContext)
	v.replaceMarker(functionBlockPrefixMarker, blockPrefix)

	if v.currentFuncPrefix.Len() > 0 {
		v.currentFuncPrefix.WriteString(v.newline)
	}

	v.replaceMarker(functionPrefixMarker, v.currentFuncPrefix.String())

	if useFuncExecutionContext {
		if v.namedReturnDeferMode {
			// Close the func(...) call (attaches to the lambda's `}` → `});`), then return the
			// named results — which the deferred code / recover may have mutated — and close the
			// block body opened in the exec-context above.
			returnExpr := strings.Join(v.namedReturnNames, ", ")

			if len(v.namedReturnNames) > 1 {
				returnExpr = "(" + returnExpr + ")"
			}

			indentInner := v.indent(v.indentLevel + 1)
			indentOuter := v.indent(v.indentLevel)
			savedIndent := v.indentLevel
			v.indentLevel = 0
			v.writeOutputLn(");")
			v.writeOutputLn("%sreturn %s;", indentInner, returnExpr)
			v.writeOutputLn("%s}", indentOuter)
			v.indentLevel = savedIndent
		} else {
			v.writeOutputLn(");")
		}
	} else if signatureOnly {
		// Bodyless (assembly/cgo) function: emit a `partial` declaration; the body is
		// supplied by a hand-written companion or the PartialStubGenerator.
		v.writeOutputLn(";")
	} else {
		v.targetFile.WriteString(v.newline)
	}

	v.inFunction = false
}

// identIsParameter checks if the given identifier is a parameter in the current function.
func (v *Visitor) identIsParameter(ident *ast.Ident) bool {
	if v.paramNames == nil || !v.paramNames.Contains(ident.Name) {
		return false
	}

	// The name matches a parameter, but a local can SHADOW a parameter of the same name. Only the
	// actual parameter object gets the deref-aliased box treatment (`Ꮡp`); a shadowing local that is
	// already a pointer (`ж<T> tΔ2`) must keep its plain form, not get a spurious `&` (CS0103 on the
	// undefined `ᏑtΔ2`). Verify the resolved object is genuinely a parameter; fall back to the name
	// match when it cannot be resolved.
	if obj := v.info.ObjectOf(ident); obj != nil && v.paramObjects != nil {
		return v.paramObjects[obj]
	}

	return true
}

func getParameters(signature *types.Signature, addRecv bool) *types.Tuple {
	var parameters *types.Tuple

	if addRecv && signature.Recv() != nil {
		// Concatenate receiver parameter with the rest of the parameters
		parameterVars := make([]*types.Var, 0, 1+signature.Params().Len())
		parameterVars = append(parameterVars, signature.Recv())

		for i := 0; i < signature.Params().Len(); i++ {
			parameterVars = append(parameterVars, signature.Params().At(i))
		}

		parameters = types.NewTuple(parameterVars...)
	} else {
		parameters = signature.Params()
	}

	return parameters
}

func (v *Visitor) generateParametersSignature(signature *types.Signature, addRecv bool) (string, string) {
	parameters := getParameters(signature, addRecv)

	if parameters == nil {
		return "", ""
	}

	result := strings.Builder{}
	var receiverAccess string
	dupBlankParams := hasDuplicateBlankParams(parameters)

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i == 0 && addRecv && signature.Recv() != nil {
			result.WriteString("this ")

			// Get receiver parameter type
			recvTypeName := v.getRefParamTypeName(param.Type())

			// Update function access to match receiver type
			receiverAccess = getAccess(recvTypeName)

			result.WriteString(v.getRefParamTypeName(param.Type()))
			result.WriteRune(' ')

			paramName := param.Name()

			if paramName == "" {
				paramName = "_"
			}

			result.WriteString(getSanitizedIdentifier(paramName))
			continue
		}

		if i > 0 {
			result.WriteString(", ")
		}

		if i == parameters.Len()-1 && signature.Variadic() {
			result.WriteString("params ")

			// If parameter is a slice, convert it to a Span
			if sliceType, ok := param.Type().(*types.Slice); ok {
				typeName, inline := v.variadicElementType(sliceType.Elem())

				if inline {
					result.WriteString("Span<" + typeName + ">")
				} else {
					result.WriteString(EllipsisOperator + typeName)
					v.addRequiredUsing(fmt.Sprintf("%s%s = Span<%s>", EllipsisOperator, typeName, typeName))
				}
			} else {
				result.WriteString("object[]")
			}

			// Variadic parameters are passed as C# param arrays, so we use a temporary
			// parameter name that will be later converted to a Go slice<T>
			result.WriteRune(' ')
			result.WriteString(getVariadicParamName(param))
		} else {
			result.WriteString(v.getCSTypeName(param.Type()))
			result.WriteRune(' ')

			paramName := param.Name()

			// Keep a lone `_`, but synthesize a unique placeholder when blanks would collide
			// (Go allows repeated blank params; C# forbids duplicate parameter names — CS0100).
			if paramName == "" || paramName == "_" {
				if dupBlankParams {
					paramName = fmt.Sprintf("_Δp%d", i)
				} else {
					paramName = "_"
				}
			}

			result.WriteString(getSanitizedIdentifier(paramName))
		}
	}

	return result.String(), receiverAccess
}

func (v *Visitor) generateResultSignature(signature *types.Signature) string {
	results := signature.Results()

	if results == nil {
		return "void"
	}

	result := strings.Builder{}

	if results.Len() == 1 {
		param := results.At(0)

		result.WriteString(v.getCSTypeName(param.Type()))

		if param.Name() != "" {
			result.WriteString(" /*")
			result.WriteString(param.Name())
			result.WriteString("*/")
		}

		return result.String()
	}

	result.WriteRune('(')

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result.WriteString(", ")
		}

		param := results.At(i)

		result.WriteString(v.getCSTypeName(param.Type()))

		if param.Name() != "" {
			result.WriteRune(' ')
			result.WriteString(getSanitizedIdentifier(param.Name()))
		}
	}

	result.WriteRune(')')

	return result.String()
}

func getVariadicParamName(param *types.Var) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(param.Name()), CapturedVarMarker)
}

func (v *Visitor) getTempVarName(varPrefix string) string {
	if v.tempVarCount == nil {
		v.tempVarCount = make(map[string]int)
	}

	count := v.tempVarCount[varPrefix]
	count++
	v.tempVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, TempVarMarker, count)
}
