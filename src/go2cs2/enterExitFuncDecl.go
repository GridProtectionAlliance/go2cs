package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

const FunctionParametersMarker = ">>MARKER:FUNCTION_%s_PARAMETERS<<"
const FunctionExecContextMarker = ">>MARKER:FUNCTION_%s_EXEC_CONTEXT<<"
const FunctionBlockPrefixMarker = ">>MARKER:FUNCTION_%s_BLOCK_PREFIX<<"

func (v *Visitor) enterFuncDecl(x *ast.FuncDecl) {
	println("Entering FuncDecl \"" + x.Name.Name + "\"")

	v.targetFile.WriteString(v.newline)
	v.writeDoc(x.Doc, x.Pos())

	v.inFunction = true
	goFunctionName := x.Name.Name
	csFunctionName := getSanitizedIdentifier(goFunctionName)

	v.currentFunction = v.info.ObjectOf(x.Name).(*types.Func)

	if v.currentFunction == nil {
		panic("Failed to find function \"" + goFunctionName + "\" in the type info")
	}

	signature := v.currentFunction.Signature()

	if x.Recv == nil {
		// Handle Go "main" function as a special case, in C# this should be capitalized "Main"
		if csFunctionName == "main" {
			csFunctionName = "Main"
		}
	}

	functionParametersMarker := fmt.Sprintf(FunctionParametersMarker, goFunctionName)
	functionExecContextMarker := fmt.Sprintf(FunctionExecContextMarker, goFunctionName)
	v.pushInnerBlockPrefix(fmt.Sprintf(FunctionBlockPrefixMarker, goFunctionName))

	v.writeOutput(fmt.Sprintf("%s static %s %s(%s)%s", getAccess(goFunctionName), generateResultSignature(signature), csFunctionName, functionParametersMarker, functionExecContextMarker))

	if x.Body != nil {
		v.enterBlockStmt(x.Body)
		v.exitBlockStmt(x.Body)
	}
}

func (v *Visitor) exitFuncDecl(x *ast.FuncDecl) {
	goFunctionName := x.Name.Name
	signatureOnly := x.Body == nil
	signature := v.currentFunction.Signature()
	useFuncExecutionContext := v.hasDefer || v.hasPanic || v.hasRecover
	parameterSignature := generateParametersSignature(signature)
	blockPrefix := ""

	if !signatureOnly {
		resultParameters := &strings.Builder{}
		arrayClones := &strings.Builder{}
		implicitPointers := &strings.Builder{}

		for i := 0; i < signature.Results().Len(); i++ {
			param := signature.Results().At(i)

			if param.Name() != "" {
				v.writeString(resultParameters, fmt.Sprintf("%s %s = default;", getCSTypeName(param.Type()), param.Name()))
			}
		}

		parameters := getParameters(signature)

		for i := 0; i < parameters.Len(); i++ {
			param := parameters.At(i)

			// For any array parameters, Go copies the array by value
			if _, ok := param.Type().(*types.Array); ok {
				v.writeStringLn(arrayClones, fmt.Sprintf("%s = %s.Clone();", param.Name(), param.Name()))
			}

			// All pointers in Go can be implicitly dereferenced, so setup a "local ref" instance to each
			if pointerType, ok := param.Type().(*types.Pointer); ok {
				v.writeStringLn(implicitPointers, fmt.Sprintf("ref %s %s = ref %s%s.val;", pointerType.Elem().String(), param.Name(), AddressPrefix, param.Name()))
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

				if i == 0 && x.Recv != nil {
					updatedSignature.WriteString("this ")
				}

				if i > 0 {
					updatedSignature.WriteString(", ")
				}

				if i == parameters.Len()-1 && signature.Variadic() {
					updatedSignature.WriteString("params ")
				}

				updatedSignature.WriteString(getCSTypeName(param.Type()))
				updatedSignature.WriteString(" ")

				if _, ok := param.Type().(*types.Pointer); ok {
					updatedSignature.WriteString(AddressPrefix)
				}

				updatedSignature.WriteString(param.Name())
			}

			parameterSignature = updatedSignature.String()
		}
	}

	// Replace function markers
	v.replaceMarker(fmt.Sprintf(FunctionParametersMarker, goFunctionName), parameterSignature)

	var funcExecutionContext string

	if useFuncExecutionContext {
		var deferParam, panicParam, recoverParam string

		if v.hasDefer {
			deferParam = "defer"
		} else {
			deferParam = "_"
		}

		if v.hasPanic {
			panicParam = "panic"
		} else {
			panicParam = "_"
		}

		if v.hasRecover {
			recoverParam = "recover"
		} else {
			recoverParam = "_"
		}

		funcExecutionContext = fmt.Sprintf(" => func((%s, %s, %s) =>", deferParam, panicParam, recoverParam)
	} else {
		funcExecutionContext = ""
	}

	v.replaceMarker(fmt.Sprintf(FunctionExecContextMarker, goFunctionName), funcExecutionContext)
	v.replaceMarker(fmt.Sprintf(FunctionBlockPrefixMarker, goFunctionName), blockPrefix)

	if useFuncExecutionContext {
		v.writeOutput(");")
	} else if signatureOnly {
		v.writeOutput(";")
	}

	v.inFunction = false

	println("Exiting FuncDecl \"" + x.Name.Name + "\"")
}

func getParameters(signature *types.Signature) *types.Tuple {
	var parameters *types.Tuple

	if signature.Recv() == nil {
		parameters = signature.Params()
	} else {
		// Concatenate receiver parameter with the rest of the parameters
		parameterVars := make([]*types.Var, 0, 1+signature.Params().Len())
		parameterVars = append(parameterVars, signature.Recv())
		for i := 0; i < signature.Params().Len(); i++ {
			parameterVars = append(parameterVars, signature.Params().At(i))
		}
		parameters = types.NewTuple(parameterVars...)
	}

	return parameters
}

func generateParametersSignature(signature *types.Signature) string {
	parameters := getParameters(signature)

	if parameters == nil {
		return ""
	}

	result := strings.Builder{}

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i == 0 && signature.Recv() != nil {
			result.WriteString("this ")
		}

		if i > 0 {
			result.WriteString(", ")
		}

		if i == parameters.Len()-1 && signature.Variadic() {
			result.WriteString("params ")
		}

		result.WriteString(getCSTypeName(param.Type()))
		result.WriteString(" ")
		result.WriteString(param.Name())
	}

	return result.String()
}

func generateResultSignature(signature *types.Signature) string {
	results := signature.Results()

	if results == nil {
		return "void"
	}

	if results.Len() == 1 {
		return getCSTypeName(results.At(0).Type())
	}

	result := strings.Builder{}

	result.WriteString("(")

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result.WriteString(", ")
		}

		result.WriteString(getCSTypeName(results.At(i).Type()))
	}

	result.WriteString(")")

	return result.String()
}
