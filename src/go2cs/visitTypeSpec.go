package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)

	// Handle type alias
	if typeSpec.Assign.IsValid() {
		// Get types.Type from typeSpec.Type expr
		typeSpecType := v.info.TypeOf(typeSpec.Type)

		if typeSpecType == nil {
			panic(fmt.Sprintf("Failed to get type for type alias %s", name))
		}

		typeName := convertToCSFullTypeName(v.getFullTypeName(typeSpecType, false))

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
		v.visitIdent(typeSpecType, name)
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, name, doc)
	case *ast.MapType:
		v.visitMapType(typeSpecType)
	case *ast.ParenExpr:
		v.targetFile.WriteString(v.convParenExpr(typeSpecType))
	case *ast.SelectorExpr:
		v.targetFile.WriteString(v.convSelectorExpr(typeSpecType, DefaultLambdaContext()))
	case *ast.StarExpr:
		v.targetFile.WriteString(v.convStarExpr(typeSpecType))
	case *ast.StructType:
		v.visitStructType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, false)
	default:
		panic(fmt.Sprintf("Unexpected TypeSpec type: %#v", typeSpecType))
	}
}
