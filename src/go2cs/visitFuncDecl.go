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

func (v *Visitor) visitFuncDecl(funcDecl *ast.FuncDecl) {
	v.inFunction = true
	v.capturedVarCount = nil
	v.tempVarCount = nil
	v.captureReceiver = false
	v.useUnsafeFunc = false

	goFunctionName := funcDecl.Name.Name
	csFunctionName := getSanitizedFunctionName(goFunctionName)

	v.currentFuncDecl = funcDecl
	v.currentFuncName = csFunctionName
	v.currentFuncPrefix = &strings.Builder{}

	v.varNames = make(map[*types.Var]string)

	currentFuncType := v.info.ObjectOf(funcDecl.Name).(*types.Func)

	if currentFuncType == nil {
		panic("Failed to find function \"" + goFunctionName + "\" in the type info")
	}

	signature := currentFuncType.Signature()
	v.currentFuncSignature = signature

	// Analyze function variables for reassignments and redeclarations (variable shadows)
	v.performVariableAnalysis(funcDecl, signature)

	// Collect parameter names from the function declaration
	if v.paramNames == nil {
		v.paramNames = HashSet[string]{}
	} else {
		v.paramNames.Clear()
	}

	for _, param := range funcDecl.Type.Params.List {
		for _, name := range param.Names {
			v.paramNames.Add(name.Name)
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
			if structType, exprType := v.extractStructType(field.Type); structType != nil {
				v.indentLevel++
				v.visitStructType(structType, exprType, fieldName, field.Comment, true)
				v.indentLevel--
			}
		}
	}

	// Loop through function parameters to check if any are structs
	if funcDecl.Type.Params != nil {
		for _, field := range funcDecl.Type.Params.List {
			for _, name := range field.Names {
				// Check if the parameter type is a struct or pointer to a struct
				if structType, exprType := v.extractStructType(field.Type); structType != nil {
					v.indentLevel++
					v.visitStructType(structType, exprType, name.Name, field.Comment, true)
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
		v.visitBlockStmt(funcDecl.Body, blockContext)
	}

	signatureOnly := funcDecl.Body == nil
	useFuncExecutionContext := v.hasDefer || v.hasRecover
	parameterSignature, receiverAccess := v.generateParametersSignature(signature, true)
	blockPrefix := ""

	// If receiver access is not public, update function access to match
	if len(receiverAccess) > 0 && receiverAccess != "public" {
		functionAccess = receiverAccess
	}

	if !signatureOnly {
		resultParameters := &strings.Builder{}
		arrayClones := &strings.Builder{}
		implicitPointers := &strings.Builder{}

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			resultParams := signature.Results()
			paramIndex := 0

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

						resultParameters.WriteString(v.newline)

						v.writeString(resultParameters, "%s%s %s = default!;", v.indent(v.indentLevel+1), v.getCSTypeName(param.Type()), paramName)

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
				if i == 0 && funcDecl.Recv != nil {
					// Skip receiver parameter
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

			for i := 0; i < parameters.Len(); i++ {
				param := parameters.At(i)

				if i == 0 && funcDecl.Recv != nil {
					updatedSignature.WriteString("this ")

					// Get receiver parameter type
					recvTypeName := v.getRefParamTypeName(param.Type())

					// Update function access to match receiver type
					functionAccess = getAccess(recvTypeName)

					updatedSignature.WriteString(recvTypeName)
					updatedSignature.WriteRune(' ')
					updatedSignature.WriteString(getSanitizedIdentifier(param.Name()))
					continue
				}

				if i > 0 {
					updatedSignature.WriteString(", ")
				}

				if i == parameters.Len()-1 && signature.Variadic() {
					updatedSignature.WriteString("params ")

					// If parameter is a slice, convert it to a Span
					if sliceType, ok := param.Type().(*types.Slice); ok {
						typeName := v.getCSTypeName(sliceType.Elem())

						updatedSignature.WriteString(ElipsisOperator + typeName)
						v.addRequiredUsing(fmt.Sprintf("%s%s = Span<%s>", ElipsisOperator, typeName, typeName))
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
						updatedSignature.WriteString(AddressPrefix)
						updatedSignature.WriteString(param.Name())
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

	if funcDecl.Body == nil {
		v.replaceMarker(functionPartialMarker, " partial")
	} else {
		v.replaceMarker(functionPartialMarker, "")
	}

	v.replaceMarker(functionParametersMarker, parameterSignature)

	if isModuleInitializer {
		v.replaceMarker(functionAttributeMarker, "[GoInit] ")
	} else if strings.HasPrefix(parameterSignature, "this ref ") {
		if v.captureReceiver {
			v.replaceMarker(functionAttributeMarker, "[GoRecv(\"capture\")] ")
		} else {
			v.replaceMarker(functionAttributeMarker, "[GoRecv] ")
		}
	} else {
		v.replaceMarker(functionAttributeMarker, "")
	}

	var funcExecutionContext string

	if useFuncExecutionContext {
		var deferParam, recoverParam string

		if v.hasDefer {
			deferParam = "defer"
		} else {
			deferParam = "_"
		}

		if v.hasRecover {
			recoverParam = "recover"
		} else {
			recoverParam = "_"
		}

		funcExecutionContext = fmt.Sprintf(" => func((%s, %s) =>", deferParam, recoverParam)
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
		v.writeOutputLn(");")
	} else if signatureOnly {
		v.writeOutput(";")
	} else {
		v.targetFile.WriteString(v.newline)
	}

	v.inFunction = false
}

// identIsParameter checks if the given identifier is a parameter in the current function.
func (v *Visitor) identIsParameter(ident *ast.Ident) bool {
	if v.paramNames == nil {
		return false
	}

	// Check if the identifier's name is in the parameter names hash set
	return v.paramNames.Contains(ident.Name)
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
				typeName := v.getCSTypeName(sliceType.Elem())

				result.WriteString(ElipsisOperator + typeName)
				v.addRequiredUsing(fmt.Sprintf("%s%s = Span<%s>", ElipsisOperator, typeName, typeName))
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

			if paramName == "" {
				paramName = "_"
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
