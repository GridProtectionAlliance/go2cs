package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexListExpr(indexListExpr *ast.IndexListExpr) string {
	callExprContext := DefaultCallExprContext()
	callExprContext.sourceIsTypeParams = true

	// The base (X) renders WITHOUT its own generic type arguments — this appends the explicit
	// `<Indices>` here, so convSelectorExpr must not also append the inferred instance args
	// (`concurrent.NewHashTrieMap<K, V><K, V>` — unique, CS1525 cascade). See convIndexExpr.
	xContext := DefaultLambdaContext()
	xContext.suppressGenericTypeArgs = true

	return fmt.Sprintf("%s<%s>", v.convExpr(indexListExpr.X, []ExprContext{xContext}), v.convExprList(indexListExpr.Indices, indexListExpr.Lbrack, callExprContext))
}
