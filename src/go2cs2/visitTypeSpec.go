package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, typeSpec.Name.Name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType)
	case *ast.Ident:
		v.targetFile.WriteString(v.convIdent(typeSpecType))
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, typeSpec.Name.Name, doc)
	case *ast.MapType:
		v.visitMapType(typeSpecType)
	case *ast.ParenExpr:
		v.targetFile.WriteString(v.convParenExpr(typeSpecType))
	case *ast.SelectorExpr:
		v.targetFile.WriteString(v.convSelectorExpr(typeSpecType))
	case *ast.StarExpr:
		v.targetFile.WriteString(v.convStarExpr(typeSpecType))
	case *ast.StructType:
		v.visitStructType(typeSpecType, typeSpec.Name.Name, doc)
	default:
		panic(fmt.Sprintf("Unexpected TypeSpec type: %#v", typeSpecType))
	}
}
