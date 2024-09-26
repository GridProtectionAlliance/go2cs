package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convExpr(expr ast.Expr) string {
	switch exprType := expr.(type) {
	case *ast.ArrayType:
		return v.convArrayType(exprType)
	case *ast.BasicLit:
		return v.convBasicLit(exprType, true)
	case *ast.BinaryExpr:
		return v.convBinaryExpr(exprType)
	case *ast.CallExpr:
		return v.convCallExpr(exprType)
	case *ast.ChanType:
		return v.convChanType(exprType)
	case *ast.CompositeLit:
		return v.convCompositeLit(exprType)
	case *ast.Ellipsis:
		return v.convEllipsis(exprType)
	case *ast.FuncLit:
		return v.convFuncLit(exprType)
	case *ast.FuncType:
		return v.convFuncType(exprType)
	case *ast.Ident:
		return v.convIdent(exprType)
	case *ast.IndexExpr:
		return v.convIndexExpr(exprType)
	case *ast.IndexListExpr:
		return v.convIndexListExpr(exprType)
	case *ast.InterfaceType:
		return v.convInterfaceType(exprType)
	case *ast.KeyValueExpr:
		return v.convKeyValueExpr(exprType)
	case *ast.MapType:
		return v.convMapType(exprType)
	case *ast.ParenExpr:
		return v.convParenExpr(exprType)
	case *ast.SelectorExpr:
		return v.convSelectorExpr(exprType)
	case *ast.SliceExpr:
		return v.convSliceExpr(exprType)
	case *ast.StarExpr:
		return v.convStarExpr(exprType)
	case *ast.StructType:
		return v.convStructType(exprType)
	case *ast.TypeAssertExpr:
		return v.convTypeAssertExpr(exprType)
	case *ast.UnaryExpr:
		return v.convUnaryExpr(exprType)
	case *ast.BadExpr:
		println(fmt.Sprintf("WARNING: BadExpr encountered: %#v", exprType))
		return ""
	default:
		panic(fmt.Sprintf("Unexpected Expr type: %#v", exprType))
	}
}
