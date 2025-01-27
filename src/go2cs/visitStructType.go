package main

import (
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
			var ident *ast.Ident
			var ok bool

			if ident, ok = field.Type.(*ast.Ident); !ok {
				if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if ident, ok = ptrType.X.(*ast.Ident); !ok {
						continue
					}
				} else {
					continue
				}
			}

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
					promotions.Add(structTypeName)
				} else {
					promotedInterfaceImplementations[csFullTypeName] = NewHashSet([]string{structTypeName})
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
			} else {
				if ptrType, ok := identType.(*types.Pointer); ok {
					if _, ok = ptrType.Elem().(*types.Named); !ok {
						continue
					}
				} else if _, ok = identType.(*types.Struct); !ok {
					continue
				}

				// Handle promoted struct implementations
				v.writeOutput("public partial ref %s %s { get; }", csFullTypeName, getSanitizedIdentifier(goTypeName))
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
}
