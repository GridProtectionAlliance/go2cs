package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

const InterfaceTypeAttributeMarker = ">>MARKER:INTERFACE_TYPE_ATTRS<<"
const InterfacePostAtributeMarker = ">>MARKER:POST_INTERFACE_ATTRS<<"
const InterfaceInheritanceMarker = ">>MARKER:INHERITED_INTERFACES<<"

// For interface types with generic constraints, we will be adding a C# type parameter to the
// converted Go interface to handle operators. Since methods in interfaces can have their own
// type constraints, we mark the type so that it will not conflict with generic method types
const TypeT = ShadowVarMarker + "T"

// Handles interface types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool) (interfaceTypeName string) {
	for _, field := range interfaceType.Methods.List {
		// Check if this is an actual method (has a function type)
		if funcType, ok := field.Type.(*ast.FuncType); ok {
			var indentOffset int

			if v.inFunction {
				indentOffset = 1
			}

			// Loop through function results to check if any are structs
			if funcType.Results != nil {
				for index, resultField := range funcType.Results.List {
					var fieldName string

					if resultField.Names == nil {
						fieldName = fmt.Sprintf("%sR%d", name, index)
					} else {
						fieldName = fmt.Sprintf("%s_%s", name, resultField.Names[0].Name)
					}

					// Check if the return type is a struct or pointer to a struct
					if structType, exprType := v.extractStructType(resultField.Type); structType != nil {
						v.indentLevel += indentOffset
						v.visitStructType(structType, exprType, fieldName, resultField.Comment, true)
						v.indentLevel -= indentOffset
					}
				}
			}

			// Loop through function parameters to check if any are structs
			if funcType.Params != nil {
				for _, paramField := range funcType.Params.List {
					for _, paramName := range paramField.Names {
						// Check if the parameter type is a struct or pointer to a struct
						if structType, exprType := v.extractStructType(paramField.Type); structType != nil {
							v.indentLevel += indentOffset
							v.visitStructType(structType, exprType, fmt.Sprintf("%s_%s", name, paramName.Name), paramField.Comment, true)
							v.indentLevel -= indentOffset
						}
					}
				}
			}
		}
	}

	var target *strings.Builder
	var preLiftIndentLevel int

	// Intra-function type declarations are not allowed in C#
	if lifted {
		if v.inFunction {
			target = &strings.Builder{}

			if !strings.HasPrefix(name, v.currentFuncName+"_") {
				name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
			}

			preLiftIndentLevel = v.indentLevel
			v.indentLevel = 0
		}

		interfaceTypeName = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = interfaceTypeName
	} else {
		interfaceTypeName = name
	}

	if target == nil {
		target = v.targetFile
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writeDocString(target, doc, interfaceType.Pos())

	result := &strings.Builder{}
	inheritedInterfaces := []string{}
	typeConstraints := HashSet[ConstraintType]{}
	var operatorSets HashSet[OperatorSet]
	outerIndent := v.indent(v.indentLevel)

	result.WriteString(outerIndent)
	result.WriteString(fmt.Sprintf("[GoType%s]%spartial interface %s%s{", InterfaceTypeAttributeMarker, InterfacePostAtributeMarker, getSanitizedIdentifier(interfaceTypeName), InterfaceInheritanceMarker))
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
			resultSignature := v.generateResultSignature(signature)
			parameterSignature, _ := v.generateParametersSignature(signature, false)

			typeLenDeviation += token.Pos(len(parameterSignature) - v.getSourceParameterSignatureLen(signature))
			typeLenDeviation += token.Pos(len(resultSignature) - v.getSourceResultSignatureLen(signature))

			result.WriteString(fmt.Sprintf("%s%s %s(%s);", innerIndent, resultSignature, csMethodName, parameterSignature))
			v.writeCommentString(result, method.Comment, method.Type.End()+typeLenDeviation)
			result.WriteString(v.newline)
		} else if method.Type != nil {
			if isConstraint, methodCount := v.isTypeConstraint(method.Type); isConstraint {
				result.WriteString(fmt.Sprintf("%s//  Type constraints: %s%s", innerIndent, v.getPrintedNode(method.Type), v.newline))
				typeConstraints.UnionWithSet(v.getConstraintTypeSetFromExpr(method.Type))
				operatorSets = getOperatorSet(typeConstraints)
				result.WriteString(fmt.Sprintf("%s// Derived operators: %s%s", innerIndent, getOperatorSetAsString(operatorSets), v.newline))

				// If type constraint constains any methods, add it to the inherited interfaces
				if methodCount > 0 {
					inheritedInterfaces = append(inheritedInterfaces, fmt.Sprintf("%s<%s>", v.convExpr(method.Type, nil), TypeT))
				}
			} else {
				inheritedInterfaces = append(inheritedInterfaces, v.convExpr(method.Type, nil))
			}
		} else {
			panic("Unexpected method declaration in interface: %s" + v.getPrintedNode(method))
		}
	}

	v.indentLevel--
	result.WriteString(v.indent(v.indentLevel))
	result.WriteRune('}')
	result.WriteString(v.newline)

	inheritedResult := ""

	if len(typeConstraints) > 0 {
		inheritedResult = fmt.Sprintf("%s<%s>", inheritedResult, TypeT)
	}

	interfaceAttrs := ""
	postAttrs := " "

	if lifted {
		// Add "dyn" implementation attribute to lifted types since
		// they cannot be directly implemented in C# code. For these
		// types, a reflection based type implementation is used when
		// type assertions and comparisons are needed.
		interfaceAttrs = "dyn"
	}

	if len(operatorSets) > 0 {
		if len(interfaceAttrs) > 0 {
			interfaceAttrs += "; "
		}

		interfaceAttrs += fmt.Sprintf("operators = %s", getOperatorSetAttributes(operatorSets))
		postAttrs = v.newline
	}

	if len(interfaceAttrs) > 0 {
		interfaceAttrs = fmt.Sprintf("(\"%s\")", interfaceAttrs)
	}

	if len(inheritedInterfaces) > 0 {
		inheritedResult += " :" + v.newline

		for i, inheritedInterface := range inheritedInterfaces {
			if i > 0 {
				inheritedResult += "," + v.newline
			}

			inheritedResult += innerIndent + inheritedInterface
		}

		inheritedResult += v.newline + outerIndent

		// Track which interfaces this interface inherits from so
		// duplicate interface implementations can be avoided
		packageLock.Lock()
		interfaceInheritances[interfaceTypeName] = NewHashSet(inheritedInterfaces)
		packageLock.Unlock()
	} else {
		inheritedResult += " "
	}

	target.WriteString(strings.ReplaceAll(strings.ReplaceAll(strings.ReplaceAll(result.String(),
		InterfaceTypeAttributeMarker, interfaceAttrs),
		InterfacePostAtributeMarker, postAttrs),
		InterfaceInheritanceMarker, inheritedResult))

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel = preLiftIndentLevel
	}

	return
}

func (v *Visitor) getSourceParameterSignatureLen(signature *types.Signature) int {
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

		result += len(v.getTypeName(param.Type(), false))

		if param.Name() != "" {
			result += 1 + len(param.Name())
		}
	}

	return result
}

func (v *Visitor) getSourceResultSignatureLen(signature *types.Signature) int {
	results := signature.Results()

	if results == nil {
		return 0
	}

	if results.Len() == 1 {
		return len(v.getTypeName(results.At(0).Type(), false))
	}

	result := 2

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result += 2
		}

		result += len(v.getTypeName(results.At(i).Type(), false))
	}

	return result
}
