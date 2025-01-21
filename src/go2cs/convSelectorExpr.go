package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr, context LambdaContext) string {
	if v.isMethodValue(selectorExpr, context.isCallExpr) && context.isAssignment {
		return fmt.Sprintf("() => %s.%s()", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, DefaultIdentContext()))
	}

	return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, DefaultIdentContext()))
}
