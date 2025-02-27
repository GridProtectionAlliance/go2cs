package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexListExpr(indexListExpr *ast.IndexListExpr) string {
	callExprContext := DefaultCallExprContext()
	callExprContext.sourceIsTypeParams = true

	return fmt.Sprintf("%s<%s>", v.convExpr(indexListExpr.X, nil), v.convExprList(indexListExpr.Indices, indexListExpr.Lbrack, callExprContext))
}
