package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strconv"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, interfaceType.Pos())

	v.writeOutputLn("[GoType] partial interface %s {", getSanitizedIdentifier(name))
	v.indentLevel++

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) != 1 {
			println("WARNING: Expected one name per InterfaceType method, found " + strconv.Itoa(len(method.Names)))
		}

		v.writeDoc(method.Doc, method.Pos())

		goMethodName := method.Names[0].Name
		csMethodName := getSanitizedFunctionName(goMethodName)
		typeLenDeviation := token.Pos(len(csMethodName) - len(goMethodName))

		methodType := v.info.ObjectOf(method.Names[0]).(*types.Func)

		if methodType == nil {
			panic("Failed to find interface method \"" + goMethodName + "\" in the type info")
		}

		signature := methodType.Signature()
		resultSignature := generateResultSignature(signature)
		parameterSignature := v.generateParametersSignature(signature, false)

		typeLenDeviation += token.Pos(len(parameterSignature) - getSourceParameterSignatureLen(signature))
		typeLenDeviation += token.Pos(len(resultSignature) - getSourceResultSignatureLen(signature))

		v.writeOutput("%s %s %s(%s);", getAccess(goMethodName), resultSignature, csMethodName, parameterSignature)
		v.writeComment(method.Comment, method.Type.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	}

	v.indentLevel--
	v.writeOutputLn("}")
}

func getSourceParameterSignatureLen(signature *types.Signature) int {
	parameters := signature.Params()

	if parameters == nil {
		return 0
	}

	result := 0

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i > 0 {
			result += 2
		}

		result += len(getTypeName(param.Type()))

		if param.Name() != "" {
			result += 1 + len(param.Name())
		}
	}

	return result
}

func getSourceResultSignatureLen(signature *types.Signature) int {
	results := signature.Results()

	if results == nil {
		return 0
	}

	if results.Len() == 1 {
		return len(getTypeName(results.At(0).Type()))
	}

	result := 2

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result += 2
		}

		result += len(getTypeName(results.At(i).Type()))
	}

	return result
}
