package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strconv"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, name string, doc *ast.CommentGroup) {
	v.writeOutputLn("/* %s */", v.getPrintedNode(interfaceType))

	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, interfaceType.Pos())

	v.writeOutputLn("[GoType(\"interface\")]")
	v.writeOutputLn("%s partial interface %s {", getAccess(name), getSanitizedIdentifier(name))
	v.indentLevel++

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) != 1 {
			println("WARNING: Expected one name per InterfaceType method, found " + strconv.Itoa(len(method.Names)))
		}

		v.writeDoc(method.Doc, method.Pos())

		goMethodName := method.Names[0].Name
		csMethodName := getSanitizedIdentifier(goMethodName)
		typeLenDeviation := token.Pos(len(csMethodName) - len(goMethodName))

		methodType := v.info.ObjectOf(method.Names[0]).(*types.Func)

		if methodType == nil {
			panic("Failed to find interface method \"" + goMethodName + "\" in the type info")
		}

		signature := methodType.Signature()
		resultSignature := generateResultSignature(signature)
		parameterSignature := generateParametersSignature(signature, false)

		typeLenDeviation += token.Pos(len(parameterSignature) - getSourceParameterSignatureLen(signature))
		typeLenDeviation += token.Pos(len(resultSignature) - getSourceResultSignatureLen(signature))

		v.writeOutput(fmt.Sprintf("%s %s %s(%s);", getAccess(goMethodName), resultSignature, csMethodName, parameterSignature))
		v.writeComment(method.Comment, method.Type.End()+typeLenDeviation)
		v.targetFile.WriteString(v.newline)
	}

	v.indentLevel--
	v.writeOutputLn("}")
}
