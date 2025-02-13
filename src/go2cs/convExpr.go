package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

type KeyValueSource int

const (
	StructSource KeyValueSource = iota
	MapSource
	ArraySource
)

type ExprContext interface {
	getDefault() StmtContext
}

type CallExprContext struct {
	u8StringArgOK     map[int]bool
	argTypeIsPtr      map[int]bool
	hasSpreadOperator bool
	keyValueSource    KeyValueSource
	keyValueIdent     *ast.Ident
	forceMultiLine    bool
	interfaceType     types.Type
	sourceIsRuneArray bool
}

func DefaultCallExprContext() *CallExprContext {
	return &CallExprContext{
		u8StringArgOK:     make(map[int]bool),
		argTypeIsPtr:      make(map[int]bool),
		hasSpreadOperator: false,
		keyValueSource:    StructSource,
		keyValueIdent:     nil,
		forceMultiLine:    false,
		interfaceType:     nil,
		sourceIsRuneArray: false,
	}
}

func (c CallExprContext) getDefault() StmtContext {
	return DefaultCallExprContext()
}

type BasicLitContext struct {
	u8StringOK        bool
	sourceIsRuneArray bool
}

func DefaultBasicLitContext() BasicLitContext {
	return BasicLitContext{
		u8StringOK:        true,
		sourceIsRuneArray: false,
	}
}

func (c BasicLitContext) getDefault() StmtContext {
	return DefaultBasicLitContext()
}

type ArrayTypeContext struct {
	compositeInitializer bool
	maxLength            int
}

func DefaultArrayTypeContext() ArrayTypeContext {
	return ArrayTypeContext{
		compositeInitializer: false,
		maxLength:            0,
	}
}

func (c ArrayTypeContext) getDefault() StmtContext {
	return DefaultArrayTypeContext()
}

type LambdaContext struct {
	isAssignment  bool
	isCallExpr    bool
	deferredDecls *strings.Builder
}

func DefaultLambdaContext() LambdaContext {
	return LambdaContext{
		isAssignment:  false,
		isCallExpr:    false,
		deferredDecls: nil,
	}
}

func (c LambdaContext) getDefault() StmtContext {
	return DefaultLambdaContext()
}

type TupleResultContext struct {
	isTupleResult bool
}

func DefaultTupleResultContext() TupleResultContext {
	return TupleResultContext{
		isTupleResult: false,
	}
}

func (c TupleResultContext) getDefault() StmtContext {
	return DefaultTupleResultContext()
}

type IdentContext struct {
	isPointer bool
	isType    bool
	name      string
}

func DefaultIdentContext() IdentContext {
	return IdentContext{
		isPointer: false,
		isType:    false,
		name:      "",
	}
}

func (c IdentContext) getDefault() StmtContext {
	return DefaultIdentContext()
}

type KeyValueContext struct {
	source KeyValueSource
	ident  *ast.Ident
}

func DefaultKeyValueContext() KeyValueContext {
	return KeyValueContext{
		source: StructSource,
		ident:  nil,
	}
}

func (c KeyValueContext) getDefault() StmtContext {
	return DefaultKeyValueContext()
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
		context := getExprContext[ArrayTypeContext](contexts)
		return v.convArrayType(exprType, context)
	case *ast.BasicLit:
		context := getExprContext[BasicLitContext](contexts)
		return v.convBasicLit(exprType, context)
	case *ast.BinaryExpr:
		context := getExprContext[PatternMatchExprContext](contexts)
		return v.convBinaryExpr(exprType, context)
	case *ast.CallExpr:
		context := getExprContext[LambdaContext](contexts)
		return v.convCallExpr(exprType, context)
	case *ast.ChanType:
		return v.convChanType(exprType)
	case *ast.CompositeLit:
		context := getExprContext[KeyValueContext](contexts)
		return v.convCompositeLit(exprType, context)
	case *ast.FuncLit:
		context := getExprContext[LambdaContext](contexts)
		return v.convFuncLit(exprType, context)
	case *ast.Ident:
		context := getExprContext[IdentContext](contexts)
		return v.convIdent(exprType, context)
	case *ast.IndexExpr:
		return v.convIndexExpr(exprType)
	case *ast.IndexListExpr:
		return v.convIndexListExpr(exprType)
	case *ast.KeyValueExpr:
		context := getExprContext[KeyValueContext](contexts)
		return v.convKeyValueExpr(exprType, context)
	case *ast.MapType:
		return v.convMapType(exprType)
	case *ast.ParenExpr:
		return v.convParenExpr(exprType)
	case *ast.SelectorExpr:
		context := getExprContext[LambdaContext](contexts)
		return v.convSelectorExpr(exprType, context)
	case *ast.SliceExpr:
		return v.convSliceExpr(exprType)
	case *ast.StarExpr:
		return v.convStarExpr(exprType)
	case *ast.TypeAssertExpr:
		return v.convTypeAssertExpr(exprType)
	case *ast.StructType:
		context := getExprContext[IdentContext](contexts)
		return v.convStructType(exprType, context)
	case *ast.UnaryExpr:
		context := getExprContext[TupleResultContext](contexts)
		return v.convUnaryExpr(exprType, context)
	case *ast.BadExpr:
		println(fmt.Sprintf("WARNING: BadExpr encountered: %#v", exprType))
		return ""
	default:
		panic(fmt.Sprintf("Unexpected Expr type: %#v", exprType))
	}
}
