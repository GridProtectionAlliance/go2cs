package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)
	identType := v.getIdentType(typeSpec.Name)

	// Handle type alias
	if typeSpec.Assign.IsValid() {
		// Get types.Type from typeSpec.Type expr
		typeSpecType := v.info.TypeOf(typeSpec.Type)

		if typeSpecType == nil {
			panic(fmt.Sprintf("Failed to get type for type alias %s", name))
		}

		var usePackagePrefix bool

		// Check if the aliased type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(typeSpec.Type); structType != nil && !v.liftedTypeExists(structType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitStructType(structType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		// Check if the aliased type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(typeSpec.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitInterfaceType(interfaceType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		var typeNamePrefix string

		if usePackagePrefix {
			typeNamePrefix = getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix)) + "/"
		}

		typeName := convertToCSFullTypeName(typeNamePrefix + v.getFullTypeName(typeSpecType, false))

		v.typeAliasDeclarations.WriteString(fmt.Sprintf("global using %s = %s;%s", name, typeName, v.newline))

		// Add exported type aliases to package info
		if getAccess(name) == "public" {
			packageLock.Lock()
			exportedTypeAliases[name] = typeName
			packageLock.Unlock()
		}

		return
	}

	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType, name)
	case *ast.Ident:
		v.visitIdent(typeSpecType, identType, name, v.inFunction)
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	case *ast.MapType:
		v.visitMapType(typeSpecType)
	case *ast.ParenExpr:
		v.targetFile.WriteString(v.convParenExpr(typeSpecType, DefaultLambdaContext()))
	case *ast.SelectorExpr:
		v.targetFile.WriteString(v.convSelectorExpr(typeSpecType, DefaultLambdaContext()))
	case *ast.StarExpr:
		v.targetFile.WriteString(v.convStarExpr(typeSpecType, DefaultStarExprContext()))
	case *ast.StructType:
		v.visitStructType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	default:
		panic(fmt.Sprintf("Unexpected TypeSpec type: %#v", typeSpecType))
	}
}
