package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitType(n ast.Expr, name string, comment *ast.CommentGroup) {
	switch x := n.(type) {
	case *ast.ArrayType:
		v.enterArrayType(x, name, comment)
		v.exitArrayType(x)
	case *ast.ChanType:
		//v.visitChanType(x, n)
	case *ast.FuncType:
		//v.visitFuncType(x, n)
	case *ast.Ident:
		//v.visitIdent(x, n)
	case *ast.MapType:
		//v.visitMapType(x, n)
	case *ast.ParenExpr:
		//v.visitParenExpr(x, n)
	case *ast.SelectorExpr:
		//v.visitSelectorExpr(x, n)
	case *ast.StarExpr:
		//v.visitStarExpr(x, n)
	case *ast.StructType:
		v.enterStructType(x, name)
		v.exitStructType(x)
	default:
		panic(fmt.Sprintf("unexpected ast.TypeSpec Type: %#v", x))
	}
}
