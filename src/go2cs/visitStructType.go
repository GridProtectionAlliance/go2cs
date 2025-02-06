package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// Handles struct types in the context of a TypeSpec, ValueSpec, or FieldList
func (v *Visitor) visitStructType(structType *ast.StructType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool) {
	var target *strings.Builder

	// Intra-function type declarations are not allowed in C#
	if lifted {
		target = &strings.Builder{}

		if !strings.HasPrefix(name, v.currentFuncName+"_") {
			name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
		}

		v.liftedTypeMap[identType] = v.getUniqueLiftedTypeName(name)
		v.indentLevel--
	}

	if target == nil {
		target = v.targetFile
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writeDocString(target, doc, structType.Pos())

	structTypeName := getSanitizedIdentifier(name)
	v.writeStringLn(target, "[GoType] partial struct %s {", structTypeName)
	v.indentLevel++

	// Track promoted interface methods that could be shadowed by receivers
	type InterfaceMethodInfo struct {
		interfaceType *types.Interface
		interfaceName string
		methodName    string
	}

	promotedInterfaceMethods := HashSet[InterfaceMethodInfo]{}

	for _, field := range structType.Fields.List {
		v.writeDocString(target, field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeString(target, "[GoTag(")
			target.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false}))
			target.WriteString(")]")
			target.WriteString(v.newline)
		}

		// Check if field is a struct or a pointer to a struct
		if ptrType, ok := field.Type.(*ast.StarExpr); ok {
			if subStructType, ok := ptrType.X.(*ast.StructType); ok {
				subStructIdentType := v.getExprType(ptrType.X)
				v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true)

				// Track sub-struct types
				subStructTypes := v.subStructTypes[identType]

				if subStructTypes == nil {
					subStructTypes = make([]types.Type, 0)
				}

				subStructTypes = append(subStructTypes, v.getExprType(ptrType))
				v.subStructTypes[identType] = subStructTypes
			}
		} else if subStructType, ok := field.Type.(*ast.StructType); ok {
			subStructIdentType := v.getExprType(field.Type)
			v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true)

			// Track sub-struct types
			subStructTypes := v.subStructTypes[identType]

			if subStructTypes == nil {
				subStructTypes = make([]types.Type, 0)
			}

			subStructTypes = append(subStructTypes, subStructIdentType)
			v.subStructTypes[identType] = subStructTypes
		}

		fieldType := v.getType(field.Type, false)
		goTypeName := v.getTypeName(fieldType)
		goFullTypeName := v.getFullTypeName(fieldType)
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

				v.writeString(target, "public %s %s;", csFullTypeName, getSanitizedIdentifier(goTypeName))
			} else {
				if ptrType, ok := identType.(*types.Pointer); ok {
					if _, ok = ptrType.Elem().(*types.Named); !ok {
						continue
					}
				} else if _, ok = identType.(*types.Struct); !ok {
					continue
				}

				// Handle promoted struct implementations
				v.writeString(target, "public partial ref %s %s { get; }", csFullTypeName, getSanitizedIdentifier(goTypeName))
			}

			v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
			target.WriteString(v.newline)
		} else {
			for _, ident := range field.Names {
				v.writeString(target, "public %s %s;", csFullTypeName, getSanitizedIdentifier(ident.Name))
				v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
				target.WriteString(v.newline)
			}
		}
	}

	v.indentLevel--
	v.writeStringLn(target, "}")

	if lifted {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel++
	}
}

func (v *Visitor) getUniqueLiftedTypeName(typeName string) string {
	typeName = getSanitizedIdentifier(typeName)
	uniqueTypeName := typeName
	count := 0

	for v.liftedTypeNames.Contains(uniqueTypeName) {
		count++
		uniqueTypeName = fmt.Sprintf("%s%s%d", typeName, TempVarMarker, count)
	}

	v.liftedTypeNames.Add(uniqueTypeName)

	return uniqueTypeName
}
