package main

import (
	"fmt"
	"go/ast"
)

type ExprContext interface {
	getDefault() StmtContext
}

type CallExprContext struct {
	u8StringArgOK map[int]bool
}

func DefaultCallExprContext() CallExprContext {
	return CallExprContext{
		u8StringArgOK: make(map[int]bool),
	}
}

func (c CallExprContext) getDefault() StmtContext {
	return DefaultCallExprContext()
}

type BasicLitContext struct {
	u8StringOK bool
}

func DefaultBasicLitContext() BasicLitContext {
	return BasicLitContext{
		u8StringOK: true,
	}
}

func (c BasicLitContext) getDefault() StmtContext {
	return DefaultBasicLitContext()
}

// Handles pattern match expressions, e.g.: "x is 1 or > 3"
type PatternMatchExprContext struct {
	usePattenMatch bool
	declareIsExpr  bool
}

func DefaultPatternMatchExprContext() PatternMatchExprContext {
	return PatternMatchExprContext{
		usePattenMatch: false,
		declareIsExpr:  false,
	}
}

func (c PatternMatchExprContext) getDefault() StmtContext {
	return DefaultPatternMatchExprContext()
}

func getExprContext[TContext ExprContext](contexts []ExprContext) TContext {
	var zeroValue TContext

	if len(contexts) == 0 {
		return zeroValue.getDefault().(TContext)
	}

	for _, context := range contexts {
		if context != nil {
			if targetContext, ok := context.(TContext); ok {
				return targetContext
			}
		}
	}

	return zeroValue.getDefault().(TContext)
}

func (v *Visitor) convExpr(expr ast.Expr, contexts []ExprContext) string {
	switch exprType := expr.(type) {
	case *ast.ArrayType:
		return v.convArrayType(exprType)
	case *ast.BasicLit:
		context := getExprContext[BasicLitContext](contexts)
		return v.convBasicLit(exprType, context)
	case *ast.BinaryExpr:
		context := getExprContext[PatternMatchExprContext](contexts)
		return v.convBinaryExpr(exprType, context)
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
