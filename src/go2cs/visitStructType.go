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
	type InterfaceMethod struct {
		interfaceName string
		methodName    string
		ifaceType     *types.Interface
	}

	promotedInterfaceMethods := NewHashSet([]InterfaceMethod{})

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

				if iface, ok := identType.(*types.Interface); ok {
					// Add to promoted interface implementations
					packageLock.Lock()
					if promotions, exists := promotedInterfaceImplementations[csFullTypeName]; exists {
						promotions[structTypeName] = ""
					} else {
						promotedInterfaceImplementations[csFullTypeName] = map[string]string{structTypeName: ""}
					}
					packageLock.Unlock()

					// Record the methods of the promoted interface
					for i := 0; i < iface.NumMethods(); i++ {
						method := iface.Method(i)

						promotedInterfaceMethods.Add(InterfaceMethod{
							interfaceName: ident.Name,
							methodName:    method.Name(),
							ifaceType:     iface,
						})
					}
				} else if _, ok := identType.(*types.Struct); ok {
					// It's a struct type
				}
			}

			v.writeOutput("public %s %s;", csFullTypeName, getSanitizedIdentifier(goTypeName))
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

	// Iterate through the method set for the struct type to identify direct receiver methods
	if obj != nil {
		methodSet := types.NewMethodSet(types.NewPointer(obj))

		for i := 0; i < methodSet.Len(); i++ {
			method := methodSet.At(i)
			methodName := method.Obj().Name()

			// Check if this method exists as a direct receiver method on our struct
			if recv := method.Obj().(*types.Func).Type().(*types.Signature).Recv(); recv != nil {
				// Confirm this method matches a promoted interface method
				var ifaceMethod *types.Func
				var iface InterfaceMethod

				for candidate := range promotedInterfaceMethods {
					if candidate.methodName == methodName {
						iface = candidate
						for j := 0; j < candidate.ifaceType.NumMethods(); j++ {
							methodCandidate := candidate.ifaceType.Method(j)

							if methodCandidate.Name() == methodName {
								ifaceMethod = methodCandidate
								break
							}
						}

						break
					}
				}

				// Skip if no matching promoted method exists
				if ifaceMethod == nil {
					continue
				}

				// Ensure the method signature matches
				if !types.Identical(method.Type(), ifaceMethod.Type()) {
					continue
				}

				// Confirm this method is explicitly implemented for the struct and not a promoted field
				if method.Obj().Pkg() == v.pkg {
					packageLock.Lock()
					if promotions, exists := promotedInterfaceImplementations[iface.interfaceName]; exists {
						overrides := promotions[structTypeName]

						if len(overrides) > 0 {
							overrides = fmt.Sprintf("%s, %s", overrides, iface.methodName)
						} else {
							overrides = iface.methodName
						}

						promotions[structTypeName] = overrides
					} else {
						println(fmt.Sprintf("Failed to find promoted interface implementations for %s", iface.interfaceName))
					}
					packageLock.Unlock()
				}
			}
		}
	}
}
