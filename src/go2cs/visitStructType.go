package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// Handles struct types in the context of a TypeSpec, ValueSpec, or FieldList
func (v *Visitor) visitStructType(structType *ast.StructType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool) (structTypeName string) {
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

		structTypeName = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = structTypeName
	} else {
		structTypeName = name
	}

	if target == nil {
		target = v.targetFile
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writeDocString(target, doc, structType.Pos())

	structTypeName = getSanitizedIdentifier(structTypeName)
	typeParams, constraints := v.getGenericDefinition(identType)

	if len(constraints) == 0 {
		constraints = " "
	} else {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	var dynamic string

	if lifted {
		dynamic = "(\"dyn\")"
	}

	v.writeStringLn(target, "[GoType%s] partial struct %s%s%s{", dynamic, structTypeName, typeParams, constraints)
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
		goTypeName := v.getTypeName(fieldType, false)
		goFullTypeName := v.getFullTypeName(fieldType, false)
		csFullTypeName := convertToCSTypeName(goFullTypeName)
		typeLenDeviation := token.Pos(len(csFullTypeName) - len(goFullTypeName))

		var arrayInitializer string

		if arrayType, ok := field.Type.(*ast.ArrayType); ok {
			if arrayType.Len != nil {
				arrayInitializer = fmt.Sprintf(" = new(%s)", v.convExpr(arrayType.Len, nil))
			}
		}

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

				v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
			} else {
				var handled bool

				if ptrType, ok := identType.(*types.Pointer); ok {
					if _, ok = ptrType.Elem().(*types.Named); !ok {
						v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
						handled = true
					}
				} else if _, ok = identType.(*types.Struct); !ok {
					v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
					handled = true
				}

				// Handle promoted struct implementations
				if !handled {
					v.writeString(target, "%s partial ref %s %s { get; }", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
				}
			}

			v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
			target.WriteString(v.newline)
		} else {
			for _, ident := range field.Names {
				v.writeString(target, "%s %s %s%s;", getAccess(ident.Name), csFullTypeName, getCoreSanitizedIdentifier(ident.Name), arrayInitializer)
				v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
				target.WriteString(v.newline)
			}
		}
	}

	v.indentLevel--
	v.writeStringLn(target, "}")

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel = preLiftIndentLevel
	}

	return
}
