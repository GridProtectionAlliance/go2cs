package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) visitExpr(n ast.Expr) {
	switch x := n.(type) {
	case *ast.Ident:
		//v.visitIdent(x)
	case *ast.Ellipsis:
		//v.visitEllipsis(x)
	case *ast.BasicLit:
		//v.visitBasicLit(x)
	case *ast.FuncLit:
		//v.visitFuncLit(x)
	case *ast.CompositeLit:
		//v.visitCompositeLit(x)
	case *ast.ParenExpr:
		//v.visitParenExpr(x)
	case *ast.SelectorExpr:
		//v.visitSelectorExpr(x)
	case *ast.IndexExpr:
		//v.visitIndexExpr(x)
	case *ast.IndexListExpr:
		//v.visitIndexListExpr(x)
	case *ast.SliceExpr:
		//v.visitSliceExpr(x)
	case *ast.TypeAssertExpr:
		//v.visitTypeAssertExpr(x)
	case *ast.CallExpr:
		//v.visitCallExpr(x)
	case *ast.StarExpr:
		//v.visitStarExpr(x)
	case *ast.UnaryExpr:
		//v.visitUnaryExpr(x)
	case *ast.BinaryExpr:
		//v.visitBinaryExpr(x)
	case *ast.KeyValueExpr:
		//v.visitKeyValueExpr(x)
	case *ast.ArrayType:
		//v.visitArrayType(x)
	case *ast.StructType:
		//v.visitStructType(x)
	case *ast.FuncType:
		//v.visitFuncType(x)
	case *ast.InterfaceType:
		//v.visitInterfaceType(x)
	case *ast.MapType:
		//v.visitMapType(x)
	case *ast.ChanType:
		//v.visitChanType(x)
	case *ast.BadExpr:
		println(fmt.Sprintf("WARNING: BadExpr encountered: %#v", x))
	default:
		panic(fmt.Sprintf("unexpected ast.Decl: %#v", x))
	}
}
