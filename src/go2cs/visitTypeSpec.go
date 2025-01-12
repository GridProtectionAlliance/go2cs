package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)

	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType, name)
	case *ast.Ident:
		v.targetFile.WriteString(v.convIdent(typeSpecType, DefaultIdentContext()))
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
		v.visitStructType(typeSpecType, name, doc)
	default:
		panic(fmt.Sprintf("Unexpected TypeSpec type: %#v", typeSpecType))
	}
}
