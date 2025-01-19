package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

const InterfaceInheritanceMarker = ">>MARKER:INHERITED_INTERFACES<<"

// Handles map types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, interfaceType.Pos())

	result := &strings.Builder{}
	inheritedInterfaces := []string{}
	outerIndent := v.indent(v.indentLevel)

	result.WriteString(outerIndent)
	result.WriteString(fmt.Sprintf("[GoType] partial interface %s%s{", getSanitizedIdentifier(name), InterfaceInheritanceMarker))
	result.WriteString(v.newline)

	v.indentLevel++
	innerIndent := v.indent(v.indentLevel)

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) == 1 {

			v.writeDocString(result, method.Doc, method.Pos())

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

			result.WriteString(innerIndent)
			result.WriteString(fmt.Sprintf("%s %s %s(%s);", getAccess(goMethodName), resultSignature, csMethodName, parameterSignature))
			v.writeCommentString(result, method.Comment, method.Type.End()+typeLenDeviation)
			result.WriteString(v.newline)
		} else if method.Type != nil {
			inheritedInterfaces = append(inheritedInterfaces, v.convExpr(method.Type, nil))
		} else {
			panic("Unexpected method declaration in interface: %s" + v.getPrintedNode(method))
		}
	}

	v.indentLevel--
	result.WriteString(v.indent(v.indentLevel))
	result.WriteRune('}')
	result.WriteString(v.newline)

	interfaceResult := result.String()

	if len(inheritedInterfaces) > 0 {
		inheritedResult := " :" + v.newline

		for i, inheritedInterface := range inheritedInterfaces {
			if i > 0 {
				inheritedResult += "," + v.newline
			}

			inheritedResult += innerIndent + inheritedInterface
		}

		inheritedResult += v.newline + outerIndent

		interfaceResult = strings.ReplaceAll(interfaceResult, InterfaceInheritanceMarker, inheritedResult)

		// Track which interfaces this interface inherits from so
		// duplicate interface implementations can be avoided
		packageLock.Lock()
		interfaceInheritances[name] = NewHashSet(inheritedInterfaces)
		packageLock.Unlock()
	} else {
		interfaceResult = strings.ReplaceAll(interfaceResult, InterfaceInheritanceMarker, " ")
	}

	v.targetFile.WriteString(interfaceResult)
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
