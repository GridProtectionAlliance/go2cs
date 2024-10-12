package main

import (
	"fmt"
	"go/ast"
)

type ExprContext interface {
}

type CallExprContext struct {
	u8StringArgOK map[int]bool
}

type BasicLitContext struct {
	u8StringOK bool
}

// Handles pattern match expressions, e.g.: "x is 1 or > 3"
type PatternMatchExprContext struct {
	declareIsExpr bool
}

func (v *Visitor) convExpr(expr ast.Expr, context ExprContext) string {
	switch exprType := expr.(type) {
	case *ast.ArrayType:
		return v.convArrayType(exprType)
	case *ast.BasicLit:
		if context, ok := context.(*BasicLitContext); ok && context != nil {
			return v.convBasicLit(exprType, context.u8StringOK)
		}

		return v.convBasicLit(exprType, true)
	case *ast.BinaryExpr:
		if context, ok := context.(*PatternMatchExprContext); ok && context != nil {
			return v.convBinaryExpr(exprType, context)
		}

		return v.convBinaryExpr(exprType, nil)
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
