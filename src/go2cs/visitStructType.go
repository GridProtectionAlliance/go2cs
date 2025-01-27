package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

// Handles struct types in the context of a TypeSpec
func (v *Visitor) visitStructType(structType *ast.StructType, name string, doc *ast.CommentGroup) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, structType.Pos())

	structTypeName := getSanitizedIdentifier(name)
	v.writeOutputLn("[GoType] partial struct %s {", structTypeName)
	v.indentLevel++

	// Track promoted interface methods that could be shadowed by receivers
	type InterfaceMethodInfo struct {
		interfaceType *types.Interface
		interfaceName string
		methodName    string
	}

	promotedInterfaceMethods := NewHashSet([]InterfaceMethodInfo{})

	for _, field := range structType.Fields.List {
		v.writeDoc(field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeOutput("[GoTag(")
			v.targetFile.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false}))
			v.targetFile.WriteString(")]")
			v.targetFile.WriteString(v.newline)
		}

		fieldType := v.getType(field.Type, false)
		goTypeName := getTypeName(fieldType)
		goFullTypeName := getFullTypeName(fieldType)
		csFullTypeName := convertToCSTypeName(goFullTypeName)
		typeLenDeviation := token.Pos(len(csFullTypeName) - len(goFullTypeName))

		if field.Names == nil {
			// Check for promoted fields
			if ident, ok := field.Type.(*ast.Ident); ok {
				// Lookup identity to determine if it's an interface
				identObj := v.info.ObjectOf(ident)

				if identObj == nil {
					continue // Could not find the object of ident
				}

				identType := identObj.Type().Underlying()

				if interfaceType, ok := identType.(*types.Interface); ok {
					// Add to promoted interface implementations
					packageLock.Lock()

					if promotions, exists := promotedInterfaceImplementations[csFullTypeName]; exists {
						promotions[structTypeName] = ""
					} else {
						promotedInterfaceImplementations[csFullTypeName] = map[string]string{structTypeName: ""}
					}

					packageLock.Unlock()

					// Record the methods of the promoted interface
					for i := 0; i < interfaceType.NumMethods(); i++ {
						method := interfaceType.Method(i)

						promotedInterfaceMethods.Add(InterfaceMethodInfo{
							interfaceType: interfaceType,
							interfaceName: ident.Name,
							methodName:    method.Name(),
						})
					}

					v.writeOutput("public %s %s;", csFullTypeName, getSanitizedIdentifier(goTypeName))
				} else if _, ok := identType.(*types.Struct); ok {
					// Handle promoted struct implementations
					v.writeOutput("public partial ref %s %s { get; }", csFullTypeName, getSanitizedIdentifier(goTypeName))
				}
			}

			v.writeComment(field.Comment, field.Type.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		} else {
			for _, ident := range field.Names {
				v.writeOutput("public %s %s;", csFullTypeName, getSanitizedIdentifier(ident.Name))
				v.writeComment(field.Comment, field.Type.End()+typeLenDeviation)
				v.targetFile.WriteString(v.newline)
			}
		}
	}

	v.indentLevel--
	v.writeOutputLn("}")

	// Check receiver methods that shadow interface methods - these have priority over promoted interface methods
	obj := v.getType(structType, false)

	if obj != nil {
		// Iterate through the method set for the struct type to identify direct receiver methods
		methodSet := types.NewMethodSet(types.NewPointer(obj))

		for i := 0; i < methodSet.Len(); i++ {
			method := methodSet.At(i)
			methodName := method.Obj().Name()

			// Check if this method exists as a direct receiver method on our struct
			if recv := method.Obj().(*types.Func).Type().(*types.Signature).Recv(); recv != nil {
				// Confirm this method matches a promoted interface method
				var interfaceMethod *types.Func
				var promotedMethod InterfaceMethodInfo

				for candidate := range promotedInterfaceMethods {
					if candidate.methodName == methodName {
						promotedMethod = candidate

						for j := 0; j < candidate.interfaceType.NumMethods(); j++ {
							methodCandidate := candidate.interfaceType.Method(j)

							if methodCandidate.Name() == methodName {
								interfaceMethod = methodCandidate
								break
							}
						}

						break
					}
				}

				// Skip if no matching promoted method exists
				if interfaceMethod == nil {
					continue
				}

				// Ensure the method signature matches
				if !types.Identical(method.Type(), interfaceMethod.Type()) {
					continue
				}

				// Confirm this method is explicitly implemented for the struct and not a promoted field
				if method.Obj().Pkg() == v.pkg {
					packageLock.Lock()

					if promotions, exists := promotedInterfaceImplementations[promotedMethod.interfaceName]; exists {
						overrides := promotions[structTypeName]

						if len(overrides) > 0 {
							overrides = fmt.Sprintf("%s, %s", overrides, promotedMethod.methodName)
						} else {
							overrides = promotedMethod.methodName
						}

						promotions[structTypeName] = overrides
					} else {
						println(fmt.Sprintf("Failed to find promoted interface implementations for %s", promotedMethod.interfaceName))
					}

					packageLock.Unlock()
				}
			}
		}
	}
}
